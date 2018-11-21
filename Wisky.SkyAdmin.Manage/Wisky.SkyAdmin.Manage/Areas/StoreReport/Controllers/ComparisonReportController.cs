using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using HmsService.Models.Entities;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;


namespace Wisky.SkyAdmin.Manage.Areas.StoreReport.Controllers
{
    public class ComparisonReportController : Controller
    {
        // GET: StoreReport/ComparisonReport
        public ActionResult Index(int storeId)
        {
            var storeApi = new StoreApi();
            ViewBag.BrandCreatedDate = storeApi.Get(storeId).CreateDate.Value.ToString("dd/MM/yyyy");
            return View();
        }

        public async Task<JsonResult> GetAllProducts(int storeId, string searchTokens, int page)
        {
            try
            {
                var productApi = new ProductApi();
                 var result = await productApi.GetAllStoreActiveProductsForReport(storeId); 

               
                if (!string.IsNullOrWhiteSpace(searchTokens))
                {
                    result = result.Where(q => Utils.CustomContains(q.ProductName, searchTokens) || Utils.CustomContains(q.CateName, searchTokens));
                }

                var list = result
                    .OrderBy(q => q.CateName)
                    .Skip(page * 20)
                    .Take(20)
                    .GroupBy(q => q.CateName)
                    .Select(p => new
                    {
                        text = p.Key,
                        children = p.Select(a => new
                        {
                            id = a.ProductID,
                            text = a.ProductName
                        })
                    });

                var total = result.Count();

                return Json(new { success = true, list = list, total = total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetAllProductCategories(int storeId, string searchTokens, int page)
        {
            try
            {
                var productCateApi = new ProductCategoryApi();

                // var result = productCateApi.GetProductCategoriesForStoreReport(storeId);//cmt => test method 2
                var result = productCateApi.GetProductCategoriesForStoreReport(storeId);
                if (!string.IsNullOrWhiteSpace(searchTokens))
                {
                    result = result.Where(q => Utils.CustomContains(q.CateName, searchTokens));
                }
              
                var list = result
                    .OrderBy(q => q.CateName)
                    .Skip(page * 20)
                    .Take(20)
                    .Select(p => new
                    {
                        id = p.CateID,
                        text = p.CateName
                    });

                var total = result.Count();
                return Json(new { success = true, list = list, total = total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CateProductComparisonReport(int storeId, string startTime, string endTime, string selectedItems, bool isFilterProduct)
        {
            DataViewModel model;
            IEnumerable<string> nameList;
            if (!isFilterProduct)
            {
                IEnumerable<ReportViewModel> categories = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetCategoriesComparisonData(storeId, startTime, endTime, categories);
                nameList = categories.Select(q => q.Name);
            }
            else
            {
                IEnumerable<ReportViewModel> products = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetProductsComparisonData(storeId, startTime, endTime, products);
                nameList = products.Select(q => q.Name);
            }

            if (model != null)
            {

                int count = 0;
                var reportDict = model.reportDict;
                var allRevenue = model.allRevenue;
                var reportDT = reportDict.Values.Select(q => new object[] {
                    ++count,
                    q.date.ToString("dd/MM/yyyy"),
                    q.totalList,
                    q.totalList.Max(),
                    q.finalList,
                    q.finalList.Max()
                });

                var dateList = reportDict.Keys;


                return Json(new
                {
                    isEmpty = false,
                    datatable = new { header = nameList, data = reportDT },
                    datachart = new { dateList = dateList, data = allRevenue }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isEmpty = true }, JsonRequestBehavior.AllowGet);

        }

        public DataViewModel GetCategoriesComparisonData(int storeId, string startTime, string endTime, IEnumerable<ReportViewModel> categories)
        {
            if (categories.Count() > 1)
            {
                try
                {
                    var today = Utils.GetCurrentDateTime().GetEndOfDate();
                    var sTime = startTime.ToDateTime().GetEndOfDate();
                    var eTime = endTime.ToDateTime().GetEndOfDate();
                    var dateProductApi = new DateProductApi();
                    var orderDetailApi = new OrderDetailApi();
                    IDictionary<string, ReportItem> cateReportDict = new Dictionary<string, ReportItem>();
                    List<RevenueList> allCateRevenue = new List<RevenueList>();
                    var cateNameList = categories.Select(q => q.Name);
                    foreach (var category in categories)
                    {
                        var categoryReports = dateProductApi.GetDateProductForStoreReportByCategory(sTime, eTime, storeId, category.ID);
                        List<double> cateTotalList = new List<double>();
                        List<double> cateFinalList = new List<double>();
                        for (var t = sTime; t <= eTime; t = t.AddDays(1))
                        {
                            double totalAmount = 0;
                            double finalAmount = 0;
                            if (t != today)
                            {
                                if (categoryReports != null)
                                {
                                    var dateCateReport = categoryReports.Where(q => q.Date == t);
                                    totalAmount = dateCateReport.Sum(q => q.TotalAmount);
                                    finalAmount = dateCateReport.Sum(q => q.FinalAmount);
                                }
                            }
                            else
                            {
                                var todayProductOrder = orderDetailApi.GetStoreTodayOrderDetailByProductCategory(storeId, category.ID);
                                totalAmount = todayProductOrder.Sum(q => q.TotalAmount);
                                finalAmount = todayProductOrder.Sum(q => q.FinalAmount);
                            }

                            cateTotalList.Add(totalAmount);
                            cateFinalList.Add(finalAmount);

                            //Add data to datatable
                            string key = t.ToString("dd/MM/yyyy");
                            if (cateReportDict.ContainsKey(key))
                            {
                                cateReportDict[key].totalList.Add(totalAmount);
                                cateReportDict[key].finalList.Add(finalAmount);
                            }
                            else
                            {
                                var totals = new List<double>();
                                var finals = new List<double>();
                                totals.Add(totalAmount);
                                finals.Add(finalAmount);
                                cateReportDict.Add(key, new ReportItem
                                {
                                    date = t,
                                    totalList = totals,
                                    finalList = finals
                                });
                            }
                        }
                        RevenueList cateRevenueList = new RevenueList
                        {
                            ID = category.ID,
                            Name = category.Name,
                            totalList = cateTotalList,
                            finalList = cateFinalList
                        };
                        allCateRevenue.Add(cateRevenueList);
                    }

                    return new DataViewModel
                    {
                        allRevenue = allCateRevenue,
                        reportDict = cateReportDict
                    };
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public DataViewModel GetProductsComparisonData(int storeId, string startTime, string endTime, IEnumerable<ReportViewModel> products)
        {
            //chỉ lấy dữ liệu khi có ít nhất 2 sản phẩm
            if (products.Count() > 1)
            {
                try
                {
                    var today = Utils.GetCurrentDateTime().GetEndOfDate();
                    var sTime = startTime.ToDateTime().GetEndOfDate();
                    var eTime = endTime.ToDateTime().GetEndOfDate();
                    var dateProductApi = new DateProductApi();
                    var orderDetailApi = new OrderDetailApi();
                    IDictionary<string, ReportItem> productReportDict = new Dictionary<string, ReportItem>();
                    List<RevenueList> allProductRevenue = new List<RevenueList>();
                    foreach (var product in products)
                    {
                        var productReports = dateProductApi.GetDateProductForStoreReportByProduct(sTime, eTime, storeId, product.ID);
                        List<double> productTotalList = new List<double>();
                        List<double> productFinalList = new List<double>();
                        for (var t = sTime; t <= eTime; t = t.AddDays(1))
                        {
                            double totalAmount = 0;
                            double finalAmount = 0;
                            if (t != today)
                            {
                                if (productReports != null)
                                {
                                    var dateProductReport = productReports.Where(q => q.Date == t);
                                    totalAmount = dateProductReport.Sum(q => q.TotalAmount);
                                    finalAmount = dateProductReport.Sum(q => q.FinalAmount);
                                }
                            }
                            else
                            {
                                var todayProductOrder = orderDetailApi.GetStoreTodayOrderDetailByProduct(storeId, product.ID);
                                totalAmount = todayProductOrder.Sum(q => q.TotalAmount);
                                finalAmount = todayProductOrder.Sum(q => q.FinalAmount);
                            }

                            productTotalList.Add(totalAmount);
                            productFinalList.Add(finalAmount);

                            //Add data to datatable
                            string key = t.ToString("dd/MM/yyyy");
                            if (productReportDict.ContainsKey(key))
                            {
                                productReportDict[key].totalList.Add(totalAmount);
                                productReportDict[key].finalList.Add(finalAmount);
                            }
                            else
                            {
                                var totals = new List<double>();
                                var finals = new List<double>();
                                totals.Add(totalAmount);
                                finals.Add(finalAmount);
                                productReportDict.Add(key, new ReportItem
                                {
                                    date = t,
                                    totalList = totals,
                                    finalList = finals
                                });
                            }
                        }
                        RevenueList productRevenueList = new RevenueList
                        {
                            ID = product.ID,
                            Name = product.Name,
                            totalList = productTotalList,
                            finalList = productFinalList
                        };
                        allProductRevenue.Add(productRevenueList);
                    }

                    return new DataViewModel
                    {
                        allRevenue = allProductRevenue,
                        reportDict = productReportDict
                    };
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public ActionResult ExportCateProductDataToExcel(int storeId, string startTime, string endTime, string selectedItems, bool isFilterProduct)
        {
            IDictionary<string, ReportItem> model;
            IEnumerable<string> itemList;
            string title;
            string dateRange = startTime + "-" + endTime;
            string fileName;
            string storeName = (new StoreApi()).Get(storeId).Name.ToUpper();
            if (!isFilterProduct)
            {
                IEnumerable<ReportViewModel> categories = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetCategoriesComparisonData(storeId, startTime, endTime, categories).reportDict;
                itemList = categories.Select(q => q.Name);
                title = "Báo cáo đối sánh theo Ngành hàng ( cửa hàng " + storeName + ")";
                fileName = "CategoryComparisonReport_" + storeName + "_" + dateRange;
            }
            else
            {
                IEnumerable<ReportViewModel> products = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetProductsComparisonData(storeId, startTime, endTime, products).reportDict;
                itemList = products.Select(q => q.Name);
                title = "Báo cáo đối sánh theo Sản phẩm ( cửa hàng " + storeName + ")";
                fileName = "ProductsComparisonReport_" + storeName + "_" + dateRange;
            }

            if (model == null)
            {
                return new EmptyResult();
            }

            var dateList = model.Keys;
            var numOfItem = itemList.Count();
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add(title);
                #region Headers
                char startHeaderChar = 'A';
                int startHeaderNo = 2;
                var noCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (startHeaderChar++) + (startHeaderNo + 1)];//A2:A3
                var dayCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (startHeaderChar++) + (startHeaderNo + 1)];//B2:B3
                char endHeaderChar = (char)(startHeaderChar + numOfItem - 1);
                var beforeCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (endHeaderChar++) + startHeaderNo];
                startHeaderChar = endHeaderChar;
                endHeaderChar = (char)(startHeaderChar + numOfItem - 1);
                var afterCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (endHeaderChar++) + startHeaderNo];

                noCell.Merge = true;
                noCell.Value = "STT";
                dayCell.Merge = true;
                dayCell.Value = "Ngày";
                beforeCell.Merge = true;
                beforeCell.Value = "Trước giảm giá";
                afterCell.Merge = true;
                afterCell.Value = "Sau giảm giá";
                startHeaderChar = 'C';
                startHeaderNo = 3;
                foreach (string item in itemList)
                {
                    endHeaderChar = (char)(startHeaderChar + numOfItem);
                    ws.Cells["" + startHeaderChar + startHeaderNo].Value = item;
                    ws.Cells["" + endHeaderChar + startHeaderNo].Value = item;
                    startHeaderChar++;
                }
                var endHeaderNo = startHeaderNo;
                startHeaderChar = 'A';
                startHeaderNo = 2;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + endHeaderChar + endHeaderNo.ToString()].Style.Font.Bold = true;
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + endHeaderChar + endHeaderNo.ToString()].AutoFitColumns();
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + endHeaderChar + endHeaderNo.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + endHeaderChar + endHeaderNo.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                #endregion
                #region Content
                startHeaderNo = 4;
                int count = 0;
                foreach (var date in dateList)
                {
                    startHeaderChar = 'A';
                    ws.Cells["" + (startHeaderChar++) + startHeaderNo].Value = ++count;
                    ws.Cells["" + (startHeaderChar++) + startHeaderNo].Value = date;
                    var totalList = model[date].totalList;
                    var finalList = model[date].finalList;
                    foreach (var totalAmount in totalList)
                    {
                        ws.Cells["" + (startHeaderChar++) + startHeaderNo].Value = totalAmount;
                    }
                    foreach (var finalAmount in finalList)
                    {
                        ws.Cells["" + (startHeaderChar++) + startHeaderNo].Value = finalAmount;
                    }
                    startHeaderNo++;
                }
                #endregion
                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                var titleCell = ws.Cells["A1:G1"];
                titleCell.Merge = true;
                titleCell.Value = title;
                titleCell.Style.Font.Size = 24;
                titleCell.Style.Font.Bold = true;
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = fileName + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(ms, contentType, fileDownloadName);
            }


        }
    }

    public class DataViewModel
    {
        public IDictionary<string, ReportItem> reportDict { get; set; }
        public IEnumerable<RevenueList> allRevenue { get; set; }
    }

    public class ReportViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class RevenueList
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<double> totalList { get; set; }
        public List<double> finalList { get; set; }
    }

    public class ReportItem
    {
        public DateTime date { get; set; }
        public List<double> totalList { get; set; }
        public List<double> finalList { get; set; }
    }

}
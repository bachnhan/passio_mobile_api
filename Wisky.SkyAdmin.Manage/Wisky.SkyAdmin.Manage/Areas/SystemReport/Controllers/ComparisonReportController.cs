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

namespace Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers
{
    public class ComparisonReportController : Controller
    {
        // GET: SystemReport/ComparisonReport
        public ActionResult Index()
        {
            return View();
        }

        #region Store
        public JsonResult StoresComparisonReport(string startTime, string endTime, string selectedStores, int brandId)
        {
            try
            {
                var reportData = this.GetStoresComparisonData(startTime, endTime, brandId, selectedStores, false);

                return Json(new
                {
                    success = true,
                    datatable = new
                    {
                        itemList = reportData.ItemNameList,
                        data = reportData.Reports
                    },
                    datachart = new
                    {
                        itemList = reportData.ItemNameList,
                        dateList = reportData.DateList,
                        revenueList = reportData.RevenueList
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult StoreGroupsComparisonReport(string startTime, string endTime, string selectedGroups, int brandId)
        {
            try
            {
                var reportData = this.GetStoreGroupsComparisonData(startTime, endTime, brandId, selectedGroups, false);

                return Json(new
                {
                    success = true,
                    datatable = new
                    {
                        itemList = reportData.ItemNameList,
                        data = reportData.Reports
                    },
                    datachart = new
                    {
                        itemList = reportData.ItemNameList,
                        dateList = reportData.DateList,
                        revenueList = reportData.RevenueList
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllStores(int brandId)
        {
            try
            {
                var storeApi = new StoreApi();

                var result = storeApi.GetActiveStoreByBrandId(brandId).Select(a => new
                {
                    Text = a.Name,
                    Value = a.ID.ToString()
                }).ToArray();

                return Json(new { success = true, list = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllStoreGroups(int brandId)
        {
            try
            {
                var storeGroupApi = new StoreGroupApi();

                var result = storeGroupApi.GetStoreGroupByBrandId(brandId).Select(a => new
                {
                    Text = a.GroupName,
                    Value = a.GroupID
                }).ToArray();

                return Json(new { success = true, list = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ReportData GetStoresComparisonData(string sTime, string eTime, int brandId, string selectedStores, bool printExcel)
        {
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var dateReportApi = new DateReportApi();
            List<string> storeList = new List<string>();
            List<string> dateList = new List<string>();
            List<List<double>> totalList = new List<List<double>>();
            List<List<double>> finalList = new List<List<double>>();
            List<List<int>> orderList = new List<List<int>>();

            var storeIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(selectedStores);
            foreach (var id in storeIds)
            {
                storeList.Add(storeApi.GetStoreById(id).Name);

                totalList.Add(new List<double>());
                finalList.Add(new List<double>());
                orderList.Add(new List<int>());
            }

            DateTime curDate = Utils.GetCurrentDateTime();
            DateTime startDate;
            DateTime endDate;
            if (sTime == "" || eTime == "")
            {
                startDate = new DateTime(curDate.Year, curDate.Month, 1);
                endDate = curDate.GetEndOfDate();
            }
            else
            {
                startDate = sTime.ToDateTime().GetStartOfDate();
                endDate = eTime.ToDateTime().GetEndOfDate();
            }

            curDate = curDate.GetStartOfDate();
            for (int i = 0; i < storeIds.Count; ++i)
            {
                var storeReports = dateReportApi.GetStoreReportByTimeRange(startDate, endDate, brandId, storeIds[i]);

                for (DateTime t = startDate; t <= endDate; t = t.AddDays(1))
                {
                    var storeId = storeIds[i];
                    if (t != curDate)
                    {
                        var report = storeReports.Where(r => r.Date == t.GetEndOfDate());

                        totalList[i].Add(report.Select(q => q.TotalAmount ?? 0).DefaultIfEmpty(0).Sum());
                        finalList[i].Add(report.Select(q => q.FinalAmount ?? 0).DefaultIfEmpty(0).Sum());
                        orderList[i].Add(report.Select(q => q.TotalOrder).DefaultIfEmpty(0).Sum());
                    }
                    else
                    {
                        var report = orderApi.GetStoreOrderFinishByDate(t, storeId).ToList();

                        totalList[i].Add(report.Select(r => r.TotalAmount).DefaultIfEmpty(0).Sum());
                        finalList[i].Add(report.Select(r => r.FinalAmount).DefaultIfEmpty(0).Sum());
                        orderList[i].Add(report.Count);
                    }
                }
            }

            for (DateTime t = startDate; t <= endDate; t = t.AddDays(1))
            {
                dateList.Add(t.ToString("dd/MM/yyyy"));
            }

            var rs = new List<dynamic>();
            var numOfReport = dateList.Count;

            List<IConvertible> rowData;
            List<double> rowTotalAmount;
            List<double> rowFinalAmount;
            List<int> rowTotalOrder;

            for (int i = 0; i < dateList.Count; ++i)
            {
                rowData = new List<IConvertible>();

                rowData.Add(i + 1);
                rowData.Add(dateList[i]);

                // Add dữ liệu totalAmount
                for (int j = 0; j < storeIds.Count; ++j)
                {
                    rowData.Add(totalList[j][i]);
                }
                // Add dữ liệu finalAmount
                for (int j = 0; j < storeIds.Count; ++j)
                {
                    rowData.Add(finalList[j][i]);
                }
                // Add dữ liệu finalAmount
                for (int j = 0; j < storeIds.Count; ++j)
                {
                    rowData.Add(orderList[j][i]);
                }

                // Add dữ liệu max amount của total và final amount
                // nếu không in excel (để in màu cho datatable)
                if (!printExcel)
                {
                    rowTotalAmount = new List<double>();
                    rowFinalAmount = new List<double>();
                    rowTotalOrder = new List<int>();

                    for (int j = 0; j < storeIds.Count; ++j)
                    {
                        rowTotalAmount.Add(totalList[j][i]);
                    }

                    for (int j = 0; j < storeIds.Count; ++j)
                    {
                        rowFinalAmount.Add(finalList[j][i]);
                    }

                    for (int j = 0; j < storeIds.Count; ++j)
                    {
                        rowTotalOrder.Add(orderList[j][i]);
                    }

                    rowData.Add(rowTotalAmount.Max());
                    rowData.Add(rowFinalAmount.Max());
                    rowData.Add(rowTotalOrder.Max());
                }

                rs.Add(rowData.ToArray());
            }

            List<dynamic> revenueList = new List<dynamic>();
            for (int i = 0; i < storeIds.Count; ++i)
            {
                revenueList.Add(new
                {
                    totalList = totalList[i],
                    finalList = finalList[i],
                    totalOrder = orderList[i]
                });
            }

            ReportData rd = new ReportData()
            {
                ItemNameList = storeList,
                Reports = rs,
                RevenueList = revenueList,
                DateList = dateList
            };
            return rd;
        }

        public ReportData GetStoreGroupsComparisonData(string sTime, string eTime, int brandId, string selectedGroups, bool printExcel)
        {
            List<dynamic> revenueList = new List<dynamic>();
            List<string> storeGroups = new List<string>();
            List<string> dateList = new List<string>();
            List<List<double>> totalAmountList = new List<List<double>>();
            List<List<double>> finalAmountList = new List<List<double>>();
            List<List<int>> orderList = new List<List<int>>();

            var storeApi = new StoreApi();
            var storeGroupApi = new StoreGroupApi();
            var dateReportApi = new DateReportApi();
            var orderApi = new OrderApi();

            // Lấy ngày đầu tháng tới ngày hiện tại
            DateTime curDate = Utils.GetCurrentDateTime();
            DateTime startDate;
            DateTime endDate;

            if (sTime == "" || eTime == "")
            {
                startDate = new DateTime(curDate.Year, curDate.Month, 1);
                endDate = curDate.GetEndOfDate();
            }
            else
            {
                startDate = sTime.ToDateTime().GetStartOfDate();
                endDate = eTime.ToDateTime().GetEndOfDate();
            }

            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                dateList.Add(d.ToString("dd/MM/yyyy"));
            }
            curDate = curDate.GetStartOfDate();

            var groupIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(selectedGroups);
            foreach (var id in groupIds)
            {
                storeGroups.Add(storeGroupApi.GetStoreGroupByIDEntity(id).GroupName);
                List<double> total = new List<double>();
                List<double> final = new List<double>();
                List<int> order = new List<int>();

                var groupReports = dateReportApi.GetStoreGroupReportByTimeRange(startDate, endDate, brandId, id);
                var stores = storeApi.GetStoreByGroupId(id).ToList();
                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    double allStoreTotalAmount = 0;
                    double allStoreFinalAmount = 0;
                    int allStoreOrder = 0;

                    foreach (var store in stores)
                    {
                        if (d != curDate)
                        {
                            var report = groupReports.Where(r => r.Date == d.GetEndOfDate() && r.StoreID == store.ID);

                            allStoreTotalAmount += report.Select(q => (int?)q.TotalAmount).Sum() ?? 0;
                            allStoreFinalAmount += report.Select(q => (int?)q.FinalAmount).Sum() ?? 0;
                            allStoreOrder += report.Select(q => q.TotalOrder).Sum();
                        }
                        else
                        {
                            var report = orderApi.GetStoreOrderFinishByDate(curDate, store.ID).ToList();

                            allStoreTotalAmount += report.Select(q => (int?)q.TotalAmount).Sum() ?? 0;
                            allStoreFinalAmount += report.Select(q => (int?)q.FinalAmount).Sum() ?? 0;
                            allStoreOrder += report.Count();
                        }
                    }

                    total.Add(allStoreTotalAmount);
                    final.Add(allStoreFinalAmount);
                    order.Add(allStoreOrder);
                }

                totalAmountList.Add(total);
                finalAmountList.Add(final);
                orderList.Add(order);

                revenueList.Add(new
                {
                    totalList = total,
                    finalList = final,
                    orderList = order
                });
            }

            var rs = new List<dynamic>();
            var numOfReport = dateList.Count;

            List<IConvertible> rowData;
            List<double> rowTotalAmount;
            List<double> rowFinalAmount;
            List<int> rowOrder;

            for (int i = 0; i < numOfReport; ++i)
            {
                rowData = new List<IConvertible>();

                rowData.Add(i + 1);
                rowData.Add(dateList[i]);

                // Add dữ liệu totalAmount
                for (var j = 0; j < storeGroups.Count; ++j)
                {
                    rowData.Add(totalAmountList[j][i]);
                }
                // Add dữ liệu finalAmount
                for (var j = 0; j < storeGroups.Count; ++j)
                {
                    rowData.Add(finalAmountList[j][i]);
                }
                // Add dữ liệu order count
                for (var j = 0; j < storeGroups.Count; ++j)
                {
                    rowData.Add(orderList[j][i]);
                }

                // Add dữ liệu max amount của total và final amount
                // nếu không in excel (để in màu cho datatable)
                if (!printExcel)
                {
                    rowTotalAmount = new List<double>();
                    rowFinalAmount = new List<double>();
                    rowOrder = new List<int>();

                    for (var j = 0; j < storeGroups.Count; ++j)
                    {
                        rowTotalAmount.Add(totalAmountList[j][i]);
                    }
                    for (var j = 0; j < storeGroups.Count; ++j)
                    {
                        rowFinalAmount.Add(finalAmountList[j][i]);
                    }
                    for (var j = 0; j < storeGroups.Count; ++j)
                    {
                        rowOrder.Add(orderList[j][i]);
                    }

                    rowData.Add(rowTotalAmount.Max());
                    rowData.Add(rowFinalAmount.Max());
                    rowData.Add(rowOrder.Max());
                }

                rs.Add(rowData.ToArray());
            }

            ReportData rd = new ReportData()
            {
                ItemNameList = storeGroups,
                Reports = rs,
                DateList = dateList,
                RevenueList = revenueList
            };

            return rd;
        }

        public ActionResult ExportExcelGroupReport(string startTime, string endTime, string selectedItems, bool printStore, int brandId)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                string fileDownloadName;
                #region Headers
                ReportData rd;
                if (printStore)
                {
                    rd = GetStoresComparisonData(startTime, endTime, brandId, selectedItems, true);
                    fileDownloadName = "Doanh thu cửa hàng từ " + startTime.Replace("/", "-")
                        + " đến " + endTime.Replace("/", "-") + ".xlsx";
                }
                else
                {
                    rd = GetStoreGroupsComparisonData(startTime, endTime, brandId, selectedItems, true);
                    fileDownloadName = "Doanh thu nhóm cửa hàng từ " + startTime.Replace("/", "-")
                        + " đến " + endTime.Replace("/", "-") + ".xlsx";
                }

                // 2 stores là F, 3 stores là H
                //var endColChar = (char)('C' + (rd.ItemNameList.Count * 2 - 1));
                // 2 stores là H, 3 stores là K vì thêm cột Tổng Order
                var endColChar = (char)('B' + (rd.ItemNameList.Count * 3));
                // C1:D1 hoặc C1:E1
                var totalColAddress = "C1:" + ((char)('B' + rd.ItemNameList.Count)) + "1";
                // E1:F1 hoặc F1:H1
                var finalColAddress = ((char)(totalColAddress.ElementAt(3) + 1)) + "1:" + ((char)((totalColAddress.ElementAt(3) + rd.ItemNameList.Count)) + "1");
                // G1:H1 hoặc I1:K1
                var orderColAddress = ((char)(finalColAddress.ElementAt(3) + 1)) + "1:" + ((char)((finalColAddress.ElementAt(3) + rd.ItemNameList.Count)) + "1");

                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["B1:B2"].Merge = true;
                ws.Cells[totalColAddress].Merge = true;
                ws.Cells[finalColAddress].Merge = true;
                ws.Cells[orderColAddress].Merge = true;

                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["B1:B2"].Value = "Ngày";
                ws.Cells[totalColAddress].Value = "Doanh thu trước giảm giá";
                ws.Cells[finalColAddress].Value = "Doanh thu sau giảm giá";
                ws.Cells[orderColAddress].Value = "Tổng hóa đơn";

                // In tên cửa hàng
                int i = 0;
                int endChar = 'B' + (rd.ItemNameList.Count * 3);
                for (char c = 'C'; c <= endChar; ++c)
                {
                    ws.Cells["" + c + 2].Value = rd.ItemNameList[(i++) % rd.ItemNameList.Count];
                }

                #endregion
                #region Set style for rows and columns
                ws.Cells["A1:" + endColChar + "2"].AutoFitColumns();
                ws.Cells["A1:" + endColChar + "2"].Style.Font.Bold = true;
                ws.Cells["A1:" + endColChar + "2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A1:" + endColChar + "2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A1:" + endColChar + "2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["A1:" + endColChar + "2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);

                ws.View.FreezePanes(3, 1);
                #endregion
                #region Set values for cells                
                var startColChar = 'A';
                var startColNum = 3;
                foreach (var report in rd.Reports)
                {
                    foreach (var item in report)
                    {
                        ws.Cells["" + (startColChar++) + startColNum].Value = string.Format(
                            CultureInfo.InvariantCulture, "{0:0,0}", item);
                    }
                    startColChar = 'A';
                    startColNum++;
                }
                ws.Cells["A1:" + endColChar + (startColNum - 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }

        #endregion

        #region Product

        public ActionResult IndexProduct(int brandId)
        {
            BrandApi brandApi = new BrandApi();
            ViewBag.BrandCreatedDate = brandApi.Get(brandId).CreateDate.ToString("dd/MM/yyyy");
            return View();
        }

        public async Task<JsonResult> GetAllProducts(int brandId, string searchTokens, int page)
        {
            try
            {
                var productApi = new ProductApi();
                var result = await productApi.GetAllBrandActiveProductsForReport(brandId);
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

        public async Task<JsonResult> GetProductByCateId(int brandId, int cateId)
        {
            try
            {
                var productApi = new ProductApi();
                var result = await productApi.GetAllActiveByProductCategoryAndPatternAsync(cateId, brandId, "");

                var list = result
                    .OrderBy(q => q.ProductCategory.CateName)
                    .Select(p => new
                    {
                        text = p.ProductName,
                        id = p.ProductID
                    });

                return Json(new { success = true, list = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllProductCategories(int brandId, string searchTokens, int page)
        {
            try
            {
                var productCateApi = new ProductCategoryApi();

                var result = productCateApi.GetProductCategoriesForReport(brandId);
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

        // Load all categories for normal functions
        public JsonResult GetAllProductCates(int brandId)
        {
            try
            {
                var productCateApi = new ProductCategoryApi();

                var result = productCateApi.GetProductCategoriesForReport(brandId);

                var list = result.OrderBy(q => q.CateName).Select(p => new
                {
                    id = p.CateID,
                    text = p.CateName
                });

                return Json(new { success = true, list = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CateProductComparisonReport(int brandId, string startTime, string endTime, string selectedItems, bool isFilterProduct)
        {
            DataViewModel model;
            IEnumerable<string> nameList;
            if (!isFilterProduct)
            {
                IEnumerable<ReportViewModel> categories = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetCategoriesComparisonData(brandId, startTime, endTime, categories);
                nameList = categories.Select(q => q.Text);
            }
            else
            {
                IEnumerable<ReportViewModel> products = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetProductsComparisonData(brandId, startTime, endTime, products);
                nameList = products.Select(q => q.Text);
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
                    q.finalList.Max(),
                    q.amountList,
                    q.amountList.Max()
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

        public DataViewModel GetCategoriesComparisonData(int brandId, string startTime, string endTime, IEnumerable<ReportViewModel> categories)
        {
            if (categories.Count() > 0)
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
                    //var cateNameList = categories.Select(q => q.Text);
                    foreach (var category in categories)
                    {
                        var categoryReports = dateProductApi.GetDateProductReportByCategory(sTime, eTime, brandId, category.ID);
                        List<double> cateTotalList = new List<double>();
                        List<double> cateFinalList = new List<double>();
                        List<int> quantityList = new List<int>();
                        for (var t = sTime; t <= eTime; t = t.AddDays(1))
                        {
                            double totalAmount = 0;
                            double finalAmount = 0;
                            int quantity = 0;
                            if (t != today)
                            {
                                if (categoryReports != null)
                                {
                                    var dateCateReport = categoryReports.Where(q => q.Date == t);
                                    totalAmount = dateCateReport.Sum(q => q.TotalAmount);
                                    finalAmount = dateCateReport.Sum(q => q.FinalAmount);
                                    quantity = dateCateReport.Sum(q => q.Quantity);
                                }
                            }
                            else
                            {
                                var todayProductOrder = orderDetailApi.GetTodayOrderDetailByProductCategory(brandId, category.ID);
                                totalAmount = todayProductOrder.Sum(q => q.TotalAmount);
                                finalAmount = todayProductOrder.Sum(q => q.FinalAmount);
                                quantity = todayProductOrder.Sum(q => q.Quantity);
                            }

                            cateTotalList.Add(totalAmount);
                            cateFinalList.Add(finalAmount);
                            quantityList.Add(quantity);

                            //Add data to datatable
                            string key = t.ToString("dd/MM/yyyy");
                            if (cateReportDict.ContainsKey(key))
                            {
                                cateReportDict[key].totalList.Add(totalAmount);
                                cateReportDict[key].finalList.Add(finalAmount);
                                cateReportDict[key].amountList.Add(quantity);
                            }
                            else
                            {
                                var totals = new List<double>();
                                var finals = new List<double>();
                                var quantities = new List<int>();
                                totals.Add(totalAmount);
                                finals.Add(finalAmount);
                                quantities.Add(quantity);
                                cateReportDict.Add(key, new ReportItem
                                {
                                    date = t,
                                    totalList = totals,
                                    finalList = finals,
                                    amountList = quantities
                                });
                            }
                        }
                        RevenueList cateRevenueList = new RevenueList
                        {
                            ID = category.ID,
                            Name = category.Text,
                            totalList = cateTotalList,
                            finalList = cateFinalList,
                            amountList = quantityList
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

        public DataViewModel GetProductsComparisonData(int brandId, string startTime, string endTime, IEnumerable<ReportViewModel> products)
        {
            //chỉ lấy dữ liệu khi có ít nhất 2 sản phẩm
            if (products.Count() > 0)
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
                        var productReports = dateProductApi.GetDateProductReportByProduct(sTime, eTime, brandId, product.ID);
                        List<double> productTotalList = new List<double>();
                        List<double> productFinalList = new List<double>();
                        List<int> productAmountList = new List<int>();
                        for (var t = sTime; t <= eTime; t = t.AddDays(1))
                        {
                            double totalAmount = 0;
                            double finalAmount = 0;
                            int productAmount = 0;
                            if (t != today)
                            {
                                if (productReports != null)
                                {
                                    var dateProductReport = productReports.Where(q => q.Date == t);
                                    totalAmount = dateProductReport.Sum(q => q.TotalAmount);
                                    finalAmount = dateProductReport.Sum(q => q.FinalAmount);
                                    productAmount = dateProductReport.Sum(q => q.Quantity);
                                }
                            }
                            else
                            {
                                var todayProductOrder = orderDetailApi.GetTodayOrderDetailByProduct(brandId, product.ID);
                                totalAmount = todayProductOrder.Sum(q => q.TotalAmount);
                                finalAmount = todayProductOrder.Sum(q => q.FinalAmount);
                                productAmount = todayProductOrder.Sum(q => q.Quantity);
                            }

                            productTotalList.Add(totalAmount);
                            productFinalList.Add(finalAmount);
                            productAmountList.Add(productAmount);

                            //Add data to datatable
                            string key = t.ToString("dd/MM/yyyy");
                            if (productReportDict.ContainsKey(key))
                            {
                                productReportDict[key].totalList.Add(totalAmount);
                                productReportDict[key].finalList.Add(finalAmount);
                                productReportDict[key].amountList.Add(productAmount);
                            }
                            else
                            {
                                var totals = new List<double>();
                                var finals = new List<double>();
                                var amounts = new List<int>();
                                totals.Add(totalAmount);
                                finals.Add(finalAmount);
                                amounts.Add(productAmount);
                                productReportDict.Add(key, new ReportItem
                                {
                                    date = t,
                                    totalList = totals,
                                    finalList = finals,
                                    amountList = amounts
                                });
                            }
                        }
                        RevenueList productRevenueList = new RevenueList
                        {
                            ID = product.ID,
                            Name = product.Text,
                            totalList = productTotalList,
                            finalList = productFinalList,
                            amountList = productAmountList
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

        public ActionResult ExportCateProductDataToExcel(int brandId, string startTime, string endTime, string selectedItems, bool isFilterProduct)
        {
            IDictionary<string, ReportItem> model;
            IEnumerable<string> itemList;
            string title;
            string dateRange = startTime + "-" + endTime;
            string fileName;

            if (!isFilterProduct)
            {
                IEnumerable<ReportViewModel> categories = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetCategoriesComparisonData(brandId, startTime, endTime, categories).reportDict;
                itemList = categories.Select(q => q.Text);
                title = "Báo cáo đối sánh theo loại sản phẩm";
                fileName = "CategoryComparisonReport_" + dateRange;
            }
            else
            {
                IEnumerable<ReportViewModel> products = JsonConvert.DeserializeObject<IEnumerable<ReportViewModel>>(selectedItems);
                model = GetProductsComparisonData(brandId, startTime, endTime, products).reportDict;
                itemList = products.Select(q => q.Text);
                title = "Báo cáo đối sánh theo sản phẩm";
                fileName = "ProductsComparisonReport_" + dateRange;
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
                char quantityHeaderChar = '0';
                char startHeaderChar = 'A';
                int startHeaderNo = 2;
                var noCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (startHeaderChar++) + (startHeaderNo + 1)];//A2:A3
                var dayCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (startHeaderChar++) + (startHeaderNo + 1)];//B2:B3
                char endHeaderChar = (char)(startHeaderChar + numOfItem - 1);
                var beforeCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (endHeaderChar++) + startHeaderNo];
                startHeaderChar = endHeaderChar;
                endHeaderChar = (char)(startHeaderChar + numOfItem - 1);
                var afterCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (endHeaderChar++) + startHeaderNo];
                startHeaderChar = endHeaderChar;
                endHeaderChar = (char)(startHeaderChar + numOfItem - 1);
                var quantityCell = ws.Cells["" + startHeaderChar + startHeaderNo + ":" + (endHeaderChar++) + startHeaderNo];

                noCell.Merge = true;
                noCell.Value = "STT";
                dayCell.Merge = true;
                dayCell.Value = "Ngày";
                beforeCell.Merge = true;
                beforeCell.Value = "Trước giảm giá";
                afterCell.Merge = true;
                afterCell.Value = "Sau giảm giá";
                quantityCell.Merge = true;
                quantityCell.Value = "Số lượng sản phẩm";
                startHeaderChar = 'C';
                startHeaderNo = 3;
                foreach (string item in itemList)
                {
                    endHeaderChar = (char)(startHeaderChar + numOfItem);
                    quantityHeaderChar = (char)(endHeaderChar + numOfItem);
                    ws.Cells["" + startHeaderChar + startHeaderNo].Value = item;
                    ws.Cells["" + endHeaderChar + startHeaderNo].Value = item;
                    ws.Cells["" + quantityHeaderChar + startHeaderNo].Value = item;
                    startHeaderChar++;
                }
                var endHeaderNo = startHeaderNo;
                startHeaderChar = 'A';
                startHeaderNo = 2;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + quantityHeaderChar + endHeaderNo.ToString()].Style.Font.Bold = true;
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + quantityHeaderChar + endHeaderNo.ToString()].AutoFitColumns();
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + quantityHeaderChar + endHeaderNo.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + startHeaderChar + startHeaderNo.ToString() +
                    ":" + quantityHeaderChar + endHeaderNo.ToString()]
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
                    var quantityList = model[date].amountList;
                    foreach (var totalAmount in totalList)
                    {
                        ws.Cells["" + (startHeaderChar++) + startHeaderNo].Value = totalAmount;
                    }
                    foreach (var finalAmount in finalList)
                    {
                        ws.Cells["" + (startHeaderChar++) + startHeaderNo].Value = finalAmount;
                    }
                    foreach (var quantity in quantityList)
                    {
                        ws.Cells["" + (startHeaderChar++) + startHeaderNo].Value = quantity;
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
                ws.Cells["A1:" + (char)('A' + (numOfItem * 3) + 1) + "34"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                var titleCell = ws.Cells["A1:" + (char)('A' + (numOfItem * 3) + 1) + "1"];
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
        #endregion
    }

    public class DataViewModel
    {
        public IDictionary<string, ReportItem> reportDict { get; set; }
        public IEnumerable<RevenueList> allRevenue { get; set; }
    }

    public class ReportViewModel
    {
        public int ID { get; set; }
        public string Text { get; set; }
    }

    public class RevenueList
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<double> totalList { get; set; }
        public List<double> finalList { get; set; }
        public List<int> amountList { get; set; }
    }

    public class ReportItem
    {
        public DateTime date { get; set; }
        public List<double> totalList { get; set; }
        public List<double> finalList { get; set; }
        public List<int> amountList { get; set; }
    }

    public class ReportData
    {
        public List<string> ItemNameList { get; set; }
        public List<dynamic> Reports { get; set; }
        public List<dynamic> RevenueList { get; set; }
        public List<string> DateList { get; set; }
    }

}

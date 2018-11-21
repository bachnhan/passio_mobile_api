using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Forms;
using HmsService.Filter;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Wisky.SkyAdmin.Manage.Controllers;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Wisky.SkyAdmin.Manage.Areas.PosReport.Controllers
{
    [Authorize(Roles = "BrandManager, StoreManager, StoreReportViewer, Manager")]
    public class StoreReportController : DomainBasedController
    {
        // GET: PosReport/StoreReport
        [ValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return View();
        }


        #region Category Report
        public ActionResult StoreCategoryReport(int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            var listCategory = productCategoryApi.GetActiveProductCategoriesByBrandId(brandId);
            return View(listCategory);
        }

        public JsonResult LoadCategoryReport(JQueryDataTableParamModel param, int brandId, int storeId, string startTime, string endTime, int catetoryId)
        {
            var productCategoryApi = new ProductCategoryApi();
            var orderDetailApi = new OrderDetailApi();

            //Create List category report
            var categories = new List<Tuple<string, int, int, int, string, string>>();

            //Get list category in DB
            var listCategory = catetoryId == 0 ? productCategoryApi.GetActiveProductCategoriesByBrandId(brandId) :
                productCategoryApi.GetActiveProductCategoriesByBrandId(brandId).Where(a => a.CateID == catetoryId);
            var totalProduct = 0;
            //var isAdmin = Roles.GetRolesForUser().Contains("Administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");
            //Check startTime and EndTime input
            if ((startTime == "" && endTime == "") || (startTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy") && endTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy")))
            {
                var dateToGet = Utils.GetCurrentDateTime();
                //if (!isAdmin)
                //{
                //    dateToGet = Utils.GetCurrentDateTime().AddDays(-1);
                //}

                //var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                //var endDate = dateNow.AddDays(-1).GetEndOfDate();

                var startDate = dateToGet.GetStartOfDate();
                var endDate = dateToGet.GetEndOfDate();

                IEnumerable<OrderDetail> dateProdcuts;

                if (storeId > 0)
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId);
                    dateProdcuts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId).ToList();
                }
                else
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductAllStoreByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate());
                    dateProdcuts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate).ToList();
                }

                //Group dateProdcuts
                var result = dateProdcuts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.FinalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    TotalBill = r.Count(a => a.Status == (int)OrderStatusEnum.Finish)
                }).ToList();

                //Total Amount
                var finalAmount = dateProdcuts.Sum(a => a.FinalAmount);

                //Total Bill
                decimal totalBill = (decimal)dateProdcuts.Count(a => a.Status == (int)OrderStatusEnum.Finish);

                //Rent
                //                var orders = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                //                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                foreach (var itemCat in listCategory)
                {

                    var quantity = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.ToTal);
                    var totalDiscount = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalDiscount);
                    var totalBillCate = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalBill);
                    decimal test = 0;
                    if (finalAmount > 0)
                    {
                        test = (Convert.ToDecimal(total) / Convert.ToDecimal(finalAmount) * 100);
                    }
                    var rateRevenue = test.ToString("0.00") + "%";

                    decimal test1 = 0;
                    if (totalBill != 0)
                    {
                        test1 = (Convert.ToDecimal(totalBillCate) / totalBill * 100);
                    }

                    var rateOrder = test1.ToString("0.00") + "%";

                    if (total > 0)
                        categories.Add(new Tuple<string, int, int, int, string, string>(itemCat.CateName, quantity, (int)total, (int)totalDiscount, rateOrder, rateRevenue));
                    totalProduct += quantity;
                }
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            dataTable = 0,
                //            dataChart = new
                //            {
                //                listCategories = 0,
                //                totalProduct = 0,
                //            },
                //        }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
                IEnumerable<OrderDetail> dateProdcuts;

                if (storeId > 0)
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId);
                    dateProdcuts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId).ToList();
                }
                else
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductAllStoreByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate());
                    dateProdcuts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate).ToList();
                }

                //Group dateProdcuts
                var result = dateProdcuts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.FinalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    //                    TotalBill = 123 //tam thoi de do, thinking .....                   
                    TotalBill = r.Count(a => a.Status == (int)OrderStatusEnum.Finish)
                }).ToList();
                //Total Amount
                var finalAmount = dateProdcuts.Sum(a => a.FinalAmount);

                //Total Bill
                //decimal totalBill = (decimal)dateProdcuts.Sum(a => a.OrderQuantity);
                decimal totalBill = (decimal)dateProdcuts.Count(a => a.Status == (int)OrderStatusEnum.Finish);

                ////Rent
                //var rents = _rentService.GetRentsByTimeRange(storeId, startDate, endDate)
                //        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                foreach (var itemCat in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.ToTal);
                    var totalDiscount = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalDiscount);
                    //--CuongHH-- 
                    var totalBillCate = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalBill);
                    decimal test = 0;
                    if (finalAmount > 0)
                    {
                        test = (Convert.ToDecimal(total) / Convert.ToDecimal(finalAmount) * 100);
                    }
                    var rateRevenue = test.ToString("0.00") + "%";

                    decimal test1 = 0;
                    //--CuongHH--  
                    if (totalBill != 0)
                    {
                        test1 = (Convert.ToDecimal(totalBillCate) / totalBill * 100);
                    }
                    var rateOrder = test1.ToString("0.00") + "%";

                    if (total > 0)
                        categories.Add(new Tuple<string, int, int, int, string, string>(itemCat.CateName, quantity, (int)total, (int)totalDiscount, rateOrder, rateRevenue));
                    totalProduct += quantity;
                }

            }
            int count = 1;
            var list = categories.Select(a => new IConvertible[]
            {
                count++,
                a.Item1,
                a.Item2,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.Item3),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.Item4),
                a.Item5,
                a.Item6
            }).ToList();
            return Json(new
            {
                dataTable = list,
                dataChart = new
                {
                    listCategories = categories,
                    totalProduct = totalProduct,
                },
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadComparisonStoreReport(string startTime, string endTime, string cateName)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var startDate = dateNow.GetStartOfDate();
            var endDate = dateNow.GetEndOfDate();
            var storeApi = new StoreApi();
            var productCategoryApi = new ProductCategoryApi();
            var dateProductApi = new DateProductApi();
            if ((startTime != "" && endTime != "") || (startTime != Utils.GetCurrentDateTime().ToString("dd/MM/yyyy") && endTime != Utils.GetCurrentDateTime().ToString("dd/MM/yyyy")))
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }

            var category = productCategoryApi.GetProductCategories().Where(a => a.CateName == cateName).ToList()[0];
            var listStores = storeApi.GetStores().Where(x => x.isAvailable == true && x.Type == 5);
            List<TempStoreFinalAmountViewModel> listComparisonStores = new List<TempStoreFinalAmountViewModel>();
            List<string> listStoreName = new List<string>();
            List<double> listFinalAmount = new List<double>();

            foreach (var store in listStores)
            {
                var storeFinalAmount = dateProductApi.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), store.ID)
                    .Where(w => w.CategoryId_ == category.CateID)
                    .GroupBy(g => g.CategoryId_)
                    .Select(sl => new
                    {
                        StoreName = store.Name,
                        FinalAmount = sl.Sum(x => x.FinalAmount)
                    }).ToList();

                if (storeFinalAmount.Count > 0)
                {
                    var storeName = storeFinalAmount[0].StoreName;
                    var finalAmount = storeFinalAmount[0].FinalAmount;
                    listComparisonStores.Add(new TempStoreFinalAmountViewModel
                    {
                        StoreName = storeName,
                        FinalAmount = finalAmount
                    });

                    listStoreName.Add(storeName);
                    listFinalAmount.Add(finalAmount);
                }
            }

            var finalList = listComparisonStores.Select(a => new IConvertible[] {
                a.StoreName,
                a.FinalAmount
            }).ToArray();

            return Json(new
            {
                dataTable = finalList,
                dataChart = new
                {
                    xAxis = listStoreName,
                    yAxis = listFinalAmount
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public void exportExcel(List<string> headers, IEnumerable<object> _list, ref string fileName, ref bool success)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            //folderDlg.ShowNewFolderButton = true;
            string selectedPath = "";
            //Environment.SpecialFolder root = Environment.SpecialFolder.DesktopDirectory;

            DialogResult confirm = folderDlg.ShowDialog();
            if (confirm == DialogResult.OK)
            {

                Environment.SpecialFolder root = folderDlg.RootFolder;
                selectedPath = folderDlg.SelectedPath;
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    int length = selectedPath.Length;
                    int temp = selectedPath.LastIndexOf("\\");
                    if (selectedPath.LastIndexOf("\\") == length - 1)
                    {

                        fileName = selectedPath + fileName + ".xls";
                    }
                    else
                    {
                        fileName = selectedPath + "\\" + fileName + ".xls";
                    }
                    var result = ExportToExcelExtensions.ExportToExcel(headers, _list, fileName);
                    if (result)
                    {
                        success = true;
                    }
                }
            }
        }
        public List<dynamic> ExportCategoryTableToExcel(int brandId, string startTime, string endTime, int catetoryId, int storeId)
        {
            #region Get data
            //var id = Session["storeId"].ToString();
            //var storeId = Convert.ToInt32(id);
            //Create List category report
            var orderApi = new OrderApi();
            var storeApi = new StoreApi();
            var orderDetailApi = new OrderDetailApi();
            var productCategoryApi = new ProductCategoryApi();
            var categories = new List<Tuple<string, int, int, int, string, string>>();

            //Get list category in DB
            var listCategory = catetoryId == 0 ? productCategoryApi.GetActiveProductCategoriesByBrandId(brandId) :
                productCategoryApi.GetActiveProductCategoriesByBrandId(brandId).Where(a => a.CateID == catetoryId);
            var totalProduct = 0;
            //Check startTime and EndTime input
            if ((startTime == "" && endTime == "") || (startTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy") && endTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy")))
            {
                var dateNow = Utils.GetCurrentDateTime();

                //var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                //var endDate = dateNow.AddDays(-1).GetEndOfDate();

                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                IEnumerable<OrderDetail> dateProdcuts;

                if (storeId > 0)
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId);
                    dateProdcuts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId);
                }
                else
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductAllStoreByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate());
                    dateProdcuts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate);
                }

                //Group dateProdcuts
                var result = dateProdcuts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.FinalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    TotalBill = 123 //tam thoi de do, thinking .....                   
                    //TotalBill = r.Sum(a => a  .OrderQuantity)
                }).ToList();

                //Total Amount
                var finalAmount = dateProdcuts.Sum(a => a.FinalAmount);

                //Total Bill
                //decimal totalBill = (decimal)dateProdcuts.Sum(a => a.OrderQuantity);
                decimal totalBill = 123;

                //Rent
                var rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                foreach (var itemCat in listCategory)
                {

                    var quantity = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.ToTal);
                    var totalDiscount = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalDiscount);
                    var totalBillCate = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalBill);
                    decimal test = 0;
                    if (finalAmount != 0)
                    {
                        test = (Convert.ToDecimal(total) / Convert.ToDecimal(finalAmount) * 100);
                    }
                    var rateRevenue = test.ToString("0.00") + "%";

                    decimal test1 = 0;
                    if (totalBill != 0)
                    {
                        test1 = (Convert.ToDecimal(totalBillCate) / totalBill * 100);
                    }

                    var rateOrder = test1.ToString("0.00") + "%";

                    if (total != null)
                        categories.Add(new Tuple<string, int, int, int, string, string>(itemCat.CateName, quantity, (int)total, (int)totalDiscount, rateOrder, rateRevenue));
                    totalProduct += quantity;
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                IEnumerable<OrderDetail> dateProdcuts;

                if (storeId > 0)
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId);
                    dateProdcuts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId);
                }
                else
                {
                    //Get DateProduct
                    //dateProdcuts = _dateProductService.GetDateProductAllStoreByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate());
                    dateProdcuts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate);
                }

                //Group dateProdcuts
                var result = dateProdcuts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.FinalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    TotalBill = 123 //tam thoi de do, thinking .....                   
                    //TotalBill = r.Sum(a => a  .OrderQuantity)
                }).ToList();
                //Total Amount
                var finalAmount = dateProdcuts.Sum(a => a.FinalAmount);

                //Total Bill
                //decimal totalBill = (decimal)dateProdcuts.Sum(a => a.OrderQuantity);
                decimal totalBill = 123;

                ////Rent
                //var rents = _rentService.GetRentsByTimeRange(storeId, startDate, endDate)
                //        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                foreach (var itemCat in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.ToTal);
                    var totalDiscount = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalDiscount);
                    //--CuongHH-- 
                    var totalBillCate = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.TotalBill);
                    decimal test = 0;
                    if (finalAmount != 0)
                    {
                        test = (Convert.ToDecimal(total) / Convert.ToDecimal(finalAmount) * 100);
                    }
                    var rateRevenue = test.ToString("0.00") + "%";

                    decimal test1 = 0;
                    //--CuongHH--  
                    if (totalBill != 0)
                    {
                        test1 = (Convert.ToDecimal(totalBillCate) / totalBill * 100);
                    }
                    var rateOrder = test1.ToString("0.00") + "%";

                    if (total != null)
                        categories.Add(new Tuple<string, int, int, int, string, string>(itemCat.CateName, quantity, (int)total, (int)totalDiscount, rateOrder, rateRevenue));
                    totalProduct += quantity;
                }

            }
            List<dynamic> listDT = new List<dynamic>();
            foreach (var a in categories)
            {
                listDT.Add(new
                {
                    a = a.Item1,
                    b = a.Item2,
                    c = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.Item3),
                    d = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.Item4),
                    e = a.Item5,
                    f = a.Item6
                });
            }
            #endregion

            //Thread thdSyncRea = new Thread(new ThreadStart(browseFile));
            //if (thdSyncRea.IsAlive);
            //thdSyncRea.SetApartmentState(ApartmentState.STA);
            //thdSyncRea.Start();
            #region Export to Excel
            //List<string> header = new List<string>();
            //header.Add("STT;1;1");
            //header.Add("Tên danh mục;1;1");
            //header.Add("Số lượng sản phẩm;1;1");
            //header.Add("Tổng doanh thu;1;1");
            //header.Add("Giảm giá;1;1");
            //header.Add("Tỉ lệ xuất hiện trên hóa đơn/Tổng hóa đơn;1;1");
            //header.Add("Tỉ lệ đóng góp doanh số;1;1");

            //var storeName = storeApi.GetStoreNameByID(storeId);
            //var sDate = startTime.Replace("/", "-");
            //var eDate = endTime.Replace("/", "-");
            //var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
            //string fileName = "BáoCáoTheoNgànhHàng_" + storeName + dateRange;

            ////var success = ExportToExcelExtensions.ExportToExcel(header, list, fileName);
            //bool success = false;
            //Thread thdSyncRead = new Thread(new ThreadStart(() => exportExcel(header, list, ref fileName, ref success)));
            //thdSyncRead.SetApartmentState(ApartmentState.STA);
            //thdSyncRead.Start();
            //thdSyncRead.Join(120000);
            //if (!success)
            //{
            //    thdSyncRead.Abort();
            //}
            #endregion
            return listDT;


        }
       
        public ActionResult ReportCategoryExportExcelEPPlus(int brandId, string startTime, string endTime, int categoryId, int storeId)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo theo ngành hàng");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = ExportCategoryTableToExcel(brandId, startTime, endTime, categoryId, storeId);
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên danh mục";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng sản phẩm";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng doanh thu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tỉ lệ xuất hiện trên hóa đơn/Tổng hóa đơn";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tỉ lệ đóng góp doanh số";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells             
                foreach (var data in listDT)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.a;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.b;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.c;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.d;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.e;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.f;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo theo ngành hàng từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion

        #region Product Category Report
        public ActionResult StoreProductCategoryReport(string storeid)
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        public JsonResult LoadProductCategoryReport(int brandId, string startTime, string endTime, int storeId, int? startHour, int? endHour)
        {
            #region new version
            var st = new Stopwatch();
            var productCategoryApi = new ProductCategoryApi();
            var orderDetailApi = new OrderDetailApi();
            //Create empty list product category
            List<GroupCategoryReportModalViewModel> fillterList = new List<GroupCategoryReportModalViewModel>();
            List<TempProductByCategoryViewModel> listProductByCategory = new List<TempProductByCategoryViewModel>();
            List<decimal> listPercentQuantityChart = new List<decimal>();
            List<string> listProductNameChart = new List<string>();

            //Get category in DB
            st.Start();

            var listCategory = productCategoryApi.GetActiveProductCategoriesByBrandId(brandId).Where(a => a.Type == 1);
            Debug.WriteLine("Get list category: " + st.ElapsedMilliseconds);
            var isAdmin = User.IsInRole("Administrator");
            if (!startHour.HasValue && !endHour.HasValue)
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            dataTable = 0,
                //        }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
                st.Restart();
                IEnumerable<OrderDetail> dateProducts;
                if (storeId > 0)
                {
                    dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId).ToList();

                }
                else
                {
                    dateProducts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate).ToList();
                }
                Debug.WriteLine("Get product list from otderDetailService: " + st.ElapsedMilliseconds);

                st.Restart();
                var result =
                    dateProducts.GroupBy(
                        r =>
                            new
                            {
                                r.ProductID
                            }).Select(r => new
                            {
                                ProductId = r.Key.ProductID,
                                Quantity = r.Sum(a => a.Quantity),
                                ToTal = r.Sum(a => a.FinalAmount)
                            }).ToList();
                Debug.WriteLine("Group by product id: " + st.ElapsedMilliseconds);

                // Total quantity
                st.Restart();
                var finalTotalQuantity = result.Sum(s => s.Quantity);
                Debug.WriteLine("Calculate final quantity: " + st.ElapsedMilliseconds);

                if (finalTotalQuantity > 0)
                {
                    st.Restart();
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.Active);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            var percentQuantity = totalQuantity;
                            //var percentQuantity = (Convert.ToDecimal(totalQuantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            fillterList.Add(new GroupCategoryReportModalViewModel
                            {
                                ProductId = itemP.ProductID,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }

                    Debug.WriteLine("Filter list (no hour range): " + st.ElapsedMilliseconds);
                }
                else
                {
                    st.Restart();
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.Active);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            fillterList.Add(new GroupCategoryReportModalViewModel
                            {
                                ProductId = itemP.ProductID,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }

                    Debug.WriteLine("Filter list (no hour range): " + st.ElapsedMilliseconds);
                }
            }
            else
            {
                #region Cách mới
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            dataTable = 0,
                //        }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
                st.Restart();
                IEnumerable<OrderDetail> dateProducts;
                if (storeId > 0)
                {
                    dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId, startHour, endHour).ToList();

                }
                else
                {
                    dateProducts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate, startHour, endHour).ToList();
                }
                Debug.WriteLine("Get product list from otderDetailService: " + st.ElapsedMilliseconds);

                st.Restart();
                var result =
                    dateProducts.GroupBy(
                        r =>
                            new
                            {
                                r.ProductID
                            }).Select(r => new
                            {
                                ProductId = r.Key.ProductID,
                                Quantity = r.Sum(a => a.Quantity),
                                ToTal = r.Sum(a => a.FinalAmount)
                            }).ToList();
                Debug.WriteLine("Group by product id: " + st.ElapsedMilliseconds);

                // Total quantity
                st.Restart();
                var finalTotalQuantity = result.Sum(s => s.Quantity);
                Debug.WriteLine("Calculate final quantity: " + st.ElapsedMilliseconds);

                if (finalTotalQuantity > 0)
                {
                    st.Restart();
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.Active);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            var percentQuantity = totalQuantity;
                            //var percentQuantity = (Convert.ToDecimal(totalQuantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            fillterList.Add(new GroupCategoryReportModalViewModel
                            {
                                ProductId = itemP.ProductID,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }

                    Debug.WriteLine("Filter list (no hour range): " + st.ElapsedMilliseconds);
                }
                #endregion
                #region Cách cũ
                //var startDate = startTime.ToDateTime();
                //var endDate = endTime.ToDateTime();
                ////if (!isAdmin)
                ////{
                ////    if (startDate == DateTime.Today)
                ////    {
                ////        return Json(new
                ////        {
                ////            datatable = 0,                     
                ////        }, JsonRequestBehavior.AllowGet);
                ////    }
                ////    if (endDate >= DateTime.Today)
                ////    {
                ////        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                ////    }
                ////}
                //startDate = startDate.GetStartOfDate();
                //endDate = endDate.GetEndOfDate();


                //for (DateTime i = startDate; startDate <= endDate; startDate = startDate.AddDays(1))
                //{
                //    st.Restart();
                //    var sTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startHour.Value, 0, 0);
                //    var eTime = new DateTime();
                //    if (endHour == 24)
                //    {
                //        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
                //    }
                //    else
                //    {
                //        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, endHour.Value, 0, 0);
                //    }

                //    var dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(sTime, eTime, storeId).ToList();
                //    var result =
                //    dateProducts.GroupBy(
                //        r =>
                //            new
                //            {
                //                r.ProductID
                //            }).Select(r => new
                //            {
                //                ProductId = r.Key.ProductID,
                //                Quantity = r.Sum(a => a.Quantity),
                //                ToTal = r.Sum(a => a.FinalAmount)
                //            }).ToList();

                //    Debug.WriteLine("Get list from orderDetailServie (hour range): " + st.ElapsedMilliseconds);

                //    st.Restart();
                //    foreach (var itemCat in listCategory)
                //    {
                //        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true).ToList();

                //        foreach (var itemP in listProduct)
                //        {
                //            //var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                //            //fillterList.Remove(productItem);

                //            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                //            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                //            //var totalPrice = (int)sum;

                //            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                //            if (productItem != null)
                //            {
                //                productItem.TotalPrice = productItem.TotalPrice + totalPrice;
                //                productItem.Quantity = productItem.Quantity + totalQuantity;
                //            }
                //            else
                //            {
                //                fillterList.Add(new GroupCategoryReportModalViewModel
                //                {
                //                    ProductId = itemP.ProductID,
                //                    CateName = itemP.ProductCategory.CateName,
                //                    ProductName = itemP.ProductName,
                //                    Quantity = totalQuantity,
                //                    TotalPrice = totalPrice,
                //                });
                //            }
                //        }
                //    }

                //    Debug.WriteLine("Filter list (hour range): " + st.ElapsedMilliseconds);
                //}

                //st.Restart();
                //var finalTotalQuantity = fillterList.Sum(s => s.Quantity);
                //if (finalTotalQuantity > 0)
                //{
                //    var listProductName = fillterList.Select(s => s.ProductName).ToList();
                //    var listQuantity = fillterList.Select(s => s.Quantity).ToList();

                //    for (int i = 0; i < listProductName.Count(); i++)
                //    {
                //        var productName = listProductName[i];
                //        var quantity = listQuantity[i];
                //        var percentQuantity = quantity;
                //        //var percentQuantity = (Convert.ToDecimal(quantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                //        if (percentQuantity > 0)
                //        {
                //            listProductNameChart.Add(productName);
                //            listPercentQuantityChart.Add(percentQuantity);
                //        }
                //    }
                //}

                //Debug.WriteLine("Data for chart (hour range): " + st.ElapsedMilliseconds);
                #endregion
            }

            st.Restart();
            var list = fillterList.Select(a => new IConvertible[]
            {
                a.ProductName,
                a.CateName,
                string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.Quantity),
                string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.TotalPrice),
                a.ProductId,
            }).ToList();
            Debug.WriteLine("Select list to return: " + st.ElapsedMilliseconds);

            if (listPercentQuantityChart.Count > 0)
            {
                return Json(new
                {
                    dataTable = list,
                    dataChart = new
                    {
                        listProductName = listProductNameChart,
                        listPercentQuantity = listPercentQuantityChart,
                    },
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    dataTable = list,
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            #region oldVerison
            ////Create empty list product category
            //List<GroupCategoryReportModal> fillterList = new List<GroupCategoryReportModal>();

            ////Get category in DB
            //var listCategory = _productCategoryService.GetProductCategories().Where(a => a.IsDisplayed && a.Type == 1);
            ////Check StartTime and EndTime input
            //if ((startTime == "" && endTime == "") || (startTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy") && endTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy")))
            //{
            //    var dateNow = Utils.GetCurrentDateTime();
            //    var startDate = dateNow.GetStartOfDate();
            //    var endDate = dateNow.GetEndOfDate();

            //    IEnumerable<DateProduct> dateProdcuts;

            //    if (storeId > 0)
            //    {
            //        dateProdcuts = _dateProductService.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId).ToList();

            //    }
            //    else
            //    {
            //        dateProdcuts = _dateProductService.GetDateProductAllStoreByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate()).ToList();
            //    }

            //    var result =
            //        dateProdcuts.GroupBy(
            //            r =>
            //                new
            //                {
            //                    r.ProductId,

            //                }).Select(r => new
            //                {
            //                    ProductId = r.Key.ProductId,
            //                    Quantity = r.Sum(a => a.Quantity),
            //                    ToTal = r.Sum(a => a.FinalAmount),
            //                    ToTalAtStore = r.Sum(a => a.QuantityAtStore),
            //                    TotalTakeAway = r.Sum(a => a.QuantityTakeAway),
            //                    TotalDelivery = r.Sum(a => a.QuantityDelivery),
            //                    Discount = r.Sum(a => a.Discount)
            //                }).ToList();

            //    var totalProduct = 0;
            //    foreach (var itemCat in listCategory)
            //    {
            //        var categoryQuantity = 0;
            //        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
            //        foreach (var itemP in listProduct)
            //        {
            //            categoryQuantity += itemP.DateProducts.Count();
            //            totalProduct += categoryQuantity;
            //            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
            //            fillterList.Remove(productItem);
            //            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
            //            var totalQuantityAtStore = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTalAtStore);
            //            var totalQuantityTakeAway = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.TotalTakeAway);
            //            var totalQuantityDelivery = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.TotalDelivery);
            //            var sum = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
            //            var discount = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Discount);
            //            // var totalOrder = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.TotalOrder);

            //            var totalPrice = (int)sum;

            //            // phân loại category
            //            fillterList.Add(new GroupCategoryReportModal
            //            {
            //                ProductId = itemP.ProductID,
            //                CateName = itemP.ProductCategory.CateName,
            //                ProductName = itemP.ProductName,
            //                Quantity = totalQuantity,
            //                TotalPrice = totalPrice,
            //                QuantityAtStore = (int)totalQuantityAtStore,
            //                QuantityTakeAway = (int)totalQuantityTakeAway,
            //                QuantityDelivery = (int)totalQuantityDelivery,
            //                Discount = discount,
            //                //TotalOrder = (int)totalOrder
            //            });

            //        }
            //        //categories.Add(new Tuple<string, int>(itemCat.CateName, categoryQuantity));
            //    }

            //}
            //else
            //{
            //    var startDate = startTime.ToDateTime();
            //    var endDate = endTime.ToDateTime();

            //    IEnumerable<DateProduct> dateProdcuts;

            //    if (storeId > 0)
            //    {
            //        dateProdcuts = _dateProductService.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId).ToList();

            //    }
            //    else
            //    {
            //        dateProdcuts = _dateProductService.GetDateProductAllStoreByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate()).ToList();
            //    }

            //    var result =
            //        dateProdcuts.GroupBy(
            //            r =>
            //                new
            //                {
            //                    r.ProductId,

            //                }).Select(r => new
            //                {
            //                    ProductId = r.Key.ProductId,
            //                    Quantity = r.Sum(a => a.Quantity),
            //                    ToTal = r.Sum(a => a.TotalAmount),
            //                    ToTalAtStore = r.Sum(a => a.QuantityAtStore),
            //                    TotalTakeAway = r.Sum(a => a.QuantityTakeAway),
            //                    TotalDelivery = r.Sum(a => a.QuantityDelivery),
            //                    Discount = r.Sum(a => a.Discount),
            //                    //TotalOrder = r.Sum(a => a.OrderQuantity)
            //                }).ToList();
            //    var totalProduct = 0;
            //    foreach (var itemCat in listCategory)
            //    {

            //        var categoryQuantity = 0;
            //        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
            //        foreach (var itemP in listProduct)
            //        {
            //            categoryQuantity += itemP.DateProducts.Count();
            //            totalProduct += categoryQuantity;
            //            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
            //            fillterList.Remove(productItem);
            //            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
            //            var totalQuantityAtStore = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTalAtStore);
            //            var totalQuantityTakeAway = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.TotalTakeAway);
            //            var totalQuantityDelivery = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.TotalDelivery);
            //            var sum = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
            //            var totalPrice = (int)sum;
            //            var discount = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Discount);
            //            //var totalOrder = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.TotalOrder);

            //            // phân loại category
            //            fillterList.Add(new GroupCategoryReportModal
            //            {
            //                ProductId = itemP.ProductID,
            //                CateName = itemP.ProductCategory.CateName,
            //                ProductName = itemP.ProductName,
            //                Quantity = totalQuantity,
            //                TotalPrice = totalPrice,
            //                QuantityAtStore = (int)totalQuantityAtStore,
            //                QuantityTakeAway = (int)totalQuantityTakeAway,
            //                QuantityDelivery = (int)totalQuantityDelivery,
            //                Discount = discount,
            //                //TotalOrder = (int)totalOrder
            //            });

            //        }
            //        //categories.Add(new Tuple<string, int>(itemCat.CateName, categoryQuantity));
            //    }

            //}
            //var list = fillterList.Select(a => new IConvertible[]
            //{
            //   a.ProductName,
            //   a.CateName,
            //   a.Quantity,
            //   string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.TotalPrice),
            //   a.QuantityAtStore,
            //   a.QuantityTakeAway,
            //   a.QuantityDelivery,
            //   string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.Discount),
            //   //a.TotalOrder
            //}).ToArray();
            ////return PartialView("_LoadRevenueReport", reportList.OrderBy(a => a.StartTime));
            //return Json(new
            //{
            //    sEcho = param.sEcho,
            //    iTotalRecords = list.Count(),
            //    iTotalDisplayRecords = list.Count(),
            //    aaData = list
            //}, JsonRequestBehavior.AllowGet);
            #endregion
        }

        public ActionResult ExportProductCategoryTableToExcel(int brandId, string startTime, string endTime, int storeId, int? startHour, int? endHour)
        {
            #region Get data
            var productCategoryApi = new ProductCategoryApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            List<dynamic> listExcel = new List<dynamic>();
            //var st = new Stopwatch();
            //Create empty list product category
            List<GroupCategoryReportModalViewModel> fillterList = new List<GroupCategoryReportModalViewModel>();
            //            List<TempProductByCategoryViewModel> listProductByCategory = new List<TempProductByCategoryViewModel>();
            List<decimal> listPercentQuantityChart = new List<decimal>();
            List<string> listProductNameChart = new List<string>();

            //Get category in DB
            //st.Start();
            var listCategory = productCategoryApi.GetActiveProductCategoriesByBrandId(brandId).Where(a => a.Type == 1);
            //Debug.WriteLine("Get list category: " + st.ElapsedMilliseconds);

            if (!startHour.HasValue && !endHour.HasValue)
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                //st.Restart();
                var dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId).ToList();
                //Debug.WriteLine("Get product list from otderDetailService: " + st.ElapsedMilliseconds);

                //st.Restart();
                var result =
                    dateProducts.GroupBy(
                        r =>
                            new
                            {
                                r.ProductID
                            }).Select(r => new
                            {
                                ProductId = r.Key.ProductID,
                                Quantity = r.Sum(a => a.Quantity),
                                ToTal = r.Sum(a => a.FinalAmount)
                            }).ToList();
                //Debug.WriteLine("Group by product id: " + st.ElapsedMilliseconds);

                // Total quantity
                //st.Restart();
                var finalTotalQuantity = result.Sum(s => s.Quantity);
                //Debug.WriteLine("Calculate final quantity: " + st.ElapsedMilliseconds);

                if (finalTotalQuantity > 0)
                {
                    //st.Restart();
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.Active);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = listExcel.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            listExcel.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            var percentQuantity = totalQuantity;
                            //var percentQuantity = (Convert.ToDecimal(totalQuantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            listExcel.Add(new GroupCategoryReportModalViewModel
                            {
                                ProductId = itemP.ProductID,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }

                    //Debug.WriteLine("Filter list (no hour range): " + st.ElapsedMilliseconds);
                }
                else
                {
                    //st.Restart();
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.Active);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = listExcel.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            listExcel.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            listExcel.Add(new GroupCategoryReportModalViewModel
                            {
                                ProductId = itemP.ProductID,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }

                    //Debug.WriteLine("Filter list (no hour range): " + st.ElapsedMilliseconds);
                }
            }
            else
            {
                #region Cách mới
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                //st.Restart();
                var dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId).ToList();
                //Debug.WriteLine("Get product list from otderDetailService: " + st.ElapsedMilliseconds);

                //st.Restart();
                var result =
                    dateProducts.GroupBy(
                        r =>
                            new
                            {
                                r.ProductID
                            }).Select(r => new
                            {
                                ProductId = r.Key.ProductID,
                                Quantity = r.Sum(a => a.Quantity),
                                ToTal = r.Sum(a => a.FinalAmount)
                            }).ToList();
                //Debug.WriteLine("Group by product id: " + st.ElapsedMilliseconds);

                // Total quantity
                //st.Restart();
                var finalTotalQuantity = result.Sum(s => s.Quantity);
                //Debug.WriteLine("Calculate final quantity: " + st.ElapsedMilliseconds);

                if (finalTotalQuantity > 0)
                {
                    //st.Restart();
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.Active);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = listExcel.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            listExcel.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            var percentQuantity = totalQuantity;
                            //var percentQuantity = (Convert.ToDecimal(totalQuantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            listExcel.Add(new GroupCategoryReportModalViewModel
                            {
                                ProductId = itemP.ProductID,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }

                    //Debug.WriteLine("Filter list (no hour range): " + st.ElapsedMilliseconds);
                }
                else
                {
                    //st.Restart();
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.Active);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = listExcel.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            listExcel.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            listExcel.Add(new GroupCategoryReportModalViewModel
                            {
                                ProductId = itemP.ProductID,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }
                }
                #endregion

                #region Cách cũ
                //var startDate = startTime.ToDateTime();
                //var endDate = endTime.ToDateTime();


                //for (DateTime i = startDate; startDate <= endDate; startDate = startDate.AddDays(1))
                //{
                //    //st.Restart();
                //    var sTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startHour.Value, 0, 0);
                //    var eTime = new DateTime();
                //    if (endHour == 24)
                //    {
                //        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
                //    }
                //    else
                //    {
                //        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, endHour.Value, 0, 0);
                //    }

                //    var dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(sTime, eTime, storeId)
                //        .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
                //    var result =
                //    dateProducts.GroupBy(
                //        r =>
                //            new
                //            {
                //                r.ProductID
                //            }).Select(r => new
                //            {
                //                ProductId = r.Key.ProductID,
                //                Quantity = r.Sum(a => a.Quantity),
                //                ToTal = r.Sum(a => a.FinalAmount)
                //            }).ToList();

                //    //Debug.WriteLine("Get list from orderDetailServie (hour range): " + st.ElapsedMilliseconds);

                //    //st.Restart();
                //    foreach (var itemCat in listCategory)
                //    {
                //        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true).ToList();

                //        foreach (var itemP in listProduct)
                //        {
                //            //var productItem = listExcel.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                //            //listExcel.Remove(productItem);

                //            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                //            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                //            //var totalPrice = (int)sum;

                //            var productItem = listExcel.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                //            if (productItem != null)
                //            {
                //                productItem.TotalPrice = productItem.TotalPrice + totalPrice;
                //                productItem.Quantity = productItem.Quantity + totalQuantity;
                //            }
                //            else
                //            {
                //                listExcel.Add(new GroupCategoryReportModalViewModel
                //                {
                //                    ProductId = itemP.ProductID,
                //                    CateName = itemP.ProductCategory.CateName,
                //                    ProductName = itemP.ProductName,
                //                    Quantity = totalQuantity,
                //                    TotalPrice = totalPrice,
                //                });
                //            }
                //        }
                //    }

                //    //Debug.WriteLine("Filter list (hour range): " + st.ElapsedMilliseconds);
                //}

                ////st.Restart();
                //var finalTotalQuantity = listExcel.Sum(s => s.Quantity);
                //if (finalTotalQuantity > 0)
                //{
                //    var listProductName = listExcel.Select(s => s.ProductName).ToList();
                //    var listQuantity = listExcel.Select(s => s.Quantity).ToList();

                //    for (int i = 0; i < listProductName.Count(); i++)
                //    {
                //        var productName = listProductName[i];
                //        var quantity = listQuantity[i];
                //        //var percentQuantity = (Convert.ToDecimal(quantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                //        var percentQuantity = quantity;
                //        if (percentQuantity > 0)
                //        {
                //            listProductNameChart.Add(productName);
                //            listPercentQuantityChart.Add(percentQuantity);
                //        }
                //    }
                //}
                #endregion
                //Debug.WriteLine("Data for chart (hour range): " + st.ElapsedMilliseconds);
            }
            #endregion

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "No";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngành hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng tiền";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells      
                int count = 0;
                foreach (var data in listExcel)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ProductName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CateName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalPrice);
                    StartHeaderChar = 'A';
                }
                var storeName = storeApi.GetStoreNameByID(storeId);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BáoCáoTheoSảnPhẩm_CửaHàng_" + storeName + dateRange + ".xlsx";
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
                return this.File(ms, contentType, fileName);
            }
            #endregion

        }
        #endregion

        #region Hour report

        public ActionResult StoreHourReport()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        public JsonResult LoadHourReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {

            var hourReport = new List<HourReportModel>();
            for (int i = 6; i < 23; i++)
            {
                hourReport.Add(new HourReportModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)

                });
            }
            //var isAdmin = Roles.GetRolesForUser().Contains("Administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");

            if ((startTime == "" && endTime == "") || (startTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy") && endTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy")))
            {
                var dateToGet = Utils.GetCurrentDateTime();
                //if (!isAdmin)
                //{
                //    dateToGet = Utils.GetCurrentDateTime().AddDays(-1);
                //}
                var startDate = dateToGet.GetStartOfDate();
                var endDate = dateToGet.GetEndOfDate();

                IEnumerable<Order> rents;
                var orderAPI = new OrderApi();
                if (storeId > 0)
                {

                    rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                //var totalOrder = rents.Count();
                //var totalAmount = rents.Sum(a=> a.FinalAmount);
                //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;

                }
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            datatable = 0,
                //            datachart = new
                //            {
                //                Time = 0,
                //                TakeAway = 0,
                //                AtStore = 0,
                //                Delivery = 0
                //            }
                //        }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
                TimeSpan spanTime = endDate - startDate;

                IEnumerable<Order> rents;
                var orderAPI = new OrderApi();
                if (storeId > 0)
                {
                    rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                }
            }
            int count = 1;
            var list = hourReport.Select(a => new IConvertible[]
            {
                count++,
                a.StartTime + ":00 - " + a.EndTime + ":00",
                a.TakeAway,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceTakeAway),
                a.AtStore,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceAtStore),
                a.Delivery,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceDelivery),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice)

            }).ToList();
            var _Time = hourReport.Select(a => a.StartTime + ":00 - " + a.EndTime + ":00").ToList();
            var _takeAway = hourReport.Select(a => a.TakeAway).ToList();
            var _atStore = hourReport.Select(a => a.AtStore).ToList();
            var _delivery = hourReport.Select(a => a.Delivery).ToList();
            //return PartialView("_LoadRevenueReport", reportList.OrderBy(a => a.StartTime));
            return Json(new
            {
                datatable = list,
                datachart = new
                {
                    Time = _Time,
                    TakeAway = _takeAway,
                    AtStore = _atStore,
                    Delivery = _delivery
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportHourTableToExcel(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            List<dynamic> listExcel = new List<dynamic>();
            var storeApi = new StoreApi();
            #region Get data
            var hourReport = new List<HourReportModel>();
            for (int i = 6; i < 23; i++)
            {
                listExcel.Add(new HourReportModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)

                });
            }

            if ((startTime == "" && endTime == "") || (startTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy") && endTime == Utils.GetCurrentDateTime().ToString("dd/MM/yyyy")))
            {
                var dateNow = Utils.GetCurrentDateTime();
                //var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                //var endDate = dateNow.AddDays(-1).Date.GetEndOfDate();
                //var startDate = dateNow.AddDays(-30).Date.GetStartOfDate();
                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();
                IEnumerable<Order> rents;
                var orderAPI = new OrderApi();
                if (storeId > 0)
                {
                    rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                //var totalOrder = rents.Count();
                //var totalAmount = rents.Sum(a=> a.FinalAmount);
                //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in listExcel)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;

                }

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                TimeSpan spanTime = endDate - startDate;
                IEnumerable<Order> rents;
                var orderAPI = new OrderApi();
                if (storeId > 0)
                {
                    rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in listExcel)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;


                }
            }
            var list = hourReport.Select(a => new
            {
                a = a.StartTime + ":00 - " + a.EndTime + ":00",
                b = a.TakeAway,
                c = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceTakeAway),
                d = a.AtStore,
                e = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceAtStore),
                f = a.Delivery,
                g = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceDelivery),
                h = a.TotalQuantity,
                i = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice)

            }).ToList();
            #endregion

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khoảng thời gian";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lương (Mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lương (Tại quán)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng (Giao hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng cộng";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Thành tiền";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in listExcel)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.StartTime + ":00 - " + data.EndTime + ":00";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.PriceTakeAway);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.AtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.PriceAtStore);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Delivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.PriceDelivery);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalQuantity;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalPrice);
                    StartHeaderChar = 'A';
                }
                var storeName = storeApi.GetStoreNameByID(storeId);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BáoCáoTheoGiờ_" + storeName + dateRange + ".xlsx";
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
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }
        #endregion

        #region DayOfWeek Report
        public ActionResult DayOfWeekReport()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        public JsonResult LoadDayOfWeekReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            var orderApi = new OrderApi();
            var dayOfWeekReport = new List<DayOfWeekReportViewModel>();
            for (int i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Monday,
                        DayOfWeek = "Thứ hai"
                    });
                }
                else if (i == 1)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Tuesday,
                        DayOfWeek = "Thứ ba"
                    });
                }
                else if (i == 2)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Wednesday,
                        DayOfWeek = "Thứ tư"
                    });
                }
                else if (i == 3)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Thursday,
                        DayOfWeek = "Thứ năm"
                    });
                }
                else if (i == 4)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Friday,
                        DayOfWeek = "Thứ sáu"
                    });
                }
                else if (i == 5)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Saturday,
                        DayOfWeek = "Thứ bảy"
                    });
                }
                else if (i == 6)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Sunday,
                        DayOfWeek = "Chủ nhật"
                    });
                }
            }
            //var isAdmin = Roles.IsUserInRole("Administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");
            if (startTime == "" && endTime == "")
            {
                var dateToGet = Utils.GetCurrentDateTime();
                //if (!isAdmin)
                //{
                //    dateToGet = Utils.GetCurrentDateTime().AddDays(-1);
                //}
                var startDate = dateToGet.AddDays(1 - (int)dateToGet.DayOfWeek).GetStartOfDate();
                var endDate = dateToGet.GetEndOfDate();

                IEnumerable<Order> rents;

                if (storeId > 0)
                {
                    rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayOfWeekReport)
                {
                    var takeAway = result.Where(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.TotalOrder);
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.Money);

                    var atStore = result.Where(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.AtStore = (atStore == null) ? 0 : atStore.Sum(a => a.TotalOrder);
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Sum(a => a.Money);

                    var delivery = result.Where(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.Delivery = (delivery == null) ? 0 : delivery.Sum(a => a.TotalOrder);
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Sum(a => a.Money);

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                }
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            datatable = 0,
                //            dataChart = new
                //            {
                //                WeekDay = 0,
                //                TakeAway = 0,
                //                AtStore = 0,
                //                Delivery = 0
                //            }
                //        }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
                TimeSpan spanTime = endDate - startDate;

                IEnumerable<Order> rents;

                if (storeId > 0)
                {
                    rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayOfWeekReport)
                {
                    var takeAway = result.Where(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.TotalOrder);
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.Money);

                    var atStore = result.Where(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.AtStore = (atStore == null) ? 0 : atStore.Sum(a => a.TotalOrder);
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Sum(a => a.Money);

                    var delivery = result.Where(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.Delivery = (delivery == null) ? 0 : delivery.Sum(a => a.TotalOrder);
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Sum(a => a.Money);

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                }
            }

            var list = dayOfWeekReport.Select(a => new IConvertible[]
            {
                a.DayOfWeek,
                a.TakeAway,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceTakeAway),
                a.AtStore,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceAtStore),
                a.Delivery,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceDelivery),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice)

            }).ToList();

            var _WeekDay = dayOfWeekReport.Select(a => a.DayOfWeek).ToList();
            var _TakeAway = dayOfWeekReport.Select(a => a.TakeAway).ToList();
            var _AtStore = dayOfWeekReport.Select(a => a.AtStore).ToList();
            var _Delivery = dayOfWeekReport.Select(a => a.Delivery).ToList();

            return Json(new
            {
                datatable = list,
                dataChart = new
                {
                    WeekDay = _WeekDay,
                    TakeAway = _TakeAway,
                    AtStore = _AtStore,
                    Delivery = _Delivery
                }
            }, JsonRequestBehavior.AllowGet);
        }
        public List<dynamic> ExportDayOfWeekTableToExcel(int brandId, string startTime, string endTime, int storeId)
        {
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();

            #region Get data
            var dayOfWeekReport = new List<DayOfWeekReportViewModel>();
            for (int i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Monday,
                        DayOfWeek = "Thứ hai"
                    });
                }
                else if (i == 1)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Tuesday,
                        DayOfWeek = "Thứ ba"
                    });
                }
                else if (i == 2)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Wednesday,
                        DayOfWeek = "Thứ tư"
                    });
                }
                else if (i == 3)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Thursday,
                        DayOfWeek = "Thứ năm"
                    });
                }
                else if (i == 4)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Friday,
                        DayOfWeek = "Thứ sáu"
                    });
                }
                else if (i == 5)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Saturday,
                        DayOfWeek = "Thứ bảy"
                    });
                }
                else if (i == 6)
                {
                    dayOfWeekReport.Add(new DayOfWeekReportViewModel()
                    {
                        Day = DayOfWeek.Sunday,
                        DayOfWeek = "Chủ nhật"
                    });
                }
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();

                var startDate = dateNow.AddDays(1 - (int)dateNow.DayOfWeek).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                IEnumerable<Order> rents;

                if (storeId > 0)
                {
                    rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                //var totalOrder = rents.Count();
                //var totalAmount = rents.Sum(a=> a.FinalAmount);
                //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayOfWeekReport)
                {
                    var takeAway = result.Where(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.TotalOrder);
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.Money);

                    var atStore = result.Where(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.AtStore = (atStore == null) ? 0 : atStore.Sum(a => a.TotalOrder);
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Sum(a => a.Money);

                    var delivery = result.Where(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.Delivery = (delivery == null) ? 0 : delivery.Sum(a => a.TotalOrder);
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Sum(a => a.Money);

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                TimeSpan spanTime = endDate - startDate;

                IEnumerable<Order> rents;

                if (storeId > 0)
                {
                    rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayOfWeekReport)
                {
                    var takeAway = result.Where(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.TotalOrder);
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.Money);

                    var atStore = result.Where(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.AtStore = (atStore == null) ? 0 : atStore.Sum(a => a.TotalOrder);
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Sum(a => a.Money);

                    var delivery = result.Where(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime.Value.DayOfWeek == item.Day);
                    item.Delivery = (delivery == null) ? 0 : delivery.Sum(a => a.TotalOrder);
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Sum(a => a.Money);

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                }
            }
            List<dynamic> list = new List<dynamic>();
            foreach (var item in dayOfWeekReport)
            {
                list.Add(new
                {
                    a = item.DayOfWeek,
                    b = item.TakeAway,
                    c = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", item.PriceTakeAway),
                    d = item.AtStore,
                    e = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", item.PriceAtStore),
                    f = item.Delivery,
                    g = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", item.PriceDelivery),
                    h = item.TotalQuantity,
                    i = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", item.TotalPrice)
                });
            }
            //var list = dayOfWeekReport.Select(a => new
            //{
            //    a = a.DayOfWeek,
            //    b = a.TakeAway,
            //    c = string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.PriceTakeAway),
            //    d = a.AtStore,
            //    e = string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.PriceAtStore),
            //    f = a.Delivery,
            //    g = string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.PriceDelivery),
            //    h = a.TotalQuantity,
            //    i = string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.TotalPrice)

            //}).ToList();
            #endregion

            #region Export to Excel
            //List<string> header = new List<string>();
            //header.Add("#;1;1");
            //header.Add("Thứ;1;1");
            //header.Add("Số lượng(Mang đi);1;1");
            //header.Add("Thành tiền;1;1");
            //header.Add("Số lượng(Tại store);1;1");
            //header.Add("Thành tiền;1;1");
            //header.Add("Số lượng(Giao hàng);1;1");
            //header.Add("Thành tiền;1;1");
            //header.Add("Tổng cộng;1;1");
            //header.Add("Thành tiền;1;1");

            //var storeName = storeApi.GetStoreNameByID(storeId);
            //var sDate = startTime.Replace("/", "-");
            //var eDate = endTime.Replace("/", "-");
            //var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
            //string fileName = "BáoCáoTheoThứ_" + storeName + dateRange;

            //bool success = false;
            //Thread thdSyncRead = new Thread(new ThreadStart(() => exportExcel(header, list, ref fileName, ref success)));
            //thdSyncRead.SetApartmentState(ApartmentState.STA);
            //thdSyncRead.Start();
            //thdSyncRead.Join(120000);
            //if (!success)
            //{
            //    thdSyncRead.Abort();
            //}
            #endregion
            return list;
        }
        public ActionResult ReportDayOfWeekExportExcelEPPlus(int brandId, string startTime, string endTime, int storeId)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo theo thứ");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = ExportDayOfWeekTableToExcel(brandId, startTime, endTime, storeId);
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thứ";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Tại store)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Giao hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng cộng";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Thành tiền";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in listDT)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.a;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.b;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.c;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.d;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.e;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.f;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.g;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.h;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.i;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo theo thứ từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion

        #region Month Report
        public ActionResult MonthReport()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        public JsonResult LoadMonthReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            var dateReportApi = new DateReportApi();
            var monthReport = new List<MonthReportViewModel>();
            for (int i = 1; i < 13; i++)
            {
                monthReport.Add(new MonthReportViewModel()
                {
                    Month = i,
                    MonthName = "Tháng " + i
                });
            }

            // -- duynnm --
            var dateNow = Utils.GetCurrentDateTime();
            //var isAdmin = Roles.IsUserInRole("administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");
            //if (!isAdmin)
            //{
            //    dateNow = dateNow.AddDays(-1);
            //}
            var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
            var endDate = dateNow.GetEndOfDate();

            if (startTime != "" || endTime != "")
            {

                startDate = startTime.ToDateTime();
                endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            datatable = 0,
                //            dataChart = new
                //            {
                //                MonthName = 0,
                //                TakeAway = 0,
                //                AtStore = 0,
                //                Delivery = 0
                //            }
                //        }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
            }

            IEnumerable<DateReport> dateReport;

            if (storeId > 0)
            {
                dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, storeId);
            }
            else
            {
                dateReport = dateReportApi.GetDateReportTimeRange(startDate, endDate);
            }

            //var totalOrder = rents.Count();
            //var totalAmount = rents.Sum(a=> a.FinalAmount);
            //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

            var result = dateReport.GroupBy(r => new { Time = r.Date.Month }).Select(r => new
            {
                OrderTime = r.Key.Time,
                //-- CuongHH--
                TotalOrder = r.Sum(a => a.TotalOrder),
                TotalFinalAmount = r.Sum(a => a.FinalAmount),
                TotalDiscount = r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail),
                TotalTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                TotalDelivery = r.Sum(a => a.TotalOrderDelivery),
                TotalAtStore = r.Sum(a => a.TotalOrderAtStore)
            }).ToList();

            foreach (var item in monthReport)
            {
                //-- CuongHH--
                var resultMonth = result.Where(a => a.OrderTime == item.Month);
                item.TotalOrder = resultMonth.Sum(a => a.TotalOrder);
                item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount);
                item.TotalFinalAmount = (double)resultMonth.Sum(a => a.TotalFinalAmount);
                item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway);
                item.Delivery = resultMonth.Sum(a => a.TotalDelivery);
                item.AtStore = resultMonth.Sum(a => a.TotalAtStore);
            }

            var list = monthReport.Select(a => new IConvertible[]
            {
                a.MonthName,
                a.TakeAway,
                a.AtStore,
                a.Delivery,
                a.TotalOrder,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalDiscount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalFinalAmount)
            }).ToArray();

            var _MonthName = monthReport.Select(a => a.MonthName).ToArray();
            var _TakeAway = monthReport.Select(a => a.TakeAway).ToArray();
            var _AtStore = monthReport.Select(a => a.AtStore).ToArray();
            var _Delivery = monthReport.Select(a => a.Delivery).ToArray();

            return Json(new
            {
                datatable = list,
                dataChart = new
                {
                    MonthName = _MonthName,
                    TakeAway = _TakeAway,
                    AtStore = _AtStore,
                    Delivery = _Delivery
                }
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportMonthTableToExcel(int brandId, string startTime, string endTime, int storeId)
        {
            var storeApi = new StoreApi();
            var dateReportApi = new DateReportApi();
            List<dynamic> listExcel = new List<dynamic>();
            #region Get data
            var monthReport = new List<MonthReportViewModel>();
            for (int i = 1; i < 13; i++)
            {
                listExcel.Add(new MonthReportViewModel()
                {
                    Month = i,
                    MonthName = "Tháng " + i
                });
            }

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            // -- duynnm --
            IEnumerable<DateReport> dateReport;

            if (storeId > 0)
            {
                // dateReport = _dateReportService.GetDateReportByStoreId(storeId).Where(a=> a.Date >=startDate);
                dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, storeId);
            }
            else
            {
                //dateReport = _dateReportService.GetDateReport();
                dateReport = dateReportApi.GetDateReportTimeRange(startDate, endDate);
            }


            //var totalOrder = rents.Count();
            //var totalAmount = rents.Sum(a=> a.FinalAmount);
            //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

            var result = dateReport.GroupBy(r => new { Time = r.Date.Month }).Select(r => new
            {
                OrderTime = r.Key.Time,
                //-- CuongHH--
                TotalOrder = r.Sum(a => a.TotalOrder),
                TotalFinalAmount = r.Sum(a => a.FinalAmount),
                TotalDiscount = r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail),
                TotalTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                TotalDelivery = r.Sum(a => a.TotalOrderDelivery),
                TotalAtStore = r.Sum(a => a.TotalOrderAtStore)
            }).ToList();

            foreach (var item in listExcel)
            {
                //-- CuongHH--
                var resultMonth = result.Where(a => a.OrderTime == item.Month);
                item.TotalOrder = resultMonth.Sum(a => a.TotalOrder);
                item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount);
                item.TotalFinalAmount = (double)resultMonth.Sum(a => a.TotalFinalAmount);
                item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway);
                item.Delivery = resultMonth.Sum(a => a.TotalDelivery);
                item.AtStore = resultMonth.Sum(a => a.TotalAtStore);
            }

            var list = monthReport.Select(a => new
            {
                a = a.MonthName,
                b = a.TakeAway,
                c = a.AtStore,
                d = a.Delivery,
                e = a.TotalOrder,
                f = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalDiscount),
                g = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalFinalAmount)
            });
            #endregion

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tháng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng hóa đơn(Mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng hóa đơn(Tại store)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng hóa đơn(Giao hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng số hóa đơn";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Doanh thu";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in listExcel)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.MonthName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.AtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Delivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalDiscount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalFinalAmount);
                    StartHeaderChar = 'A';
                }
                var storeName = storeApi.GetStoreNameByID(storeId);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BáoCáoTheoTháng_" + storeName + dateRange + ".xlsx";
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
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }

        #endregion

        #region Shift Report

        public ActionResult StoreShiftReport()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        public JsonResult LoadShiftReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            var shiftModel = new List<ShiftReportViewModel>();
            var hasChart = false;
            List<string> listDate = new List<string>();
            List<double> listTotalMoneyShift1 = new List<double>();
            List<double> listTotalMoneyShift2 = new List<double>();

            var orderApi = new OrderApi();
            //var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            //var isAdmin = Roles.IsUserInRole("Administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");
            if (startTime == "" && endTime == "")
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                //if (!isAdmin)
                //{
                //    startDate = startDate.AddDays(-1);
                //    endDate = endDate.AddDays(-1);
                //}
                // -- duynnm --
                IEnumerable<Order> rents;
                if (storeId > 0)
                {
                    rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                          .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                //var result = rents.GroupBy(r => new { person = r.CheckInPerson }).Select(r => new
                //{
                //    Person = r.Key.person,
                //    //OrderType = r.Key.OrderType,
                //    //OrderTime = r.Key.Time,
                //    TotalOrder = r.Count(),
                //    Money = r.Sum(a => a.FinalAmount)
                //}).ToList();

                //var person = rents.GroupBy(a => new { cashier = a.CheckInPerson }).Select(a => new
                //{
                //    Cashier = a.Key.cashier,
                //    Total = a.Count()
                //}).ToList();

                //double totalOrderShift1 = 0;
                //double totalPriceShift1 = 0;

                //double totalOrderShift2 = 0;
                //double totalPriceShift2 = 0;

                //string shift1 = null;
                //string shift2 = null;

                //int i = 1;

                //foreach (var x in person)
                //{
                //    if (i <= result.Select(a => a.Person).Count())
                //    {
                //        if (i % 2 != 0) // person number is an odd
                //        {
                //            totalOrderShift1 = totalOrderShift1 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.TotalOrder);
                //            totalPriceShift1 = totalPriceShift1 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.Money);
                //        }
                //        else if (i % 2 == 0) // person number is an even
                //        {
                //            totalOrderShift2 = totalOrderShift2 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.TotalOrder);
                //            totalPriceShift2 = totalPriceShift2 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.Money);
                //        }


                //    }



                //    i++;
                //}

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount)
                });

                var shift1 = result.Where(a => a.OrderTime >= 6 && a.OrderTime <= 13);
                var totalOrderShift1 = shift1.Sum(a => a.TotalOrder);
                var totalPriceShift1 = shift1.Sum(a => a.Money);

                var shift2 = result.Where(a => a.OrderTime > 13 && a.OrderTime <= 23);
                var totalOrderShift2 = shift2.Sum(a => a.TotalOrder);
                var totalPriceShift2 = shift2.Sum(a => a.Money);
                shiftModel.Add(new ShiftReportViewModel()
                {
                    StartTime = startDate.ToString("dd/MM/yyyy"),
                    TotalOrderShift1 = totalOrderShift1,
                    TotalPriceShift1 = totalPriceShift1,
                    AverageShift1 = totalOrderShift1 != 0 ? totalPriceShift1 / totalOrderShift1 : 0,

                    TotalOrderShift2 = totalOrderShift2,
                    TotalPriceShift2 = totalPriceShift2,
                    AverageShift2 = totalOrderShift2 != 0 ? totalPriceShift2 / totalOrderShift2 : 0
                });
                startDate = startDate.AddDays(1);
                //ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                //ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            dataTable = 0,
                //            dataChart = new
                //            {
                //                listDate = 0,
                //                listTotalMoneyShift1 = 0,
                //                listTotalMoneyShift2 = 0
                //            }
                //        }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
                hasChart = true;

                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {

                    // -- duynnm --
                    IEnumerable<Order> rents;
                    if (storeId > 0)
                    {
                        rents = orderApi.GetOrdersByTimeRange(storeId, startDate.GetStartOfDate(), startDate.GetEndOfDate(), brandId)
                              .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    }
                    else
                    {
                        rents = orderApi.GetAllOrdersByDate(startDate.GetStartOfDate(), startDate.GetEndOfDate(), brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    }
                    //var result = rents.GroupBy(r => new { person = r.CheckInPerson }).Select(r => new
                    //{
                    //    Person = r.Key.person,
                    //    //OrderType = r.Key.OrderType,
                    //    //OrderTime = r.Key.Time,
                    //    TotalOrder = r.Count(),
                    //    Money = r.Sum(a => a.FinalAmount)
                    //}).ToList();

                    //var person = rents.GroupBy(a => new { cashier = a.CheckInPerson }).Select(a => new
                    //{
                    //    Cashier = a.Key.cashier,
                    //    Total = a.Count()
                    //}).ToList();

                    //double totalOrderShift1 = 0;
                    //double totalPriceShift1 = 0;

                    //double totalOrderShift2 = 0;
                    //double totalPriceShift2 = 0;

                    //int i = 1;

                    //foreach (var x in person)
                    //{
                    //    if (i <= result.Select(a => a.Person).Count())
                    //    {
                    //        if (i % 2 != 0) // person number is an odd
                    //        {
                    //            totalOrderShift1 = totalOrderShift1 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.TotalOrder);
                    //            totalPriceShift1 = totalPriceShift1 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.Money);
                    //        }
                    //        else if (i % 2 == 0) // person number is an even
                    //        {
                    //            totalOrderShift2 = totalOrderShift2 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.TotalOrder);
                    //            totalPriceShift2 = totalPriceShift2 + result.Where(a => a.Person == x.Cashier.ToString()).Sum(a => a.Money);
                    //        }
                    //    }
                    //    i++;
                    //}
                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount)
                    });

                    var shift1 = result.Where(a => a.OrderTime >= 6 && a.OrderTime <= 13);
                    var totalOrderShift1 = shift1.Sum(a => a.TotalOrder);
                    var totalPriceShift1 = shift1.Sum(a => a.Money);

                    var shift2 = result.Where(a => a.OrderTime > 13 && a.OrderTime <= 23);
                    var totalOrderShift2 = shift2.Sum(a => a.TotalOrder);
                    var totalPriceShift2 = shift2.Sum(a => a.Money);
                    shiftModel.Add(new ShiftReportViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        TotalOrderShift1 = totalOrderShift1,
                        TotalPriceShift1 = totalPriceShift1,
                        AverageShift1 = totalOrderShift1 != 0 ? totalPriceShift1 / totalOrderShift1 : 0,

                        TotalOrderShift2 = totalOrderShift2,
                        TotalPriceShift2 = totalPriceShift2,
                        AverageShift2 = totalOrderShift2 != 0 ? totalPriceShift2 / totalOrderShift2 : 0
                    });
                    startDate = startDate.AddDays(1);
                    //var shift1 = result.Where(a => a.OrderTime >= 6 && a.OrderTime <= 13).ToList();
                    //var totalOrderShift1 = shift1.Sum(a => a.TotalOrder);
                    //var totalPriceShift1 = shift1.Sum(a => a.Money);

                    //var shift2 = result.Where(a => a.OrderTime > 13 && a.OrderTime <= 23).ToList();
                    //var totalOrderShift2 = shift2.Sum(a => a.TotalOrder);
                    //var totalPriceShift2 = shift2.Sum(a => a.Money);
                }

                //ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                //ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }

            foreach (var item in shiftModel)
            {
                listDate.Add(item.StartTime);
                listTotalMoneyShift1.Add(item.TotalPriceShift1);
                listTotalMoneyShift2.Add(item.TotalPriceShift2);
            }

            //Prepare list for datatable
            var list = shiftModel.Select(a => new IConvertible[]
            {
                a.StartTime,
                a.TotalOrderShift1,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPriceShift1),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.AverageShift1),
                a.TotalOrderShift2,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPriceShift2),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.AverageShift2),

            }).ToArray();
            //return PartialView("_LoadRevenueReport", reportList.OrderBy(a => a.StartTime));
            if (hasChart)
            {
                return Json(new
                {
                    dataTable = list,
                    dataChart = new
                    {
                        listDate = listDate,
                        listTotalMoneyShift1 = listTotalMoneyShift1,
                        listTotalMoneyShift2 = listTotalMoneyShift2
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    dataTable = list,

                }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<dynamic> ExportShiftReportToExcel(int brandId, string startTime, string endTime, int storeId)
        {
            var shiftModel = new List<ShiftReportViewModel>();
            var hasChart = false;
            List<string> listDate = new List<string>();
            List<double> listTotalMoneyShift1 = new List<double>();
            List<double> listTotalMoneyShift2 = new List<double>();
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();


            //Set today string
            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");

            if (startTime == today && endTime == today)
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                // -- duynnm --
                IEnumerable<Order> rents;
                if (storeId > 0)
                {
                    rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                          .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount)
                });

                var shift1 = result.Where(a => a.OrderTime >= 6 && a.OrderTime <= 13);
                var totalOrderShift1 = shift1.Sum(a => a.TotalOrder);
                var totalPriceShift1 = shift1.Sum(a => a.Money);

                var shift2 = result.Where(a => a.OrderTime > 13 && a.OrderTime <= 23);
                var totalOrderShift2 = shift2.Sum(a => a.TotalOrder);
                var totalPriceShift2 = shift2.Sum(a => a.Money);

                shiftModel.Add(new ShiftReportViewModel()
                {
                    StartTime = startTime,
                    TotalOrderShift1 = totalOrderShift1,
                    TotalPriceShift1 = totalPriceShift1,
                    AverageShift1 = totalOrderShift1 != 0 ? totalPriceShift1 / totalOrderShift1 : 0,

                    TotalOrderShift2 = totalOrderShift2,
                    TotalPriceShift2 = totalPriceShift2,
                    AverageShift2 = totalOrderShift2 != 0 ? totalPriceShift2 / totalOrderShift2 : 0
                });
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                hasChart = true;

                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {

                    // -- duynnm --
                    IEnumerable<Order> rents;
                    if (storeId > 0)
                    {
                        rents = orderApi.GetOrdersByTimeRange(storeId, startDate.GetStartOfDate(), startDate.GetEndOfDate(), brandId)
                              .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    }
                    else
                    {
                        rents = orderApi.GetAllOrdersByDate(startDate.GetStartOfDate(), startDate.GetEndOfDate(), brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    }

                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount)
                    }).ToList();

                    var shift1 = result.Where(a => a.OrderTime >= 6 && a.OrderTime <= 13);
                    var totalOrderShift1 = shift1.Sum(a => a.TotalOrder);
                    var totalPriceShift1 = shift1.Sum(a => a.Money);

                    var shift2 = result.Where(a => a.OrderTime > 13 && a.OrderTime <= 23);
                    var totalOrderShift2 = shift2.Sum(a => a.TotalOrder);
                    var totalPriceShift2 = shift2.Sum(a => a.Money);

                    shiftModel.Add(new ShiftReportViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        TotalOrderShift1 = totalOrderShift1,
                        TotalPriceShift1 = totalPriceShift1,
                        AverageShift1 = totalOrderShift1 != 0 ? totalPriceShift1 / totalOrderShift1 : 0,

                        TotalOrderShift2 = totalOrderShift2,
                        TotalPriceShift2 = totalPriceShift2,
                        AverageShift2 = totalOrderShift2 != 0 ? totalPriceShift2 / totalOrderShift2 : 0
                    });
                    startDate = startDate.AddDays(1);

                }
            }

            foreach (var item in shiftModel)
            {
                listDate.Add(item.StartTime);
                listTotalMoneyShift1.Add(item.TotalPriceShift1);
                listTotalMoneyShift2.Add(item.TotalPriceShift2);
            }
            List<dynamic> list = new List<dynamic>();
            //Prepare list for datatable
            foreach (var a in shiftModel)
            {
                list.Add(new
                {
                    a = a.StartTime,
                    b = a.TotalOrderShift1,
                    c = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPriceShift1),
                    d = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.AverageShift1),
                    e = a.TotalOrderShift2,
                    f = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPriceShift2),
                    g = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.AverageShift2),
                });
            }

            #region xuất excel cũ
            //List<string> header = new List<string>();
            //header.Add("STT;1;2;r");
            //header.Add("Ngày;1;2;r");

            //header.Add("Ca1;1;3;c");
            //header.Add("Bill;2;1");
            //header.Add("Tổng tiền;2;1");
            //header.Add("TB Bill;2;1");

            //header.Add("Ca2;1;3;c");
            //header.Add("Bill;2;1");
            //header.Add("Tổng tiền;2;1");
            //header.Add("TB Bill;2;1");

            //var sTime = startTime.ToDateTime().ToString("dd-MM-yyyy");
            //var eTime = endTime.ToDateTime().ToString("dd-MM-yyyy");
            //var dateRange = "(" + sTime + (sTime == eTime ? "" : " - " + eTime) + ")";
            //var storeName = storeApi.GetStoreNameByID(storeId);
            //string fileName = "BáoCáoTheoCa_HệThống_" + storeName + dateRange;

            //bool success = false;
            //Thread thdSyncRead = new Thread(new ThreadStart(() => exportExcel(header, list, ref fileName, ref success)));
            //thdSyncRead.SetApartmentState(ApartmentState.STA);
            //thdSyncRead.Start();
            //thdSyncRead.Join(120000);
            //if (!success)
            //{
            //    thdSyncRead.Abort();
            //}
            #endregion
            return list;

        }

        public ActionResult ReportShiftExportExcelEPPlus(int brandId, string startTime, string endTime, int storeId)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo theo ca");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = ExportShiftReportToExcel(brandId, startTime, endTime, storeId);
                #region MergeHeaders
                ws.Cells["A1:B2"].Merge = true;
                ws.Cells["C1:E1"].Merge = true;
                ws.Cells["F1:H1"].Merge = true;
                #endregion
                #region Headers                
                ws.Cells["A1:B2"].Value = "Ngày";
                ws.Cells["C1:E1"].Value = "Ca1";
                ws.Cells["C2"].Value = "Tổng số Bill";
                ws.Cells["D2"].Value = "Tổng tiền";
                ws.Cells["E2"].Value = "TB Bill";
                ws.Cells["F1:H1"].Value = "Ca2";
                ws.Cells["F2"].Value = "Tổng số Bill";
                ws.Cells["G2"].Value = "Tổng tiền";
                ws.Cells["H2"].Value = "TB Bill";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns  
                //              
                ws.Cells["A1:B2"].Style.Font.Bold = true;
                ws.Cells["A1:B2"].AutoFitColumns();
                ws.Cells["A1:B2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["A1:B2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.Cells["A1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                //
                ws.Cells["C1:H1"].Style.Font.Bold = true;
                ws.Cells["C1:H1"].AutoFitColumns();
                ws.Cells["C1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["C1:H1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.Cells["C1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                //
                ws.Cells["C2:H2"].Style.Font.Bold = true;
                ws.Cells["C2:H2"].AutoFitColumns();
                ws.Cells["C2:H2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["C2:H2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                ws.Cells["C2:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells 
                StartHeaderNumber = 2;
                foreach (var data in listDT)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber) + ":" + (StartHeaderChar--) + (StartHeaderNumber)].Merge = true;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber) + ":" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.a;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.b;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.c;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.d;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.e;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.f;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.g;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo theo ca từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion

        #region Day report
        //Xem báo cáo ngày khác
        public ActionResult StoreReportOrtherDay()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        //Lấy danh sách báo cáo của những ngày khác
        public JsonResult LoadDateByStatusDatatable(JQueryDataTableParamModel param, string sStartTime, string sEndTime, int storeId)
        {
            var dateReportApi = new DateReportApi();
            var startTime = sStartTime?.ToDateTime().GetStartOfDate();
            var endTime = sEndTime?.ToDateTime().GetEndOfDate();
            var st = new Stopwatch();
            st.Start();
            var total = dateReportApi.GetDateReport().Count(a => a.StoreID == storeId);
            var totalQuery = dateReportApi.GetDateReport().Count(a =>
                a.StoreID == storeId &&
                (!startTime.HasValue || a.Date >= startTime.Value) &&
                (!endTime.HasValue || endTime.Value >= a.Date));
            Debug.WriteLine(st.ElapsedMilliseconds);
            st.Restart();
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"];
            Func<DateReport, object> sortBy = (s => s.Date);
            var rs = dateReportApi.GetDateReport()
                .Where(
                    a =>
                        a.StoreID == storeId && (!startTime.HasValue || a.Date >= startTime.Value) &&
                        (!endTime.HasValue || endTime.Value >= a.Date))
                .OrderByDescending(sortBy)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength);

            Debug.WriteLine(st.ElapsedMilliseconds);
            st.Restart();
            var count = param.iDisplayStart;
            var rp = rs.Select(a => new IConvertible[]
            {
                a.ID,
                ++count,
                a.Date.ToString("dd-MM-yyyy"),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.TotalAmount),
                //a.TotalAmount ,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.Discount + @a.DiscountOrderDetail),
                //(a.Discount + a.DiscountOrderDetail),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.FinalAmount),
                //a.FinalAmount,
                a.Status,
                a.ID
            }).ToList();
            Debug.WriteLine(st.ElapsedMilliseconds);
            st.Restart();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalQuery,
                aaData = rp
            }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "StoreManager, StoreReportViewer")]
        [StoreFilter]
        public ActionResult StoreDateReport(int? dateReportId, int brandId)
        {
            var productApi = new ProductApi();
            var dateReportApi = new DateReportApi();
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();

            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            var storeId = Convert.ToInt32(RouteData.Values["storeId"].ToString());
            ViewBag.Product = productApi.GetProducts().ToList();
            //ViewBag.Product = listProduct.ToList();
            if (dateReportId.HasValue)
            {
                var currentDate = dateReportApi.GetDateReportById(dateReportId.Value).FirstOrDefault();
                if (currentDate != null)
                {
                    var fromDate = currentDate.Date.GetStartOfDate();
                    var toDate = currentDate.Date.GetEndOfDate();
                    var rents = orderApi.GetOrdersByTimeRange(storeId, fromDate, toDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                    ViewBag.TotalFinal = rents.Sum(item => item.OrderDetails.Sum(a => a.FinalAmount));
                    ViewBag.TotalBill = rents.Count();
                    ViewBag.TotalBillAtStore = rents.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                    ViewBag.TotalBillTakeAway = rents.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                    ViewBag.TotalBillDelivery = rents.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                    var disFee = rents.Sum(a => a.Discount) + rents.Sum(a => a.DiscountOrderDetail);

                    ViewBag.DiscountFee = disFee;
                    var orderDetails = orderDetailApi.GetOrderDetailsByTimeRange(fromDate, toDate, storeId)
                        .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
                    ViewBag.TotalProduct = orderDetails.Count();


                    currentDate.FinalAmount = currentDate.FinalAmount;

                    var productStatistic = orderDetails.GroupBy(a => a.ProductID);
                    List<DateProduct> dateProducts = new List<DateProduct>();
                    foreach (var item in productStatistic)
                    {
                        var rs = new DateProduct()
                        {
                            ProductId = item.Key,
                            StoreID = storeId,
                            Quantity = item.Sum(b => b.Quantity),
                            Date = Utils.GetCurrentDateTime(),
                            TotalAmount = item.Sum(b => b.TotalAmount),
                            ProductName_ = item.FirstOrDefault().Product.ProductName
                        };
                        dateProducts.Add(rs);
                    }
                    ViewBag.DateProducts = dateProducts;

                    return View("StoreDateReport", currentDate);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                var fromDate = Utils.GetCurrentDateTime().GetStartOfDate();
                // var toDate = Utils.GetCurrentDateTime().AddDays(1).Date.GetEndOfDate();
                var toDate = Utils.GetCurrentDateTime().GetEndOfDate();
                //var rents = _rentService.GetRentsByTimeRange(storeId, fromDate, toDate)
                //    .Where(a => a.RentType != (int)RentTypeEnum.DropProduct).ToList();
                var rents = orderApi.GetRentsByTimeRange(storeId, fromDate, toDate).Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                var finalAmount = rents.Sum(item => item.FinalAmount);

                ViewBag.TotalBill = rents.Count();
                ViewBag.TotalBillAtStore = rents.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                ViewBag.TotalBillTakeAway = rents.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                ViewBag.TotalBillDelivery = rents.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);

                var orderDetails =
                    orderDetailApi.GetOrderDetailsByTimeRange(fromDate, toDate, storeId).ToList();

                ViewBag.TotalProduct = orderDetailApi.CountOrderDetailsByTimeRange(fromDate, toDate, storeId, (int)OrderTypeEnum.DropProduct); //_orderDetailService.GetOrderDetailsByTimeRange(fromDate, toDate, storeId).Count(a => a.Rent.RentType != (int)RentTypeEnum.DropProduct);

                var totalAmount = rents.Sum(item => item.TotalAmount);

                ViewBag.TotalFinal = finalAmount;
                var totalDiscount = rents.Sum(a => a.Discount) + rents.Sum(a => a.DiscountOrderDetail);
                ViewBag.DiscountFee = totalDiscount;
                var dateReport = new DateReport
                {
                    StoreID = storeId,
                    CreateBy = "system",
                    Status = (int)DateReportStatusEnum.Approved,
                    Date = Utils.GetCurrentDateTime(),
                    Discount = rents.Sum(a => a.Discount),
                    DiscountOrderDetail = rents.Sum(a => a.DiscountOrderDetail),
                    TotalAmount = totalAmount,
                    FinalAmount = finalAmount
                };
                var productStatistic = orderDetails.GroupBy(a => a.ProductID);
                List<DateProduct> dateProducts = new List<DateProduct>();
                foreach (var item in productStatistic)
                {
                    var rs = new DateProduct()
                    {
                        ProductId = item.Key,
                        StoreID = storeId,
                        Quantity = item.Sum(b => b.Quantity),
                        Date = Utils.GetCurrentDateTime(),
                        TotalAmount = item.Sum(b => b.TotalAmount),
                        ProductName_ = item.FirstOrDefault().Product.ProductName
                    };
                    dateProducts.Add(rs);
                }
                ViewBag.DateProducts = dateProducts;

                //ViewBag.DateProducts = productStatistic.Select(a => new DateProduct()
                //{
                //    ProductId = a.Key,
                //    StoreID = storeId,
                //    Quantity = a.Sum(b => b.Quantity),
                //    Date = Utils.GetCurrentDateTime(),
                //    TotalAmount = a.Sum(b => b.TotalAmount),
                //    ProductName_ = a.FirstOrDefault().Product.ProductName
                //}).ToList();

                var productItemStatistic =
                    productStatistic.Select(a => new Tuple<Product, int>(a.FirstOrDefault().Product, a.Sum(b => b.Quantity)));
                var compositionsStatistic = productItemStatistic.SelectMany(a => a.Item1.ProductItemCompositionMappings.Select(b => new Tuple<ProductItemCompositionMapping, int>(b, a.Item2)))
                    .GroupBy(a => a.Item1.ItemID);
                List<DateProductItem> dateProductItems = new List<DateProductItem>();
                foreach (var item in compositionsStatistic)
                {
                    var rs = new DateProductItem()
                    {
                        StoreId = storeId,
                        Date = Utils.GetCurrentDateTime(),
                        ProductItemID = item.Key,
                        ProductItemName = item.FirstOrDefault().Item1.ProductItem.ItemName,
                        Quantity = (int)item.Sum(b => b.Item2 * b.Item1.Quantity),
                        Unit = item.FirstOrDefault().Item1.ProductItem.Unit
                    };
                    dateProductItems.Add(rs);
                }
                ViewBag.DateItemProduct = dateProductItems;
                //ViewBag.DateItemProduct = compositionsStatistic.Select(a => new DateProductItem
                //{
                //    StoreId = storeId,
                //    Date = Utils.GetCurrentDateTime(),
                //    ProductItemID = a.Key,
                //    ProductItemName = a.FirstOrDefault().Item1.ProductItem.ItemName,
                //    Quantity = a.Sum(b => b.Item2 * b.Item1.Quantity),
                //    Unit = a.FirstOrDefault().Item1.ProductItem.Unit
                //});

                return View("StoreDateReport", dateReport);
            }
        }

        public ActionResult TabOrderDetail(string date)
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            //var reportDate = date.ToDateTime();
            var reportDate = date.ToDateTime();
            ViewBag.ReportDate = reportDate;
            return PartialView("_TabOrder");
        }

        [Authorize(Roles = "BrandManager, StoreManager, StoreReportViewer")]
        public JsonResult LoadOrderDatatables(JQueryDataTableParamModel param, int brandId, string reportDate, int storeId, int[] productId, int? largerPrice, int? smallerPrice)
        {
            var st = new Stopwatch();
            st.Start();
            //, int? largerPrice, int? smallerPrice
            //var rpDate = reportDate.ToDateTime();
            var rpDate = reportDate.ToDateTime();
            var startTime = new DateTime(rpDate.Year, rpDate.Month, rpDate.Day, 0, 0, 0);
            var endTime = new DateTime(rpDate.Year, rpDate.Month, rpDate.Day, 23, 59, 59);
            IEnumerable<Order> filteredListItems = null;
            var orderApi = new OrderApi();
            var orders = orderApi.GetOrdersByTimeRange(storeId, startTime, endTime, brandId).ToList();
            Debug.WriteLine(Utils.GetCurrentDateTime().ToString("dd/MM/yyyy HH:mm:ss"));
            Debug.WriteLine("LoadOrderDatatables GetRentsByTimeRange Count 1: " + st.ElapsedMilliseconds);
            st.Restart();
            filteredListItems = orders
                .Where(d =>
                    (d.OrderStatus == (int)OrderStatusEnum.Finish && d.OrderType != (int)OrderTypeEnum.DropProduct &&
                     (!d.SourceID.HasValue ||
                      d.DeliveryStatus == (int)DeliveryStatus.Finish)) &&
                      d.CheckInDate.HasValue &&
                    (string.IsNullOrEmpty(param.sSearch) || d.InvoiceID.Contains(param.sSearch) ||
                     d.CheckInPerson.ToLower().Contains(param.sSearch.ToLower())) &&
                    (productId == null || !productId.Any() || d.OrderDetails.Any(a => productId.Contains(a.ProductID))) &&
                    (largerPrice == null || d.TotalAmount >= largerPrice) &&
                    (smallerPrice == null || d.TotalAmount <= smallerPrice))
                .OrderBy(a => a.CheckInDate)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength);


            int count = 1;
            var listorders = filteredListItems.Select(a => new IConvertible[]
            {
                count++,
                a.InvoiceID,
                a.OrderDetailsTotalQuantity, //a.OrderDetails.Sum(o=>o.Quantity),
                a.TotalAmount,
                (a.Discount + a.DiscountOrderDetail),
                a.FinalAmount,
                a.CheckInDate.Value.ToString("dd/MM/yyyy") + " " + a.CheckInDate.Value.ToShortTimeString(),
                a.OrderType,
                a.CheckInPerson,
                a.RentID,
                a.Store.Name
            });
            Debug.WriteLine("4: " + st.ElapsedMilliseconds);
            st.Stop();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = filteredListItems.Count(),
                iTotalDisplayRecords = orders.Count(),
                aaData = listorders
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadOrderDetailDatables(JQueryDataTableParamModel param, int rentId)
        {
            var orderDetailApi = new OrderDetailApi();
            var orderDetail = orderDetailApi.GetOrderDetailsByRentId(rentId)
                .OrderBy(a => a.OrderDate)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength);
            var totalCount = orderDetailApi.GetOrderDetailsByRentId(rentId).Count();
            var list = orderDetail.Select(a => new IConvertible[]
            {
                a.OrderDetailID,
                a.Product.ProductName,
                a.UnitPrice,
                a.Quantity,
                a.Discount
            });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalCount,
                iTotalDisplayRecords = totalCount,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TabProduct(string date)
        {
            //var reportDate = date.ToDateTime();
            var reportDate = date.ToDateTime();
            ViewBag.ReportDate = reportDate;
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return PartialView("_TabProduct");
        }

        public JsonResult LoadAllProduct()
        {
            var productApi = new ProductApi();
            var product = productApi.GetProducts();
            return Json(new
            {
                success = true,
                data = product.Select(a => new
                {
                    id = a.ProductID,
                    text = a.ProductName
                })
            });
        }

        [Authorize(Roles = "BrandManager, StoreManager, StoreReportViewer")]
        public JsonResult LoadProductDatatables(JQueryDataTableParamModel param, string reportDate, int storeId)
        {
            int count = 1;
            IEnumerable<DateProduct> dateProducts;
            //var rpDate = reportDate.ToDateTime();
            var rpDate = reportDate.ToDateTime();
            var startTime = rpDate.GetStartOfDate();
            var endTime = rpDate.GetEndOfDate();
            IEnumerable<DateProduct> filteredListItems;
            var dateReportApi = new DateReportApi();
            var orderDetailApi = new OrderDetailApi();
            var dateProductApi = new DateProductApi();

            var model = dateReportApi.GetDateReportByDate(endTime, storeId);
            var total = 0;
            if (model == null)
            {
                dateProducts = orderDetailApi.GetProductByDate(startTime, storeId)
                    .OrderBy(a => a.ID)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);
                total = orderDetailApi.GetProductByDate(startTime, storeId).Count();
                var listProduct = dateProducts.Select(a => new IConvertible[]
                {
                    count++,
                    a.ProductName_,
                    a.Quantity,
                    a.TotalAmount.ToString("N0")
                });
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = total,
                    aaData = listProduct
                }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                //dateProducts = _dateProductService.GetDateProductByDateAndStore(endTime, storeId);
                total = dateProductApi.GetDateProductByDateAndStore(endTime, storeId).Count();
                // Search.
                var totalQuery = 0;
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredListItems = dateProductApi.GetDateProductByDateAndStore(endTime, storeId)
                        .Where(
                        d => (d.ProductName_ != null && d.ProductName_.ToLower().Contains(param.sSearch.ToLower()))
                    ).OrderBy(a => a.ID)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);
                    totalQuery = dateProductApi.GetDateProductByDateAndStore(endTime, storeId)
                        .Count(
                            d => (d.ProductName_ != null && d.ProductName_.ToLower().Contains(param.sSearch.ToLower()))
                    );
                }
                else
                {
                    filteredListItems = dateProductApi.GetDateProductByDateAndStore(endTime, storeId)
                        .OrderBy(a => a.ID)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);
                    totalQuery = dateProductApi.GetDateProductByDateAndStore(endTime, storeId).Count();
                }
                var listProduct = filteredListItems.Select(a => new IConvertible[]
                {
                    count++,
                    a.ProductName_,
                    a.Quantity,
                    a.TotalAmount.ToString("N0")
                });
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalQuery,
                    aaData = listProduct
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //public JsonResult GetTopProductDatatables(JQueryDataTableParamModel param, string reportDate, int storeId)
        //{
        //    var rpDate = reportDate.ToDateTime();
        //    var model = _dateReportService.GetDateReportByDate(rpDate, storeId);



        //    return null;
        //}

        public ActionResult TabProductItem(string date)
        {
            var reportDate = date.ToDateTime();
            ViewBag.ReportDate = reportDate;
            return PartialView("_TabProductItem");
        }

        [Authorize(Roles = "BrandManager, StoreManager, StoreReportViewer")]
        public async Task<JsonResult> LoadProductItemDatatables(JQueryDataTableParamModel param, string reportDate, int storeId)
        {
            int count = 1;
            var rpDate = reportDate.ToDateTime();
            var dateReportApi = new DateReportApi();
            var dateProductItemApi = new DateProductItemApi();
            var dateProductApi = new DateProductApi();
            var productItemApi = new ProductItemApi();
            var model = await dateReportApi.GetDateReportByDate(rpDate, storeId);
            var total = 0;
            if (model == null || model.Status == (int)DateReportStatusEnum.Approved)
            {
                IEnumerable<DateProductItem> filteredListItems;
                //var dateProductItem = _dateProductItemService.GetDateProductItemByDayAndStore(rpDate, storeId);
                total = dateProductItemApi.GetDateProductItemByDayAndStore(rpDate, storeId).Count();
                // Search.
                var totalQuery = 0;
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredListItems = dateProductItemApi.GetDateProductItemByDayAndStore(rpDate, storeId)
                        .Where(
                        d => (d.ProductItemName != null && d.ProductItemName.ToLower().Contains(param.sSearch.ToLower()))
                    ).OrderBy(a => a.ID)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);
                    totalQuery = dateProductItemApi.GetDateProductItemByDayAndStore(rpDate, storeId)
                        .Count(
                            d =>
                                (d.ProductItemName != null &&
                                 d.ProductItemName.ToLower().Contains(param.sSearch.ToLower()))
                    );
                }
                else
                {
                    filteredListItems = dateProductItemApi.GetDateProductItemByDayAndStore(rpDate, storeId)
                        .OrderBy(a => a.ID)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);
                    totalQuery = dateProductItemApi.GetDateProductItemByDayAndStore(rpDate, storeId).Count();
                }

                var listProductItem = filteredListItems.Select(a => new IConvertible[]
                {
                    count++,
                    a.ProductItemName,
                    a.Quantity,
                    a.Unit
                });
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalQuery,
                    aaData = listProductItem
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                IEnumerable<DateProduct> dateProducts = dateProductApi.GetDateProductByDateAndStore(rpDate, storeId);
                var dateProductItem = productItemApi.GetProductItemByDate(dateProducts)
                    .OrderBy(a => a.ID)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);

                total = productItemApi.GetProductItemByDate(dateProducts).Count();
                var listProductItem = dateProductItem.Select(a => new IConvertible[]
                {
                    count++,
                    a.ProductItemName,
                    a.Quantity,
                    a.Unit
                });
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = total,
                    aaData = listProductItem
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TabCashier(string date)
        {
            var reportDate = date.ToDateTime();
            ViewBag.ReportDate = reportDate;
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return PartialView("_TabCashier");
        }

        [Authorize(Roles = "BrandManager, StoreManager, StoreReportViewer")]
        public JsonResult LoadCashierDatatables(JQueryDataTableParamModel param, string reportDate, int storeId)
        {
            var checkDate = reportDate.ToDateTime();
            var orderApi = new OrderApi();
            var orders = orderApi.GetRentsByTimeRange(storeId, checkDate.GetStartOfDate(), checkDate.GetEndOfDate()).ToList();
            var model = orders.Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish)
                .GroupBy(a => a.CheckInPerson)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength);

            int i = 0;

            var modelCount = model.Count();

            var aspNetUserApi = new AspNetUserApi();
            var list = model.Select(a => new IConvertible[]
                {
                    ++i,
                string.IsNullOrWhiteSpace(a.Key) ? "N/A" : aspNetUserApi.GetUserByUsername(a.Key).FullName,
                string.IsNullOrWhiteSpace(a.Key) ? "N/A" : a.Key,
                    a.Count(),
                    a.Sum(b => b.FinalAmount)
            });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecord = modelCount,
                iTotalDisplayRecords = modelCount,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Quantitative report

        public ActionResult StoreQuantitativeReport()
        {
            //Abc
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        [HttpPost]
        public JsonResult LoadQuantitativeReport(int brandId, string startTime, string endTime)
        {
            //Abc
            var productItemApi = new ProductItemApi();
            var productApi = new ProductApi();
            var orderDetailApi = new OrderDetailApi();
            var dateProductItemApi = new DateProductItemApi();
            var dateProductApi = new DateProductApi();

            var id = RouteData.Values["storeId"].ToString();
            int storeId = Convert.ToInt32(id);
            var products = new List<Tuple<int, string, int, double>>();
            var productItems = new List<Tuple<int, string, double, string, double>>();
            var productItemPrices = productItemApi.GetAvailableProductItems().Select(q => new { ItemId = q.ItemID, Price = q.Price }).ToList();

            //var isAdmin = Roles.IsUserInRole("Administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");
            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                //if (!isAdmin)
                //{

                //}
                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();
                //var dateProdcuts = _dateProductService.GetDateProductByTimeRange(startDate, endDate,
                //    storeId);
                var dateProdcuts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate).Where(q => q.StoreId == storeId);

                //Get OrderDetail
                var orderDetails =
                    orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
                    .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                var compositionsStatistic = orderDetails.SelectMany(a => a.Product.ProductItemCompositionMappings.Select(b => new Tuple<ProductItemCompositionMapping, int>(b, a.Quantity)))
                .GroupBy(a => a.Item1.ItemID);

                var dateItemProduct = compositionsStatistic.Join(productItemApi.GetProductItems(), a => a.Key, a => a.ItemID, (a, b) => new DateProductItem
                {
                    StoreId = storeId,
                    Date = dateNow,
                    ProductItemID = a.Key,
                    ProductItemName = b.ItemName,
                    Quantity = (int)a.Sum(c => c.Item2 * c.Item1.Quantity),
                    Unit = b.Unit
                }).AsQueryable();

                foreach (var item in dateProdcuts)
                {
                    var price = item.Quantity * item.Product.Price;/*productItemPrices.FirstOrDefault(q => q.ItemId == item.);*/
                    //var price = (productPrice == null) ? 0 : (productPrice.Price) ?? 0;
                    if (products.Any(a => a.Item1 == item.ProductID))
                    {
                        var product = products.FirstOrDefault(a => a.Item1 == item.ProductID);
                        products.Remove(product);
                        products.Add(new Tuple<int, string, int, double>(item.ProductID, item.Product.ProductName, item.Quantity + product.Item3
                            , (item.Quantity + product.Item3) * price));
                    }
                    else
                    {
                        products.Add(new Tuple<int, string, int, double>(item.ProductID, item.Product.ProductName, item.Quantity, item.Quantity * price));
                    }
                }


                foreach (var item in dateItemProduct)
                {
                    var productItemPrice = productItemPrices.FirstOrDefault(q => q.ItemId == item.ProductItemID);
                    var price = (productItemPrice == null) ? 0 : (productItemPrice.Price) ?? 0;
                    if (productItems.Any(a => a.Item1 == item.ProductItemID))
                    {
                        var productItem = productItems.FirstOrDefault(a => a.Item1 == item.ProductItemID);
                        productItems.Remove(productItem);
                        productItems.Add(new Tuple<int, string, double, string, double>(item.ProductItemID, item.ProductItemName, item.Quantity + productItem.Item3, item.Unit, productItem.Item5 + item.Quantity * price));
                    }
                    else
                    {
                        productItems.Add(new Tuple<int, string, double, string, double>(item.ProductItemID, item.ProductItemName, item.Quantity, item.Unit, item.Quantity * price));
                    }
                }
                //ViewBag.Start = startDate;
                //ViewBag.End = endDate;
                //ViewBag.Product = products;
                //ViewBag.ProductItem = productItems;
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                //if (!isAdmin)
                //{
                //    if (startDate == DateTime.Today)
                //    {
                //        return Json(new
                //        {
                //            portionItemValue = 0,
                //            dataTable = new
                //            {
                //                listProduct = 0,
                //                listProductItem = 0
                //            }
                //        }, JsonRequestBehavior.DenyGet);
                //    }
                //    if (endDate >= DateTime.Today)
                //    {
                //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
                //    }
                //}
                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
                var dateProdcuts = dateProductApi.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId);
                var dateProdcutItems = dateProductItemApi.GetDateProductItemByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(),
                    storeId);


                foreach (var item in dateProdcuts)
                {
                    if (products.Any(a => a.Item1 == item.ProductId))
                    {
                        var product = products.FirstOrDefault(a => a.Item1 == item.ProductId);
                        products.Remove(product);
                        products.Add(new Tuple<int, string, int, double>(item.ProductId, item.ProductName_, (item.Quantity + product.Item3), item.FinalAmount));
                    }
                    else
                    {
                        products.Add(new Tuple<int, string, int, double>(item.ProductId, item.ProductName_, item.Quantity, item.FinalAmount));
                    }
                }

                foreach (var item in dateProdcutItems)
                {
                    var productItemPrice = productItemPrices.FirstOrDefault(q => q.ItemId == item.ProductItemID);
                    var price = (productItemPrice == null) ? 0 : (productItemPrice.Price) ?? 0;
                    if (productItems.Any(a => a.Item1 == item.ProductItemID))
                    {
                        var productItem = productItems.FirstOrDefault(a => a.Item1 == item.ProductItemID);
                        productItems.Remove(productItem);
                        productItems.Add(new Tuple<int, string, double, string, double>(item.ProductItemID, item.ProductItemName, item.Quantity + productItem.Item3, item.Unit, productItem.Item5 + item.Quantity * price));
                    }
                    else
                    {
                        productItems.Add(new Tuple<int, string, double, string, double>(item.ProductItemID, item.ProductItemName, item.Quantity, item.Unit, item.Quantity * price));
                    }
                }
                //ViewBag.Start = startDate;
                //ViewBag.End = endDate;
                //ViewBag.Product = products;
                //ViewBag.ProductItem = productItems;
            }
            //return PartialView("_LoadQuantitativeReport");
            var count = 0;
            double totalProductItemPrice = 0;
            double totalProductPrice = 0;
            foreach (var item in productItems)
            {
                totalProductItemPrice += item.Item5;
            }
            foreach (var item in products)
            {
                totalProductPrice += item.Item4;
            }
            var listProduct = products.Select(a => new IConvertible[]
                {
                    ++count,
                    a.Item2,
                    a.Item3
                }).ToArray();

            count = 0;
            var listProductItem = productItems.Select(a => new IConvertible[]
                {
                    ++count,
                    a.Item2,
                    a.Item3,
                    a.Item4,
                    a.Item5.ToString("C0",CultureInfo.GetCultureInfo("vi-VN")),
                }).ToArray();
            var portionItemValue = (totalProductPrice == 0) ? 0 : (double)totalProductItemPrice / totalProductPrice;
            return Json(new
            {
                portionItemValue = portionItemValue.ToString("P1"),
                dataTable = new
                {
                    listProduct = listProduct,
                    listProductItem = listProductItem
                }
            }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult ExportProductQuantitativeTableToExcel(string startTime, string endTime)
        {
            //Abc
            var dateProductApi = new DateProductApi();
            var dateProductItemApi = new DateProductItemApi();

            var id = RouteData.Values["storeId"].ToString();
            int storeId = Convert.ToInt32(id);
            var products = new List<Tuple<int, string, int>>();
            var productItems = new List<Tuple<int, string, double, string>>();
            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                var endDate = dateNow.AddDays(-1).GetEndOfDate();
                var dateProdcuts = dateProductApi.GetDateProductByTimeRange(startDate, endDate,
                    storeId);
                var dateProdcutItems = dateProductItemApi.GetDateProductItemByTimeRange(startDate, endDate,
                    storeId);
                foreach (var item in dateProdcuts)
                {
                    if (products.Any(a => a.Item1 == item.ProductId))
                    {
                        var product = products.FirstOrDefault(a => a.Item1 == item.ProductId);
                        products.Remove(product);
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity + product.Item3));
                    }
                    else
                    {
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity));
                    }
                }

                foreach (var item in dateProdcutItems)
                {
                    if (productItems.Any(a => a.Item1 == item.ProductItemID))
                    {
                        var productItem = productItems.FirstOrDefault(a => a.Item1 == item.ProductItemID);
                        productItems.Remove(productItem);
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity + productItem.Item3, item.Unit));
                    }
                    else
                    {
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity, item.Unit));
                    }
                }
                //ViewBag.Start = startDate;
                //ViewBag.End = endDate;
                //ViewBag.Product = products;
                //ViewBag.ProductItem = productItems;
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                var dateProdcuts = dateProductApi.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId);
                var dateProdcutItems = dateProductItemApi.GetDateProductItemByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(),
                    storeId);
                foreach (var item in dateProdcutItems)
                {
                    if (productItems.Any(a => a.Item1 == item.ProductItemID))
                    {
                        var productItem = productItems.FirstOrDefault(a => a.Item1 == item.ProductItemID);
                        productItems.Remove(productItem);
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity + productItem.Item3, item.Unit));
                    }
                    else
                    {
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity, item.Unit));
                    }
                }

                foreach (var item in dateProdcuts)
                {
                    if (products.Any(a => a.Item1 == item.ProductId))
                    {
                        var product = products.FirstOrDefault(a => a.Item1 == item.ProductId);
                        products.Remove(product);
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity + product.Item3));
                    }
                    else
                    {
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity));
                    }
                }
                //ViewBag.Start = startDate;
                //ViewBag.End = endDate;
                //ViewBag.Product = products;
                //ViewBag.ProductItem = productItems;
            }
            //return PartialView("_LoadQuantitativeReport");
            var listProduct = products.Select(a => new
            {
                a = a.Item2,
                b = a.Item3
            }).ToArray();

            List<string> header = new List<string>();
            //header.Add("#");
            //header.Add("Tên sản phẩm");
            //header.Add("Số lượng bán ra");
            header.Add("STT;1;1;r");
            header.Add("Tên sản phẩm;1;1;r");
            header.Add("Số lượng bán ra;1;1;r");
            string fileName = "TổngQuanSảnPhẩm_Tháng";

            bool success = false;
            Thread thdSyncRead = new Thread(new ThreadStart(() => exportExcel(header, listProduct, ref fileName, ref success)));
            thdSyncRead.SetApartmentState(ApartmentState.STA);
            thdSyncRead.Start();
            thdSyncRead.Join(120000);
            if (!success)
            {
                thdSyncRead.Abort();
            }

            return Json(new
            {
                success = success,
                fileName = fileName,
            }, JsonRequestBehavior.AllowGet);

        }
        public List<dynamic> ExportProductItemQuantitativeTableToExcel(string startTime, string endTime)
        {
            //Abc
            var dateProductApi = new DateProductApi();
            var dateProductItemApi = new DateProductItemApi();

            var id = RouteData.Values["storeId"].ToString();
            int storeId = Convert.ToInt32(id);
            var products = new List<Tuple<int, string, int>>();
            var productItems = new List<Tuple<int, string, double, string>>();
            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                var endDate = dateNow.AddDays(-1).GetEndOfDate();
                var dateProdcuts = dateProductApi.GetDateProductByTimeRange(startDate, endDate,
                    storeId);
                var dateProdcutItems = dateProductItemApi.GetDateProductItemByTimeRange(startDate, endDate,
                    storeId);
                foreach (var item in dateProdcuts)
                {
                    if (products.Any(a => a.Item1 == item.ProductId))
                    {
                        var product = products.FirstOrDefault(a => a.Item1 == item.ProductId);
                        products.Remove(product);
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity + product.Item3));
                    }
                    else
                    {
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity));
                    }
                }

                foreach (var item in dateProdcutItems)
                {
                    if (productItems.Any(a => a.Item1 == item.ProductItemID))
                    {
                        var productItem = productItems.FirstOrDefault(a => a.Item1 == item.ProductItemID);
                        productItems.Remove(productItem);
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity + productItem.Item3, item.Unit));
                    }
                    else
                    {
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity, item.Unit));
                    }
                }
                //ViewBag.Start = startDate;
                //ViewBag.End = endDate;
                //ViewBag.Product = products;
                //ViewBag.ProductItem = productItems;
            }
            else
            {
                var startDate = startTime.ToDateTime();
                var endDate = endTime.ToDateTime();
                var dateProdcuts = dateProductApi.GetDateProductByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId);
                var dateProdcutItems = dateProductItemApi.GetDateProductItemByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(),
                    storeId);
                foreach (var item in dateProdcutItems)
                {
                    if (productItems.Any(a => a.Item1 == item.ProductItemID))
                    {
                        var productItem = productItems.FirstOrDefault(a => a.Item1 == item.ProductItemID);
                        productItems.Remove(productItem);
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity + productItem.Item3, item.Unit));
                    }
                    else
                    {
                        productItems.Add(new Tuple<int, string, double, string>(item.ProductItemID, item.ProductItemName, item.Quantity, item.Unit));
                    }
                }

                foreach (var item in dateProdcuts)
                {
                    if (products.Any(a => a.Item1 == item.ProductId))
                    {
                        var product = products.FirstOrDefault(a => a.Item1 == item.ProductId);
                        products.Remove(product);
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity + product.Item3));
                    }
                    else
                    {
                        products.Add(new Tuple<int, string, int>(item.ProductId, item.ProductName_, item.Quantity));
                    }
                }
                //ViewBag.Start = startDate;
                //ViewBag.End = endDate;
                //ViewBag.Product = products;
                //ViewBag.ProductItem = productItems;
            }
            List<dynamic> listdt = new List<dynamic>();
            foreach (var a in productItems)
            {
                listdt.Add(new
                {
                    a = a.Item2,
                    b = a.Item3,
                    c = a.Item4
                });
            }

            //List<string> header = new List<string>();
            //header.Add("#;1;1");
            //header.Add("Tên nguyên liệu;1;1");
            //header.Add("Số lượng;1;1");
            //header.Add("Đơn vị;1;1");

            //string fileName = "TổngQuanSảnPhẩm_Tháng";

            //var success = ExportToExcelExtensions.ExportToExcel(header, listProductItem, fileName);

            return listdt;
        }
        public ActionResult ExportProductItemQuantitativeExcelEPPlus(string startTime, string endTime)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = ExportProductItemQuantitativeTableToExcel(startTime, endTime);
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "#";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên nguyên liệu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Đơn vị";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells     
                int no = 1;
                foreach (var data in listDT)
                {
                    // STT	TÊN NGUYÊN LiỆU	ĐV	T. ĐẦU	NHẬP	XuẤT	BÁN	TỒN LT	TỒN TT	(+/-)
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = no++;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.a;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.b;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.c;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Tổng quan sản phẩm tháng " + startTime.ToString() + " - " + endTime.ToString() + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion

        #region Revenue Report
        //Index Revenue Page one store
        public ActionResult StoreRevenueReport()
        {
            //Get storeId, storeName form URL
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        //Load Revenue Report
        public JsonResult LoadRevenueReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            var dateReportApi = new DateReportApi();
            //Create list temp repot
            var reportList = new List<TempStoreRevenueReportItemViewModel>();
            DateTime e;
            //var isAdmin = Roles.IsUserInRole("Administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");
            string timeLine = startTime;

            //Mặc định lấy tháng này
            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            //if (!isAdmin)
            //{
            //    if (startDate == DateTime.Today)
            //    {
            //        return Json(new
            //        {
            //            datatable = 0,
            //            datachart = new
            //            {
            //                dateList = 0,
            //                totalAmount = 0,
            //                totalDiscountFee = 0,
            //                totalFinal = 0
            //            }
            //        }, JsonRequestBehavior.AllowGet);
            //    }
            //    if (endDate >= DateTime.Today)
            //    {
            //        endDate = Utils.GetCurrentDateTime().AddDays(-1);
            //    }
            //}
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();

            e = endDate;

            Stopwatch sw = new Stopwatch();
            sw.Start();


            //Chạy từ đầu tháng đến ngày hôm qua
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {

                // -- duynnm  --
                IEnumerable<DateReport> dateReport;

                //Nếu storeId > 0 thì lấy theo từng cửa hàng (mặc định storeId = 0 là của toàn bộ hệ thống) 
                if (storeId > 0)
                {
                    dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, startDate.GetEndOfDate(), storeId);

                }
                else
                {
                    dateReport = dateReportApi.GetDateReportTimeRange(startDate, startDate.GetEndOfDate());
                }

                //var totalBill = _rentService.GetRentsByTimeRange(storeId, startDate, startDate.GetEndOfDate())
                //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                //Tổng hóa đơn bán tại cửa hàng
                var billAtStore = dateReport.Sum(a => a.TotalOrderAtStore);

                //Tổng hóa đơn giao hàng
                var billDelivery = dateReport.Sum(a => a.TotalOrderDelivery);

                //Tổng hóa đơn mua mang đi
                var billTakeAway = dateReport.Sum(a => a.TotalOrderTakeAway);

                //Tổng hóa đơn bán được
                var totalBill = dateReport.Sum(a => a.TotalOrder);

                //Tổng doanh thu chưa giảm giá
                var totalAmount = (double)dateReport.Sum(a => a.TotalAmount);

                //Tổng giảm giá
                var discountFee = (double)dateReport.Sum(a => a.Discount) + (double)dateReport.Sum(a => a.DiscountOrderDetail);

                //Tổng doanh thu sau giảm giá
                var totalFinal = (double)dateReport.Sum(a => a.FinalAmount);


                reportList.Add(new TempStoreRevenueReportItemViewModel()
                {
                    StartTime = startDate,
                    TimeLine = startDate,
                    TotalDiscountFee = discountFee,
                    TotalFinal = totalFinal,
                    TotalAmount = totalAmount,
                    TotalBill = totalBill,
                    BillAtStore = billAtStore,
                    BillDelivery = billDelivery,
                    BillTakeAway = billTakeAway
                });
                e = e.AddDays(-1);
                startDate = startDate.AddDays(1);

            }
            sw.Stop();

            Console.WriteLine("Stop 1: ", sw.Elapsed);

            sw.Reset();
            sw.Start();

            var list = reportList.Select(a => new IConvertible[]
            {
                a.StartTime.ToShortDateString(),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.TotalAmount), //a.OrderDetails.Sum(o=>o.Quantity),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.TotalDiscountFee),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.TotalFinal),
                a.BillAtStore,
                a.BillDelivery,
                a.BillTakeAway,
                a.TotalBill
            }).ToList();

            var _DateLine = reportList.Select(a => a.TimeLine.ToShortDateString());

            var _TotalAmount = reportList.Select(a => a.TotalAmount);

            var _TotalDiscountFee = reportList.Select(a => a.TotalDiscountFee);

            var _TotalFinal = reportList.Select(a => a.TotalFinal);

            sw.Stop();

            Console.WriteLine("Stop 2: ", sw.Elapsed);

            return Json(new
            {
                datatable = list,
                datachart = new
                {
                    dateList = _DateLine,
                    totalAmount = _TotalAmount,
                    totalDiscountFee = _TotalDiscountFee,
                    totalFinal = _TotalFinal
                }
            }, JsonRequestBehavior.AllowGet);
        }
        // Chart Load Revenue Report
        public List<dynamic> ExportRevenueTableToExcel(int brandId, string startTime, string endTime, int storeId)
        {
            #region Get data
            var storeApi = new StoreApi();
            var dateReportApi = new DateReportApi();
            //Create list temp repot
            var reportList = new List<TempStoreRevenueReportItemViewModel>();
            double total = 0;
            double totalFinalAmount = 0;
            double totalDiscountFee = 0;
            DateTime e;
            string timeLine = startTime;

            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();

            e = endDate;

            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {

                // -- duynnm  --
                IEnumerable<DateReport> dateReport;

                if (storeId > 0)
                {
                    dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, startDate.GetEndOfDate(), storeId);

                }
                else
                {
                    dateReport = dateReportApi.GetDateReportTimeRange(startDate, startDate.GetEndOfDate());
                }

                var billAtStore = dateReport.Sum(a => a.TotalOrderAtStore);
                var billDelivery = dateReport.Sum(a => a.TotalOrderDelivery);
                var billTakeAway = dateReport.Sum(a => a.TotalOrderTakeAway);
                var totalBill = dateReport.Sum(a => a.TotalOrder);

                var totalAmount = (double)dateReport.Sum(a => a.TotalAmount);

                var discountFee = (double)dateReport.Sum(a => a.Discount) + (double)dateReport.Sum(a => a.DiscountOrderDetail);

                var totalFinal = (double)dateReport.Sum(a => a.FinalAmount);

                total += totalAmount;
                totalDiscountFee += discountFee;
                totalFinalAmount += totalFinal;
                reportList.Add(new TempStoreRevenueReportItemViewModel()
                {
                    StartTime = startDate,
                    TimeLine = startDate,
                    TotalDiscountFee = discountFee,
                    TotalFinal = totalFinal,
                    TotalAmount = totalAmount,
                    TotalBill = totalBill,
                    BillAtStore = billAtStore,
                    BillDelivery = billDelivery,
                    BillTakeAway = billTakeAway
                });
                e = e.AddDays(-1);
                startDate = startDate.AddDays(1);

            }

            List<dynamic> listdt = new List<dynamic>();
            foreach (var a in reportList.OrderBy(a => a.StartTime))
            {
                listdt.Add(new
                {
                    a = a.StartTime.ToString("dd/MM/yyyy"),
                    b = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.TotalAmount), //a.OrderDetails.Sum(o=>o.Quantity),
                    c = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.TotalDiscountFee),
                    d = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", @a.TotalFinal),
                    e = a.BillAtStore,
                    f = a.BillDelivery,
                    g = a.BillTakeAway,
                    h = a.TotalBill
                });
            }
            #endregion

            #region Export to Excel
            //List<string> header = new List<string>();
            //header.Add("#;1;2;r");
            //header.Add("Ngày;1;2;r");
            //header.Add("Doanh Thu;1;3;c");
            //header.Add("Tổng doanh thu;2;1");
            //header.Add("Giảm giá;2;1");
            //header.Add("Doanh thu sau giảm giá;2;1");
            //header.Add("Hoá đơn;1;4;c");
            //header.Add("Hoá đơn(Tại Store);2;1");
            //header.Add("Hoá đơn(Giao hàng);2;1");
            //header.Add("Hoá đơn(Delivery);2;1");
            //header.Add("Tổng hoá đơn;2;1");

            //string storeName = storeApi.GetStoreNameByID(storeId);
            //string sTime = startTime.Replace("/", "-");
            //string eTime = endTime.Replace("/", "-");
            //string dateRange = "(" + sTime + (sTime == eTime ? "" : " _ " + eTime) + ")";
            //string fileName = "Doanh Thu Theo Ngày_" + storeName + dateRange;

            //bool success = false;
            //Thread thdSyncRead = new Thread(new ThreadStart(() => exportExcel(header, list, ref fileName, ref success)));
            //thdSyncRead.SetApartmentState(ApartmentState.STA);
            //thdSyncRead.Start();
            //thdSyncRead.Join(120000);
            //if (!success)
            //{
            //    thdSyncRead.Abort();
            //}
            #endregion
            return listdt;

        }

        public ActionResult ExportRevenueExcelEPPlus(int brandId, string startTime, string endTime, int storeId)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = ExportRevenueTableToExcel(brandId, startTime, endTime, storeId);
                #region MergeHeaders
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["B1:D1"].Merge = true;
                ws.Cells["E1:H1"].Merge = true;
                #endregion
                #region Headers                
                ws.Cells["A1:A2"].Value = "Ngày";
                ws.Cells["B1:D1"].Value = "Doanh thu";
                ws.Cells["B2"].Value = "Tổng doanh thu";
                ws.Cells["C2"].Value = "Giảm giá";
                ws.Cells["D2"].Value = "Doanh thu sau giảm giá";
                ws.Cells["E1:H1"].Value = "Hóa đơn";
                ws.Cells["E2"].Value = "Hóa đơn (Tại Store)";
                ws.Cells["F2"].Value = "Hóa đơn (Giao hàng)";
                ws.Cells["G2"].Value = "Hóa đơn (Delivery)";
                ws.Cells["H2"].Value = "Tổng hóa đơn";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns  
                //              
                ws.Cells["A1:A2"].Style.Font.Bold = true;
                ws.Cells["A1:A2"].AutoFitColumns();
                ws.Cells["A1:A2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.Cells["A1:A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                //
                ws.Cells["B1:H1"].Style.Font.Bold = true;
                ws.Cells["B1:H1"].AutoFitColumns();
                ws.Cells["B1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["B1:H1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.Cells["B1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                //
                ws.Cells["B2:H2"].Style.Font.Bold = true;
                ws.Cells["B2:H2"].AutoFitColumns();
                ws.Cells["B2:H2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["B2:H2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                ws.Cells["B2:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells 
                StartHeaderNumber = 2;
                foreach (var data in listDT)
                {
                    ws.Cells["" + (StartHeaderChar) + (++StartHeaderNumber) + ":" + (StartHeaderChar) + (StartHeaderNumber)].Merge = true;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber) + ":" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.a;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.b;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.c;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.d;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.e;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.f;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.g;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.h;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Doanh thu theo ngày " + startTime.ToString() + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion

        #region Payment Type Report
        //Index Revenue Page one store
        public ActionResult PaymentTypeReport()
        {
            //Get storeId, storeName form URL
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        //Load PaymentType Report
        public JsonResult LoadPaymentTypeReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {

            //Create list temp repot
            var reportList = new List<TempPaymentTypeReportItemViewModel>();
            double cash = 0;
            double bank = 0;
            double directBill = 0;
            var orderApi = new OrderApi();

            if (startTime == "" && endTime == "")
            {
                //var dateNow = Utils.GetCurrentDateTime();
                var dateNow = Utils.GetCurrentDateTime();

                //var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                //var endDate = dateNow.AddDays(-1).GetStartOfDate();
                var startDate = dateNow.AddDays(-7).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                //var condition = dateNow.AddDays(-7).GetStartOfDate();

                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    cash = 0;
                    bank = 0;
                    directBill = 0;
                    IEnumerable<Order> dateReport;
                    if (storeId == 0)
                    {
                        dateReport = orderApi.GetAllOrderByDate(startDate, startDate.GetEndOfDate(), brandId).ToList();
                    }
                    else
                    {
                        dateReport = orderApi.GetRentsByTimeRange2(storeId, startDate, startDate.GetEndOfDate()).ToList();
                    }
                    foreach (var item in dateReport)
                    {
                        cash += item.Payments.Where(a => a.Type == (double)PaymentType.Cash).Sum(a => a.Amount);
                        bank += item.Payments.Where(a => a.Type == (double)PaymentType.Bank).Sum(a => a.Amount);
                        directBill += item.Payments.Where(a => a.Type == (double)PaymentType.DirectBill).Sum(a => a.Amount);
                    }

                    reportList.Add(new TempPaymentTypeReportItemViewModel()
                    {
                        Time = startDate,
                        Cash = cash,
                        Bank = bank,
                        DirectBill = directBill,

                    });
                    startDate = startDate.AddDays(1);
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    cash = 0;
                    bank = 0;
                    directBill = 0;
                    IEnumerable<Order> dateReport;
                    if (storeId == 0)
                    {
                        dateReport = orderApi.GetAllOrderByDate(startDate, startDate.GetEndOfDate(), brandId).ToList();
                    }
                    else
                    {
                        dateReport = orderApi.GetRentsByTimeRange2(storeId, startDate, startDate.GetEndOfDate()).ToList();
                    }
                    foreach (var item in dateReport)
                    {
                        cash += item.Payments.Where(a => a.Type == (int)PaymentType.Cash).Sum(a => a.Amount);
                        bank += item.Payments.Where(a => a.Type == (int)PaymentType.Bank).Sum(a => a.Amount);
                        directBill += item.Payments.Where(a => a.Type == (int)PaymentType.DirectBill).Sum(a => a.Amount);
                    }
                    //cash += dateReport.Sum(q => (double?)q.Payments.Where(a => a.Type == (int)PaymentType.Cash).Sum(a => (double?)a.Amount ?? 0) ?? 0);
                    //bank += dateReport.Sum(q => (double?)q.Payments.Where(a => a.Type == (int)PaymentType.Bank).Sum(a => (double?)a.Amount ?? 0) ?? 0);
                    //directBill += dateReport.Sum(q => (double?)q.Payments.Where(a => a.Type == (int)PaymentType.DirectBill).Sum(a => (double?)a.Amount ?? 0) ?? 0);
                    reportList.Add(new TempPaymentTypeReportItemViewModel()
                    {
                        Time = startDate,
                        Cash = cash,
                        Bank = bank,
                        DirectBill = directBill,

                    });
                    startDate = startDate.AddDays(1);

                }
            }
            var list = reportList.Select(a => new IConvertible[]
            {
                a.Time.ToString("dd/MM/yyyy"),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Cash),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Bank),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.DirectBill),
            }).ToArray();

            var _time = reportList.Select(a => a.Time.ToString("dd/MM/yyyy")).ToArray();
            var _Cash = reportList.Select(a => a.Cash).ToArray();
            var _Bank = reportList.Select(a => a.Bank).ToArray();
            var _DireckBill = reportList.Select(a => a.DirectBill).ToArray();

            return Json(new
            {
                datatable = list,
                dataChart = new
                {
                    Time = _time,
                    Cash = _Cash,
                    Bank = _Bank,
                    Direckbill = _DireckBill,
                }
            }, JsonRequestBehavior.AllowGet);
        }
        // Chart PaymentType Report
        // export to excel
        public ActionResult ExportPaymentTypeTableToExcel(int brandId, string startTime, string endTime, int storeId)
        {
            #region Get Data
            var reportList = new List<TempPaymentTypeReportItemViewModel>();
            double cash = 0;
            double bank = 0;
            double directBill = 0;
            var orderApi = new OrderApi();

            if (startTime == "" && endTime == "")
            {
                //var dateNow = Utils.GetCurrentDateTime();
                var dateNow = Utils.GetCurrentDateTime();

                //var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                //var endDate = dateNow.AddDays(-1).GetStartOfDate();
                var startDate = dateNow.AddDays(-7).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                //var condition = dateNow.AddDays(-7).GetStartOfDate();

                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    cash = 0;
                    bank = 0;
                    directBill = 0;
                    IEnumerable<Order> dateReport;
                    if (storeId == 0)
                    {
                        dateReport = orderApi.GetAllOrderByDate(startDate, startDate.GetEndOfDate(), brandId).ToList();
                    }
                    else
                    {
                        dateReport = orderApi.GetRentsByTimeRange2(storeId, startDate, startDate.GetEndOfDate()).ToList();
                    }
                    foreach (var item in dateReport)
                    {
                        cash += item.Payments.Where(a => a.Type == (double)PaymentType.Cash).Sum(a => a.Amount);
                        bank += item.Payments.Where(a => a.Type == (double)PaymentType.Bank).Sum(a => a.Amount);
                        directBill += item.Payments.Where(a => a.Type == (double)PaymentType.DirectBill).Sum(a => a.Amount);
                    }

                    reportList.Add(new TempPaymentTypeReportItemViewModel()
                    {
                        Time = startDate,
                        Cash = cash,
                        Bank = bank,
                        DirectBill = directBill,

                    });
                    startDate = startDate.AddDays(1);
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    cash = 0;
                    bank = 0;
                    directBill = 0;
                    IEnumerable<Order> dateReport;
                    if (storeId == 0)
                    {
                        dateReport = orderApi.GetAllOrderByDate(startDate, startDate.GetEndOfDate(), brandId).ToList();
                    }
                    else
                    {
                        dateReport = orderApi.GetRentsByTimeRange2(storeId, startDate, startDate.GetEndOfDate()).ToList();
                    }
                    foreach (var item in dateReport)
                    {
                        cash += item.Payments.Where(a => a.Type == (int)PaymentType.Cash).Sum(a => a.Amount);
                        bank += item.Payments.Where(a => a.Type == (int)PaymentType.Bank).Sum(a => a.Amount);
                        directBill += item.Payments.Where(a => a.Type == (int)PaymentType.DirectBill).Sum(a => a.Amount);
                    }
                    reportList.Add(new TempPaymentTypeReportItemViewModel()
                    {
                        Time = startDate,
                        Cash = cash,
                        Bank = bank,
                        DirectBill = directBill,

                    });
                    startDate = startDate.AddDays(1);

                }
            }
            #endregion

            #region Export Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                int count = 0;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tiền mặt";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thẻ tín dụng";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Thẻ thành viên";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in reportList)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Time.ToString("dd/MM/yyyy");
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Cash;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Bank;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.DirectBill;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                string fileName = "Báo cáo theo loại hình thanh toán từ " + startTime.Replace("/", "-") + " đến " + endTime.Replace("/", "-") + ".xlsx"; ;
                var fileDownloadName = fileName + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion
        }
        #endregion

        #region Contributed Sales Report
        //Index Revenue Page one store
        public ActionResult ContributedSalesReport()
        {
            //Get storeId, storeName form URL
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        //Load Report
        public JsonResult LoadContributedSalesReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            var orderApi = new OrderApi();
            var hourReport = new List<TempContributedReportItemViewModel>();
            for (int i = 6; i < 23; i++)
            {
                hourReport.Add(new TempContributedReportItemViewModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)

                });
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                //var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                //var endDate = dateNow.AddDays(-1).Date.GetEndOfDate();
                //var startDate = dateNow.AddDays(-30).Date.GetStartOfDate();
                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();
                var rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                //var totalOrder = rents.Count();
                //var totalAmount = rents.Sum(a=> a.FinalAmount);
                //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;

                    if (item.TotalPrice != 0)
                    {
                        item.PersentTakeAway = (item.PriceTakeAway / item.TotalPrice) * 100;
                        item.PersentDelivery = (item.PriceDelivery / item.TotalPrice) * 100;
                        item.PersentAtStore = (item.PriceAtStore / item.TotalPrice) * 100;
                    }
                    else
                    {
                        item.PersentTakeAway = 0;
                        item.PersentDelivery = 0;
                        item.PersentAtStore = 0;
                    }
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                TimeSpan spanTime = endDate - startDate;
                var rents = orderApi.GetRentsByTimeRange(storeId, startDate.GetStartOfDate(), endDate.GetEndOfDate())
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;

                    if (item.TotalPrice != 0)
                    {
                        item.PersentTakeAway = (item.PriceTakeAway / item.TotalPrice) * 100;
                        item.PersentDelivery = (item.PriceDelivery / item.TotalPrice) * 100;
                        item.PersentAtStore = (item.PriceAtStore / item.TotalPrice) * 100;
                    }
                    else
                    {
                        item.PersentTakeAway = 0;
                        item.PersentDelivery = 0;
                        item.PersentAtStore = 0;
                    }
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }
            var list = hourReport.Select(a => new IConvertible[]
            {
                a.StartTime + ":00 - " + a.EndTime + ":00",

                a.TakeAway,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0.0} %", a.PersentTakeAway),

                a.AtStore,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0.0} %", a.PersentAtStore),

                a.Delivery,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0.0} %", a.PersentDelivery),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice)

            }).ToArray();
            var _DateLine = hourReport.Select(a => a.StartTime + ":00 - " + a.EndTime + ":00").ToArray();

            var _BillAtStore = hourReport.Select(a => a.PersentAtStore).ToArray();

            var _BillDelivery = hourReport.Select(a => a.PersentDelivery).ToArray();

            var _BillTakeAway = hourReport.Select(a => a.PersentTakeAway).ToArray();
            //return PartialView("_LoadRevenueReport", reportList.OrderBy(a => a.StartTime));
            return Json(new
            {
                datatable = list,
                datachart = new
                {
                    dateLine = _DateLine,
                    BillAtStore = _BillAtStore,
                    BillDelivery = _BillDelivery,
                    BillTakeAway = _BillTakeAway
                }
            }, JsonRequestBehavior.AllowGet);
        }
        // Chart Load Revenue Report
        public JsonResult ExportContributedSalesTableToExcel(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            var orderApi = new OrderApi();
            var hourReport = new List<TempContributedReportItemViewModel>();
            for (int i = 6; i < 23; i++)
            {
                hourReport.Add(new TempContributedReportItemViewModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)

                });
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                //var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                //var endDate = dateNow.AddDays(-1).Date.GetEndOfDate();
                //var startDate = dateNow.AddDays(-30).Date.GetStartOfDate();
                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();
                var rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                //var totalOrder = rents.Count();
                //var totalAmount = rents.Sum(a=> a.FinalAmount);
                //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;

                    if (item.TotalPrice != 0)
                    {
                        item.PersentTakeAway = (item.PriceTakeAway / item.TotalPrice) * 100;
                        item.PersentDelivery = (item.PriceDelivery / item.TotalPrice) * 100;
                        item.PersentAtStore = (item.PriceAtStore / item.TotalPrice) * 100;
                    }
                    else
                    {
                        item.PersentTakeAway = 0;
                        item.PersentDelivery = 0;
                        item.PersentAtStore = 0;
                    }
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                TimeSpan spanTime = endDate - startDate;
                var rents = orderApi.GetRentsByTimeRange(storeId, startDate.GetStartOfDate(), endDate.GetEndOfDate())
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                    item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                    item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                    item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;

                    if (item.TotalPrice != 0)
                    {
                        item.PersentTakeAway = (item.PriceTakeAway / item.TotalPrice) * 100;
                        item.PersentDelivery = (item.PriceDelivery / item.TotalPrice) * 100;
                        item.PersentAtStore = (item.PriceAtStore / item.TotalPrice) * 100;
                    }
                    else
                    {
                        item.PersentTakeAway = 0;
                        item.PersentDelivery = 0;
                        item.PersentAtStore = 0;
                    }
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }
            var list = hourReport.Select(a => new IConvertible[]
            {
                a.StartTime + ":00 - " + a.EndTime + ":00",

                a.TakeAway,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0.0} %", a.PersentTakeAway),

                a.AtStore,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0.0} %", a.PersentAtStore),

                a.Delivery,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0.0} %", a.PersentDelivery),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice)

            }).ToArray();

            List<string> header = new List<string>();
            header.Add("#;1;1");
            header.Add("Giờ;1;1");
            header.Add("Số lượng(Mang đi);1;1");
            header.Add("% trên tổng tiền;1;1");
            header.Add("Số lượng(Tại store);1;1");
            header.Add("% trên tổng tiề;1;1");
            header.Add("Số lượng(Giao hàng);1;1");
            header.Add("% trên tổng tiềb;1;1");
            header.Add("Tổng cộng;1;1");
            header.Add("Thành tiền;1;1");

            string fileName = "TổngQuanSảnPhẩm_Tháng";

            var success = ExportToExcelExtensions.ExportToExcel(header, list, fileName);

            return Json(new
            {
                success = success
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Product Date Progress
        public ActionResult StoreProductDateProgress()
        {
            //Get storeId, storeName form URL
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }
        public JsonResult LoadProductCategoriesProgress(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            //Create empty list product category
            List<GroupCategoryReportModalViewModel> fillterList = new List<GroupCategoryReportModalViewModel>();

            //Get category in DB
            var productCategoryApi = new ProductCategoryApi();
            var listCategory = productCategoryApi.GetProductCategories().Where(a => a.IsDisplayed && a.Type == 1);

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var dateProductApi = new DateProductApi();
            var dateProducts = dateProductApi.GetDateProductByTimeRange(startDate, endDate, storeId);
            var result =
            dateProducts.GroupBy(
                    r =>
                        new
                        {
                            r.ProductId,
                        }).Select(r => new
                        {
                            ProductId = r.Key.ProductId,
                        });

            foreach (var itemCat in listCategory)
            {
                var listProduct = itemCat.Products.Where(a => a.Active
                                                        && (string.IsNullOrEmpty(param.sSearch)
                                                        || a.ProductName.ToLower().Contains(param.sSearch.ToLower())));
                foreach (var itemP in listProduct)
                {
                    var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                    fillterList.Remove(productItem);

                    // phân loại category
                    fillterList.Add(new GroupCategoryReportModalViewModel
                    {
                        ProductId = itemP.ProductID,
                        CateName = itemP.ProductCategory.CateName,
                        ProductName = itemP.ProductName,
                    });

                }
                //categories.Add(new Tuple<string, int>(itemCat.CateName, categoryQuantity));
            }

            var pagedList = fillterList.Skip(param.iDisplayStart).Take(param.iDisplayLength);

            var list = fillterList.Select(a => new IConvertible[]
            {
                a.ProductName,
                a.CateName,
                a.Quantity,
                a.ProductId,
            }).ToArray();

            //return PartialView("_LoadRevenueReport", reportList.OrderBy(a => a.StartTime));
            //return Json(new
            //{
            //    datatable = list
            //}, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = fillterList.Count,
                iTotalDisplayRecords = fillterList.Count,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadDetailProductProgress(string startTime, string endTime, int storeId, int productId)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var dateProductApi = new DateProductApi();

            List<TempDetailProductProgressViewModel> listDetail = new List<TempDetailProductProgressViewModel>();
            List<string> listDate = new List<string>();
            List<int> listQuantity = new List<int>();
            for (var curDate = startDate; curDate <= endDate; curDate = curDate.AddDays(1))
            {

                var dateProducts = dateProductApi.GetDateProductByTimeRange(curDate.GetStartOfDate(), curDate.GetEndOfDate(), storeId)
                    .Where(w => w.ProductId == productId)
                    .GroupBy(g => g.ProductId)
                    .Select(sl => new
                    {
                        Quantity = sl.Sum(sm => sm.Quantity)
                    }).ToList();

                if (dateProducts.Count > 0)
                {
                    listDetail.Add(new TempDetailProductProgressViewModel
                    {
                        Date = curDate.ToString("dd/MM/yyyy"),
                        Quantity = dateProducts[0].Quantity
                    });

                    listQuantity.Add(dateProducts[0].Quantity);
                }
                else
                {
                    listDetail.Add(new TempDetailProductProgressViewModel
                    {
                        Date = curDate.ToString("dd/MM/yyyy"),
                        Quantity = 0
                    });

                    listQuantity.Add(0);
                }

                listDate.Add(curDate.ToString("dd/MM/yyyy"));
            }

            var finalList = listDetail.Select(a => new IConvertible[]
            {
                a.Date,
                a.Quantity
            }).ToArray();

            return Json(new
            {
                dataTable = finalList,
                dataChart = new
                {
                    xAxis = listDate,
                    yAxis = listQuantity
                }
            }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Product Month Progress
        public ActionResult StoreProductMonthProgress()
        {
            //Get storeId, storeName form URL
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }
        public JsonResult LoadDetailProductMonthProgress(string startTime, string endTime, int storeId, int productId)
        {
            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            var dateProductApi = new DateProductApi();

            List<TempDetailProductProgressViewModel> listDetail = new List<TempDetailProductProgressViewModel>();
            List<string> listDate = new List<string>();
            List<int> listQuantity = new List<int>();

            var limit = ("01" + "/" + endDate.Month.ToString().PadLeft(2, '0') + "/" + endDate.Year.ToString().PadLeft(2, '0')).ToDateTime();
            for (var curDate = startDate; curDate < limit; curDate = curDate.AddMonths(1))
            {
                var sDate = curDate;
                var eDate = new DateTime(curDate.Year, curDate.Month, DateTime.DaysInMonth(curDate.Year, curDate.Month));

                var dateProducts = dateProductApi.GetDateProductByTimeRange(sDate.GetStartOfDate(), eDate.GetEndOfDate(), storeId)
                    .Where(w => w.ProductId == productId)
                    .GroupBy(g => g.ProductId)
                    .Select(sl => new
                    {
                        Quantity = sl.Sum(sm => sm.Quantity)
                    }).ToList();

                if (dateProducts.Count > 0)
                {
                    listDetail.Add(new TempDetailProductProgressViewModel
                    {
                        Date = curDate.ToString("MM/yyyy"),
                        Quantity = dateProducts[0].Quantity
                    });

                    listQuantity.Add(dateProducts[0].Quantity);
                }
                else
                {
                    listDetail.Add(new TempDetailProductProgressViewModel
                    {
                        Date = curDate.ToString("MM/yyyy"),
                        Quantity = 0
                    });

                    listQuantity.Add(0);
                }

                listDate.Add(curDate.ToString("MM/yyyy"));
            }

            #region Last month in range
            var dateProductsLastMonth = dateProductApi.GetDateProductByTimeRange(limit.GetStartOfDate(), endDate.GetEndOfDate(), storeId)
                   .Where(w => w.ProductId == productId)
                   .GroupBy(g => g.ProductId)
                   .Select(sl => new
                   {
                       Quantity = sl.Sum(sm => sm.Quantity)
                   }).ToList();

            if (dateProductsLastMonth.Count > 0)
            {
                listDetail.Add(new TempDetailProductProgressViewModel
                {
                    Date = limit.ToString("MM/yyyy"),
                    Quantity = dateProductsLastMonth[0].Quantity
                });

                listQuantity.Add(dateProductsLastMonth[0].Quantity);
            }
            else
            {
                listDetail.Add(new TempDetailProductProgressViewModel
                {
                    Date = limit.ToString("MM/yyyy"),
                    Quantity = 0
                });

                listQuantity.Add(0);
            }

            listDate.Add(limit.ToString("MM/yyyy"));
            #endregion

            var finalList = listDetail.Select(a => new IConvertible[]
            {
                a.Date,
                a.Quantity
            }).ToArray();

            return Json(new
            {
                dataTable = finalList,
                dataChart = new
                {
                    xAxis = listDate,
                    yAxis = listQuantity
                }
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Selling Channel
        public ActionResult StoreSellingChannelReport()
        {
            //Get storeId, storeName form URL
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }

        public JsonResult GetSellingChannelReportDate(int brandId, string startTime, string endTime, int storeId)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            var orderApi = new OrderApi();

            var listDataAllChannel = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
                .GroupBy(g => g.SourceType)
                .Select(sl => new
                {
                    Channel = sl.Key.ToString(),
                    TotalOrder = sl.Count(),
                    TotalAmount = sl.Sum(s => s.TotalAmount),
                    TotalDiscount = sl.Sum(s => s.Discount) + sl.Sum(s => s.DiscountOrderDetail),
                    FinalAmount = sl.Sum(s => s.FinalAmount),
                });

            var returnList = listDataAllChannel.Select(sl => new IConvertible[] {
                sl.Channel,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", sl.TotalOrder),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", sl.TotalAmount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", sl.TotalDiscount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", sl.FinalAmount),
            }).ToList();

            var listChannel = Enum.GetValues(typeof(SourceTypeEnum));

            return Json(new
            {
                dataTable = returnList,
                dataOrderChart = new
                {
                    listChannel = listChannel,
                    listTotalOrder = listDataAllChannel.Select(s => s.TotalOrder),
                },
                dataAmountChart = new
                {
                    listChannel = listChannel,
                    listTotalAmount = listDataAllChannel.Select(s => s.TotalAmount),
                    listTotalDiscount = listDataAllChannel.Select(s => s.TotalDiscount),
                    listFinalAmount = listDataAllChannel.Select(s => s.FinalAmount),
                },
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion



        //public void exportExcel(List<string> headers, IEnumerable<object> _list, ref string fileName, ref bool success)
        //{
        //    FolderBrowserDialog folderDlg = new FolderBrowserDialog();
        //    //folderDlg.ShowNewFolderButton = true;
        //    string selectedPath = "";
        //    //Environment.SpecialFolder root = Environment.SpecialFolder.DesktopDirectory;

        //    DialogResult confirm = folderDlg.ShowDialog();
        //    if (confirm == DialogResult.OK)
        //    {

        //        Environment.SpecialFolder root = folderDlg.RootFolder;
        //        selectedPath = folderDlg.SelectedPath;
        //        if (!string.IsNullOrEmpty(selectedPath))
        //        {
        //            int length = selectedPath.Length;
        //            int temp = selectedPath.LastIndexOf("\\");
        //            if (selectedPath.LastIndexOf("\\") == length - 1)
        //            {

        //                fileName = selectedPath + fileName + ".xls";
        //            }
        //            else
        //            {
        //                fileName = selectedPath + "\\" + fileName + ".xls";
        //            }
        //            var result = ExportToExcelExtensions.ExportToExcel(headers, _list, fileName);
        //            if (result)
        //            {
        //                success = true;
        //            }
        //        }
        //    }
        //}

        [HttpPost]
        public async Task<JsonResult> ReCalculatorStoreReport(string startTime)
        {
            var storeApi = new StoreApi();
            var dateReportApi = new DateReportApi();
            var dateProductApi = new DateProductApi();
            var dateProductItemApi = new DateProductItemApi();
            var orderDetailApi = new OrderDetailApi();
            var orderApi = new OrderApi();
            var productApi = new ProductApi();
            var productItemApi = new ProductItemApi();
            var reportApi = new ReportAPI();
            var reportTrackingApi = new ReportTrackingApi();
            try
            {
                var startDate = DateTime.Parse(startTime);

                var id = RouteData.Values["storeId"].ToString();
                var storeId = Convert.ToInt32(id);

                var store = await storeApi.GetStoreByID(storeId);
                #region Delete Old Report
                //Delete DateReport 
                var oldDateReport = await dateReportApi.GetDateReportByDate(startDate.GetStartOfDate(), storeId);
                if (oldDateReport != null)
                {
                    await dateReportApi.DeleteDateReportAsync(oldDateReport.ID);
                }
                //Delete DateProduct
                var oldDateProduct = dateProductApi.GetDateProductByDateAndStore(startDate.GetStartOfDate(), storeId);
                if (oldDateProduct != null)
                {
                    foreach (var item in oldDateProduct.ToList())
                    {
                        await dateProductApi.DeleteDateProductAsync(item.ID);
                    }
                }
                //Delete DateProductItem
                var oldDateProductItem = dateProductItemApi.GetDateProductItemByDayAndStore(startDate.GetStartOfDate(), storeId);
                if (oldDateProductItem != null)
                {
                    foreach (var item in oldDateProductItem.ToList())
                    {
                        await dateProductItemApi.DeleteDateProductItemAsync(item.ID);
                    }
                }

                #endregion

                //Get orderDetail
                var orderDetails =
                   orderDetailApi.GetOrderDetailsByTimeRange(startDate.GetStartOfDate(), startDate.GetEndOfDate(), storeId)
                   .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                //Get rent
                var rents = orderApi.GetRentsByTimeRange(storeId, startDate.GetStartOfDate(), startDate.GetEndOfDate())
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                //Push DateProduct
                var dateProducts =
                orderDetails.GroupBy(a => a.ProductID)
                    .Join(productApi.GetAllProducts(), a => a.Key, a => a.ProductID, (a, b) => new DateProduct()
                    {
                        ProductId = a.Key,
                        StoreID = store.ID,
                        Quantity = a.Sum(c => c.Quantity),
                        Date = startDate.GetEndOfDate(),
                        TotalAmount = a.Sum(c => c.TotalAmount),
                        FinalAmount = a.Sum(c => c.FinalAmount),
                        Discount = a.Sum(c => c.Discount),
                        ProductName_ = b.ProductName,
                        Product = b,
                        CategoryId_ = b.ProductCategory.CateID
                    }).ToList();
                //Push DateReport
                var dateReport = new DateReport
                {
                    StoreID = store.ID,
                    CreateBy = "system",
                    Status = (int)DateReportStatusEnum.Approved,
                    Date = startDate.GetEndOfDate(),
                    Discount = rents.Sum(a => a.Discount),
                    DiscountOrderDetail = rents.Sum(a => a.DiscountOrderDetail),
                    TotalAmount = rents.Sum(a => a.TotalAmount),
                    FinalAmount = rents.Sum(a => a.FinalAmount),
                    TotalCash = 0,
                    TotalOrder = rents.Count(),
                    TotalOrderAtStore = rents.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore),
                    TotalOrderTakeAway = rents.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway),
                    TotalOrderDelivery = rents.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery),
                    TotalOrderDetail = 0,
                    TotalOrderFeeItem = 0
                };
                var compositionsStatistic = dateProducts.SelectMany(a => a.Product.ProductItemCompositionMappings.Select(b => new Tuple<ProductItemCompositionMapping, int>(b, a.Quantity)))
                .GroupBy(a => a.Item1.ItemID);

                //Push DateProductItem
                var dateItemProduct = compositionsStatistic.Join(productItemApi.GetProductItems(), a => a.Key, a => a.ItemID, (a, b) => new DateProductItem
                {
                    StoreId = store.ID,
                    Date = startDate.GetEndOfDate(),
                    ProductItemID = a.Key,
                    ProductItemName = b.ItemName,
                    Quantity = (int)a.Sum(c => c.Item2 * c.Item1.Quantity),
                    Unit = b.Unit
                }).AsQueryable();

                var result = reportApi.ReCreateReportDate(dateReport, orderDetails, dateItemProduct, store, rents);
                var dateTracking = await reportTrackingApi.GetReportTrackingByDateAndStoreId(startDate, storeId);
                if (dateTracking != null)
                {
                    dateTracking.DateUpdate = DateTime.Now;
                    dateTracking.IsUpdate = true;
                    dateTracking.UpdatePerson = "system"; //Tam thoi
                    await reportTrackingApi.EditAsync(dateTracking.Id, dateTracking);
                }

                return Json(new
                {
                    success = true,
                    message = "Báo cáo được chạy lại thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Báo cáo không được chạy lại. Lỗi do server"
                });
            }

        }
    }
}
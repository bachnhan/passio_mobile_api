using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Forms;
//using System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using HmsService.Models.Entities;
using Wisky.SkyAdmin.Manage.Controllers;
using System.IO;
using OfficeOpenXml;

namespace Wisky.SkyAdmin.Manage.Areas.PosReport.Controllers
{
    [Authorize(Roles = "BrandManager, Manager")]
    public class SystemReportController : DomainBasedController
    {
        // GET: PosReport/SystemReport
        public ActionResult Index()
        {
            return View();
        }

        #region Báo cáo doanh thu tháng
        public ActionResult RevenueReport()
        {
            return View();
        }

        public JsonResult LoadRevenueReport(JQueryDataTableParamModel param, string startTime, string endTime, int brandId)
        {
            var listDateReport = new List<TempSystemRevenueReportItem>();
            Stopwatch st = new Stopwatch();
            var storeApi = new StoreApi();
            var dateReportApi = new DateReportApi();

            //var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            var today = Utils.GetCurrentDateTime();
            try
            {
                if (startTime == "" || endTime == "")
                {
                    // 1. get ngay (ngày đầu tháng -> ngày hiện tại)
                    var dateNow = Utils.GetCurrentDateTime();
                    var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                    var tempStartDate = startDate;
                    var endDate = dateNow.GetEndOfDate();
                    // 2. lấy list store
                    var storeList = storeApi.GetStores(brandId)
                        .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch))
                        .OrderBy(a => a.Name).ToList();
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    for (var d = startDate; startDate <= endDate; d.AddDays(1))
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, startDate.GetEndOfDate(), store.ID).ToList();

                            var totalAmount = (double)dateReport.Sum(a => a.TotalAmount);
                            var finalAmount = (double)dateReport.Sum(a => a.FinalAmount);
                            var discountFee = (double)dateReport.Sum(a => a.Discount) + (double)dateReport.Sum(a => a.DiscountOrderDetail);

                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                        }
                        listDateReport.Add(new TempSystemRevenueReportItem()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            TotalAmount = totalDateAmount,
                            FinalAmount = finalDateAmount,
                            TotalDiscountFee = discountDateFee
                        });
                        startDate = startDate.AddDays(1);
                    }
                }
                else
                {
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    var tempStartDate = startDate;
                    // 2. lấy list store
                    var storeList = storeApi.GetStoresByBrandId(brandId)
                        .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch))
                        .OrderBy(a => a.Name);
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    st.Start();
                    for (var d = startDate; startDate <= endDate; d.AddDays(1))
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, startDate.GetEndOfDate(), store.ID).ToList();

                            var totalAmount = (double)dateReport.Sum(a => a.TotalAmount);
                            var finalAmount = (double)dateReport.Sum(a => a.FinalAmount);
                            var discountFee = (double)dateReport.Sum(a => a.Discount) + (double)dateReport.Sum(a => a.DiscountOrderDetail);

                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;

                        }
                        listDateReport.Add(new TempSystemRevenueReportItem()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            TotalAmount = totalDateAmount,
                            FinalAmount = finalDateAmount,
                            TotalDiscountFee = discountDateFee
                        });
                        startDate = startDate.AddDays(1);
                    }
                    st.Stop();
                }
                var list = listDateReport.Select(a => new IConvertible[]
                        {
                        a.StartTime,
                        string.Format(CultureInfo.InvariantCulture,
                           "{0:0,0}", a.TotalAmount),
                        string.Format(CultureInfo.InvariantCulture,
                           "{0:0,0}", a.TotalDiscountFee),
                        string.Format(CultureInfo.InvariantCulture,
                           "{0:0,0}", a.FinalAmount)
                        })/*.ToArray()*/;

                //var totalRecords = await customerStores.CountAsync();
                //var totalRecords = 10;

                //return Json(new
                //{
                //    sEcho = param.sEcho,
                //    iTotalRecords = totalRecords,
                //    iTotalDisplayRecords = list.Count(),
                //    aaData = list
                //}, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    dataList = list
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<dynamic> GetListRevenueReport(string startTime, string endTime, int brandId)
        {
            var listDateReport = new List<TempSystemRevenueReportItem>();
            var storeApi = new StoreApi();
            var dateReportApi = new DateReportApi();
            List<dynamic> list = new List<dynamic>();
            double AllTotalAmount = 0;
            double AllFinalAmount = 0;
            double AllDiscountFee = 0;
            int count = 0;
            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            if (startTime == "" || endTime == "")
            {
                // 1. get ngay (ngày đầu tháng -> ngày hiện tại)
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = new DateTime(dateNow.Year, dateNow.Month, 1);
                var tempStartDate = startDate;
                var endDate = dateNow.GetEndOfDate();
                // 2. lấy list store
                var storeList = storeApi.GetActiveStoreByBrandId(brandId)
                    .OrderBy(a => a.Name).ToList();
                // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    double totalDateAmount = 0;
                    double finalDateAmount = 0;
                    double discountDateFee = 0;
                    foreach (var store in storeList)
                    {
                        var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, startDate.GetEndOfDate(), store.ID).ToList();

                        var totalAmount = (double)dateReport.Sum(a => a.TotalAmount);
                        var finalAmount = (double)dateReport.Sum(a => a.FinalAmount);
                        var discountFee = (double)dateReport.Sum(a => a.Discount) + (double)dateReport.Sum(a => a.DiscountOrderDetail);

                        totalDateAmount += totalAmount;
                        finalDateAmount += finalAmount;
                        discountDateFee += discountFee;
                        AllTotalAmount += totalAmount;
                        AllFinalAmount += finalAmount;
                        AllDiscountFee += discountFee;
                    }
                    list.Add(new
                    {
                        No = ++count,
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        TotalAmount = totalDateAmount,
                        FinalAmount = finalDateAmount,
                        TotalDiscountFee = discountDateFee
                    });
                    startDate = startDate.AddDays(1);
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                // 1. get ngay (ngày đầu tháng -> ngày hiện tại)
                var dateNow = Utils.GetCurrentDateTime();
                var tempStartDate = startDate;
                // 2. lấy list store
                var storeList = storeApi.GetActiveStoreByBrandId(brandId)
                    .OrderBy(a => a.Name);
                // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    double totalDateAmount = 0;
                    double finalDateAmount = 0;
                    double discountDateFee = 0;
                    foreach (var store in storeList)
                    {
                        var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, startDate.GetEndOfDate(), store.ID).ToList();

                        var totalAmount = (double)dateReport.Sum(a => a.TotalAmount);
                        var finalAmount = (double)dateReport.Sum(a => a.FinalAmount);
                        var discountFee = (double)dateReport.Sum(a => a.Discount) + (double)dateReport.Sum(a => a.DiscountOrderDetail);

                        totalDateAmount += totalAmount;
                        finalDateAmount += finalAmount;
                        discountDateFee += discountFee;
                        AllTotalAmount += totalAmount;
                        AllFinalAmount += finalAmount;
                        AllDiscountFee += discountFee;
                    }
                    list.Add(new
                    {
                        No = ++count,
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        TotalAmount = totalDateAmount,
                        FinalAmount = finalDateAmount,
                        TotalDiscountFee = discountDateFee
                    });
                    startDate = startDate.AddDays(1);
                }
            }
            list.Add(new
            {
                No = "",
                StartTime = "Tổng tất cả các ngày",
                TotalAmount = AllTotalAmount,
                FinalAmount = AllFinalAmount,
                TotalDiscountFee = AllDiscountFee,
            });
            return list;
        }

        public ActionResult ExportExcelRevenueReport(string startTime, string endTime, int brandId)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var list = GetListRevenueReport(startTime, endTime, brandId);
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng doanh thu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Doanh thu sau giảm giá";
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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StartTime;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalAmount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalDiscountFee;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.FinalAmount;
                    StartHeaderChar = 'A';
                }
                //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = "";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalAmount).ToString();
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalDiscountFee).ToString();
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => q.FinalAmount).ToString();
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Doanh thu từ " + startTime + " đến " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }


        #endregion

        #region Báo cáo theo giờ
        public JsonResult LoadHourReportComparison(string startTime, string endTime, int? Time, int brandId)
        {
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var listStores = storeApi.GetStoresByBrandIdAndType(brandId, 5);

            List<DateProductViewModel> listDateProductByCateId = new List<DateProductViewModel>();
            List<string> listStoreName = new List<string>();
            List<double> listFinalAmount = new List<double>();
            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.Date.GetEndOfDate();

                var orders = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish && a.CheckinHour == Time);

                var result = orders.GroupBy(a => a.Store).Select(b => new
                {
                    StoreId = b.Key.ID,
                    FinalAmount = b.Sum(c => c.FinalAmount)
                });

                foreach (var store in listStores)
                {
                    var storeName = store.Name;
                    listStoreName.Add(storeName);
                    var finalAmount = result.Where(a => a.StoreId == store.ID);
                    listFinalAmount.Add(finalAmount.Sum(a => a.FinalAmount));
                }

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                var rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                     .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish && a.CheckinHour == Time);


                var result = rents.GroupBy(a => a.Store).Select(b => new
                {
                    StoreId = b.Key.ID,
                    FinalAmount = b.Sum(c => c.FinalAmount)
                });

                foreach (var store in listStores)
                {
                    var storeName = store.Name;
                    listStoreName.Add(storeName);
                    var finalAmount = result.Where(a => a.StoreId == store.ID).ToList();
                    listFinalAmount.Add(finalAmount.Sum(a => a.FinalAmount));
                }
            }

            return Json(new
            {
                dataChart = new
                {
                    StoreName = listStoreName,
                    FinalAmount = listFinalAmount
                }
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportHourAllStoreToExcel(string startTime, string endTime, int brandId)
        {
            #region Get Detail
            var orderApi = new OrderApi();
            var hourReport = new List<HourReportModel>();
            List<dynamic> list = new List<dynamic>();
            int count = 0;
            for (int i = 6; i < 24; i++)
            {
                hourReport.Add(new HourReportModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)

                });
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.Date.GetEndOfDate();

                var orders = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
                        .Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });

                var result = orders.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
                });


                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    list.Add(new
                    {
                        No = ++count,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder,
                        PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money,
                        AtStore = (atStore == null) ? 0 : atStore.TotalOrder,
                        PriceAtStore = (atStore == null) ? 0 : atStore.Money,
                        Delivery = (delivery == null) ? 0 : delivery.TotalOrder,
                        PriceDelivery = (delivery == null) ? 0 : delivery.Money,
                        TotalQuantity = ((takeAway == null) ? 0 : takeAway.TotalOrder) + ((atStore == null) ? 0 : atStore.TotalOrder) + ((delivery == null) ? 0 : delivery.TotalOrder),
                        TotalPrice = ((takeAway == null) ? 0 : takeAway.Money) + ((atStore == null) ? 0 : atStore.Money) + ((delivery == null) ? 0 : delivery.Money),
                    });
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                var rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });
                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
                });

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    list.Add(new
                    {
                        No = ++count,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder,
                        PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money,
                        AtStore = (atStore == null) ? 0 : atStore.TotalOrder,
                        PriceAtStore = (atStore == null) ? 0 : atStore.Money,
                        Delivery = (delivery == null) ? 0 : delivery.TotalOrder,
                        PriceDelivery = (delivery == null) ? 0 : delivery.Money,
                        TotalQuantity = ((takeAway == null) ? 0 : takeAway.TotalOrder) + ((atStore == null) ? 0 : atStore.TotalOrder) + ((delivery == null) ? 0 : delivery.TotalOrder),
                        TotalPrice = ((takeAway == null) ? 0 : takeAway.Money) + ((atStore == null) ? 0 : atStore.Money) + ((delivery == null) ? 0 : delivery.Money),
                    });
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }
            #endregion

            #region Export Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khoảng thời gian";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng (mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng (tại cửa hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng (giao hàng)";
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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StartTime + " - " + data.EndTime;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PriceTakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.AtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PriceAtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Delivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PriceDelivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalQuantity;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.TotalPrice;
                    StartHeaderChar = 'A';
                }
                //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = list.Count + 2;
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalAmount));
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalDiscountFee));
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.FinalAmount));
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo theo giờ.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion

        }
        public ActionResult ExportHourOneStoreToExcel(string startTime, string endTime, int storeIdReport, int brandId)
        {
            var orderApi = new OrderApi();
            var hourReport = new List<HourReportModel>();
            List<dynamic> list = new List<dynamic>();
            var count = 0;
            for (int i = 6; i < 24; i++)
            {
                hourReport.Add(new HourReportModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)

                });
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.Date.GetEndOfDate();
                var orders = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });


                var result = orders.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
                });


                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    list.Add(new
                    {
                        No = ++count,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder,
                        PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money,
                        AtStore = (atStore == null) ? 0 : atStore.TotalOrder,
                        PriceAtStore = (atStore == null) ? 0 : atStore.Money,
                        Delivery = (delivery == null) ? 0 : delivery.TotalOrder,
                        PriceDelivery = (delivery == null) ? 0 : delivery.Money,
                        TotalQuantity = ((takeAway == null) ? 0 : takeAway.TotalOrder) + ((atStore == null) ? 0 : atStore.TotalOrder) + ((delivery == null) ? 0 : delivery.TotalOrder),
                        TotalPrice = ((takeAway == null) ? 0 : takeAway.Money) + ((atStore == null) ? 0 : atStore.Money) + ((delivery == null) ? 0 : delivery.Money),
                    });
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                var orders = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });
                var result = orders.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
                });

                foreach (var item in hourReport)
                {
                    var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                    var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                    var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                    list.Add(new
                    {
                        No = ++count,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder,
                        PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money,
                        AtStore = (atStore == null) ? 0 : atStore.TotalOrder,
                        PriceAtStore = (atStore == null) ? 0 : atStore.Money,
                        Delivery = (delivery == null) ? 0 : delivery.TotalOrder,
                        PriceDelivery = (delivery == null) ? 0 : delivery.Money,
                        TotalQuantity = ((takeAway == null) ? 0 : takeAway.TotalOrder) + ((atStore == null) ? 0 : atStore.TotalOrder) + ((delivery == null) ? 0 : delivery.TotalOrder),
                        TotalPrice = ((takeAway == null) ? 0 : takeAway.Money) + ((atStore == null) ? 0 : atStore.Money) + ((delivery == null) ? 0 : delivery.Money),
                    });
                }
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }

            #region Export Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khoảng thời gian";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng (mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng (tại cửa hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thành tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng (giao hàng)";
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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StartTime + " - " + data.EndTime;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PriceTakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.AtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PriceAtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Delivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PriceDelivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalQuantity;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.TotalPrice;
                    StartHeaderChar = 'A';
                }
                //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = list.Count + 2;
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalAmount));
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalDiscountFee));
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.FinalAmount));
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo theo giờ_ID:"+ storeIdReport +".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion
        }
        public ActionResult IndexHourReport()
        {
            return View();
        }

        public ActionResult HourReportAllStore(JQueryDataTableParamModel param, string startTime, string endTime, int brandId)
        {

            var hourReport = new List<HourReportModel>();
            var orderApi = new OrderApi();
            for (int i = 6; i < 24; i++)
            {
                hourReport.Add(new HourReportModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)
                });
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();

                //var startDate = dateNow.AddDays(-1).GetStartOfDate();
                //var endDate = dateNow.AddDays(-1).Date.GetEndOfDate();

                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.Date.GetEndOfDate();
                //Chuyển từ rent sang order
                var orders = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
                        .Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });

                var result = orders.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
                });


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
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                var orders = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });
                var result = orders.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
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
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }
            int count = 6;
            var list = hourReport.Select(a => new IConvertible[]
                    {
                        count++,
                        a.StartTime+":00 - "+a.EndTime+":00",
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
                           "{0:0,0}", a.TotalPrice),
                        a.StartTime

                    }).ToArray();
            var _time = hourReport.Select(a => a.StartTime + ":00 - " + a.EndTime + ":00").ToArray();
            var _takeAway = hourReport.Select(a => a.TakeAway).ToArray();
            var _atStore = hourReport.Select(a => a.AtStore).ToArray();
            var _delivery = hourReport.Select(a => a.Delivery).ToArray();


            return Json(new
            {
                datatable = list,
                chartdata = new
                {
                    Time = _time,
                    TakeAway = _takeAway,
                    AtStore = _atStore,
                    Delivery = _delivery
                }
            }, JsonRequestBehavior.AllowGet);
            //return PartialView("_HourReport", hourReport);
        }

        public ActionResult HourReportOneStore(JQueryDataTableParamModel param, string startTime, string endTime, int storeIdReport, int brandId)
        {
            var hourReport = new List<HourReportModel>();
            var orderApi = new OrderApi();
            for (int i = 6; i < 24; i++)
            {
                hourReport.Add(new HourReportModel()
                {
                    StartTime = i,
                    EndTime = (i + 1)

                });
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();

                //var startDate = dateNow.AddDays(-1).GetStartOfDate();
                //var endDate = dateNow.AddDays(-1).GetEndOfDate();

                var startDate = dateNow.GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();
                var orders = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });


                var result = orders.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
                });


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
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                var orders = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).Select(q => new
                        {
                            q.RentID,
                            q.OrderType,
                            q.CheckinHour,
                            q.TotalAmount,
                        });
                var result = orders.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.TotalAmount)
                });

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
                ViewBag.Start = startDate.ToString("dd-MM-yyyy");
                ViewBag.End = endDate.ToString("dd-MM-yyyy");
            }
            int count = 6;
            var list = hourReport.Select(a => new IConvertible[]
                    {
                        count++,
                        a.StartTime+":00 - "+a.EndTime+":00",
                        a.TakeAway,
                        string.Format(CultureInfo.InvariantCulture,
                           "{0:0,0}", a.PriceTakeAway),
                        a.AtStore,
                        string.Format(CultureInfo.InvariantCulture,
                           "{0:0,0}", a.PriceAtStore),
                        a.Delivery,
                        string.Format(CultureInfo.InvariantCulture,
                           "{0:0,0}", a.Delivery),
                        a.TotalQuantity,
                        string.Format(CultureInfo.InvariantCulture,
                           "{0:0,0}", a.TotalPrice)

                    }).ToArray();
            var _time = hourReport.Select(a => a.StartTime + ":00 - " + a.EndTime + ":00").ToArray();
            var _takeAway = hourReport.Select(a => a.TakeAway).ToArray();
            var _atStore = hourReport.Select(a => a.AtStore).ToArray();
            var _delivery = hourReport.Select(a => a.Delivery).ToArray();


            return Json(new
            {
                datatable = list,
                chartdata = new
                {
                    Time = _time,
                    TakeAway = _takeAway,
                    AtStore = _atStore,
                    Delivery = _delivery
                }
            }, JsonRequestBehavior.AllowGet);
            //return PartialView("_HourReport", hourReport);
        }
        #endregion

        public ActionResult CategoryReport()
        {
            return View();
        }

        public async Task<JsonResult> LoadCategories(int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            var categories = await productCategoryApi.GetByBrandIdAsync(brandId);
            return Json(categories);
        }

        public async Task<JsonResult> LoadStores(int brandId)
        {
            var storeApi = new StoreApi();
            var stores = await storeApi.GetActiveStoreByBrandId(brandId).ToListAsync();
            return Json(stores);
        }

        
        public ActionResult IndexCategory(int brandId)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var categoryApi = new ProductCategoryApi();
            var listCategory = categoryApi.GetProductCategories(brandId).Where(q => q.IsDisplayed);
            var storeApi = new StoreApi();
            var listStore = storeApi.GetAllStore(brandId);
            ViewBag.Store = listStore.ToList();
            return View(listCategory);

        }

        public JsonResult CategoryReportAllStore(string startTime, string endTime, int catetoryId, int brandId)
        {
            var categories = new List<Tuple<string, int, int, int, string, string>>();
            var productCategoryApi = new ProductCategoryApi();
            var dateProductApi = new DateProductApi();

            IEnumerable<ProductCategoryViewModel> listCategory = catetoryId == 0 ? productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.BrandId == brandId).ToList() :
                productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.CateID == catetoryId && a.BrandId == brandId).ToList();

            var totalProduct = 0;
            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");

            if (startTime == today && endTime == today)
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();


                var dateProducts = dateProductApi.GetDateProductAllStoreByTimeRange(startDate, endDate);

                var result = dateProducts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.FinalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    TotalBill = 1,
                }).ToList();

                //Total Amount
                var finalAmount = dateProducts.ToList().Sum(a => a.FinalAmount);

                //Total Bill
                //decimal totalBill = (decimal)order.Sum(a => a.OrderQuantity);
                decimal totalBill = 1;

                foreach (var itemCate in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.ToTal);
                    var totalDiscount = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.TotalDiscount);
                    var totalBillCate = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.TotalBill);
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
                        categories.Add(new Tuple<string, int, int, int, string, string>(itemCate.CateName, quantity, (int)total, (int)totalDiscount, rateOrder, rateRevenue));
                    totalProduct += quantity;
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                var dateProducts = dateProductApi.GetDateProductAllStoreByTimeRange(startDate, endDate);

                var result = dateProducts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.FinalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    TotalBill = r.Sum(a => a.OrderQuantity),
                }).ToList();

                //Total Amount
                var finalAmount = dateProducts.Sum(a => a.FinalAmount);

                //Total Bill
                decimal totalBill = (decimal)dateProducts.Sum(a => a.OrderQuantity);

                foreach (var itemCate in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.ToTal);
                    var totalDiscount = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.TotalDiscount);
                    var totalBillCate = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.TotalBill);
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
                        categories.Add(new Tuple<string, int, int, int, string, string>(itemCate.CateName, quantity, (int)total, (int)totalDiscount, rateOrder, rateRevenue));
                    totalProduct += quantity;
                }
            }

            int count = 0;
            var listProduct = categories.Select(a => new IConvertible[]
                {
                    ++count,
                    a.Item1,
                    string.Format(CultureInfo.InvariantCulture, "{0:0,0}", @a.Item2),
                    string.Format(CultureInfo.InvariantCulture, "{0:0,0}", @a.Item3),
                    string.Format(CultureInfo.InvariantCulture, "{0:0,0}", @a.Item4),
                    a.Item5,
                    a.Item6,
                });

            return Json(new
            {
                dataTable = listProduct,
                chartData = new
                {
                    listCategories = categories,
                    totalProduct = totalProduct
                }
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ExportCategoryAllStoreToExcel(string startTime, string endTime, int categoryId, int brandId)
        {
            #region GetData
            //var brandId = 1;
            var productCategoryApi = new ProductCategoryApi();
            var dateProduct = new DateProductApi();
            List<dynamic> list = new List<dynamic>();
            int count = 0;
            //Create List category report
            var categories = new List<Tuple<string, int, int>>();

            //Get category in DB
            //var listCategory = productCategory.GetProductCategories().Where(q => q.IsDisplayed && q.CateID == catetoryId).ToList();
            IEnumerable<ProductCategoryViewModel> listCategory = categoryId == 0 ? productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed).ToList() :
                productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.CateID == categoryId).ToList();

            var totalProduct = 0;
            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");

            if (startTime == today && endTime == today)
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                var dateProducts = dateProduct.GetDateProductAllStoreByTimeRange(startDate, endDate);
                var result = dateProducts.GroupBy(r => new { r.ProductId }).Select(r => new
                {
                    CategoryId = r.Key.ProductId,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.TotalAmount)
                }).ToList();
                foreach (var itemCate in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.ToTal);
                    list.Add(new 
                    {
                        No = ++count,
                        CateName = itemCate.CateName,
                        Quantity = quantity,
                        Total = (int)total,
                    });
                    totalProduct += quantity;
                }
            }
            else
            {

                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                var dateProducts = dateProduct.GetDateProductAllStoreByTimeRange(startDate, endDate);

                var result = dateProducts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.TotalAmount)
                }).ToList();
                foreach (var itemCate in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCate.CateID).Sum(b => b.ToTal);
                    list.Add(new
                    {
                        No = ++count,
                        CateName = itemCate.CateName,
                        Quantity = quantity,
                        Total = (int)total,
                    });
                    totalProduct += quantity;
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
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên danh mục";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng sản phẩm";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng doanh thu";
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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CateName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.Total;
                    StartHeaderChar = 'A';
                }
                //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = list.Count + 2;
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalAmount));
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalDiscountFee));
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.FinalAmount));
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo theo ngành hàng.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion
        }

        public async System.Threading.Tasks.Task<ActionResult> ExportCategoryOneStoreToExcel(string startTime, string endTime, int storeIdReport, int categoryId, int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            var dateProductApi = new DateProductApi();
            List<dynamic> list = new List<dynamic>();
            int count = 0;
            var storeApi = new StoreApi();
            //Create List category report
            var categories = new List<Tuple<string, int, int>>();

            //Get list category in DB
            var listCategory = categoryId == 0 ? productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed).ToList() :
                productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.CateID == categoryId).ToList();

            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            //Check startTime and EndTime input
            if (startTime == today && endTime == today)
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                //Get order with startDate and StoreId
                var order = dateProductApi.GetDateProductByDateAndStore(startDate, (int)storeIdReport);

                //Group order
                var result = order.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.TotalAmount)
                }).ToList();

                foreach (var itemCat in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.ToTal);

                    list.Add(new
                    {
                        No = ++count,
                        CateName = itemCat.CateName,
                        Quantity = quantity,
                        Total = (int)total,
                    });
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                //Get DateProduct
                var dateProdcuts = dateProductApi.GetDateProductByTimeRange(startDate, endDate, storeIdReport);

                //Group dateProdcuts
                var result = dateProdcuts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.TotalAmount)
                }).ToList();

                foreach (var itemCat in listCategory)
                {
                    var quantity = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.Quantity);
                    var total = result.Where(a => a.CategoryId == itemCat.CateID).Sum(b => b.ToTal);
                    list.Add(new
                    {
                        No = ++count,
                        CateName = itemCat.CateName,
                        Quantity = quantity,
                        Total = (int)total,
                    });
                }
            }


            var listProduct = categories.Select(a => new
            {
                a = a.Item1,
                b = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.Item2),
                c = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.Item3)
            });
            var storeName = "TatCa";
            if(storeIdReport != 0)
            {
                storeName = storeApi.GetStoreNameByID(storeIdReport);
            }
            var categoryName = (categoryId == 0 ? "TấtCảNgànhHàng" : (await productCategoryApi.GetProductCategoryById(categoryId)).CateName);
            // (await productCategoryApi.GetProductCategoryById(catetoryId)).CateName)
            var sTime = startTime.ToDateTime().ToString("dd-MM-yyyy");
            var eTime = endTime.ToDateTime().ToString("dd-MM-yyyy");
            var dateRange = "(" + sTime + (sTime == eTime ? "" : " - " + eTime) + ")";
            string fileName = "BáoCáoTheoNgànhHàng_" + categoryName + "_" + storeName + "_HệThống" + dateRange;

            #region Export Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên danh mục";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng sản phẩm";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng doanh thu";
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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CateName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.Total;
                    StartHeaderChar = 'A';
                }
                //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = list.Count + 2;
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalAmount));
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.TotalDiscountFee));
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => Convert.ToDouble(q.FinalAmount));
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = fileName + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion
        }

        public ActionResult CategoryReportOneStore(string startTime, string endTime, int storeIdReport, int catetoryId, int brandId)
        {
            //Create List category report
            var categories = new List<Tuple<string, int, int, int, string, string>>();
            var productCategoryApi = new ProductCategoryApi();
            var dateProductApi = new DateProductApi();
            var totalProduct = 0;
            //Get list category in DB
            var listCategory = catetoryId == 0 ? productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.BrandId == brandId).ToList() :
                productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.CateID == catetoryId && a.BrandId == brandId).ToList();

            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            //Check startTime and EndTime input
            if (startTime == today && endTime == today)
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                //Get order with startDate and StoreId
                var order = dateProductApi.GetDateProductByDateAndStore(startDate, (int)storeIdReport);

                //Group order
                var result = order.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.TotalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    TotalBill = r.Sum(a => a.OrderQuantity)
                }).ToList();

                //Total Amount
                var finalAmount = order.Sum(a => a.FinalAmount);

                //Total Bill
                decimal totalBill = (decimal)order.Sum(a => a.OrderQuantity);

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
                //Get DateProduct
                var dateProdcuts = dateProductApi.GetDateProductByTimeRange(startDate, endDate, storeIdReport);

                //Group dateProdcuts
                var result = dateProdcuts.GroupBy(r => new { r.Product.ProductCategory.CateID }).Select(r => new
                {
                    CategoryId = r.Key.CateID,
                    Quantity = r.Sum(a => a.Quantity),
                    ToTal = r.Sum(a => a.TotalAmount),
                    TotalDiscount = r.Sum(a => a.Discount),
                    TotalBill = r.Sum(a => a.OrderQuantity)
                }).ToList();

                //Total Amount
                var finalAmount = dateProdcuts.Sum(a => a.FinalAmount);

                //Total Bill
                decimal totalBill = (decimal)dateProdcuts.Sum(a => a.OrderQuantity);

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

            int count = 0;
            var listProduct = categories.Select(a => new IConvertible[]
                {
                    ++count,
                    a.Item1,
                    string.Format(CultureInfo.InvariantCulture, "{0:0,0}", @a.Item2),
                    string.Format(CultureInfo.InvariantCulture, "{0:0,0}", @a.Item3),
                    string.Format(CultureInfo.InvariantCulture, "{0:0,0}", @a.Item4),
                    a.Item5,
                    a.Item6,
                });

            return Json(new
            {
                dataTable = listProduct,
                chartData = new
                {
                    listCategories = categories,
                    totalProduct = totalProduct
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadComparisonStoreReport(string startTime, string endTime, string cateName, int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            var dateProductApi = new DateProductApi();
            var storeApi = new StoreApi();
            var orderDetailApi = new OrderDetailApi();

            var category = productCategoryApi.GetProductCategories(brandId).Where(a => a.CateName == cateName).ToList()[0];
            var listStores = storeApi.GetStores(brandId).Where(x => x.isAvailable == true && x.Type == 5);

            List<DateProduct> listDateProductByCateId = new List<DateProduct>();
            List<OrderDetail> listOrderDetailByCateId = new List<OrderDetail>();
            List<string> listStoreName = new List<string>();
            List<double> listFinalAmount = new List<double>();
            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");

            if (startTime == today && endTime == today)
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                var order = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate);
                listOrderDetailByCateId = order.Where(w => w.ProductID == category.CateID).ToList();
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                listDateProductByCateId = dateProductApi.GetDateProductAllStoreByTimeRange(startDate, endDate)
                    .Where(w => w.CategoryId_ == category.CateID).ToList();
            }

            if (listDateProductByCateId.Count > 0)
            {
                foreach (var store in listStores)
                {
                    var storeID = store.ID;
                    listStoreName.Add(store.Name);

                    var dateProductsByStoreID = listDateProductByCateId.Where(w => w.StoreID == storeID)
                        .GroupBy(g => g.StoreID)
                        .Select(sl => new
                        {
                            FinalAmount = sl.Sum(x => x.FinalAmount)
                        }).ToList();

                    if (dateProductsByStoreID.Count > 0)
                    {
                        listFinalAmount.Add(dateProductsByStoreID[0].FinalAmount);
                    }
                    else
                    {
                        listFinalAmount.Add(0);
                    }
                }
            }
            else
            {
                foreach (var store in listStores)
                {
                    var storeID = store.ID;
                    listStoreName.Add(store.Name);

                    var dateProductsByStoreID = listOrderDetailByCateId.Where(w => w.StoreId == storeID)
                        .GroupBy(g => g.StoreId)
                        .Select(sl => new
                        {
                            FinalAmount = sl.Sum(x => x.FinalAmount)
                        }).ToList();

                    if (dateProductsByStoreID.Count > 0)
                    {
                        listFinalAmount.Add(dateProductsByStoreID[0].FinalAmount);
                    }
                    else
                    {
                        listFinalAmount.Add(0);
                    }
                }
            }

            Debug.WriteLine("Final amount: " + listFinalAmount.Sum(s => Convert.ToInt32(s)));

            return Json(new
            {
                dataChart = new
                {
                    xAxis = listStoreName,
                    yAxis = listFinalAmount
                }
            }, JsonRequestBehavior.AllowGet);
        }

        #region Báo cáo theo ngày
        public JsonResult RevenueReportDatatable(JQueryDataTableParamModel param, int brandId, string startTime, string endTime)
        {

            var StartTime = startTime.ToDateTime();
            var EndTime = endTime.ToDateTime();

            //Create list temp repot
            var reportList = new List<TempStoreReportItem>();

            //Get list all store
            var storeApi = new StoreApi();
            var storeList = storeApi.GetStores(brandId)
                .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch))
                .OrderBy(a => a.Name);

            var totalStore = storeList.Count();

            double total = 0;
            double totalFinalAmount = 0;
            double totalDiscountFee = 0;

            //List for comparison chart (So sánh doanh thu)
            List<string> listStoreName = new List<string>();
            List<double> listTotal = new List<double>();
            List<double> listDiscount = new List<double>();
            List<double> listFinal = new List<double>();

            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");

            //Check startTime and endTime input
            if (startTime != today || endTime != today)
            {

                var sTime = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, 23, 59, 59); ;
                var eTime = new DateTime(EndTime.Year, EndTime.Month, EndTime.Day, 0, 0, 0);

                var dateReportApi = new DateReportApi();
                var dateReportList =
                    dateReportApi.GetDateReportTimeRange(sTime, eTime).GroupBy(a => a.StoreID);

                var dateReportIncomeList = dateReportApi.GetDateReportTimeRange(sTime, eTime).OrderBy(a => a.Date).ToList();

                List<string> dateList = new List<string>();
                for (DateTime d = sTime; d.CompareTo(eTime) < 0; d = d.AddDays(1))
                {
                    dateList.Add(d.ToString("dd/MM/yyyy"));
                }

                List<double> listTotalAmount = new List<double>();
                List<double> listFinalAmount = new List<double>();
                foreach (var item in dateList)
                {
                    //var store = storeList.FirstOrDefault(a => a.ID == item.StoreID);
                    var incomeListByDate = dateReportIncomeList.FindAll(a => a.Date.ToString("dd/MM/yyyy").Substring(0, 10) == item);
                    listTotalAmount.Add((double)incomeListByDate.Sum(a => a.TotalAmount));
                    listFinalAmount.Add((double)incomeListByDate.Sum(a => a.FinalAmount));
                }


                //add store income to list
                foreach (var item in storeList)
                {
                    var store = dateReportList.FirstOrDefault(a => a.Key == item.ID);

                    var totalAmount = (double)(store == null ? 0 : store.Sum(a => a.TotalAmount));

                    var finalAmount = (double)(store == null ? 0 : store.Sum(a => a.FinalAmount));

                    var discountFee = (double)(store == null ? 0 : (store.Sum(a => a.Discount)) + store.Sum(a => a.DiscountOrderDetail));

                    var temp = new TempStoreReportItem
                    {
                        Store = item.ToEntity(),
                        TotalAmount = totalAmount,
                        FinalAmount = finalAmount,
                        Discount = discountFee
                    };
                    total += totalAmount;
                    totalFinalAmount += finalAmount;
                    totalDiscountFee += discountFee;
                    reportList.Add(temp);

                    listStoreName.Add(item.Name);
                    listTotal.Add(totalAmount);
                    listDiscount.Add(discountFee);
                    listFinal.Add(finalAmount);
                }


            }
            else
            {
                //var sTime = Utils.GetCurrentDateTime().GetStartOfDate();
                //var eTime = Utils.GetCurrentDateTime().GetEndOfDate();
                var sTime = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, 23, 59, 59); ;
                var eTime = new DateTime(EndTime.Year, EndTime.Month, EndTime.Day, 0, 0, 0);

                var rentApi = new OrderApi();
                var rents = rentApi.GetAllOrderByDate(sTime, eTime, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).GroupBy(o => o.StoreID);

                foreach (var item in storeList)
                {
                    var store = rents.FirstOrDefault(a => a.Key == item.ID);

                    var totalAmount = (double)(store == null ? 0 : store.Sum(a => a.TotalAmount));

                    var finalAmount = (double)(store == null ? 0 : store.Sum(a => a.FinalAmount));

                    var discountFee = (double)(store == null ? 0 : (store.Sum(a => a.Discount)) + store.Sum(a => a.DiscountOrderDetail));

                    var temp = new TempStoreReportItem
                    {
                        Store = item.ToEntity(),
                        TotalAmount = totalAmount,
                        FinalAmount = finalAmount,
                        Discount = discountFee
                    };
                    total += totalAmount;
                    totalFinalAmount += finalAmount;
                    totalDiscountFee += discountFee;
                    reportList.Add(temp);

                    listStoreName.Add(item.Name);
                    listTotal.Add(totalAmount);
                    listDiscount.Add(discountFee);
                    listFinal.Add(finalAmount);
                }

            }


            int count = param.iDisplayStart;
            var listProduct = reportList.Select(a => new IConvertible[]
                {
                    ++count,
                    a.Store.ShortName,
                    string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalAmount),
                    string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                    string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalAmount)
                })/*.ToList()*/;

            return Json(new
            {
                dataComparisonChart = new
                {
                    listStoreName = listStoreName,
                    listTotal = listTotal,
                    listDiscount = listDiscount,
                    listFinal = listFinal
                },
                dataTable = new
                {
                    listProduct = listProduct
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public List<dynamic> ExportRevenueTableToExcel(string startTime, string endTime, int brandId)
        {
            List<dynamic> listDt = new List<dynamic>();
            var count = 0;

            var storeApi = new StoreApi();
            var dateReportApi = new DateReportApi();
            var orderApi = new OrderApi();

            //Create list temp repot
            var reportList = new List<TempStoreReportItem>();

            //Get list all store
            var storeList = storeApi.GetActiveStoreByBrandId(brandId)
                .OrderBy(a => a.Name).ToList();

            var totalStore = storeList.Count();

            double total = 0;
            double totalFinalAmount = 0;
            double totalDiscountFee = 0;

            //List for comparison chart (So sánh doanh thu)
            List<string> listStoreName = new List<string>();
            List<double> listTotal = new List<double>();
            List<double> listDiscount = new List<double>();
            List<double> listFinal = new List<double>();

            var today = DateTime.Now.ToString("dd/MM/yyyy");
            //Check startTime and endTime input
            if (startTime != today || endTime != today)
            {
                var sTime = DateTime.Parse(startTime).GetStartOfDate();
                var eTime = DateTime.Parse(endTime).GetEndOfDate();

                var dateReportList =
                    dateReportApi.GetDateReportTimeRange(sTime, eTime).GroupBy(a => a.StoreID);

                var dateReportIncomeList = dateReportApi.GetDateReportTimeRange(sTime, eTime).OrderBy(a => a.Date).ToList();

                List<string> dateList = new List<string>();
                for (DateTime d = sTime; d.CompareTo(eTime) < 0; d = d.AddDays(1))
                {
                    dateList.Add(d.ToString("dd/MM/yyyy"));
                }

                List<double> listTotalAmount = new List<double>();
                List<double> listFinalAmount = new List<double>();
                foreach (var item in dateList)
                {
                    //var store = storeList.FirstOrDefault(a => a.ID == item.StoreID);
                    var incomeListByDate = dateReportIncomeList.FindAll(a => a.Date.ToString("dd/MM/yyyy").Substring(0, 10) == item);
                    listTotalAmount.Add((double)incomeListByDate.Sum(a => a.TotalAmount));
                    listFinalAmount.Add((double)incomeListByDate.Sum(a => a.FinalAmount));
                }


                //add store income to list
                foreach (var item in storeList)
                {
                    var store = dateReportList.FirstOrDefault(a => a.Key == item.ID);

                    var totalAmount = (double)(store == null ? 0 : store.Sum(a => a.TotalAmount));

                    var finalAmount = (double)(store == null ? 0 : store.Sum(a => a.FinalAmount));

                    var discountFee = (double)(store == null ? 0 : (store.Sum(a => a.Discount)) + store.Sum(a => a.DiscountOrderDetail));

                    //var temp = new TempStoreReportItem
                    //{
                    //    Store = item,
                    //    TotalAmount = totalAmount,
                    //    FinalAmount = finalAmount,
                    //    Discount = discountFee
                    //};
                    total += totalAmount;
                    totalFinalAmount += finalAmount;
                    totalDiscountFee += discountFee;
                    //reportList.Add(temp);

                    listStoreName.Add(item.Name);
                    listTotal.Add(totalAmount);
                    listDiscount.Add(discountFee);
                    listFinal.Add(finalAmount);

                    listDt.Add(new
                    {
                        No = count++,
                        Store = item.Name,
                        TotalAmount = totalAmount,
                        FinalAmount = finalAmount,
                        Discount = discountFee,
                    });
                }
            }
            else
            {
                //var eTime = DateTime.Now.GetEndOfDate();
                var sTime = DateTime.Parse(startTime).GetStartOfDate();
                var eTime = DateTime.Parse(endTime).GetEndOfDate();

                var rents = orderApi.GetAllRentByDate(sTime, eTime)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).GroupBy(o => o.StoreID);

                foreach (var item in storeList)
                {
                    var store = rents.FirstOrDefault(a => a.Key == item.ID);

                    var totalAmount = (double)(store == null ? 0 : store.Sum(a => a.TotalAmount));

                    var finalAmount = (double)(store == null ? 0 : store.Sum(a => a.FinalAmount));

                    var discountFee = (double)(store == null ? 0 : (store.Sum(a => a.Discount)) + store.Sum(a => a.DiscountOrderDetail));

                    //var temp = new TempStoreReportItem
                    //{
                    //    Store = item,
                    //    TotalAmount = totalAmount,
                    //    FinalAmount = finalAmount,
                    //    Discount = discountFee
                    //};
                    total += totalAmount;
                    totalFinalAmount += finalAmount;
                    totalDiscountFee += discountFee;
                    //reportList.Add(temp);

                    listStoreName.Add(item.Name);
                    listTotal.Add(totalAmount);
                    listDiscount.Add(discountFee);
                    listFinal.Add(finalAmount);

                    listDt.Add(new
                    {
                        No = count++,
                        Store = item.Name,
                        TotalAmount = totalAmount,
                        FinalAmount = finalAmount,
                        Discount = discountFee,
                    });
                }
            }

            return listDt;
        }

        private void exportExcel(List<string> headers, IEnumerable<object> _list, ref string fileName, ref bool success)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            string selectedPath = "";
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
                    var result = Utils.ExportToExcel(headers, _list, fileName);
                    if (result)
                    {
                        success = true;
                    }
                }
            }
        }

        public ActionResult ExportExcelBaoCaoTheoNgay(string startTime, string endTime, int brandId)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo theo ngày");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = ExportRevenueTableToExcel(startTime, endTime, brandId);
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "C?A HÀNG";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "T?NG DOANH THU";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "GI?M GIÁ";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "DOANH THU SAU GI?M GIÁ";
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
                    // STT	TÊN NGUYÊN Li?U	ÐV	T. Ð?U	NH?P	Xu?T	BÁN	T?N LT	T?N TT	(+/-)
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Store;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalAmount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Discount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.FinalAmount;
                    StartHeaderChar = 'A';
                }
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion

        #region Báo cáo theo thứ
        public ActionResult IndexDayOfWeekReport(string storeid)
        {
            var storeApi = new StoreApi();
            var store = storeApi.GetActive().Where(q => q.Type == 5);
            return this.View(store);
        }
        public JsonResult LoadDayOfWeekReportAllStore(JQueryDataTableParamModel param, string startTime, string endTime, int brandId)
        {
            var orderApi = new OrderApi();
            var dayofweekReport = new List<TempDayOfWeekReportModel>();

            int i;
            for (i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Monday,
                        DayOfWeek = "Thứ hai"
                    });
                }
                else if (i == 1)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Tuesday,
                        DayOfWeek = "Thứ ba"
                    });
                }
                else if (i == 2)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Wednesday,
                        DayOfWeek = "Thứ tư"
                    });
                }
                else if (i == 3)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Thursday,
                        DayOfWeek = "Thứ năm"
                    });
                }
                else if (i == 4)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Friday,
                        DayOfWeek = "Thứ sáu"
                    });
                }
                else if (i == 5)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Saturday,
                        DayOfWeek = "Thứ bảy"
                    });
                }
                else if (i == 6)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Sunday,
                        DayOfWeek = "Chủ Nhật"
                    });
                }
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.AddDays(1 - (int)dateNow.DayOfWeek).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                var rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

                //IEnumerable<OrderViewModel> rents;

                var rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

            var list = dayofweekReport.Select(a => new IConvertible[]
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
                        "{0:0,0}", a.TotalPrice),
                a.DayOfWeek

            }).ToArray();

            var _WeekDay = dayofweekReport.Select(a => a.DayOfWeek).ToArray();
            var _TakeAway = dayofweekReport.Select(a => a.TakeAway).ToArray();
            var _AtStore = dayofweekReport.Select(a => a.AtStore).ToArray();
            var _Delivery = dayofweekReport.Select(a => a.Delivery).ToArray();

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

        public JsonResult LoadDayOfWeekReportOneStore(JQueryDataTableParamModel param, string startTime, string endTime, int storeIdReport, int brandId)
        {
            var orderApi = new OrderApi();
            var dayofweekReport = new List<TempDayOfWeekReportModel>();

            int i;
            for (i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Monday,
                        DayOfWeek = "Thứ hai"
                    });
                }
                else if (i == 1)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Tuesday,
                        DayOfWeek = "Thứ ba"
                    });
                }
                else if (i == 2)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Wednesday,
                        DayOfWeek = "Thứ tư"
                    });
                }
                else if (i == 3)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Thursday,
                        DayOfWeek = "Thứ năm"
                    });
                }
                else if (i == 4)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Friday,
                        DayOfWeek = "Thứ sáu"
                    });
                }
                else if (i == 5)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Saturday,
                        DayOfWeek = "Thứ bảy"
                    });
                }
                else if (i == 6)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Sunday,
                        DayOfWeek = "Chủ Nhật"
                    });
                }
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.AddDays(1 - (int)dateNow.DayOfWeek).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

            var list = dayofweekReport.Select(a => new IConvertible[]
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
            }).ToArray();

            var _WeekDay = dayofweekReport.Select(a => a.DayOfWeek).ToArray();
            var _TakeAway = dayofweekReport.Select(a => a.TakeAway).ToArray();
            var _AtStore = dayofweekReport.Select(a => a.AtStore).ToArray();
            var _Delivery = dayofweekReport.Select(a => a.Delivery).ToArray();

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

        public JsonResult ExportDayOfWeekAllStoreTableToExcel(JQueryDataTableParamModel param, string startTime, string endTime, int brandId)
        {
            var orderApi = new OrderApi();
            var dayofweekReport = new List<TempDayOfWeekReportModel>();

            int i;
            for (i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Monday,
                        DayOfWeek = "Thứ hai"
                    });
                }
                else if (i == 1)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Tuesday,
                        DayOfWeek = "Thứ ba"
                    });
                }
                else if (i == 2)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Wednesday,
                        DayOfWeek = "Thứ tư"
                    });
                }
                else if (i == 3)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Thursday,
                        DayOfWeek = "Thứ năm"
                    });
                }
                else if (i == 4)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Friday,
                        DayOfWeek = "Thứ sáu"
                    });
                }
                else if (i == 5)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Saturday,
                        DayOfWeek = "Thứ bảy"
                    });
                }
                else if (i == 6)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Sunday,
                        DayOfWeek = "Chủ Nhật"
                    });
                }
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.AddDays(1 - (int)dateNow.DayOfWeek).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

            var list = dayofweekReport.Select(a => new
            {
                a = a.DayOfWeek,
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

            }).ToArray();

            List<string> header = new List<string>();
            header.Add("Thứ;1;1");
            header.Add("Số lượng(Mang đi);1;1");
            header.Add("Thành tiền;1;1");
            header.Add("Số lượng(Tại store);1;1");
            header.Add("Thành tiền;1;1");
            header.Add("Số lượng(Giao hàng);1;1");
            header.Add("Thành tiền;1;1");
            header.Add("Tổng cộng;1;1");
            header.Add("Thành tiền;1;1");

            string fileName = "Báo cáo theo thứ";
            bool success = false;
            Thread thdSyncRead = new Thread(new ThreadStart(() => exportExcel(header, list, ref fileName, ref success)));
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

        public JsonResult ExportDayOfWeekOneStoreTableToExcel(JQueryDataTableParamModel param, string startTime, string endTime, int storeIdReport, int brandId)
        {
            var orderApi = new OrderApi();
            var dayofweekReport = new List<TempDayOfWeekReportModel>();

            int i;
            for (i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Monday,
                        DayOfWeek = "Thứ hai"
                    });
                }
                else if (i == 1)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Tuesday,
                        DayOfWeek = "Thứ ba"
                    });
                }
                else if (i == 2)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Wednesday,
                        DayOfWeek = "Thứ tư"
                    });
                }
                else if (i == 3)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Thursday,
                        DayOfWeek = "Thứ năm"
                    });
                }
                else if (i == 4)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Friday,
                        DayOfWeek = "Thứ sáu"
                    });
                }
                else if (i == 5)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Saturday,
                        DayOfWeek = "Thứ bảy"
                    });
                }
                else if (i == 6)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Sunday,
                        DayOfWeek = "Chủ Nhật"
                    });
                }
            }

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.AddDays(1 - (int)dateNow.DayOfWeek).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetRentsByTimeRange(storeIdReport, startDate, endDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in dayofweekReport)
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

            var list = dayofweekReport.Select(a => new
            {
                a = a.DayOfWeek,
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

            }).ToArray();

            List<string> header = new List<string>();
            header.Add("Thứ;1;1");
            header.Add("Số lượng(Mang đi);1;1");
            header.Add("Thành tiền;1;1");
            header.Add("Số lượng(Tại store);1;1");
            header.Add("Thành tiền;1;1");
            header.Add("Số lượng(Giao hàng);1;1");
            header.Add("Thành tiền;1;1");
            header.Add("Tổng cộng;1;1");
            header.Add("Thành tiền;1;1");

            string fileName = "Báo cáo theo thứ";

            var success = HmsService.Models.ExportToExcelExtensions.ExportToExcel(header, list, fileName);

            return Json(new
            {
                success = success
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDayOfweekReportComparison(string startTime, string endTime, string Time, int brandId)
        {
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var dayofweekReport = new List<TempDayOfWeekReportModel>();

            int i;
            for (i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Monday,
                        DayOfWeek = "Thứ hai"
                    });
                }
                else if (i == 1)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Tuesday,
                        DayOfWeek = "Thứ ba"
                    });
                }
                else if (i == 2)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Wednesday,
                        DayOfWeek = "Thứ tư"
                    });
                }
                else if (i == 3)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Thursday,
                        DayOfWeek = "Thứ năm"
                    });
                }
                else if (i == 4)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Friday,
                        DayOfWeek = "Thứ sáu"
                    });
                }
                else if (i == 5)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Saturday,
                        DayOfWeek = "Thứ bảy"
                    });
                }
                else if (i == 6)
                {
                    dayofweekReport.Add(new TempDayOfWeekReportModel()
                    {
                        Day = DayOfWeek.Sunday,
                        DayOfWeek = "Chủ Nhật"
                    });
                }
            }

            var listStores = storeApi.GetActive().Where(x => x.isAvailable == true && x.Type == 5);

            List<DateProductViewModel> listDateProductByCateId = new List<DateProductViewModel>();
            List<string> listStoreName = new List<string>();
            List<double> listTakeAway = new List<double>();
            List<double> listAtStore = new List<double>();
            List<double> listDelivery = new List<double>();

            if (startTime == "" && endTime == "")
            {
                var dateNow = Utils.GetCurrentDateTime();
                var startDate = dateNow.AddDays(1 - (int)dateNow.DayOfWeek).GetStartOfDate();
                var endDate = dateNow.GetEndOfDate();

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate, r.StoreID }).Select(r => new
                {
                    StoreId = r.Key.StoreID,
                    OrderTime = r.Key.Time,
                    OrderType = r.Key.OrderType,
                    TotalOrder = r.Count(),
                    TotalFinal = r.Sum(a => a.FinalAmount)
                }).ToList();



                /*var rents = _rentService.GetAllRentByDate(startDate, endDate)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish && a.CheckinHour == Time).ToList();
                var result = rents.GroupBy(a => a.Store).Select(b => new
                {
                    StoreId = b.Key.ID,
                    FinalAmount = b.Sum(c => c.FinalAmount)
                });*/

                foreach (var store in listStores)
                {
                    var storeName = store.Name;
                    listStoreName.Add(storeName);

                    foreach (var item in dayofweekReport)
                    {
                        if (item.DayOfWeek == Time)
                        {
                            var takeAway = result.Where(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime.Value.DayOfWeek == item.Day && r.StoreId == store.ID);
                            listTakeAway.Add((takeAway == null) ? 0 : takeAway.Sum(a => a.TotalFinal));

                            var atStore = result.Where(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime.Value.DayOfWeek == item.Day && r.StoreId == store.ID);
                            listAtStore.Add((atStore == null) ? 0 : atStore.Sum(a => a.TotalFinal));

                            var delivery = result.Where(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime.Value.DayOfWeek == item.Day && r.StoreId == store.ID);
                            listDelivery.Add((delivery == null) ? 0 : delivery.Sum(a => a.TotalFinal));
                        }
                    }
                }

            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                IEnumerable<OrderViewModel> rents;

                rents = orderApi.GetAllOrderByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate, r.StoreID }).Select(r => new
                {
                    StoreId = r.Key.StoreID,
                    OrderTime = r.Key.Time,
                    OrderType = r.Key.OrderType,
                    TotalOrder = r.Count(),
                    TotalFinal = r.Sum(a => a.FinalAmount)
                }).ToList();



                /*var rents = _rentService.GetAllRentByDate(startDate, endDate)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish && a.CheckinHour == Time).ToList();
                var result = rents.GroupBy(a => a.Store).Select(b => new
                {
                    StoreId = b.Key.ID,
                    FinalAmount = b.Sum(c => c.FinalAmount)
                });*/


                foreach (var store in listStores)
                {
                    var storeName = store.Name;
                    listStoreName.Add(storeName);

                    foreach (var item in dayofweekReport)
                    {
                        if (item.DayOfWeek == Time)
                        {
                            var takeAway = result.Where(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime.Value.DayOfWeek == item.Day && r.StoreId == store.ID);
                            listTakeAway.Add((takeAway == null) ? 0 : takeAway.Sum(a => a.TotalFinal));

                            var atStore = result.Where(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime.Value.DayOfWeek == item.Day && r.StoreId == store.ID);
                            listAtStore.Add((atStore == null) ? 0 : atStore.Sum(a => a.TotalFinal));

                            var delivery = result.Where(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime.Value.DayOfWeek == item.Day && r.StoreId == store.ID);
                            listDelivery.Add((delivery == null) ? 0 : delivery.Sum(a => a.TotalFinal));
                        }
                    }
                }

            }

            return Json(new
            {
                conparisonChart = new
                {
                    StoreName = listStoreName,
                    TakeAway = listTakeAway,
                    AtStore = listAtStore,
                    Delivery = listDelivery
                }
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost]
        public async Task<JsonResult> LoadAllStore(int? brandId)
        {
            var storeApi = new StoreApi();

            //get stores 
            var storeList = await storeApi.GetAllStore(brandId.Value)
                                               .Select(a => new
                                               {
                                                   StoreId = a.ID,
                                                   Name = a.Name
                                               }).ToListAsync();

            return Json(storeList);
        }

        public ActionResult IndexProductCategoryReport(string storeid, int? brandId)
        {
            var storeApi = new StoreApi();
            var store = storeApi.GetStoresByBrandId(brandId).Where(a => a.isAvailable == true);
            return View(store);
        }
        public JsonResult LoadProductCategoryComparisonReport(int? brandId, string startTime, string endTime, string cateName)
        {
            #region new version
            var storeApi = new StoreApi();
            var orderDetailApi = new OrderDetailApi();
            ////var category = _productCategoryService.GetProductCategories().Where(a => a.CateName == cateName).ToList()[0];
            var listStores = storeApi.GetStoresByBrandId(brandId).Where(x => x.isAvailable == true);

            List<DateProduct> listDateProductByCateId = new List<DateProduct>();
            List<string> listStoreName = new List<string>();
            List<double> listFinalAmount = new List<double>();
            Int32 productId = Int32.Parse(cateName);

            var startDate = startTime.ToDateTime();
            startDate = startDate.GetStartOfDate();

            //var endDate = endTime.ToDateTime().GetEndOfDate();
            var endDate = endTime.ToDateTime();
            endDate = endDate.GetEndOfDate();
            var orderDetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate).ToList();


            var order = orderDetail.Select(s => new
            {
                ProductId = s.ProductID,
                StoreId = s.StoreId,
                FinalAmount = s.FinalAmount,
            }).Where(w => w.ProductId == productId);

            var result = order.GroupBy(r => new { r.StoreId }).Select(r => new
            {
                StoreId = r.Key.StoreId,
                FinalAmount = r.Sum(a => a.FinalAmount)
            }).ToList();

            foreach (var store in listStores)
            {
                var storeName = store.Name;
                listStoreName.Add(storeName);
                var finalAmount = result.Where(a => a.StoreId == store.ID);
                listFinalAmount.Add(finalAmount.Sum(a => a.FinalAmount));
            }

            Debug.Write("Hung Sum: " + listFinalAmount.Sum(s => Convert.ToInt32(s)));

            return Json(new
            {
                dataChart = new
                {
                    StoreName = listStoreName,
                    FinalAmount = listFinalAmount
                }
            }, JsonRequestBehavior.AllowGet);
            #endregion


        }

        public ActionResult ProductCategoryReportOneStore(int? brandId, string startTime, string endTime, int storeIdReport, int? startHour, int? endHour)
        {
            //Create empty list product category
            List<GroupCategoryReportModal> fillterList = new List<GroupCategoryReportModal>();
            List<TempProductByCategory> listProductByCategory = new List<TempProductByCategory>();
            List<decimal> listPercentQuantityChart = new List<decimal>();
            List<string> listProductNameChart = new List<string>();

            var productCategoryApi = new ProductCategoryApi();
            var orderDetailApi = new OrderDetailApi();
            //Get category in DB
            var listCategory = productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.Type == 1);

            //IEnumerable<OrderDetail> dateProducts;

            if (!startHour.HasValue && !endHour.HasValue)
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                //var endDate = endTime.ToDateTime().GetEndOfDate();
                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();

                var dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeIdReport).ToList();
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

                // Total quantity
                var finalTotalQuantity = result.Sum(s => s.Quantity);

                if (finalTotalQuantity > 0)
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var percentQuantity = totalQuantity;
                            var percentQuantity = (Convert.ToDecimal(totalQuantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            fillterList.Add(new GroupCategoryReportModal
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
                else
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            fillterList.Add(new GroupCategoryReportModal
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
            }
            else
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                //var endDate = endTime.ToDateTime().GetEndOfDate();
                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();

                for (DateTime i = startDate; startDate <= endDate; startDate = startDate.AddDays(1))
                {
                    var sTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startHour.Value, 0, 0);
                    var eTime = new DateTime();
                    if (endHour == 24)
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
                    }
                    else
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, endHour.Value, 0, 0);
                    }

                    var dateProducts = orderDetailApi.GetOrderDetailsByTimeRange(sTime, eTime, storeIdReport).ToList();
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
                            });

                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);

                        foreach (var itemP in listProduct)
                        {
                            //var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            //fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            if (productItem != null)
                            {
                                productItem.TotalPrice = productItem.TotalPrice + totalPrice;
                                productItem.Quantity = productItem.Quantity + totalQuantity;
                            }
                            else
                            {
                                fillterList.Add(new GroupCategoryReportModal
                                {
                                    ProductId = itemP.ProductID,
                                    CateName = itemP.ProductCategory.CateName,
                                    ProductName = itemP.ProductName,
                                    Quantity = totalQuantity,
                                    TotalPrice = totalPrice,
                                });
                            }
                        }
                    }
                }

                var finalTotalQuantity = fillterList.Sum(s => s.Quantity);
                if (finalTotalQuantity > 0)
                {
                    var listProductName = fillterList.Select(s => s.ProductName).ToList();
                    var listQuantity = fillterList.Select(s => s.Quantity).ToList();

                    for (int i = 0; i < listProductName.Count(); i++)
                    {
                        var productName = listProductName[i];
                        var quantity = listQuantity[i];
                        //var percentQuantity = listQuantity[i];
                        var percentQuantity = (Convert.ToDecimal(quantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                        if (percentQuantity > 0)
                        {
                            listProductNameChart.Add(productName);
                            listPercentQuantityChart.Add(percentQuantity);
                        }
                    }
                }
            }

            var list = fillterList.Select(a => new IConvertible[]
            {
                a.ProductName,
                a.CateName,
                string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.Quantity),
                string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.TotalPrice),
                a.ProductId,
            }).ToList();

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


        }

        public ActionResult ProductCategoryReportAllStore(int? brandId, string startTime, string endTime, int? startHour, int? endHour)
        {
            //Create empty list product category
            List<GroupCategoryReportModal> fillterList = new List<GroupCategoryReportModal>();
            List<decimal> listPercentQuantityChart = new List<decimal>();
            List<string> listProductNameChart = new List<string>();

            var productCategoryApi = new ProductCategoryApi();
            var orderDetailApi = new OrderDetailApi();

            //Get category in DB
            var listCategory = productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.Type == 1);

            if (!startHour.HasValue && !endHour.HasValue)
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();


                var dateProducts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)/*.ToList()*/;
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

                // Total quantity
                var finalTotalQuantity = result.Sum(s => s.Quantity);

                if (finalTotalQuantity > 0)
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var percentQuantity = totalQuantity;
                            var percentQuantity = (Convert.ToDecimal(totalQuantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            fillterList.Add(new GroupCategoryReportModal
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
                else
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            fillterList.Add(new GroupCategoryReportModal
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
            }
            else
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                //var endDate = endTime.ToDateTime().GetEndOfDate();
                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();

                for (DateTime i = startDate; startDate <= endDate; startDate = startDate.AddDays(1))
                {
                    var sTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startHour.Value, 0, 0);
                    var eTime = new DateTime();
                    if (endHour == 24)
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
                    }
                    else
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, endHour.Value, 0, 0);
                    }

                    var dateProducts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, sTime, eTime)/*.ToList()*/;

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

                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable.Value);

                        foreach (var itemP in listProduct)
                        {
                            //var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            //fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Select(a => a.Quantity).DefaultIfEmpty(0).Sum();
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Select(a => a.ToTal).DefaultIfEmpty(0).Sum();
                            //var totalPrice = (int)sum;

                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            if (productItem != null)
                            {
                                productItem.TotalPrice = productItem.TotalPrice + totalPrice;
                                productItem.Quantity = productItem.Quantity + totalQuantity;
                            }
                            else
                            {
                                fillterList.Add(new GroupCategoryReportModal
                                {
                                    ProductId = itemP.ProductID,
                                    CateName = itemP.ProductCategory.CateName,
                                    ProductName = itemP.ProductName,
                                    Quantity = totalQuantity,
                                    TotalPrice = totalPrice,
                                });
                            }
                        }
                    }
                }

                var finalTotalQuantity = fillterList.Sum(s => s.Quantity);
                if (finalTotalQuantity > 0)
                {
                    var listProductName = fillterList.Select(s => s.ProductName).ToList();
                    var listQuantity = fillterList.Select(s => s.Quantity).ToList();
                    for (int i = 0; i < listProductName.Count(); i++)
                    {
                        var productName = listProductName[i];
                        var quantity = listQuantity[i];
                        //var percentQuantity = quantity;
                        var percentQuantity = (Convert.ToDecimal(quantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                        if (percentQuantity > 0)
                        {
                            listProductNameChart.Add(productName);
                            listPercentQuantityChart.Add(percentQuantity);
                        }
                    }
                }
            }

            var list = fillterList.Select(a => new IConvertible[]
            {
                a.ProductName,
                a.CateName,
                string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.Quantity),
                string.Format(CultureInfo.InvariantCulture, "{0:0,0}", a.TotalPrice),
                a.ProductId,
            }).ToList();

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
        }



        public ActionResult ExportProductCategoryAllStoreToExcel(string startTime, string endTime, int? startHour, int? endHour, int? brandId)
        {
            #region Get Data
            //Create empty list product category
            List<GroupCategoryReportModal> fillterList = new List<GroupCategoryReportModal>();
            List<TempProductByCategory> listProductByCategory = new List<TempProductByCategory>();
            List<decimal> listPercentQuantityChart = new List<decimal>();
            List<string> listProductNameChart = new List<string>();
            List<dynamic> list = new List<dynamic>();
            var count = 0;
            var productCategoryApi = new ProductCategoryApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            //Get category in DB
            var listCategory = productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.Type == 1);

            if (!startHour.HasValue && !endHour.HasValue)
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();

                var dateProducts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate).ToList();

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

                // Total quantity
                var finalTotalQuantity = result.Sum(s => s.Quantity);

                if (finalTotalQuantity > 0)
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var percentQuantity = totalQuantity;
                            var percentQuantity = (Convert.ToDecimal(totalQuantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            list.Add(new 
                            {
                                No = ++count,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });
                        }
                    }
                }
                else
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            list.Add(new
                            {
                                No = ++count,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }
                }
            }
            else
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();

                for (DateTime i = startDate; startDate <= endDate; startDate = startDate.AddDays(1))
                {
                    var sTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startHour.Value, 0, 0);
                    var eTime = new DateTime();
                    if (endHour == 24)
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
                    }
                    else
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, endHour.Value, 0, 0);
                    }

                    var dateProducts = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, sTime, eTime).ToList();

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
                            });

                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);

                        foreach (var itemP in listProduct)
                        {
                            //var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            //fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            if (productItem != null)
                            {
                                productItem.TotalPrice = productItem.TotalPrice + totalPrice;
                                productItem.Quantity = productItem.Quantity + totalQuantity;
                            }
                            else
                            {
                                list.Add(new
                                {
                                    No = ++count,
                                    CateName = itemP.ProductCategory.CateName,
                                    ProductName = itemP.ProductName,
                                    Quantity = totalQuantity,
                                    TotalPrice = totalPrice
                                });
                            }
                        }
                    }
                }

                var finalTotalQuantity = fillterList.Sum(s => s.Quantity);
                if (finalTotalQuantity > 0)
                {
                    var listProductName = fillterList.Select(s => s.ProductName).ToList();
                    var listQuantity = fillterList.Select(s => s.Quantity).ToList();
                    for (int i = 0; i < listProductName.Count(); i++)
                    {
                        var productName = listProductName[i];
                        var quantity = listQuantity[i];
                        //var percentQuantity = quantity;
                        var percentQuantity = (Convert.ToDecimal(quantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                        if (percentQuantity > 0)
                        {
                            listProductNameChart.Add(productName);
                            listPercentQuantityChart.Add(percentQuantity);
                        }
                    }
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
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại sản phẩm";
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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ProductName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CateName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.TotalPrice;
                    StartHeaderChar = 'A';
                }
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo sản phẩm từ " + startTime + " đến " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion
        }

        public ActionResult ExportProductCategoryOneStoreToExcel(string startTime, string endTime, int storeIdReport,
            int? startHour, int? endHour, int? brandId)
        {
            #region Get Data
            //Create empty list product category
            List<GroupCategoryReportModal> fillterList = new List<GroupCategoryReportModal>();
            List<TempProductByCategory> listProductByCategory = new List<TempProductByCategory>();
            List<decimal> listPercentQuantityChart = new List<decimal>();
            List<string> listProductNameChart = new List<string>();
            List<dynamic> list = new List<dynamic>();
            var count = 0;
            var productCategoryApi = new ProductCategoryApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            //Get category in DB
            var listCategory = productCategoryApi.GetProductCategories(brandId).Where(a => a.IsDisplayed && a.Type == 1);

            //IEnumerable<OrderDetail> dateProducts;

            if (!startHour.HasValue && !endHour.HasValue)
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();

                var dateProducts =
                    orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeIdReport).ToList();

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

                // Total quantity
                var finalTotalQuantity = result.Sum(s => s.Quantity);

                if (finalTotalQuantity > 0)
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var percentQuantity = totalQuantity;
                            var percentQuantity = (Convert.ToDecimal(totalQuantity) /
                                                   Convert.ToDecimal(finalTotalQuantity) * 100);
                            if (percentQuantity > 0)
                            {
                                listProductNameChart.Add(itemP.ProductName);
                                listPercentQuantityChart.Add(percentQuantity);
                            }
                            //var totalPrice = (int)sum;

                            list.Add(new
                            {
                                No = ++count,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }
                }
                else
                {
                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);
                        //List<string> listProductChart = new List<string>();
                        //List<int> listQuantityChart = new List<int>();

                        foreach (var itemP in listProduct)
                        {
                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            list.Add(new
                            {
                                No = ++count,
                                CateName = itemP.ProductCategory.CateName,
                                ProductName = itemP.ProductName,
                                Quantity = totalQuantity,
                                TotalPrice = totalPrice
                            });

                        }
                    }
                }
            }
            else
            {
                var startDate = startTime.ToDateTime();
                startDate = startDate.GetStartOfDate();

                var endDate = endTime.ToDateTime();
                endDate = endDate.GetEndOfDate();

                for (DateTime i = startDate; startDate <= endDate; startDate = startDate.AddDays(1))
                {
                    var sTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startHour.Value, 0, 0);
                    var eTime = new DateTime();
                    if (endHour == 24)
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
                    }
                    else
                    {
                        eTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, endHour.Value, 0, 0);
                    }

                    var dateProducts =
                        orderDetailApi.GetOrderDetailsByTimeRange(sTime, eTime, storeIdReport).ToList();

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
                                });

                    foreach (var itemCat in listCategory)
                    {
                        var listProduct = itemCat.Products.Where(a => a.IsAvailable == true);

                        foreach (var itemP in listProduct)
                        {
                            //var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            //fillterList.Remove(productItem);

                            var totalQuantity = result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.Quantity);
                            var totalPrice = (int)result.Where(a => a.ProductId == itemP.ProductID).Sum(b => b.ToTal);
                            //var totalPrice = (int)sum;

                            var productItem = fillterList.FirstOrDefault(a => a.ProductId == itemP.ProductID);
                            if (productItem != null)
                            {
                                productItem.TotalPrice = productItem.TotalPrice + totalPrice;
                                productItem.Quantity = productItem.Quantity + totalQuantity;
                            }
                            else
                            {
                                list.Add(new
                                {
                                    No = ++count,
                                    CateName = itemP.ProductCategory.CateName,
                                    ProductName = itemP.ProductName,
                                    Quantity = totalQuantity,
                                    TotalPrice = totalPrice
                                });
                            }
                        }
                    }
                }

                var finalTotalQuantity = fillterList.Sum(s => s.Quantity);
                if (finalTotalQuantity > 0)
                {
                    var listProductName = fillterList.Select(s => s.ProductName).ToList();
                    var listQuantity = fillterList.Select(s => s.Quantity).ToList();

                    for (int i = 0; i < listProductName.Count(); i++)
                    {
                        var productName = listProductName[i];
                        var quantity = listQuantity[i];
                        //var percentQuantity = listQuantity[i];
                        var percentQuantity = (Convert.ToDecimal(quantity) / Convert.ToDecimal(finalTotalQuantity) * 100);
                        if (percentQuantity > 0)
                        {
                            listProductNameChart.Add(productName);
                            listPercentQuantityChart.Add(percentQuantity);
                        }
                    }
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
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại sản phẩm";
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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ProductName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CateName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.TotalPrice;
                    StartHeaderChar = 'A';
                }
                #endregion
                var storeName = storeApi.GetStoreNameByID(storeIdReport);
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = "Báo cáo sản phẩm " + startTime + " đến " + endTime + "_" + storeName + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion
        }

        public class TempStoreReportItem
        {
            public Store Store { get; set; }
            public double TotalAmount { get; set; }
            public double FinalAmount { get; set; }
            public double Discount { get; set; }
        }

        public class TempSystemRevenueReportItem
        {
            public string StartTime { get; set; }
            public double TotalAmount { get; set; }
            public double FinalAmount { get; set; }
            public double TotalDiscountFee { get; set; }
        }
        public class TempDayOfWeekReportModel
        {
            public System.DayOfWeek Day { get; set; }
            public string DayOfWeek { get; set; }
            public double TakeAway { get; set; }
            public double PriceTakeAway { get; set; }
            public double AtStore { get; set; }
            public double PriceAtStore { get; set; }
            public double Delivery { get; set; }
            public double PriceDelivery { get; set; }
            public double TotalQuantity { get; set; }
            public double TotalPrice { get; set; }
        }

    }
}
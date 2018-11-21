
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using Newtonsoft.Json;
using SkyWeb.DatVM.Mvc.Autofac;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Wisky.SkyAdmin.Manage.Controllers;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;


namespace Wisky.SkyAdmin.Manage.Areas.DashBoard.Controllers
{
    public class MonthDashBoardController : DomainBasedController
    {
        // GET: DashBoard/MonthDashBoard       
        public ActionResult Index()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }//End Index

        public ActionResult DashBoardMonthReport(int? storeId)
        {
            ViewBag.storeId = storeId.Value;
            return PartialView("_DashBoardMonthReport");
        }

        public JsonResult DateData(int storeId, int brandId, string _startDate, string _endDate)
        {
            var orderService = this.Service<IOrderService>();
            var dateReportService = this.Service<IDateReportService>();
            var orderApi = new OrderApi();
            var dateReportApi = new DateReportApi();
            var orderDetailApi = new OrderDetailApi();

            //DateTime startDate = DateTime.Parse(_startDate);
            //DateTime endDate = DateTime.Parse(_endDate);
            //var fromDate = startDate.GetStartOfDate();
            //var toDate = endDate.GetEndOfDate();
            DateTime startDate, endDate;

            if (!_startDate.IsNullOrWhiteSpace())
            {
                startDate = _startDate.ToDateTime();
                startDate = startDate.GetStartOfDate();
            }
            else
            {
                startDate = DateTime.Now.GetStartOfDate();
            }
            if (!_endDate.IsNullOrWhiteSpace())
            {
                endDate = _endDate.ToDateTime();
                endDate = endDate.GetEndOfDate();
            }
            else
            {
                endDate = DateTime.Now.GetEndOfDate();
            }

            var fromDate = startDate.GetStartOfDate();
            var toDate = endDate.GetEndOfDate();


            //Get month list
            var _monthList = new List<string>();
            var _dateList = new List<string>();
            if ((startDate.Month != endDate.Month) || (startDate.Year != endDate.Year))
            {
                var nextDate = startDate;
                var endDateOfMonth = startDate;
                int i = 0;
                do
                {
                    nextDate = startDate.AddMonths(i++);
                    _dateList.Add(nextDate.ToString());
                    _monthList.Add(nextDate.Month.ToString() + "/" + nextDate.Year.ToString());

                    var daysInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                    //endDateOfMonth = DateTime.Parse(daysInMonth.ToString() + "/" + nextDate.Month.ToString() + "/" + nextDate.Year.ToString()).GetEndOfDate();
                    endDateOfMonth = (daysInMonth.ToString() + "/" + nextDate.Month.ToString().PadLeft(2, '0') + "/" + nextDate.Year.ToString().PadLeft(2, '0')).ToDateTime().GetEndOfDate();
                } while (endDateOfMonth.CompareTo(toDate) < 0);
            }

            //Get genaral information
            if (fromDate == DateTime.Now.GetStartOfDate() || toDate == DateTime.Now.GetEndOfDate())
            {
                IEnumerable<Order> report;
                IEnumerable<Order> reportCancel;
                IEnumerable<Order> reportPreCancel;
                IEnumerable<OrderDetail> reportOrderCancel;


                #region List All Order
                if (storeId > 0)
                {
                    report = orderApi.GetRentsByTimeRange2(storeId, fromDate, toDate)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
                }
                else
                {
                    report = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
                }
                #endregion
                #region List Cancel order
                if ((int)storeId > 0)
                {
                    reportCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel || a.OrderStatus == (int)OrderStatusEnum.PosCancel).ToList();
                }
                else
                {
                    reportCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel || a.OrderStatus == (int)OrderStatusEnum.PosCancel).ToList();
                }
                #endregion

                #region List Pre Cancel order

                if ((int)storeId > 0)
                {
                    reportPreCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel || a.OrderStatus == (int)OrderStatusEnum.PosPreCancel).ToList();
                }
                else
                {
                    reportPreCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel || a.OrderStatus == (int)OrderStatusEnum.PosPreCancel).ToList();
                }
                #endregion

                #region List Cancel OrderDetail
                if ((int)storeId > 0)
                {
                    reportOrderCancel = orderDetailApi.GetOrderDetailsByTimeRange(DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate(), (int)storeId)
                        .Where(a => a.Status == (int)OrderStatusEnum.PosPreCancel || a.Status == (int)OrderStatusEnum.PosCancel).ToList();
                }
                else
                {
                    reportOrderCancel = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate())
                         .Where(a => a.Status == (int)OrderStatusEnum.PosPreCancel || a.Status == (int)OrderStatusEnum.PosCancel).ToList();
                }
                #endregion
                var _paymentCash = report.Sum(item => item.FinalAmount);
                var _paymentUserCard = 0;
                var _paymentCreditCard = 0;
                //Total amount
                var _totalAmount = report.Sum(item => item.TotalAmount);
                //Total discount
                var _totalDiscount = report.Sum(a => a.Discount) + report.Sum(a => a.DiscountOrderDetail);
                //Total amount after discount
                var _finalAmount = report.Sum(item => item.FinalAmount);
                //Total amount Cancel
                var _totalCancel = reportCancel.Sum(item => item.TotalAmount);
                //Total amount Pre Cancel
                var _totalPreCancel = reportPreCancel.Sum(item => item.TotalAmount);
                // Total amount Order Cancel
                var _totalOrderCancel = reportOrderCancel.Sum(item => item.FinalAmount);

                //Total bill
                var _totalBill = report.Count();
                //Total bill at store
                var _totalBillAtStore = report.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                //Total bill takeaway
                var _totalBillTakeAway = report.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                //Total bill delivery
                var _totalBillDelivery = report.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);

                return Json(new
                {
                    success = true,
                    msg = "Báo cáo chạy thành công",
                    dataAmount = new
                    {
                        totalAmount = _totalAmount,
                        totalDiscount = _totalDiscount,
                        finalAmount = _finalAmount,
                        totalCancel = _totalCancel,
                        totalPreCancel = _totalPreCancel,
                        totalOrderCancel = _totalOrderCancel,
                    },
                    dataBill = new
                    {
                        totalBill = _totalBill,
                        totalBillAtStore = _totalBillAtStore,
                        totalBillTakeAway = _totalBillTakeAway,
                        totalBillDelivery = _totalBillDelivery
                    },
                    dataPayment = new
                    {
                        payment = _finalAmount,
                        paymentCash = _paymentCash,
                        paymentUserCard = _paymentUserCard,
                        paymentCreditCard = _paymentCreditCard,
                        dateList = _dateList,
                    },
                    dateList = _dateList,
                    monthList = _monthList
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                IEnumerable<DateReport> report;
                IEnumerable<Order> reportCancel;
                IEnumerable<Order> reportPreCancel;
                IEnumerable<OrderDetail> reportOrderCancel;
                #region List All Order
                if (storeId > 0)
                {
                    report = dateReportApi.GetDateReportTimeRangeAndStore(fromDate, toDate, storeId)
                        .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();
                }
                else
                {
                    report = dateReportApi.GetDateReportTimeRangeAndBrand(fromDate, toDate, brandId)
                        .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();
                }
                #endregion

                #region List Cancel order
                if ((int)storeId > 0)
                {
                    reportCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel).ToList();
                }
                else
                {
                    reportCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel).ToList();
                }
                #endregion

                #region List Pre Cancel order

                if ((int)storeId > 0)
                {
                    reportPreCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel).ToList();
                }
                else
                {
                    reportPreCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel).ToList();
                }
                #endregion

                #region List Cancel OrderDetail
                if ((int)storeId > 0)
                {
                    reportOrderCancel = orderDetailApi.GetOrderDetailsByTimeRange(fromDate, toDate, (int)storeId)
                        .Where(a => a.Status == (int)OrderStatusEnum.PosPreCancel || a.Status == (int)OrderStatusEnum.PosCancel).ToList();
                }
                else
                {
                    reportOrderCancel = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, fromDate, toDate)
                         .Where(a => a.Status == (int)OrderStatusEnum.PosPreCancel || a.Status == (int)OrderStatusEnum.PosCancel).ToList();
                }
                #endregion

                var _paymentCash = report.Sum(item => item.TotalCash);
                var _paymentUserCard = 0;
                var _paymentCreditCard = 0;
                //Total amount
                var _totalAmount = report.Sum(item => item.TotalAmount);
                //Total discount
                var _totalDiscount = report.Sum(a => a.Discount) + report.Sum(a => a.DiscountOrderDetail);
                //Total amount after discount
                var _finalAmount = report.Sum(item => item.FinalAmount);
                //Total amount Cancel
                var _totalCancel = reportCancel.Sum(item => item.TotalAmount);
                //Total amount Pre Cancel
                var _totalPreCancel = reportPreCancel.Sum(item => item.TotalAmount);
                // Total amount Order Cancel
                var _totalOrderCancel = reportOrderCancel.Sum(item => item.TotalAmount);

                //Total bill
                var _totalBill = report.Sum(a => a.TotalOrder);
                //Total bill at store
                var _totalBillAtStore = report.Sum(a => a.TotalOrderAtStore);
                //Total bill takeaway
                var _totalBillTakeAway = report.Sum(a => a.TotalOrderTakeAway);
                //Total bill delivery
                var _totalBillDelivery = report.Sum(a => a.TotalOrderDelivery);


                return Json(new
                {
                    success = true,
                    msg = "Báo cáo chạy thành công",
                    dataAmount = new
                    {
                        totalAmount = _totalAmount,
                        totalDiscount = _totalDiscount,
                        finalAmount = _finalAmount,
                        totalCancel = _totalCancel,
                        totalPreCancel = _totalPreCancel,
                        totalOrderCancel = _totalOrderCancel,
                    },
                    dataBill = new
                    {
                        totalBill = _totalBill,
                        totalBillAtStore = _totalBillAtStore,
                        totalBillTakeAway = _totalBillTakeAway,
                        totalBillDelivery = _totalBillDelivery
                    },
                    dataPayment = new
                    {
                        payment = _finalAmount,
                        paymentCash = _paymentCash,
                        paymentUserCard = _paymentUserCard,
                        paymentCreditCard = _paymentCreditCard,
                        dateList = _dateList,
                    },
                    dateList = _dateList,
                    monthList = _monthList
                }, JsonRequestBehavior.AllowGet);
            }
        }//End DateData

        private JsonResult TotalAmountChartData(int storeId, int brandId, List<string> _dateList)
        {
            var dateReportService = this.Service<IDateReportService>();
            var dateReportApi = new DateReportApi();
            var totalAmountList = new List<double>();
            var finalAmountList = new List<double>();
            var today = DateTime.Now.GetEndOfDate();

            if (storeId > 0)
            {
                for (int i = 0; i < _dateList.Count; i++)
                {
                    //var nextDate = _dateList[i].ToDateTime();
                    var nextDate = DateTime.Parse(_dateList[i]);
                   
                    var startDateOfMonth = nextDate.GetStartOfDate();
                    DateTime endDateOfMonth;
                    if (nextDate.Month == today.Month && nextDate.Year == today.Year)
                    {
                        endDateOfMonth = DateTime.Now.GetEndOfDate();
                    }
                    else
                    {
                        var daysInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                        //endDateOfMonth = DateTime.Parse(daysInMonth.ToString() + "/" + nextDate.Month.ToString() + "/" + nextDate.Year.ToString()).GetEndOfDate();
                        endDateOfMonth = (daysInMonth.ToString() + "/" + nextDate.Month.ToString().PadLeft(2, '0') + "/" + nextDate.Year.ToString().PadLeft(2, '0')).ToDateTime().GetEndOfDate();
                    }


                    var report = dateReportApi.GetDateReportTimeRangeAndStore(startDateOfMonth, endDateOfMonth, storeId)
                        .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();

                    totalAmountList.Add((double)report.Sum(a => a.TotalAmount));
                    finalAmountList.Add((double)report.Sum(a => a.FinalAmount));

                }
            }
            else
            {
                for (int i = 0; i < _dateList.Count; i++)
                {
                    //var nextDate = _dateList[i].ToDateTime();
                    var nextDate = DateTime.Parse(_dateList[i]);


                    var startDateOfMonth = nextDate.GetStartOfDate();
                    DateTime endDateOfMonth;
                    if (nextDate.Month == today.Month && nextDate.Year == today.Year)
                    {
                        endDateOfMonth = DateTime.Now.GetEndOfDate();
                    }
                    else
                    {
                        var daysInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                        //endDateOfMonth = DateTime.Parse(daysInMonth.ToString() + "/" + nextDate.Month.ToString() + "/" + nextDate.Year.ToString()).GetEndOfDate();
                        endDateOfMonth = (daysInMonth.ToString() + "/" + nextDate.Month.ToString().PadLeft(2, '0') + "/" + nextDate.Year.ToString().PadLeft(2, '0')).ToDateTime().GetEndOfDate();
                    }

                    var report = dateReportApi.GetDateReportTimeRangeAndBrand(startDateOfMonth, endDateOfMonth, brandId)
                        .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();

                    totalAmountList.Add((double)report.Sum(a => a.TotalAmount));
                    finalAmountList.Add((double)report.Sum(a => a.FinalAmount));
                    //}
                }
            }


            return Json(new
            {
                success = true,
                msg = "Báo cáo chạy thành công",
                data = new
                {
                    totalAmountList = totalAmountList,
                    finalAmountList = finalAmountList
                },
            }, JsonRequestBehavior.AllowGet);
        }//End TotalAmountChartData

        private JsonResult BillDetailChartData(int storeId, int brandId, List<string> _dateList)
        {
            var dateReportService = this.Service<IDateReportService>();

            var dateReportApi = new DateReportApi();
            var totalBillAtStoreList = new List<int>();
            var totalBillTakeAwayList = new List<int>();
            var totalBillDeliveryList = new List<int>();
            var today = DateTime.Now.GetStartOfDate();
            List<string> _newDateList = new List<string>();

            if (storeId > 0)
            {
                for (int i = 0; i < _dateList.Count; i++)
                {
                    var nextDate = _dateList[i].ToDateTime();

                    //var startDate = nextDate.GetStartOfDate();
                    //var endDate = nextDate.GetEndOfDate();
                    var startDateOfMonth = nextDate.GetStartOfDate();
                    DateTime endDateOfMonth;
                    if (nextDate.Month == today.Month && nextDate.Year == today.Year)
                    {
                        endDateOfMonth = DateTime.Now.GetEndOfDate();
                    }
                    else
                    {
                        var daysInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                        endDateOfMonth = DateTime.Parse(daysInMonth.ToString() + "/" + nextDate.Month.ToString() + "/" + nextDate.Year.ToString()).GetEndOfDate();
                    }

                    var report = dateReportApi.GetDateReportTimeRangeAndStore(startDateOfMonth, endDateOfMonth, storeId)
                        .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();

                    var _totalBill = report.Sum(a => a.TotalOrder);
                    _newDateList.Add(nextDate.ToString().Substring(0, 10) + ": " + _totalBill);

                    totalBillAtStoreList.Add(report.Sum(a => a.TotalOrderAtStore));
                    totalBillTakeAwayList.Add(report.Sum(a => a.TotalOrderTakeAway));
                    totalBillDeliveryList.Add(report.Sum(a => a.TotalOrderDelivery));
                    //}

                }
            }
            else
            {
                for (int i = 0; i < _dateList.Count; i++)
                {
                    var nextDate = _dateList[i].ToDateTime();

                    //var startDate = nextDate.GetStartOfDate();
                    //var endDate = nextDate.GetEndOfDate();
                    var startDateOfMonth = nextDate.GetStartOfDate();
                    DateTime endDateOfMonth;
                    if (nextDate.Month == today.Month && nextDate.Year == today.Year)
                    {
                        endDateOfMonth = DateTime.Now.GetEndOfDate();
                    }
                    else
                    {
                        var daysInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                        endDateOfMonth = DateTime.Parse(daysInMonth.ToString() + "/" + nextDate.Month.ToString() + "/" + nextDate.Year.ToString()).GetEndOfDate();
                    }

                    var report = dateReportApi.GetDateReportTimeRangeAndBrand(startDateOfMonth, endDateOfMonth, brandId)
                        .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();

                    var _totalBill = report.Sum(a => a.TotalOrder);
                    _newDateList.Add(nextDate.ToString().Substring(0, 10) + ": " + _totalBill);

                    totalBillAtStoreList.Add(report.Sum(a => a.TotalOrderAtStore));
                    totalBillTakeAwayList.Add(report.Sum(a => a.TotalOrderTakeAway));
                    totalBillDeliveryList.Add(report.Sum(a => a.TotalOrderDelivery));
                }
                //}
            }

            return Json(new
            {
                success = true,
                msg = "Báo cáo chạy thành công",
                data = new
                {
                    newDateList = _newDateList,
                    totalBillAtStoreList = totalBillAtStoreList,
                    totalBillTakeAwayList = totalBillTakeAwayList,
                    totalBillDeliveryList = totalBillDeliveryList
                },
            }, JsonRequestBehavior.AllowGet);
        }//End BillDetailCharData

        [HttpGet]
        public JsonResult ChartData(int storeId, int brandId, string _option, string _dateList)
        {
            List<string> dateList = JsonConvert.DeserializeObject<List<string>>(_dateList);

            if (_option.Equals("Tổng doanh thu"))
            {
                return TotalAmountChartData(storeId, brandId, dateList);
            }
            else
            {
                return BillDetailChartData(storeId, brandId, dateList);
            }
        }//End CharData

        public ActionResult DashBoardProductReport(int storeId)
        {
            ViewBag.storeId = storeId;
            return PartialView("_DashBoardProductReport");
        }//End DashBoardProductReport
        public class ProductChartData
        {
            public string PName{ get; set; }
            public int Quanti { get; set; }
            public double FinAmount { get; set; }
            
            public ProductChartData(string PName,int Quanti,double FinAmount)
            {
                this.PName = PName;
                this.Quanti = Quanti;
                this.FinAmount = FinAmount;
            }
        }
        public JsonResult ProductChart(string _startDate, string _endDate, int storeId, int brandId)
        {
            var orderDetailService = this.Service<IOrderDetailService>();
            var dateProductService = this.Service<IDateProductService>();
            var orderDetailApi = new OrderDetailApi();
            var dateProductApi = new DateProductApi();
            DateTime startDate, endDate;
            if (!_startDate.Equals(""))
            {
                startDate = _startDate.ToDateTime();
         
                startDate = startDate.GetStartOfMonth();
            }
            else
            {
                startDate = DateTime.Now.GetStartOfMonth();
            }
            if (!_endDate.Equals(""))
            {
                endDate = _endDate.ToDateTime();
                endDate = endDate.GetEndOfMonth();
            }
            else
            {
                endDate = DateTime.Now.GetEndOfMonth();
            }
            //var time = startDate.GetStartOfDate();
            //if (time == Utils.GetCurrentDateTime().GetStartOfDate()) {

            //    IEnumerable<OrderDetail> filteredListItems;

            //    var total = 0;
            //    // Search.
            //    var totalQuery = 0;
            //    if (storeId > 0)
            //    {
            //        filteredListItems = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
            //            .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
            //            .OrderBy(a => a.Product.ProductName);
            //    }

            //    else
            //    {
            //        filteredListItems = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
            //            .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
            //            .OrderBy(a => a.Product.ProductName);
            //    }
            //    var totalFinal5List = filteredListItems.GroupBy(x => x.Product.ProductName)
            //           .Select(g => new
            //           {
            //               ProductName = g.Key,
            //               Quantity = g.Sum(z => z.Quantity),
            //               FinalAmount = g.Sum(z => z.FinalAmount)
            //           })
            //           .OrderByDescending(g => g.FinalAmount).Take(5);
            //    total = totalFinal5List.Count();
            //    totalQuery = totalFinal5List.Count();
            //    List<string> names = new List<string>();
            //    List<int> quantities = new List<int>();
            //    List<double> amounts = new List<double>();
            //    foreach (var item in totalFinal5List)
            //    {
            //        names.Add(item.ProductName);
            //        quantities.Add(item.Quantity);
            //        amounts.Add(item.FinalAmount);
            //    }
            //    return Json(new
            //    {
            //        dataChart = new
            //        {
            //            nameArray = names.ToArray(),
            //            quantityArray = quantities.ToArray(),
            //            amountArray = amounts.ToArray(),
            //        }

            //    }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
                IEnumerable<DateProduct> filteredListItems;

                startDate = _startDate.ToDateTime();
                startDate = startDate.GetStartOfDate();

                endDate = _endDate.ToDateTime();
                endDate = endDate.GetEndOfDate();
                //var total = 0;
                //var totalQuery = 0;
                IEnumerable<OrderDetail> filteredListItems2;
                if (storeId > 0)
                    {
                        filteredListItems = dateProductService.GetDateProductByTimeRange(startDate, endDate, storeId)
                            .OrderBy(a => a.ProductName_);

                        filteredListItems2 = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
                       .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                       .OrderBy(a => a.Product.ProductName);
                }
                    else
                    {
                        filteredListItems = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId)
                            .OrderBy(a => a.ProductName_);

                    filteredListItems2 = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
                       .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                       .OrderBy(a => a.Product.ProductName);
                }
                

                var totalFinal5Report = filteredListItems.GroupBy(x => x.Product.ProductName)
                       .Select(g => new
                       {
                           ProductName = g.Key,
                           Quantity = g.Sum(z => z.Quantity),
                           FinalAmount = g.Sum(z => z.FinalAmount)
                       })
                       .OrderByDescending(g => g.FinalAmount).Take(5);
                var totalFinal5ListOrder = filteredListItems2.GroupBy(x => x.Product.ProductName)
                       .Select(g => new
                       {
                           ProductName = g.Key,
                           Quantity = g.Sum(z => z.Quantity),
                           FinalAmount = g.Sum(z => z.FinalAmount)
                       })
                       .OrderByDescending(g => g.FinalAmount).Take(5);
                //total = totalFinal5List.Count();
                //totalQuery = totalFinal5List.Count();
                List<string> names = new List<string>();
                List<int> quantities = new List<int>();
                List<double> amounts = new List<double>();
                List<ProductChartData> totalFinal5 = new List<ProductChartData>();
                foreach(var item in totalFinal5Report)
                {
                    totalFinal5.Add(new ProductChartData(item.ProductName, item.Quantity, item.FinalAmount));
                }
                foreach (var item in totalFinal5ListOrder)
                {
                    totalFinal5.Add(new ProductChartData(item.ProductName, item.Quantity, item.FinalAmount));
                }
                var Final5 = totalFinal5.OrderByDescending(a => a.FinAmount).Take(5);
                foreach (var item in Final5)
                {
                    names.Add(item.PName);
                    quantities.Add(item.Quanti);
                    amounts.Add(item.FinAmount);
                }
                return Json(new
                {
                    dataChart = new
                    {
                        nameArray = names.ToArray(),
                        quantityArray = quantities.ToArray(),
                        amountArray = amounts.ToArray(),
                    }

                }, JsonRequestBehavior.AllowGet);
            //}
        
        }

        public JsonResult ProductData(JQueryDataTableParamModel param, string _startDate, string _endDate, int storeId, int brandId)
        {
            var orderDetailService = this.Service<IOrderDetailService>();
            var dateProductService = this.Service<IDateProductService>();
            var orderDetailApi = new OrderDetailApi();
            var dateProductApi = new DateProductApi();

            DateTime startDate, endDate;

            if (!_startDate.IsNullOrWhiteSpace())
            {
                startDate = _startDate.ToDateTime();
                //startDate = DateTime.ParseExact(_startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                startDate = startDate.GetStartOfMonth();
            }
            else
            {
                startDate = DateTime.Now.GetStartOfMonth();
            }
            if (!_endDate.IsNullOrWhiteSpace())
            {
                endDate = _endDate.ToDateTime();
                //endDate = DateTime.ParseExact(_endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                endDate = endDate.GetEndOfMonth();
            }
            else
            {
                endDate = DateTime.Now.GetEndOfMonth();
            }

            //var fromDate = startDate.GetStartOfDate();
            //var toDate = endDate.GetEndOfDate();

            //var startDate = _startDate.ToDateTime();
            //var endDate = _endDate.ToDateTime();

            var time = startDate.GetStartOfDate();
            if (time == Utils.GetCurrentDateTime().GetStartOfDate())
            {
                IEnumerable<OrderDetail> filteredListItems;
                var dateNow = Utils.GetCurrentDateTime();
                startDate = dateNow.GetStartOfDate();
                endDate = dateNow.GetEndOfDate();
                var total = 0;
                // Search.
                var totalQuery = 0;
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    if (storeId > 0)
                    {
                        filteredListItems = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
                            .Where(a => (a.Product.ProductName != null && a.Product.ProductName.ToLower().Contains(param.sSearch.ToLower()))
                            && a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                            .OrderBy(a => a.Product.ProductName);
                    }
                    else
                    {
                        filteredListItems = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
                            .Where(a => (a.Product.ProductName != null && a.Product.ProductName.ToLower().Contains(param.sSearch.ToLower()))
                            && a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                            .OrderBy(a => a.Product.ProductName);
                    }
                }
                else
                {
                    if (storeId > 0)
                    {
                        filteredListItems = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
                            .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                            .OrderBy(a => a.Product.ProductName);
                    }
                    else
                    {
                        filteredListItems = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
                            .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                            .OrderBy(a => a.Product.ProductName);
                    }
                }

                var totalFinalList = filteredListItems.GroupBy(x => x.Product.ProductName)
                        .Select(g => new
                        {
                            ProductName = g.Key,
                            Quantity = g.Sum(z => z.Quantity),
                            FinalAmount = g.Sum(z => z.FinalAmount)
                        })
                        .OrderByDescending(g => g.Quantity);

                var finalList = totalFinalList.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                total = totalFinalList.Count();
                totalQuery = totalFinalList.Count();

                int count = param.iDisplayStart;
                var listProduct = finalList.Select(a => new IConvertible[]
                    {
                        ++count,
                        a.ProductName,
                        a.Quantity,
                        //a.TotalAmount.ToString("N0")
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalAmount),
                    });

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalQuery,
                    aaData = listProduct,
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                IEnumerable<DateProduct> filteredListItems;

                startDate = _startDate.ToDateTime();
                startDate = startDate.GetStartOfDate();

                //var endDate = _endDate.ToDateTime().GetEndOfDate();
                endDate = _endDate.ToDateTime();
                endDate = endDate.GetEndOfDate();
                var total = 0;
                var totalQuery = 0;

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    if (storeId > 0)
                    {
                        filteredListItems = dateProductApi.GetDateProductByTimeRange(startDate, endDate, storeId)
                            .Where(
                            d => (d.ProductName_ != null && d.ProductName_.ToLower().Contains(param.sSearch.ToLower()))
                        ).OrderBy(a => a.ProductName_);
                    }
                    else
                    {
                        filteredListItems = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId)
                            .Where(
                            d => (d.ProductName_ != null && d.ProductName_.ToLower().Contains(param.sSearch.ToLower()))
                        ).OrderBy(a => a.ProductName_);
                    }
                }
                else
                {
                    if (storeId > 0)
                    {
                        filteredListItems = dateProductService.GetDateProductByTimeRange(startDate, endDate, storeId)
                            .OrderBy(a => a.ProductName_);
                    }
                    else
                    {
                        filteredListItems = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId)
                            .OrderBy(a => a.ProductName_);
                    }
                }

                var totalFinalList = filteredListItems.GroupBy(x => x.ProductName_)
                        .Select(g => new { ProductName = g.Key, Quantity = g.Sum(z => z.Quantity), FinalAmount = g.Sum(z => z.FinalAmount) })
                        .OrderByDescending(g => g.Quantity);

                var finalList = totalFinalList.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                total = totalFinalList.Count();
                totalQuery = totalFinalList.Count();

                int count = param.iDisplayStart;
                var listProduct = finalList.Select(a => new IConvertible[]
                {
                    ++count,
                    a.ProductName,
                    a.Quantity,
                    //a.TotalAmount.ToString("N0")
                    string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalAmount),
                });
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalQuery,
                    aaData = listProduct,
                }, JsonRequestBehavior.AllowGet);
            }
        }//End ProductData


        public ActionResult ExportProductTableToExcel(string _startDate, string _endDate, int storeId, int brandId)
        {
            var storeService = this.Service<IStoreService>();
            var orderDetailService = this.Service<IOrderDetailService>();
            var dateProductService = this.Service<IDateProductService>();
            var orderDetailApi = new OrderDetailApi();
            var dateProductApi = new DateProductApi();
            DateTime startDate;
            DateTime endDate;
            //var time = _startDate.ToDateTime();
            var sTime = _startDate.ToDateTime();
            var eTime = _endDate.ToDateTime();
            var dateRange = "_" + sTime.Month + (sTime.Month == eTime.Month ? "" : " - " + eTime.Month) + "";
            var storeName = "";
            if (storeId > 0)
            {
                storeName = storeService.Get(storeId).Name;
            }
            else
            {
                storeName = "Service";
            }

            if (sTime == Utils.GetCurrentDateTime())
            {
                IEnumerable<OrderDetail> filteredListItems;
                var dataNow = Utils.GetCurrentDateTime();
                startDate = dataNow.GetStartOfDate();
                endDate = dataNow.GetEndOfDate();

                if (storeId > 0)
                {
                    filteredListItems = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
                        .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                        .OrderBy(a => a.Product.ProductName);
                }
                else
                {
                    filteredListItems = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
                        .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                        .OrderBy(a => a.Product.ProductName);
                }

                var totalFinalList = filteredListItems.GroupBy(x => x.Product.ProductName)
                        .Select(g => new
                        {
                            ProductName = g.Key,
                            Quantity = g.Sum(z => z.Quantity),
                            FinalAmount = g.Sum(z => z.FinalAmount)
                        })
                        .OrderByDescending(g => g.Quantity);

                #region Export to Excel
                int count = 0;
                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("TongQuanSanPham");
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng bán ra";
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
                    foreach (var data in totalFinalList)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ProductName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                        StartHeaderChar = 'A';
                    }
                    //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = "";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalAmount).ToString();
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalDiscountFee).ToString();
                    //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => q.FinalAmount).ToString();
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "TổngQuanSảnPhẩm_" + storeName + "_TổngQuanTháng" + dateRange + ".xlsx";
                    //var fileDownloadName = "TổngQuanSảnPhẩm" + sTime.Month + "/" + sTime.Year + "đến" + eTime.Month + "/" + eTime.Year + " " + storeName + ".xlsx";
                    //var fileDownloadName = "TổngQuanSảnPhẩm " + _startDate.Replace("/", "-") + " đến " + _endDate.Replace("/", "-") + "_" + storeName + ".xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                // LÀM TỚI ĐÂY RỒI
                #endregion

            }
            else
            {
                IEnumerable<DateProduct> filteredListItems;
                startDate = _startDate.ToDateTime().GetStartOfDate();
                endDate = _endDate.ToDateTime().GetEndOfDate();

                if (storeId > 0)
                {
                    filteredListItems = dateProductApi.GetDateProductByTimeRange(startDate, endDate, storeId)
                        .OrderBy(a => a.ProductName_);
                }
                else
                {
                    filteredListItems = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId)
                        .OrderBy(a => a.ProductName_);
                }

                var totalFinalList = filteredListItems.GroupBy(x => x.ProductName_)
                        .Select(g => new
                        {
                            ProductName = g.Key,
                            Quantity = g.Sum(z => z.Quantity),
                            FinalAmount = g.Sum(z => z.FinalAmount)
                        })
                        .OrderByDescending(g => g.Quantity);

                #region Export to Excel
                int count = 0;
                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("TongQuanSanPham");
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng bán ra";
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
                    foreach (var data in totalFinalList)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ProductName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                        StartHeaderChar = 'A';
                    }
                    //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = "";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalAmount).ToString();
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalDiscountFee).ToString();
                    //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => q.FinalAmount).ToString();
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "TổngQuanSảnPhẩm_" + storeName + "_TổngQuanTháng" + dateRange + ".xlsx";
                    ///var fileDownloadName = "TổngQuanSảnPhẩm" + sTime.Month + "/" + sTime.Year + "đến" + eTime.Month + "/" + eTime.Year + " " + storeName + ".xlsx";
                    //var fileDownloadName = "TổngQuanSảnPhẩm " + _startDate.Replace("/", "-") + " đến " + _endDate.Replace("/", "-") + "_" + storeName + ".xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion

            }
        }

        public ActionResult DashBoardCashierReport(int storeId)
        {
            ViewBag.storeId = storeId;
            return PartialView("_DashBoardCashierReport");
        }//End DashBoardCashierReport

        public JsonResult CashierData(JQueryDataTableParamModel param, string _startDate, string _endDate, int storeId, int brandId)
        {
            var orderService = this.Service<IOrderService>();
            var orderApi = new OrderApi();
            var _aspNetUserService = this.Service<IAspNetUserService>();

            DateTime startDate, endDate;

            if (!_startDate.IsNullOrWhiteSpace())
            {
                startDate = _startDate.ToDateTime();                
                startDate = startDate.GetStartOfDate();
            }
            else
            {
                startDate = DateTime.Now.GetStartOfDate();
                
            }
            if (!_endDate.IsNullOrWhiteSpace())
            {
                endDate = _endDate.ToDateTime();
                endDate = endDate.GetEndOfDate();
            }
            else
            {
                endDate = DateTime.Now.GetEndOfDate();
            }

            var fromDate = startDate.GetStartOfMonth();
            var toDate = endDate.GetEndOfMonth();


            if (storeId > 0)
            {
                var totalModel = orderApi.GetRentsByTimeRange2(storeId, fromDate, toDate)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
                    .GroupBy(a => a.CheckInPerson);

                var modelCount = totalModel.Count();

                int i = param.iDisplayStart;
                var list = totalModel.ToList().Select(a => new IConvertible[]
                {
                    ++i,
                    a.Key == null ? "N/A" : _aspNetUserService.GetUserByUsernameSync(a.Key).FullName,
                    a.Key == null ? "N/A" : a.Key,
                    a.Count(),
                    a.Sum(b => b.FinalAmount)
                }).Skip(param.iDisplayStart).Take(param.iDisplayLength);

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecord = modelCount,
                    iTotalDisplayRecords = modelCount,
                    aaData = list
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var totalModel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
                    .GroupBy(a => a.CheckInPerson);

                var modelCount = totalModel.Count();

                int i = param.iDisplayStart;
                var list = totalModel.ToList().Select(a => new IConvertible[]
                {
                    ++i,
                    a.Key == null ? "N/A" : _aspNetUserService.GetUserByUsernameSync(a.Key).FullName,
                    a.Key == null ? "N/A" : a.Key,
                    a.Count(),
                    a.Sum(b => b.FinalAmount)
                }).Skip(param.iDisplayStart).Take(param.iDisplayLength);

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecord = modelCount,
                    iTotalDisplayRecords = modelCount,
                    aaData = list
                }, JsonRequestBehavior.AllowGet);
            }
        }//End CashierData

        public ActionResult ExportCashierTableToExcel(string _startDate, string _endDate, int storeId, int brandId)
        {
            var storeService = this.Service<IStoreService>();
            var orderService = this.Service<IOrderService>();
            var aspNetUserService = this.Service<IAspNetUserService>();
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var aspNetUserApi = new AspNetUserApi();
            var fromDate = _startDate.ToDateTime();
            fromDate = fromDate.GetStartOfDate();
            //var fromDate = _startDate.ToDateTime().GetStartOfDate();
            var toDate = _endDate.ToDateTime();
            toDate = toDate.GetEndOfDate();
            //var toDate = _endDate.ToDateTime().GetEndOfDate();
            var sTime = _startDate.ToDateTime();
            var eTime = _endDate.ToDateTime();
            
            var dateRange = "(" + sTime.Month + (sTime.Month == eTime.Month ? "" : " - " + eTime.Month) + ")";
            var storeName = "";
            if (storeId > 0)
            {
                storeName = storeService.Get(storeId).Name;
            }
            else
            {
                storeName = "Service";
            }

            if (storeId > 0)
            {
                var totalModel = orderApi.GetRentsByTimeRange2(storeId, fromDate, toDate)
                  .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
                  .GroupBy(a => a.CheckInPerson)
                  .Select(g => new
                  {
                      FullName = g.Key == null ? "N/A" : aspNetUserApi.GetUserByUsername(g.Key).FullName,
                      Username = g.Key == null ? "N/A" : g.Key,
                      TotalBill = g.Count(),
                      FinalAmount = g.Sum(x => x.FinalAmount)
                  });

                #region Export to Excel
                int count = 0;
                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("TongQuanNhanVien");
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Họ và tên";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên đăng nhập";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng hóa đơn";
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng tiền thu được";
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
                    foreach (var data in totalModel)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.FullName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Username;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalBill;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount); ;
                        StartHeaderChar = 'A';
                    }
                    //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = "";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalAmount).ToString();
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalDiscountFee).ToString();
                    //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => q.FinalAmount).ToString();
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "TổngQuanNhânViên_" + storeName + "_TổngQuanTháng" + dateRange + ".xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion
            }
            else
            {
                var totalModel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
                  .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList()
                  .GroupBy(a => a.CheckInPerson)
                  .Select(g => new
                  {
                      FullName = g.Key == null ? "N/A" : aspNetUserApi.GetUserByUsername(g.Key).FullName,
                      Username = g.Key == null ? "N/A" : g.Key,
                      TotalBill = g.Count(),
                      FinalAmount = g.Sum(x => x.FinalAmount)
                  });

                #region Export to Excel
                int count = 0;
                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("TongQuanNhanVien");
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Họ và tên";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên đăng nhập";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng hóa đơn";
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng tiền thu được";
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
                    foreach (var data in totalModel)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.FullName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Username;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalBill;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                        StartHeaderChar = 'A';
                    }
                    //ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = "";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tất cả các ngày";
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalAmount).ToString();
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = list.Sum(q => q.TotalDiscountFee).ToString();
                    //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = list.Sum(q => q.FinalAmount).ToString();
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "TổngQuanNhânViên_" + storeName + "_TổngQuanTháng" + dateRange + ".xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion
            }
        }//End ExportCashierTableToExcel
    }
}
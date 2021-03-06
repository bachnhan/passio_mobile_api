﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Forms;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Wisky.SkyAdmin.Manage.Controllers;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web;
using SkyWeb.DatVM.Mvc;
using System.Data.Entity;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities.Services;

namespace Wisky.SkyAdmin.Manage.Areas.StoreReport.Controllers
{
    public class TimeReportController : Controller
    {
        #region Index
        // GET: SystemReport/TimeReport
        public ActionResult Index(int storeId, int brandId)
        {
            if (storeId <= 0)
            {
                return HttpNotFound();
            }
            else
            {
                var storeApi = new StoreApi();
                var store = storeApi.GetStoreById(storeId);
                if (store == null || store.BrandId != brandId)
                {
                    return HttpNotFound();
                }
            }

            return View();
        }

        public JsonResult GetAllReportData(int brandId, int storeId)
        {
            try
            {
                var hourData = GetHourReportAmountData(brandId, storeId);
                var dayOfWeekData = GetDayOfWeekReportAmountData(brandId, storeId);
                var dayData = GetDayReportAmountData(brandId, storeId);
                var monthData = GetMonthReportAmountData(brandId, storeId);

                return Json(new
                {
                    success = true,
                    hourData = hourData,
                    dayOfWeekData = dayOfWeekData,
                    dayData = dayData,
                    monthData = monthData
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng thử lại." }, JsonRequestBehavior.AllowGet);
            }
        }

        public AmountComparison GetHourReportAmountData(int brandId, int storeId)
        {
            var orderApi = new OrderApi();
            var curDate = Utils.GetCurrentDateTime();
            var startDate = curDate.GetStartOfDate();
            AmountComparison result = new AmountComparison()
            {
                MaxAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
                MinAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
            };

            try
            {
                var report = orderApi.GetRentsByTimeRange(storeId, startDate, curDate, brandId)
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                if (report != null && report.Count() > 0)
                {
                    Order maxOrder = null;
                    Order minOrder = null;
                    foreach (var item in report)
                    {
                        if (maxOrder != null)
                        {
                            if (maxOrder.FinalAmount < item.FinalAmount)
                            {
                                maxOrder = item;
                            }

                            if (minOrder.FinalAmount > item.FinalAmount)
                            {
                                minOrder = item;
                            }
                        }
                        else
                        {
                            maxOrder = item;
                            minOrder = item;
                        }
                    }

                    result.MaxAmount.Text = maxOrder.CheckinHour.ToString();
                    result.MaxAmount.Amount = maxOrder.FinalAmount;
                    result.MinAmount.Text = minOrder.CheckinHour.ToString();
                    result.MinAmount.Amount = minOrder.FinalAmount;
                }
            }
            catch (Exception)
            {
                return result;
            }

            return result;
        }

        public AmountComparison GetDayOfWeekReportAmountData(int brandId, int storeId)
        {
            var orderApi = new OrderApi();
            var dateNow = Utils.GetCurrentDateTime();
            var startDate = dateNow.AddDays(1 - (int)dateNow.DayOfWeek).GetStartOfDate();
            var endDate = dateNow.GetEndOfDate();
            AmountComparison result = new AmountComparison()
            {
                MaxAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
                MinAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
            };

            var dayofweekReport = new List<TempDayOfWeekReportModel>();
            #region Add date
            dayofweekReport.Add(new TempDayOfWeekReportModel()
            {
                Day = DayOfWeek.Monday,
                DayOfWeek = "Thứ hai"
            });
            dayofweekReport.Add(new TempDayOfWeekReportModel()
            {
                Day = DayOfWeek.Tuesday,
                DayOfWeek = "Thứ ba"
            });
            dayofweekReport.Add(new TempDayOfWeekReportModel()
            {
                Day = DayOfWeek.Wednesday,
                DayOfWeek = "Thứ tư"
            });
            dayofweekReport.Add(new TempDayOfWeekReportModel()
            {
                Day = DayOfWeek.Thursday,
                DayOfWeek = "Thứ năm"
            });
            dayofweekReport.Add(new TempDayOfWeekReportModel()
            {
                Day = DayOfWeek.Friday,
                DayOfWeek = "Thứ sáu"
            });
            dayofweekReport.Add(new TempDayOfWeekReportModel()
            {
                Day = DayOfWeek.Saturday,
                DayOfWeek = "Thứ bảy"
            });
            dayofweekReport.Add(new TempDayOfWeekReportModel()
            {
                Day = DayOfWeek.Sunday,
                DayOfWeek = "Chủ nhật"
            });
            #endregion

            try
            {
                var rents = orderApi.GetRentsByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                if (rents != null && rents.Count() > 0)
                {
                    var rs = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount),
                    }).ToList();

                    foreach (var item in dayofweekReport)
                    {
                        var takeAway = rs.Where(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime.Value.DayOfWeek == item.Day);
                        item.TakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.TotalOrder);
                        item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Sum(a => a.Money);

                        var atStore = rs.Where(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime.Value.DayOfWeek == item.Day);
                        item.AtStore = (atStore == null) ? 0 : atStore.Sum(a => a.TotalOrder);
                        item.PriceAtStore = (atStore == null) ? 0 : atStore.Sum(a => a.Money);

                        var delivery = rs.Where(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime.Value.DayOfWeek == item.Day);
                        item.Delivery = (delivery == null) ? 0 : delivery.Sum(a => a.TotalOrder);
                        item.PriceDelivery = (delivery == null) ? 0 : delivery.Sum(a => a.Money);

                        item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                        item.TotalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                    }

                    var maxTotalPrice = dayofweekReport.Select(q => q.TotalPrice).Max();
                    var minTotalPrice = dayofweekReport.Select(q => q.TotalPrice).Min();
                    TempDayOfWeekReportModel maxDate = dayofweekReport.Where(q => q.TotalPrice == maxTotalPrice).FirstOrDefault();
                    TempDayOfWeekReportModel minDate = dayofweekReport.Where(q => q.TotalPrice == minTotalPrice).FirstOrDefault();

                    result.MaxAmount.Text = maxDate.DayOfWeek;
                    result.MaxAmount.Amount = maxDate.TotalPrice;
                    result.MinAmount.Text = minDate.DayOfWeek;
                    result.MinAmount.Amount = minDate.TotalPrice;
                }
            }
            catch (Exception)
            {
                return result;
            }

            return result;
        }

        public AmountComparison GetDayReportAmountData(int brandId, int storeId)
        {
            var orderApi = new OrderApi();
            var dateReportApi = new DateReportApi();

            var curDate = Utils.GetCurrentDateTime().GetStartOfDate();
            var startDate = curDate.GetStartOfMonth();
            var endDate = curDate.GetEndOfDate();

            AmountComparison result = new AmountComparison()
            {
                MaxAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
                MinAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
            };

            try
            {
                List<ReportAmount> list = new List<ReportAmount>();
                for (var d = startDate; d < endDate; d = d.AddDays(1))
                {
                    ReportAmount ra = new ReportAmount();
                    ra.Text = d.ToString("dd/MM");
                    if (d != curDate)
                    {
                        ra.Amount = dateReportApi.GetDateReportTimeRangeAndStore(d, d.GetEndOfDate(), storeId)
                            .Select(q => q.FinalAmount ?? 0).DefaultIfEmpty(0).Sum();
                    }
                    else
                    {
                        ra.Amount = orderApi.GetStoreOrderFinishByDate(curDate, storeId)
                            .Select(q => q.FinalAmount).DefaultIfEmpty(0).Sum();
                    }
                    list.Add(ra);
                }


                var maxAmount = list.Select(q => q.Amount).Max();
                var minAmount = list.Select(q => q.Amount).Min();
                result.MaxAmount = list.Where(q => q.Amount == maxAmount).FirstOrDefault();
                result.MinAmount = list.Where(q => q.Amount == minAmount).FirstOrDefault();
            }
            catch (Exception)
            {
                return result;
            }

            return result;
        }

        public AmountComparison GetMonthReportAmountData(int brandId, int storeId)
        {
            var orderApi = new OrderApi();
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

            var dateNow = Utils.GetCurrentDateTime();
            var startDate = new DateTime(dateNow.Year, 1, 1);
            var endDate = dateNow.GetEndOfDate();

            AmountComparison result = new AmountComparison()
            {
                MaxAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
                MinAmount = new ReportAmount()
                {
                    Text = "N/A",
                    Amount = 0
                },
            };

            try
            {
                var orders = orderApi.GetStoreOrderFinishByDate(startDate, storeId).ToList();
                var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, storeId);

                double finalAmount = orders.Select(q => q.FinalAmount).Sum();

                var reports = dateReport.GroupBy(r => new { Time = r.Date.Month }).Select(r => new
                {
                    OrderTime = r.Key.Time,
                    TotalFinalAmount = r.Sum(a => a.FinalAmount),
                }).ToList();

                foreach (var item in monthReport)
                {
                    var report = reports.Where(a => a.OrderTime == item.Month);
                    item.TotalFinalAmount = (double)report.Sum(a => a.TotalFinalAmount);
                    if (item.Month == dateNow.Month)
                    {
                        item.TotalFinalAmount = (double)report.Sum(a => a.TotalFinalAmount) + finalAmount;
                    }
                }

                var maxAmount = monthReport.Select(q => q.TotalFinalAmount).Max();
                var minAmount = monthReport.Select(q => q.TotalFinalAmount).Min();
                result.MaxAmount.Text = monthReport.Where(q => q.TotalFinalAmount == maxAmount).FirstOrDefault().MonthName;
                result.MaxAmount.Amount = maxAmount;
                result.MinAmount.Text = monthReport.Where(q => q.TotalFinalAmount == minAmount).FirstOrDefault().MonthName;
                result.MinAmount.Amount = minAmount;
            }
            catch (Exception)
            {
                return result;
            }

            return result;
        }
        #endregion

        #region DayReport
        public ActionResult DayReport(int brandId)
        {
            //return View();
            return PartialView("_DayReport");
        }
        public JsonResult DateReportTable(int brandId, string startTime, string endTime, int storeId)
        {
            int count = 1, countOrder = 0;
            var listDateReport = new List<DateReportViewModel>();
            var dateReportApi = new DateReportApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            var orderApi = new OrderApi();
            var form = DateTime.Now.GetStartOfDate();
            var to = DateTime.Now.GetEndOfDate();
            IEnumerable<Order> orders = orderApi.GetOrdersByTimeRange(storeId, form, to, brandId);

            try
            {
                //emptyList table
                var emptyListTable = new List<ReportTableView>();
                for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
                {
                    emptyListTable.Add(new ReportTableView
                    {
                        Date = i.ToString("dd/MM/yyyy"),
                        TotalAmount = 0,
                        Discount = 0,
                        FinalAmount = 0
                    });
                }
                //emptyList chart
                var emptyListChart = new List<ReportChartView>();
                for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
                {
                    emptyListChart.Add(new ReportChartView
                    {
                        Date = i.ToString("dd/MM/yyyy"),
                        TotalOrderTakeAway = 0,
                        TotalOrderAtStore = 0,
                        TotalOrderDelivery = 0
                    });
                }
                //lay du lieu tu database table datereport
                var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(sTime, eTime, storeId).ToArray();

                foreach (var item in dateReport)
                {
                    listDateReport.Add(new DateReportViewModel
                    {
                        Date = item.Date,
                        TotalOrder = item.TotalOrderAtStore + item.TotalOrderDelivery + item.TotalOrderTakeAway,
                        TotalOrderTakeAway = item.TotalOrderTakeAway,
                        FinalAmountTakeAway = item.FinalAmountTakeAway,
                        TotalOrderDelivery = item.TotalOrderDelivery,
                        FinalAmountDelivery = item.FinalAmountDelivery,
                        TotalOrderAtStore = item.TotalOrderAtStore,
                        FinalAmountAtStore = item.FinalAmountAtStore,
                        TotalAmount = item.FinalAmountAtStore + item.FinalAmountDelivery + item.FinalAmountTakeAway + item.Discount + item.DiscountOrderDetail,
                        Discount = item.Discount + item.DiscountOrderDetail,
                        FinalAmount = item.FinalAmountAtStore + item.FinalAmountDelivery + item.FinalAmountTakeAway
                    });
                }

                //So bill va doanh thu hom nay
                int OrdTakeAway = 0, OrdAtStore = 0, OrdDelivery = 0;
                double PriceOrdTakeAway = 0, PriceOrdAtStore = 0, PriceOrdDelivery = 0, discount = 0;
                foreach (var item in orders)
                {
                    if (item != null && item.OrderStatus == 2)
                    {

                        if (item.OrderType >= 4 && item.OrderType <= 6)
                        {
                            countOrder++;
                            //discount += (item.Discount + item.DiscountOrderDetail);
                        }

                        OrdTakeAway += (item.OrderType == 5) ? 1 : 0;
                        PriceOrdTakeAway += (item.OrderType == 5) ? item.FinalAmount : 0;

                        OrdAtStore += (item.OrderType == 4) ? 1 : 0;
                        PriceOrdAtStore += (item.OrderType == 4) ? item.FinalAmount : 0;

                        OrdDelivery += (item.OrderType == 6) ? 1 : 0;
                        PriceOrdDelivery += (item.OrderType == 6) ? item.FinalAmount : 0;

                    }
                }

                //group theo ngày table
                var resultTable = listDateReport.GroupBy(a => a.Date.ToString("dd/MM/yyyy")).Select(r => new ReportTableView
                {
                    Date = r.Key,
                    TotalOrder = r.Sum(a => a.TotalOrder),
                    TotalAmount = (double)r.Sum(a => a.TotalAmount),
                    FinalAmount = (double)r.Sum(a => a.FinalAmount),
                    Discount = (double)(r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail)),
                    TotalOrderTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                    FinalAmountTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                    TotalOrderDelivery = r.Sum(a => a.TotalOrderDelivery),
                    FinalAmountDelivery = r.Sum(a => a.FinalAmountDelivery),
                    TotalOrderAtStore = r.Sum(a => a.TotalOrderAtStore),
                    FinalAmountAtStore = r.Sum(a => a.FinalAmountAtStore)

                });
                //ngay hom nay table
                var tempTable = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToString("dd/MM/yyyy")).Select(r => new ReportTableView
                    {
                        Date = r.Key,
                        TotalAmount = PriceOrdTakeAway + PriceOrdDelivery + PriceOrdAtStore + (double)(r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail) + discount),
                        FinalAmount = PriceOrdTakeAway + PriceOrdDelivery + PriceOrdAtStore,
                        Discount = (double)(r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail) + discount),
                        TotalOrder = countOrder,
                        TotalOrderTakeAway = OrdTakeAway,
                        FinalAmountTakeAway = PriceOrdTakeAway,
                        TotalOrderDelivery = OrdDelivery,
                        FinalAmountDelivery = PriceOrdDelivery,
                        TotalOrderAtStore = OrdAtStore,
                        FinalAmountAtStore = PriceOrdAtStore
                    });
                if (eTime == to) resultTable = resultTable.Concat(tempTable);
                //merge voi emptyListTable
                if (resultTable != null) resultTable = emptyListTable.Concat(resultTable);
                else resultTable = emptyListTable;
                resultTable = resultTable.GroupBy(a => a.Date).
                    Select(r => new ReportTableView
                    {
                        Date = r.Key,
                        TotalOrderTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                        FinalAmountTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                        TotalOrderDelivery = r.Sum(a => a.TotalOrderDelivery),
                        FinalAmountDelivery = r.Sum(a => a.FinalAmountDelivery),
                        TotalOrderAtStore = r.Sum(a => a.TotalOrderAtStore),
                        FinalAmountAtStore = r.Sum(a => a.FinalAmountAtStore),
                        TotalOrder = r.Sum(a => a.TotalOrder),
                        TotalAmount = (double)r.Sum(a => a.TotalAmount),
                        FinalAmount = (double)r.Sum(a => a.FinalAmount),
                        Discount = (double)r.Sum(a => a.Discount)
                    });
                //list json table
                var list = resultTable.Select(a => new IConvertible[]
            {
                count++,
                a.Date,
                a.TotalOrderTakeAway,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalAmountTakeAway),
                a.TotalOrderAtStore,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalAmountAtStore),
                a.TotalOrderDelivery,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalAmountDelivery),
                a.TotalOrder,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalAmount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalAmount)
            }).ToArray();
                //group theo ngày chart
                var resultChart = dateReport.GroupBy(a => a.Date.ToString("dd/MM/yyyy")).
                    Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery)
                    });
                //ngay hom nay chart
                int takeAway = 0, atStore = 0, delivery = 0;
                foreach (var i in orders)
                {
                    if (i != null && i.OrderStatus == 2)
                    {
                        if (i.OrderType == 5)
                        {
                            takeAway += 1;
                        }
                        if (i.OrderType == 4)
                        {
                            atStore += 1;
                        }
                        if (i.OrderType == 6)
                        {
                            delivery += 1;
                        }
                    }
                }
                var tempChart = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToString("dd/MM/yyyy")).Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = takeAway,
                        TotalOrderAtStore = atStore,
                        TotalOrderDelivery = delivery
                    });
                if (eTime == to) resultChart = resultChart.Concat(tempChart);
                //merge voi emptyListChart
                if (resultChart != null) resultChart = emptyListChart.Concat(resultChart);
                else resultChart = emptyListChart;
                resultChart = resultChart.GroupBy(a => a.Date).
                    Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery)
                    });
                //json chart
                var _DateName = resultChart.Select(a => a.Date).ToArray();
                var _TakeAway = resultChart.Select(a => a.TotalOrderTakeAway).ToArray();
                var _AtStore = resultChart.Select(a => a.TotalOrderAtStore).ToArray();
                var _Delivery = resultChart.Select(a => a.TotalOrderDelivery).ToArray();

                return Json(new
                {
                    dataList = list,
                    dataChart = new
                    {
                        DateName = _DateName,
                        TakeAway = _TakeAway,
                        AtStore = _AtStore,
                        Delivery = _Delivery
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public JsonResult DateReportTableGroup(string startTime, string endTime, int groupIdd, int brandId)
        {
            var listDateReport = new List<DateReportViewModel>();
            var dateReportApi = new DateReportApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            try
            {
                //emptyList table
                var emptyListTable = new List<ReportTableView>();
                for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
                {
                    emptyListTable.Add(new ReportTableView
                    {
                        Date = i.ToShortDateString(),
                        TotalAmount = 0,
                        Discount = 0,
                        FinalAmount = 0
                    });
                }
                //emptyList chart
                var emptyListChart = new List<ReportChartView>();
                for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
                {
                    emptyListChart.Add(new ReportChartView
                    {
                        Date = i.ToShortDateString(),
                        TotalOrderTakeAway = 0,
                        TotalOrderAtStore = 0,
                        TotalOrderDelivery = 0
                    });
                }
                //lay data tu database datereport
                var storeGroupMap = new StoreGroupMappingApi();
                var store = storeGroupMap.GetStoreGroupMappingsByGroupID(groupIdd).ToArray();
                var dateReport = new List<DateReport>();
                foreach (var item in store)
                {
                    dateReport.AddRange(dateReportApi.GetDateReportTimeRangeAndStore(sTime, eTime, item.Store.ID));
                }
                foreach (var item in dateReport)
                {
                    listDateReport.Add(new DateReportViewModel
                    {
                        Date = item.Date,
                        TotalAmount = item.TotalAmount,
                        Discount = item.Discount,
                        FinalAmount = item.FinalAmount
                    });
                }
                //group theo ngày table
                var resultTable = listDateReport.GroupBy(a => a.Date.ToShortDateString()).Select(b => new ReportTableView
                {
                    Date = b.Key,
                    TotalAmount = (double)b.Sum(c => c.TotalAmount),
                    Discount = (double)b.Sum(c => c.Discount),
                    FinalAmount = (double)b.Sum(c => c.FinalAmount)
                });
                //ngay hom nay table
                var orderApi = new OrderApi();
                var form = DateTime.Now.GetStartOfDate();
                var to = DateTime.Now.GetEndOfDate();
                var orders = new List<Order>();
                foreach (var item in store)
                {
                    orders.AddRange(orderApi.GetOrdersByTimeRange(item.StoreID, form, to, brandId));
                }
                var tempTable = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToShortDateString()).Select(b => new ReportTableView
                    {
                        Date = b.Key,
                        TotalAmount = b.Sum(c => c.TotalAmount),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount)
                    });
                if (eTime == to) resultTable = resultTable.Concat(tempTable);
                //merge voi emptyListTable
                if (resultTable != null) resultTable = emptyListTable.Concat(resultTable);
                else resultTable = emptyListTable;
                resultTable = resultTable.GroupBy(a => a.Date).
                    Select(b => new ReportTableView
                    {
                        Date = b.Key,
                        TotalAmount = b.Sum(c => c.TotalAmount),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount)
                    });
                //list json table
                var list = resultTable.Select(a => new IConvertible[]
            {
                a.Date,
                a.TotalAmount,
                a.Discount,
                a.FinalAmount,
            }).ToArray();
                //group theo ngày chart
                var resultChart = dateReport.GroupBy(a => (a.Date.Day + "/" + a.Date.Month + "/" + a.Date.Year)).
                    Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery)
                    });
                //ngay hom nay chart
                int takeAway = 0, atStore = 0, delivery = 0;
                foreach (var i in orders)
                {
                    if (i != null && i.OrderStatus == 2)
                    {
                        if (i.OrderType == 5)
                        {
                            takeAway += 1;
                        }
                        if (i.OrderType == 4)
                        {
                            atStore += 1;
                        }
                        if (i.OrderType == 6)
                        {
                            delivery += 1;
                        }
                    }
                }
                var tempChart = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToShortDateString()).Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = takeAway,
                        TotalOrderAtStore = atStore,
                        TotalOrderDelivery = delivery
                    });
                if (eTime == to) resultChart.Concat(tempChart);
                //merge voi emptyListChart
                if (resultChart != null) resultChart = emptyListChart.Concat(resultChart);
                else resultChart = emptyListChart;
                resultChart = resultChart.GroupBy(a => a.Date).
                    Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery)
                    });
                //json chart
                var _DateName = resultChart.Select(a => a.Date).ToArray();
                var _TakeAway = resultChart.Select(a => a.TotalOrderTakeAway).ToArray();
                var _AtStore = resultChart.Select(a => a.TotalOrderAtStore).ToArray();
                var _Delivery = resultChart.Select(a => a.TotalOrderDelivery).ToArray();

                return Json(new
                {
                    dataList = list,
                    dataChart = new
                    {
                        DateName = _DateName,
                        TakeAway = _TakeAway,
                        AtStore = _AtStore,
                        Delivery = _Delivery
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public JsonResult DateReportTableAll(string startTime, string endTime, int brandId)
        {
            var listDateReport = new List<DateReportViewModel>();
            var dateReportApi = new DateReportApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            try
            {
                //emptyList table
                var emptyListTable = new List<ReportTableView>();
                for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
                {
                    emptyListTable.Add(new ReportTableView
                    {
                        Date = i.ToShortDateString(),
                        TotalAmount = 0,
                        Discount = 0,
                        FinalAmount = 0
                    });
                }
                //emptyList chart
                var emptyListChart = new List<ReportChartView>();
                for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
                {
                    emptyListChart.Add(new ReportChartView
                    {
                        Date = i.ToShortDateString(),
                        TotalOrderTakeAway = 0,
                        TotalOrderAtStore = 0,
                        TotalOrderDelivery = 0
                    });
                }
                //lay du lieu duoi table datereport
                var dateReport = dateReportApi.GetDateReportTimeRangeAndBrand(sTime, eTime, brandId).ToArray();
                foreach (var item in dateReport)
                {
                    listDateReport.Add(new DateReportViewModel
                    {
                        Date = item.Date,
                        TotalAmount = item.TotalAmount,
                        Discount = item.Discount,
                        FinalAmount = item.FinalAmount
                    });
                }
                //group theo ngày table
                var resultTable = listDateReport.GroupBy(a => a.Date.ToShortDateString()).Select(b => new ReportTableView
                {
                    Date = b.Key,
                    TotalAmount = (double)b.Sum(c => c.TotalAmount),
                    Discount = (double)b.Sum(c => c.Discount),
                    FinalAmount = (double)b.Sum(c => c.FinalAmount)
                });
                //ngay hom nay table                
                var orderApi = new OrderApi();
                var form = DateTime.Now.GetStartOfDate();
                var to = DateTime.Now.GetEndOfDate();
                var orders = orderApi.GetAllOrdersByDate(form, to, brandId);
                var tempTable = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToShortDateString()).Select(b => new ReportTableView
                    {
                        Date = b.Key,
                        TotalAmount = b.Sum(c => c.TotalAmount),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount)
                    });
                if (eTime == to) resultTable = resultTable.Concat(tempTable);
                //merge voi emptyListTable
                if (resultTable != null) resultTable = emptyListTable.Concat(resultTable);
                else resultTable = emptyListTable;
                resultTable = resultTable.GroupBy(a => a.Date).
                    Select(b => new ReportTableView
                    {
                        Date = b.Key,
                        TotalAmount = b.Sum(c => c.TotalAmount),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount)
                    });
                //lay json result table
                var list = resultTable.Select(a => new IConvertible[]
            {
                a.Date,
                a.TotalAmount,
                a.Discount,
                a.FinalAmount,
            }).ToArray();
                //group theo ngày chart
                var resultChart = dateReport.GroupBy(a => a.Date.ToShortDateString()).
                    Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery)
                    });
                //ngay hom nay chart
                int takeAway = 0, atStore = 0, delivery = 0;
                foreach (var i in orders)
                {
                    if (i != null && i.OrderStatus == 2)
                    {
                        if (i.OrderType == 5)
                        {
                            takeAway += 1;
                        }
                        if (i.OrderType == 4)
                        {
                            atStore += 1;
                        }
                        if (i.OrderType == 6)
                        {
                            delivery += 1;
                        }
                    }
                }
                var tempChart = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToShortDateString()).Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = takeAway,
                        TotalOrderAtStore = atStore,
                        TotalOrderDelivery = delivery
                    });
                if (eTime == to) resultChart = resultChart.Concat(tempChart);
                //merge voi emptyListChart
                if (resultChart != null) resultChart = emptyListChart.Concat(resultChart);
                else resultChart = emptyListChart;
                resultChart = resultChart.GroupBy(a => a.Date).
                    Select(b => new ReportChartView
                    {
                        Date = b.Key,
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery)
                    });
                //json chart
                var _DateName = resultChart.Select(a => a.Date).ToArray();
                var _TakeAway = resultChart.Select(a => a.TotalOrderTakeAway).ToArray();
                var _AtStore = resultChart.Select(a => a.TotalOrderAtStore).ToArray();
                var _Delivery = resultChart.Select(a => a.TotalOrderDelivery).ToArray();

                return Json(new
                {
                    dataList = list,
                    dataChart = new
                    {
                        DateName = _DateName,
                        TakeAway = _TakeAway,
                        AtStore = _AtStore,
                        Delivery = _Delivery
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public ActionResult ExportDateStoreReportToExcel(int brandId, string startTime, string endTime, int storeId)
        {
            var listDateReport = new List<DateReportViewModel>();
            var dateReportApi = new DateReportApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            #region Get data
            int countOrder = 0;
            var orderApi = new OrderApi();
            var form = DateTime.Now.GetStartOfDate();
            var to = DateTime.Now.GetEndOfDate();
            IEnumerable<Order> orders = orderApi.GetOrdersByTimeRange(storeId, form, to, brandId);
            //emptyList table
            var emptyListTable = new List<ReportTableView>();
            for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
            {
                emptyListTable.Add(new ReportTableView
                {
                    Date = i.ToString("dd/MM/yyyy"),
                    TotalAmount = 0,
                    Discount = 0,
                    FinalAmount = 0
                });
            }
            //lay du lieu tu database table datereport
            var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(sTime, eTime, storeId).ToArray();

            foreach (var item in dateReport)
            {
                listDateReport.Add(new DateReportViewModel
                {
                    Date = item.Date,
                    TotalOrder = item.TotalOrderAtStore + item.TotalOrderDelivery + item.TotalOrderTakeAway,
                    TotalOrderTakeAway = item.TotalOrderTakeAway,
                    FinalAmountTakeAway = item.FinalAmountTakeAway,
                    TotalOrderDelivery = item.TotalOrderDelivery,
                    FinalAmountDelivery = item.FinalAmountDelivery,
                    TotalOrderAtStore = item.TotalOrderAtStore,
                    FinalAmountAtStore = item.FinalAmountAtStore,
                    TotalAmount = item.FinalAmountAtStore + item.FinalAmountDelivery + item.FinalAmountTakeAway + item.Discount + item.DiscountOrderDetail,
                    Discount = item.Discount + item.DiscountOrderDetail,
                    FinalAmount = item.FinalAmountAtStore + item.FinalAmountDelivery + item.FinalAmountTakeAway
                });
            }

            //So bill va doanh thu hom nay
            int OrdTakeAway = 0, OrdAtStore = 0, OrdDelivery = 0;
            double PriceOrdTakeAway = 0, PriceOrdAtStore = 0, PriceOrdDelivery = 0, discount = 0;
            foreach (var item in orders)
            {
                if (item != null && item.OrderStatus == 2)
                {

                    if (item.OrderType >= 4 && item.OrderType <= 6)
                    {
                        countOrder++;
                        //discount += (item.DiscountOrderDetail + item.Discount);
                    }

                    OrdTakeAway += (item.OrderType == 5) ? 1 : 0;
                    PriceOrdTakeAway += (item.OrderType == 5) ? item.FinalAmount : 0;

                    OrdAtStore += (item.OrderType == 4) ? 1 : 0;
                    PriceOrdAtStore += (item.OrderType == 4) ? item.FinalAmount : 0;

                    OrdDelivery += (item.OrderType == 6) ? 1 : 0;
                    PriceOrdDelivery += (item.OrderType == 6) ? item.FinalAmount : 0;

                }
            }

            //group theo ngày table
            var resultTable = listDateReport.GroupBy(a => a.Date.ToString("dd/MM/yyyy")).Select(r => new ReportTableView
            {
                Date = r.Key,
                TotalOrder = r.Sum(a => a.TotalOrder),
                TotalAmount = (double)r.Sum(a => a.TotalAmount),
                FinalAmount = (double)r.Sum(a => a.FinalAmount),
                Discount = (double)(r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail)),
                TotalOrderTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                FinalAmountTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                TotalOrderDelivery = r.Sum(a => a.TotalOrderDelivery),
                FinalAmountDelivery = r.Sum(a => a.FinalAmountDelivery),
                TotalOrderAtStore = r.Sum(a => a.TotalOrderAtStore),
                FinalAmountAtStore = r.Sum(a => a.FinalAmountAtStore)

            });
            //ngay hom nay table
            var tempTable = orders.Where(a => a.OrderStatus == 2).
                GroupBy(a => a.CheckInDate.Value.ToString("dd/MM/yyyy")).Select(r => new ReportTableView
                {
                    Date = r.Key,
                    TotalAmount = PriceOrdTakeAway + PriceOrdDelivery + PriceOrdAtStore + (double)(discount + r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail)),
                    FinalAmount = PriceOrdTakeAway + PriceOrdDelivery + PriceOrdAtStore,
                    Discount = (double)(r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail) + discount),
                    TotalOrder = countOrder,
                    TotalOrderTakeAway = OrdTakeAway,
                    FinalAmountTakeAway = PriceOrdTakeAway,
                    TotalOrderDelivery = OrdDelivery,
                    FinalAmountDelivery = PriceOrdDelivery,
                    TotalOrderAtStore = OrdAtStore,
                    FinalAmountAtStore = PriceOrdAtStore
                });
            if (eTime == to) resultTable = resultTable.Concat(tempTable);
            //merge voi emptyListTable
            if (resultTable != null) resultTable = emptyListTable.Concat(resultTable);
            else resultTable = emptyListTable;
            resultTable = resultTable.GroupBy(a => a.Date).
                Select(r => new ReportTableView
                {
                    Date = r.Key,
                    TotalOrderTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                    FinalAmountTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                    TotalOrderDelivery = r.Sum(a => a.TotalOrderDelivery),
                    FinalAmountDelivery = r.Sum(a => a.FinalAmountDelivery),
                    TotalOrderAtStore = r.Sum(a => a.TotalOrderAtStore),
                    FinalAmountAtStore = r.Sum(a => a.FinalAmountAtStore),
                    TotalOrder = r.Sum(a => a.TotalOrder),
                    TotalAmount = (double)r.Sum(a => a.TotalAmount),
                    FinalAmount = (double)r.Sum(a => a.FinalAmount),
                    Discount = (double)r.Sum(a => a.Discount)
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
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill mang đi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu mang đi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill tại store";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu tai store";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill giao hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu giao hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng số bill";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng doanh thu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
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
                foreach (var data in resultTable)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Date;

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderTakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmountTakeAway);

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderAtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmountAtStore);

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderDelivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmountDelivery);

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalAmount);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.Discount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmount);
                    StartHeaderChar = 'A';
                }
                string storeName;
                var storeApi = new StoreApi();
                if (storeId > 0)
                {
                    storeName = storeApi.GetStoreNameByID(storeId);
                }
                else
                    storeName = "Tổng quan các cửa hàng";
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoTheoNgay_" + "Store_" + storeName + dateRange + ".xlsx";
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
        public ActionResult ExportDateStoreReportToExcel2(int brandId, string startTime, string endTime, int storeId, int selectedStoreId)
        {
            var listDateReport = new List<DateReportViewModel>();
            var dateReportApi = new DateReportApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            #region Get data
            int countOrder = 0;
            var orderApi = new OrderApi();
            var form = sTime;
            var to = eTime;
            IEnumerable <Order> orders = null;
            if (selectedStoreId == 0)
            {
                orders = orderApi.GetAllOrderByDate2(form, to, brandId);
            }
            else
            {
                orders = orderApi.GetOrdersByTimeRange(selectedStoreId, form, to, brandId);
            }
            //emptyList table
            var emptyListTable = new List<ReportTableView>();
            for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
            {
                emptyListTable.Add(new ReportTableView
                {
                    Date = i.ToString("dd/MM/yyyy"),
                    TotalAmount = 0,
                    Discount = 0,
                    FinalAmount = 0
                });
            }
            //lay du lieu tu database table datereport
            var dateReport = dateReportApi.GetDateReportTimeRangeAndStore(sTime, eTime, storeId).ToArray();

            foreach (var item in dateReport)
            {
                listDateReport.Add(new DateReportViewModel
                {
                    Date = item.Date,
                    TotalOrder = item.TotalOrderAtStore + item.TotalOrderDelivery + item.TotalOrderTakeAway,
                    TotalOrderTakeAway = item.TotalOrderTakeAway,
                    FinalAmountTakeAway = item.FinalAmountTakeAway,
                    TotalOrderDelivery = item.TotalOrderDelivery,
                    FinalAmountDelivery = item.FinalAmountDelivery,
                    TotalOrderAtStore = item.TotalOrderAtStore,
                    FinalAmountAtStore = item.FinalAmountAtStore,
                    TotalAmount = item.FinalAmountAtStore + item.FinalAmountDelivery + item.FinalAmountTakeAway + item.Discount + item.DiscountOrderDetail,
                    Discount = item.Discount + item.DiscountOrderDetail,
                    FinalAmount = item.FinalAmountAtStore + item.FinalAmountDelivery + item.FinalAmountTakeAway
                });
            }

            //So bill va doanh thu hom nay
            int OrdTakeAway = 0, OrdAtStore = 0, OrdDelivery = 0;
            double PriceOrdTakeAway = 0, PriceOrdAtStore = 0, PriceOrdDelivery = 0, discount = 0;
            foreach (var item in orders)
            {
                if (item != null && item.OrderStatus == 2)
                {

                    if (item.OrderType >= 4 && item.OrderType <= 6)
                    {
                        countOrder++;
                        //discount += (item.DiscountOrderDetail + item.Discount);
                    }

                    OrdTakeAway += (item.OrderType == 5) ? 1 : 0;
                    PriceOrdTakeAway += (item.OrderType == 5) ? item.FinalAmount : 0;

                    OrdAtStore += (item.OrderType == 4) ? 1 : 0;
                    PriceOrdAtStore += (item.OrderType == 4) ? item.FinalAmount : 0;

                    OrdDelivery += (item.OrderType == 6) ? 1 : 0;
                    PriceOrdDelivery += (item.OrderType == 6) ? item.FinalAmount : 0;

                }
            }

            //group theo ngày table
            var resultTable = listDateReport.GroupBy(a => a.Date.ToString("dd/MM/yyyy")).Select(r => new ReportTableView
            {
                Date = r.Key,
                TotalOrder = r.Sum(a => a.TotalOrder),
                TotalAmount = (double)r.Sum(a => a.TotalAmount),
                FinalAmount = (double)r.Sum(a => a.FinalAmount),
                Discount = (double)(r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail)),
                TotalOrderTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                FinalAmountTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                TotalOrderDelivery = r.Sum(a => a.TotalOrderDelivery),
                FinalAmountDelivery = r.Sum(a => a.FinalAmountDelivery),
                TotalOrderAtStore = r.Sum(a => a.TotalOrderAtStore),
                FinalAmountAtStore = r.Sum(a => a.FinalAmountAtStore)

            });
            //ngay hom nay table
            var tempTable = orders.Where(a => a.OrderStatus == 2).
                GroupBy(a => a.CheckInDate.Value.ToString("dd/MM/yyyy")).Select(r => new ReportTableView
                {
                    Date = r.Key,
                    TotalAmount = PriceOrdTakeAway + PriceOrdDelivery + PriceOrdAtStore + (double)(discount + r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail)),
                    FinalAmount = PriceOrdTakeAway + PriceOrdDelivery + PriceOrdAtStore,
                    Discount = (double)(r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail) + discount),
                    TotalOrder = countOrder,
                    TotalOrderTakeAway = OrdTakeAway,
                    FinalAmountTakeAway = PriceOrdTakeAway,
                    TotalOrderDelivery = OrdDelivery,
                    FinalAmountDelivery = PriceOrdDelivery,
                    TotalOrderAtStore = OrdAtStore,
                    FinalAmountAtStore = PriceOrdAtStore
                });
            if (eTime == to) resultTable = resultTable.Concat(tempTable);
            //merge voi emptyListTable
            if (resultTable != null) resultTable = emptyListTable.Concat(resultTable);
            else resultTable = emptyListTable;
            resultTable = resultTable.GroupBy(a => a.Date).
                Select(r => new ReportTableView
                {
                    Date = r.Key,
                    TotalOrderTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                    FinalAmountTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                    TotalOrderDelivery = r.Sum(a => a.TotalOrderDelivery),
                    FinalAmountDelivery = r.Sum(a => a.FinalAmountDelivery),
                    TotalOrderAtStore = r.Sum(a => a.TotalOrderAtStore),
                    FinalAmountAtStore = r.Sum(a => a.FinalAmountAtStore),
                    TotalOrder = r.Sum(a => a.TotalOrder),
                    TotalAmount = (double)r.Sum(a => a.TotalAmount),
                    FinalAmount = (double)r.Sum(a => a.FinalAmount),
                    Discount = (double)r.Sum(a => a.Discount)
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
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill mang đi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu mang đi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill tại store";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu tai store";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill giao hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu giao hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng số bill";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng doanh thu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
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
                foreach (var data in resultTable)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Date;

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderTakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmountTakeAway);

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderAtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmountAtStore);

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderDelivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmountDelivery);

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalAmount);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.Discount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmount);
                    StartHeaderChar = 'A';
                }
                string storeName;
                var storeApi = new StoreApi();
                if (storeId == 0)
                {
                    if(selectedStoreId == 0)
                    {
                        storeName = "Hệ thống";
                    }
                    else
                    {
                        storeName = storeApi.GetStoreNameByID(selectedStoreId);
                    }
                }
                else
                {
                    storeName = storeApi.GetStoreNameByID(selectedStoreId);
                }
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoTheoNgay_" + "Store_" + storeName + dateRange + ".xlsx";
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
        public ActionResult ExportDateGroupReportToExcel(string startTime, string endTime, int groupIdd, int brandId)
        {
            var listDateReport = new List<DateReportViewModel>();
            var dateReportApi = new DateReportApi();
            var storeGroupMap = new StoreGroupMappingApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            #region Get data
            //emptyList table
            var emptyListTable = new List<ReportExcelView>();
            for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
            {
                emptyListTable.Add(new ReportExcelView
                {
                    Date = i.ToShortDateString(),
                    TotalOrder = 0,
                    Discount = 0,
                    FinalAmount = 0,
                    TotalOrderTakeAway = 0,
                    TotalOrderDelivery = 0,
                    TotalOrderAtStore = 0
                });
            }
            //lay data tu datable table datereport
            var dateReport = new List<DateReport>();
            var store = storeGroupMap.GetStoreGroupMappingsByGroupID(groupIdd).ToArray();
            foreach (var item in store)
            {
                dateReport.AddRange(dateReportApi.GetDateReportTimeRangeAndStore(sTime, eTime, item.StoreID));
            }
            foreach (var item in dateReport)
            {
                listDateReport.Add(new DateReportViewModel
                {
                    Date = item.Date,
                    TotalOrder = item.TotalOrder,
                    Discount = item.Discount,
                    FinalAmount = item.FinalAmount,
                    TotalOrderTakeAway = item.TotalOrderTakeAway,
                    TotalOrderDelivery = item.TotalOrderDelivery,
                    TotalOrderAtStore = item.TotalOrderAtStore
                });
            }
            //group theo ngày
            var resultTable = listDateReport.GroupBy(a => a.Date.ToShortDateString()).Select(b => new ReportExcelView
            {
                Date = b.Key,
                TotalOrder = b.Sum(c => c.TotalOrder),
                Discount = (double)b.Sum(c => c.Discount),
                FinalAmount = (double)b.Sum(c => c.FinalAmount),
                TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery),
                TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore)
            });
            //ngay hom nay           
            var form = DateTime.Now.GetStartOfDate();
            var to = DateTime.Now.GetEndOfDate();
            if (eTime == to)
            {
                var orderApi = new OrderApi();
                var orders = new List<Order>();
                foreach (var item in store)
                {
                    orders.AddRange(orderApi.GetOrdersByTimeRange(item.StoreID, form, to, brandId));
                }
                int takeAway = 0, atStore = 0, delivery = 0;
                foreach (var i in orders)
                {
                    if (i != null && i.OrderStatus == 2)
                    {
                        if (i.OrderType == 5)
                        {
                            takeAway += 1;
                        }
                        if (i.OrderType == 4)
                        {
                            atStore += 1;
                        }
                        if (i.OrderType == 6)
                        {
                            delivery += 1;
                        }
                    }
                }
                var temp = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToShortDateString()).Select(b => new ReportExcelView
                    {
                        Date = b.Key,
                        TotalOrder = orders.Count(),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount),
                        TotalOrderTakeAway = takeAway,
                        TotalOrderDelivery = delivery,
                        TotalOrderAtStore = atStore
                    });
                resultTable = resultTable.Concat(temp);
                //merge voi emptyListTable
                if (resultTable != null) resultTable = emptyListTable.Concat(resultTable);
                else resultTable = emptyListTable;
                resultTable = resultTable.GroupBy(a => a.Date).
                    Select(b => new ReportExcelView
                    {
                        Date = b.Key,
                        TotalOrder = b.Sum(c => c.TotalOrder),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount),
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore)
                    });
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
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
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
                foreach (var data in resultTable)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Date;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderTakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderAtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderDelivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.Discount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmount);
                    StartHeaderChar = 'A';
                }
                string groupName;
                var storeGroupApi = new StoreGroupApi();
                groupName = storeGroupApi.GetStoreGroupByID(groupIdd).GroupName;
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoTheoNgay_" + "Group_" + groupName + dateRange + ".xlsx";
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
        public ActionResult ExportDateAllReportToExcel(string startTime, string endTime, int brandId)
        {
            var listDateReport = new List<DateReportViewModel>();
            var dateReportApi = new DateReportApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            #region Get data
            //emptyList table
            var emptyListTable = new List<ReportExcelView>();
            for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
            {
                emptyListTable.Add(new ReportExcelView
                {
                    Date = i.ToShortDateString(),
                    TotalOrder = 0,
                    Discount = 0,
                    FinalAmount = 0,
                    TotalOrderTakeAway = 0,
                    TotalOrderDelivery = 0,
                    TotalOrderAtStore = 0
                });
            }
            //lay data tren database table datereport
            var dateReport = dateReportApi.GetDateReportTimeRangeAndBrand(sTime, eTime, brandId).ToArray();
            foreach (var item in dateReport)
            {
                listDateReport.Add(new DateReportViewModel
                {
                    Date = item.Date,
                    TotalOrder = item.TotalOrder,
                    Discount = item.Discount,
                    FinalAmount = item.FinalAmount,
                    TotalOrderTakeAway = item.TotalOrderTakeAway,
                    TotalOrderDelivery = item.TotalOrderDelivery,
                    TotalOrderAtStore = item.TotalOrderAtStore
                });
            }
            //group theo ngày
            var resultTable = listDateReport.GroupBy(a => a.Date.ToShortDateString()).Select(b => new ReportExcelView
            {
                Date = b.Key,
                TotalOrder = b.Sum(c => c.TotalOrder),
                Discount = (double)b.Sum(c => c.Discount),
                FinalAmount = (double)b.Sum(c => c.FinalAmount),
                TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery),
                TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore)
            });
            //ngay hom nay           
            var form = DateTime.Now.GetStartOfDate();
            var to = DateTime.Now.GetEndOfDate();
            if (eTime == to)
            {
                var orderApi = new OrderApi();
                var orders = orderApi.GetAllOrdersByDate(form, to, brandId);
                int takeAway = 0, atStore = 0, delivery = 0;
                foreach (var i in orders)
                {
                    if (i != null && i.OrderStatus == 2)
                    {
                        if (i.OrderType == 5)
                        {
                            takeAway += 1;
                        }
                        if (i.OrderType == 4)
                        {
                            atStore += 1;
                        }
                        if (i.OrderType == 6)
                        {
                            delivery += 1;
                        }
                    }
                }
                var temp = orders.Where(a => a.OrderStatus == 2).
                    GroupBy(a => a.CheckInDate.Value.ToShortDateString()).Select(b => new ReportExcelView
                    {
                        Date = b.Key,
                        TotalOrder = orders.Count(),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount),
                        TotalOrderTakeAway = takeAway,
                        TotalOrderDelivery = delivery,
                        TotalOrderAtStore = atStore
                    });
                resultTable = resultTable.Concat(temp);
                //merge voi emptyListTable
                if (resultTable != null) resultTable = emptyListTable.Concat(resultTable);
                else resultTable = emptyListTable;
                resultTable = resultTable.GroupBy(a => a.Date).
                    Select(b => new ReportExcelView
                    {
                        Date = b.Key,
                        TotalOrder = b.Sum(c => c.TotalOrder),
                        Discount = b.Sum(c => c.Discount),
                        FinalAmount = b.Sum(c => c.FinalAmount),
                        TotalOrderTakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        TotalOrderDelivery = b.Sum(c => c.TotalOrderDelivery),
                        TotalOrderAtStore = b.Sum(c => c.TotalOrderAtStore)
                    });
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
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
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
                foreach (var data in resultTable)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Date;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderTakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderAtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrderDelivery;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.Discount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalAmount);
                    StartHeaderChar = 'A';
                }
                string brandName;
                var brandApi = new BrandApi();
                brandName = brandApi.GetBrandById(brandId).BrandName;
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoTheoNgay_" + "Brand_" + brandName + dateRange + ".xlsx";
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
        public class ReportTableView
        {
            public string Date { get; set; }
            public int TotalOrder { get; set; }
            public int TotalOrderAtStore { get; set; }
            public int TotalOrderTakeAway { get; set; }
            public int TotalOrderDelivery { get; set; }
            public virtual Nullable<double> FinalAmountAtStore { get; set; }
            public virtual Nullable<double> FinalAmountTakeAway { get; set; }
            public virtual Nullable<double> FinalAmountDelivery { get; set; }
            public double TotalAmount { get; set; }
            public double Discount { get; set; }
            public double FinalAmount { get; set; }
        }
        public class ReportChartView
        {
            public string Date { get; set; }
            public int TotalOrderTakeAway { get; set; }
            public int TotalOrderAtStore { get; set; }
            public int TotalOrderDelivery { get; set; }
        }
        public class ReportExcelView
        {
            public string Date { get; set; }
            public int TotalOrder { get; set; }
            public double Discount { get; set; }
            public double FinalAmount { get; set; }
            public int TotalOrderTakeAway { get; set; }
            public int TotalOrderDelivery { get; set; }
            public int TotalOrderAtStore { get; set; }
        }
        #endregion

        #region Báo cáo theo thứ
        public ActionResult DayOfWeekReport(int storeId, int brandId)
        {
            var storeApi = new StoreApi();
            var store = storeApi.GetActiveStoreByBrandId(brandId);
            ViewBag.storeId = storeId.ToString();

            //return this.View(store);
            return PartialView("_DayOfWeekReport", store);
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
                    //rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                    //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    rents = orderApi.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    //rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                    //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    rents = orderApi.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
                }


                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                    Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
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

                    var orderDiscount = result.Where(r => r.OrderType >= 4 && r.OrderType <= 6 && r.OrderTime.Value.DayOfWeek == item.Day);

                    item.Discount = orderDiscount.Sum(a => a.Discount);
                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                    item.TotalPrice = item.Discount + item.FinalPrice;
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
                    Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
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

                    var orderDiscount = result.Where(r => r.OrderType >= 4 && r.OrderType <= 6 && r.OrderTime.Value.DayOfWeek == item.Day);

                    item.Discount = orderDiscount.Sum(a => a.Discount);
                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                    item.TotalPrice = item.Discount + item.FinalPrice;
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
                        "{0:0,0}", item.TotalPrice),
                    j = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", item.Discount),
                    k = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", item.FinalPrice),
                });
            }

            #endregion

            return list;
        }
        public ActionResult ReportDayOfWeekExportExcelEPPlus(int brandId, string startTime, string endTime, int storeIdReport)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo theo thứ");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = ExportDayOfWeekTableToExcel(brandId, startTime, endTime, storeIdReport);
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thứ";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu(Mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Tại store)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu(Tại store)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Giao hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu(Giao hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng số bill";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng doanh thu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tiền giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng doanh thu sau giảm giá";
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
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.i;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.j;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.k;
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
                startTime = startTime.Replace('/', '-');

                endTime = endTime.Replace('/', '-');
                var fileDownloadName = "Báo cáo theo thứ từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        public JsonResult LoadDayOfWeekReportOneStore(JQueryDataTableParamModel param, string startTime, string endTime, int storeIdReport, int brandId)
        {
            var orderApi = new OrderApi();
            var dayofweekReport = new List<TempDayOfWeekReportModel>();
            int count = 1;
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

                IEnumerable<Order> rents;
                rents = orderApi.GetOrdersByTimeRange(storeIdReport, startDate, endDate, brandId)
                        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                    Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
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

                    var orderDiscount = result.Where(r => r.OrderType >= 4 && r.OrderType <= 6 && r.OrderTime.Value.DayOfWeek == item.Day);

                    item.Discount = orderDiscount.Sum(a => a.Discount);
                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                    item.TotalPrice = item.Discount + item.FinalPrice;
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                TimeSpan spanTime = endDate - startDate;

                IEnumerable<Order> rents;

                //rents = orderApi.GetOrdersByTimeRange(storeIdReport, startDate, endDate, brandId)
                //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                rents = orderApi.GetOrdersByTimeRange(storeIdReport, startDate, endDate, brandId)
                   .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);

                var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckInDate }).Select(r => new
                {
                    OrderType = r.Key.OrderType,
                    OrderTime = r.Key.Time,
                    TotalOrder = r.Count(),
                    Money = r.Sum(a => a.FinalAmount),
                    Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
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

                    var orderDiscount = result.Where(r => r.OrderType >= 4 && r.OrderType <= 6 && r.OrderTime.Value.DayOfWeek == item.Day);

                    item.Discount = orderDiscount.Sum(a => a.Discount);
                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                    item.TotalPrice = item.Discount + item.FinalPrice;
                }
            }

            var list = dayofweekReport.Select(a => new IConvertible[]
            {
                count++,
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
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)
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
        public JsonResult ExportDayOfWeekOneStoreTableToExcel(JQueryDataTableParamModel param, string startTime, string endTime, int storeId, int brandId)
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

                IEnumerable<Order> rents;

                rents = orderApi.GetRentsByTimeRange(storeId, startDate, endDate, brandId)
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
                    item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                }
            }
            else
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();

                TimeSpan spanTime = endDate - startDate;

                IEnumerable<Order> rents;

                rents = orderApi.GetRentsByTimeRange(storeId, startDate, endDate, brandId)
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
                    item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
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
                        "{0:0,0}", a.FinalPrice)

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
        public class TempDayOfWeekReportModel
        {
            public System.DayOfWeek Day { get; set; }
            public string DayOfWeek { get; set; }
            public int TakeAway { get; set; }
            public double PriceTakeAway { get; set; }
            public int AtStore { get; set; }
            public double PriceAtStore { get; set; }
            public int Delivery { get; set; }
            public double PriceDelivery { get; set; }
            public double Discount { get; set; }
            public double TotalPrice { get; set; }
            public int TotalQuantity { get; set; }
            public double FinalPrice { get; set; }
        }
        public class TempSystemRevenueReportItem
        {
            public string StartTime { get; set; }
            public double TotalAmount { get; set; }
            public double FinalAmount { get; set; }
            public double TotalDiscountFee { get; set; }
        }
        #endregion

        #region Hour report
        public ActionResult HourReport()
        {
            //return View();
            return PartialView("_HourReport");
        }

        public JsonResult LoadHourReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            var dateNow = Utils.GetCurrentDateTime();
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

            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();
            TimeSpan spanTime = endDate - startDate;
            int count = 1;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            if (storeId > 0)
            {
                //rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                                    .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
            }
            else
            {
                //rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                    .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
            }

            var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
            {
                OrderType = r.Key.OrderType,
                OrderTime = r.Key.Time,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
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

                var orderDiscount = result.Where(r => r.OrderType >= 4 && r.OrderType <= 6 && r.OrderTime == item.StartTime);

                item.Discount = orderDiscount.Sum(a => a.Discount);
                item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                item.TotalPrice = item.Discount + item.FinalPrice;
            }

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
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)

            }).ToList();
            var _Time = hourReport.Select(a => a.StartTime + ":00 - " + a.EndTime + ":00").ToList();
            var _takeAway = hourReport.Select(a => a.TakeAway).ToList();
            var _atStore = hourReport.Select(a => a.AtStore).ToList();
            var _delivery = hourReport.Select(a => a.Delivery).ToList();
            //return PartialView("_LoadRevenueReport", reportList.OrderBy(a => a.StartTime));
            return Json(new
            {
                datatable = list,
                dataChart = new
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

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            TimeSpan spanTime = endDate - startDate;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            if (storeId > 0)
            {
                //rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                rents = orderAPI.GetOrdersByTimeRange(storeId, startDate, endDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
            }
            else
            {
                //rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
            }
            var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
            {
                OrderType = r.Key.OrderType,
                OrderTime = r.Key.Time,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
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

                var orderDiscount = result.Where(r => r.OrderType >= 4 && r.OrderType <= 6 && r.OrderTime == item.StartTime);

                item.Discount = orderDiscount.Sum(a => a.Discount);
                item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                item.TotalPrice = item.Discount + item.FinalPrice;


            }

            //var list = hourReport.Select(a => new
            //{
            //    a = a.StartTime + ":00 - " + a.EndTime + ":00",

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
            //            "{0:0,0}", a.TotalPrice),
            //    j = string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.Discount),
            //    k = string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.FinalPrice),

            //}).ToList();
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
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu(Mang đi)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Tại store)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu(Tại store)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng(Giao hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu(Giao hàng)";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng số bill";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng doanh thu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tiền giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng doanh thu sau giảm giá";
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
                    StartHeaderChar = 'A';
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

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalPrice);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.Discount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.FinalPrice);
                }
                ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                var brandAPI = new BrandApi();
                var storeName = brandAPI.Get(brandId).BrandName;
                if (storeId > 0)
                {
                    storeName = storeApi.GetStoreNameByID(storeId);
                }
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

        #region MonthReport
        public ActionResult MonthReport()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            //return View();
            return PartialView("_MonthReport");
        }
        public JsonResult LoadStoreList(int brandId)
        {
            var storeapi = new StoreApi();
            var stores = storeapi.GetActiveStoreByBrandId(brandId).ToArray();
            return Json(new
            {
                store = stores,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadStoreGroupList(int brandId)
        {
            var storeGroupapi = new StoreGroupApi();
            var storesGroup = storeGroupapi.GetStoreGroupByBrandId(brandId).ToArray();
            //StoreGroupViewModel[] storesGroup = null;
            return Json(new
            {
                storeGroup = storesGroup,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadMonthReport(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int storeId)
        {
            int count = 1;
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

            var orderApi = new OrderApi();

            // -- duynnm --
            var dateNow = Utils.GetCurrentDateTime();
            //var isAdmin = Roles.IsUserInRole("administrator");
            var isAdmin = HttpContext.User.IsInRole("Administrator");
            //if (!isAdmin)
            //{
            //    dateNow = dateNow.AddDays(-1);
            //}
            var startDate = new DateTime(dateNow.Year, 1, 1);
            var endDate = dateNow.GetEndOfDate();

            if (startTime != "" || endTime != "")
            {

                startDate = startTime.ToDateTime();
                endDate = endTime.ToDateTime();

                startDate = startDate.GetStartOfDate();
                endDate = endDate.GetEndOfDate();
            }

            IEnumerable<DateReport> dateReport;

            IQueryable<Order> orders = null;
            DateTime now = DateTime.Now;
            if (startDate.Year == now.Year)
            {
                orders = orderApi.GetStoreOrderByDate(now, storeId);
            }
            dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, storeId);

            //So bill va doanh thu hom nay / update 1/11/2017
            double discount = 0, pTakeAway = 0, pAtStore = 0, pDelivery = 0;
            int takeAway = 0, atStore = 0, delivery = 0;
            if (orders != null)
            {
                var getOrderTakeAway = orders.Where(a => a.OrderType == (int)OrderTypeEnum.TakeAway && a.OrderStatus == 2);
                takeAway = getOrderTakeAway.Count();
                if (takeAway > 0)
                {
                    pTakeAway = getOrderTakeAway.Sum(a => a.FinalAmount);
                }

                var getOrderAtStore = orders.Where(a => a.OrderType == (int)OrderTypeEnum.AtStore && a.OrderStatus == 2);
                atStore = getOrderAtStore.Count();
                if (atStore > 0)
                {
                    pAtStore = getOrderAtStore.Sum(a => a.FinalAmount);
                }

                var getOrderDelivery = orders.Where(a => a.OrderType == (int)OrderTypeEnum.Delivery && a.OrderStatus == 2);
                delivery = getOrderDelivery.Count();
                if (delivery > 0)
                {
                    pDelivery = getOrderDelivery.Sum(a => a.FinalAmount);
                }
                var discountTakeAway = takeAway > 0 ? getOrderTakeAway.Sum(a => a.Discount + a.DiscountOrderDetail) : 0;
                var discountAtStore = atStore > 0 ? getOrderAtStore.Sum(a => a.Discount + a.DiscountOrderDetail) : 0;
                var discountAtDelivery = delivery > 0 ? getOrderDelivery.Sum(a => a.Discount + a.DiscountOrderDetail) : 0;
                discount = discountTakeAway + discountAtStore + discountAtDelivery;
            }
            //foreach (var i in orders)
            //{
            //    if (i != null && i.OrderStatus == 2)
            //    {
            //        finalAmount += i.FinalAmount;
            //        discount += i.DiscountOrderDetail;
            //        switch (i.OrderType)
            //        {
            //            case (int)OrderTypeEnum.TakeAway:
            //                takeAway += 1;
            //                pTakeAway += i.FinalAmount;
            //                break;
            //            case (int)OrderTypeEnum.AtStore:
            //                atStore += 1;
            //                pAtStore += i.FinalAmount;
            //                break;
            //            case (int)OrderTypeEnum.Delivery:
            //                delivery += 1;
            //                pDelivery += i.FinalAmount;
            //                break;
            //            default:
            //                break;
            //        }
            //        //if (i.OrderType == 5)
            //        //{
            //        //    takeAway += 1;
            //        //}
            //        //if(i.OrderType == 4)
            //        //{
            //        //    atStore += 1;
            //        //}
            //        //if (i.OrderType == 6)
            //        //{
            //        //    delivery += 1;
            //        //}
            //    }
            //}


            //var totalOrder = rents.Count();
            //var totalAmount = rents.Sum(a=> a.FinalAmount);
            //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

            var result = dateReport.GroupBy(r => new { Time = r.Date.Month }).Select(r => new
            {

                OrderTime = r.Key.Time,
                //-- CuongHH--
                TotalOrder = r.Sum(a => a.TotalOrder),
                TotalPrice = r.Sum(a => a.TotalAmount),
                TotalFinalAmount = r.Sum(a => a.FinalAmount),
                TotalDiscount = r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail),
                TotalTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                PriceTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                TotalDelivery = r.Sum(a => a.TotalOrderDelivery),
                PriceDelivery = r.Sum(a => a.FinalAmountDelivery),
                TotalAtStore = r.Sum(a => a.TotalOrderAtStore),
                PriceAtStore = r.Sum(a => a.FinalAmountAtStore)
            }).ToList();

            foreach (var item in monthReport)
            {
                //-- CuongHH--
                var resultMonth = result.Where(a => a.OrderTime == item.Month);
                item.TotalOrder = resultMonth.Sum(a => a.TotalAtStore) + resultMonth.Sum(a => a.TotalTakeAway) + resultMonth.Sum(a => a.TotalDelivery);
                item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount);
                item.TotalFinalAmount = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery));

                item.AtStore = resultMonth.Sum(a => a.TotalAtStore);
                item.PriceAtStore = (double)resultMonth.Sum(a => a.PriceAtStore);

                item.Delivery = resultMonth.Sum(a => a.TotalDelivery);
                item.PriceDelivery = (double)resultMonth.Sum(a => a.PriceDelivery);

                item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway);
                item.PriceTakeAway = (double)resultMonth.Sum(a => a.PriceTakeAway);


                item.TotalPrice = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery) + resultMonth.Sum(a => a.TotalDiscount));
                if (item.Month == dateNow.Month)
                {
                    item.TotalOrder = resultMonth.Sum(a => a.TotalAtStore) + resultMonth.Sum(a => a.TotalTakeAway) + resultMonth.Sum(a => a.TotalDelivery)
                        + atStore + delivery + takeAway;
                    item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount) + discount;

                    item.TotalFinalAmount = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery)
                        + pAtStore + pDelivery + pTakeAway);

                    item.AtStore = resultMonth.Sum(a => a.TotalAtStore) + atStore;
                    item.PriceAtStore = (double)resultMonth.Sum(a => a.PriceAtStore) + pAtStore;

                    item.Delivery = resultMonth.Sum(a => a.TotalDelivery) + delivery;
                    item.PriceDelivery = (double)resultMonth.Sum(a => a.PriceDelivery) + pDelivery;

                    item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway) + takeAway;
                    item.PriceTakeAway = (double)resultMonth.Sum(a => a.PriceTakeAway) + pTakeAway;


                    item.TotalPrice = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery) + resultMonth.Sum(a => a.TotalDiscount)
                        + pAtStore + pDelivery + pTakeAway + discount);

                }
            }

            var list = monthReport.Select(a => new IConvertible[]
            {
                count++,
                a.MonthName,

                a.TakeAway,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceTakeAway),

                a.AtStore,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceAtStore),

                a.Delivery,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceAtStore),

                a.TotalOrder,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalDiscount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalFinalAmount)
            }).ToArray();

            var _FinalAmount = monthReport.Select(a => a.TotalFinalAmount).ToArray();
            var _MonthName = monthReport.Select(a => a.MonthName).ToArray();
            var _TakeAway = monthReport.Select(a => a.TakeAway).ToArray();
            var _AtStore = monthReport.Select(a => a.AtStore).ToArray();
            var _Delivery = monthReport.Select(a => a.Delivery).ToArray();
            List<Object> dataPie = new List<Object>();
            for (int i = 0; i < _MonthName.Count(); i++)
            {
                dataPie.Add(new
                {
                    name = _MonthName[i],
                    y = _FinalAmount[i]
                });
            }

            return Json(new
            {
                datatable = list,
                dataChart = new
                {
                    MonthName = _MonthName,
                    TakeAway = _TakeAway,
                    AtStore = _AtStore,
                    Delivery = _Delivery
                },
                dataPie = dataPie,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadMonthReportForGroup(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int selectedGroupId)
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
            var startDate = new DateTime(dateNow.Year, 1, 1);
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

            IEnumerable<DateReport> dateReport = Enumerable.Empty<DateReport>();
            var stores = new StoreApi();
            var storeGroup = new StoreGroupApi();
            var storeInGroup = stores.GetStoreByGroupId(selectedGroupId);
            var orderApi = new OrderApi();
            IEnumerable<Order> orders = Enumerable.Empty<Order>();
            foreach (var i in storeInGroup)
            {
                DateTime now = DateTime.Now;
                if (startDate.Year == now.Year)
                {
                    orders = orders.Concat(orderApi.GetStoreOrderByDate(now, i.ID));
                }
                dateReport = dateReport.Concat(dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, i.ID));
            }
            //lay order ngay hom nay
            double finalAmount = 0, discount = 0, takeAway = 0, atStore = 0, delivery = 0;
            foreach (var i in orders)
            {
                if (i != null && i.OrderStatus == 2)
                {
                    finalAmount += i.FinalAmount;
                    discount += i.DiscountOrderDetail;
                    if (i.OrderType == 5)
                    {
                        takeAway += 1;
                    }
                    if (i.OrderType == 4)
                    {
                        atStore += 1;
                    }
                    if (i.OrderType == 6)
                    {
                        delivery += 1;
                    }
                }
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
                if (item.Month == dateNow.Month)
                {
                    item.TotalOrder = resultMonth.Sum(a => a.TotalOrder) + (int)takeAway + (int)delivery + (int)atStore;
                    item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount) + discount;
                    item.TotalFinalAmount = (double)resultMonth.Sum(a => a.TotalFinalAmount) + finalAmount;
                    item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway) + takeAway;
                    item.Delivery = resultMonth.Sum(a => a.TotalDelivery) + delivery;
                    item.AtStore = resultMonth.Sum(a => a.TotalAtStore) + atStore;
                }
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
            var _FinalAmount = monthReport.Select(a => a.TotalFinalAmount).ToArray();
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
                },
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportMonthTableToExcel(int brandId, string startTime, string endTime, int storeId)
        {
            DateTime now = DateTime.Now;
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
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

            IEnumerable<Order> orders = Enumerable.Empty<Order>();
            if (startDate.Year == now.Year)
            {
                orders = orderApi.GetStoreOrderByDate(now, storeId);
            }
            dateReport = dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, storeId);

            double finalAmount = 0, discount = 0, pTakeAway = 0, pAtStore = 0, pDelivery = 0;
            int takeAway = 0, atStore = 0, delivery = 0;
            foreach (var i  in orders)
             {
                if (i != null && i.OrderStatus == 2)
                {
                    finalAmount += i.FinalAmount;
                    discount += i.DiscountOrderDetail;
                    switch (i.OrderType)
                    {
                        case (int)OrderTypeEnum.TakeAway:
                            takeAway += 1;
                            pTakeAway += i.FinalAmount;
                            break;
                        case (int)OrderTypeEnum.AtStore:
                            atStore += 1;
                            pAtStore += i.FinalAmount;
                            break;
                        case (int)OrderTypeEnum.Delivery:
                            delivery += 1;
                            pDelivery += i.FinalAmount;
                            break;
                        default:
                            break;
                    }
                    //if (i.OrderType == 5)
                    //{
                    //    takeAway += 1;
                    //}
                    //if(i.OrderType == 4)
                    //{
                    //    atStore += 1;
                    //}
                    //if (i.OrderType == 6)
                    //{
                    //    delivery += 1;
                    //}
                }
            }
            //var totalOrder = rents.Count();
            //var totalAmount = rents.Sum(a=> a.FinalAmount);
            //var totalDiscount = rents.Sum(a=> a.Discount) + rents.Sum(a=> a.DiscountOrderDetail);

           
            var result = dateReport.GroupBy(r => new { Time = r.Date.Month }).Select(r => new
            {

                OrderTime = r.Key.Time,
                //-- CuongHH--
                TotalOrder = r.Sum(a => a.TotalOrder),
                TotalPrice = r.Sum(a => a.TotalAmount),
                TotalFinalAmount = r.Sum(a => a.FinalAmount),
                TotalDiscount = r.Sum(a => a.Discount) + r.Sum(a => a.DiscountOrderDetail),
                TotalTakeAway = r.Sum(a => a.TotalOrderTakeAway),
                PriceTakeAway = r.Sum(a => a.FinalAmountTakeAway),
                TotalDelivery = r.Sum(a => a.TotalOrderDelivery),
                PriceDelivery = r.Sum(a => a.FinalAmountDelivery),
                TotalAtStore = r.Sum(a => a.TotalOrderAtStore),
                PriceAtStore = r.Sum(a => a.FinalAmountAtStore)
            }).ToList();


            //foreach (var item in monthReport)
            //{

            //foreach ( var item in listExcel)
            //    {
            //    //-- CuongHH--
            //    var resultMonth = result.Where(a => a.OrderTime == item.Month);
            //    item.TotalOrder = resultMonth.Sum(a => a.TotalAtStore) + resultMonth.Sum(a => a.TotalTakeAway) + resultMonth.Sum(a => a.TotalDelivery);
            //    item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount);
            //    item.TotalFinalAmount = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery));

            //    item.AtStore = resultMonth.Sum(a => a.TotalAtStore);
            //    item.PriceAtStore = (double)resultMonth.Sum(a => a.PriceAtStore);

            //    item.Delivery = resultMonth.Sum(a => a.TotalDelivery);
            //    item.PriceDelivery = (double)resultMonth.Sum(a => a.PriceDelivery);

            //    item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway);
            //    item.PriceTakeAway = (double)resultMonth.Sum(a => a.PriceTakeAway);


            //    item.TotalPrice = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery) + resultMonth.Sum(a => a.TotalDiscount));
            //    if (item.Month == now.Month)
            //    {
            //        item.TotalOrder = resultMonth.Sum(a => a.TotalAtStore) + resultMonth.Sum(a => a.TotalTakeAway) + resultMonth.Sum(a => a.TotalDelivery)
            //            + atStore + delivery + takeAway;
            //        item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount) + discount;

            //        item.TotalFinalAmount = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery)
            //            + pAtStore + pDelivery + pTakeAway);

            //        item.AtStore = resultMonth.Sum(a => a.TotalAtStore) + atStore;
            //        item.PriceAtStore = (double)resultMonth.Sum(a => a.PriceAtStore) + pAtStore;

            //        item.Delivery = resultMonth.Sum(a => a.TotalDelivery) + delivery;
            //        item.PriceDelivery = (double)resultMonth.Sum(a => a.PriceDelivery) + pDelivery;

            //        item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway) + takeAway;
            //        item.PriceTakeAway = (double)resultMonth.Sum(a => a.PriceTakeAway) + pTakeAway;


            //        item.TotalPrice = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery) + resultMonth.Sum(a => a.TotalDiscount)
            //            + pAtStore + pDelivery + pTakeAway + discount);

            //    }

            //}
            var items = new List<MonthReportViewModel>();
            foreach (var item in listExcel)
            {
                //-- CuongHH--
                var resultMonth = result.Where(a => a.OrderTime == item.Month);
                item.TotalOrder = resultMonth.Sum(a => a.TotalAtStore) + resultMonth.Sum(a => a.TotalTakeAway) + resultMonth.Sum(a => a.TotalDelivery);
                item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount);
                item.TotalFinalAmount = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery));

                item.AtStore = resultMonth.Sum(a => a.TotalAtStore);
                item.PriceAtStore = (double)resultMonth.Sum(a => a.PriceAtStore);

                item.Delivery = resultMonth.Sum(a => a.TotalDelivery);
                item.PriceDelivery = (double)resultMonth.Sum(a => a.PriceDelivery);

                item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway);
                item.PriceTakeAway = (double)resultMonth.Sum(a => a.PriceTakeAway);


                item.TotalPrice = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery) + resultMonth.Sum(a => a.TotalDiscount));
                if (item.Month == now.Month)
                {
                    item.TotalOrder = resultMonth.Sum(a => a.TotalAtStore) + resultMonth.Sum(a => a.TotalTakeAway) + resultMonth.Sum(a => a.TotalDelivery)
                        + atStore + delivery + takeAway;
                    item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount) + discount;

                    item.TotalFinalAmount = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery)
                        + pAtStore + pDelivery + pTakeAway);

                    item.AtStore = resultMonth.Sum(a => a.TotalAtStore) + atStore;
                    item.PriceAtStore = (double)resultMonth.Sum(a => a.PriceAtStore) + pAtStore;

                    item.Delivery = resultMonth.Sum(a => a.TotalDelivery) + delivery;
                    item.PriceDelivery = (double)resultMonth.Sum(a => a.PriceDelivery) + pDelivery;

                    item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway) + takeAway;
                    item.PriceTakeAway = (double)resultMonth.Sum(a => a.PriceTakeAway) + pTakeAway;


                    item.TotalPrice = (double)(resultMonth.Sum(a => a.PriceAtStore) + resultMonth.Sum(a => a.PriceTakeAway) + resultMonth.Sum(a => a.PriceDelivery) + resultMonth.Sum(a => a.TotalDiscount)
                        + pAtStore + pDelivery + pTakeAway + discount);
                    
                    
                }
                items.Add(item);

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
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tháng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill mang đi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu mang đi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill tại store";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu tại store";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số bill giao hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu giao hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng số hóa đơn";
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
                //foreach (var data in listExcel)
                //{ cmt => listExcel ko có dữ liệu
                foreach(var data in items)
                { 
                    StartHeaderChar = 'A';
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.MonthName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TakeAway;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.PriceTakeAway);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.AtStore;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.PriceAtStore);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Delivery;

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.PriceDelivery);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;

                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalPrice);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalDiscount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.TotalFinalAmount);
                }
                ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                string storeName;

                storeName = storeApi.GetStoreNameByID(storeId);

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
        public ActionResult ExportMonthTableToExcelforGroup(int brandId, string startTime, string endTime, int selectedGroupId)
        {
            var storeApi = new StoreApi();
            DateTime now = DateTime.Now;
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

            IEnumerable<DateReport> dateReport = Enumerable.Empty<DateReport>();
            var stores = new StoreApi();
            var storeGroup = new StoreGroupApi();
            var storeInGroup = stores.GetStoreByGroupId(selectedGroupId);
            var orderApi = new OrderApi();
            IEnumerable<Order> orders = Enumerable.Empty<Order>();
            foreach (var i in storeInGroup)
            {
                if (startDate.Year == now.Year)
                {
                    orders = orders.Concat(orderApi.GetStoreOrderByDate(now, i.ID));
                }
                dateReport = dateReport.Concat(dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, i.ID));
            }
            double finalAmount = 0, discount = 0, takeAway = 0, atStore = 0, delivery = 0;
            foreach (var i in orders)
            {
                if (i != null && i.OrderStatus == 2)
                {
                    finalAmount += i.FinalAmount;
                    discount += i.DiscountOrderDetail;
                    if (i.OrderType == 5)
                    {
                        takeAway += 1;
                    }
                    if (i.OrderType == 4)
                    {
                        atStore += 1;
                    }
                    if (i.OrderType == 6)
                    {
                        delivery += 1;
                    }
                }
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
                if (item.Month == now.Month)
                {
                    item.TotalOrder = resultMonth.Sum(a => a.TotalOrder) + (int)takeAway + (int)delivery + (int)atStore;
                    item.TotalDiscount = (double)resultMonth.Sum(a => a.TotalDiscount) + discount;
                    item.TotalFinalAmount = (double)resultMonth.Sum(a => a.TotalFinalAmount) + finalAmount;
                    item.TakeAway = resultMonth.Sum(a => a.TotalTakeAway) + takeAway;
                    item.Delivery = resultMonth.Sum(a => a.TotalDelivery) + delivery;
                    item.AtStore = resultMonth.Sum(a => a.TotalAtStore) + atStore;
                }
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
                string storeGroupName;
                if (selectedGroupId > 0)
                {
                    storeGroupName = storeGroup.Get(selectedGroupId).GroupName;
                }
                else
                    storeGroupName = "Tổng quan các của hàng";
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BáoCáoTheoTháng_Nhóm_" + storeGroupName + dateRange + ".xlsx";
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
    }

    public class ReportAmount
    {
        public string Text { get; set; }
        public double Amount { get; set; }
    }

    public class AmountComparison
    {
        public ReportAmount MaxAmount { get; set; }
        public ReportAmount MinAmount { get; set; }
    }

    public class HourReportFollowTempModel
    {
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public double TakeAway { get; set; }
        public double PriceTakeAway { get; set; }
        public double AtStore { get; set; }
        public double PriceAtStore { get; set; }
        public double Delivery { get; set; }
        public double PriceDelivery { get; set; }
        public double TakeAwayDiscount { get; set; }
        public double TotalTakeAwayQuantity { get; set; }
        public double TotalTakeAwayPrice { get; set; }
        public double FinalTakeAwayPrice { get; set; }
        public double AtStoreDiscount { get; set; }
        public double TotalAtStoreQuantity { get; set; }
        public double TotalAtStorePrice { get; set; }
        public double FinalAtStorePrice { get; set; }
        public double DeliveryDiscount { get; set; }
        public double TotalDeliveryQuantity { get; set; }
        public double TotalDeliveryPrice { get; set; }
        public double FinalDeliveryPrice { get; set; }
    }
}
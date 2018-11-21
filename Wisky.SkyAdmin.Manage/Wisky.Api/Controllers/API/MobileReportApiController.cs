using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using Jose;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Wisky.Api.Controllers.API
{
    public class MobileReportApiController : Controller
    {
        private const string privateKey = ConstantManager.PRIVATE_KEY;
        private static byte[] secretKey = Encoding.UTF8.GetBytes(privateKey);
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="fbAccessToken"></param>
        /// <param name="brandID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Login(string username, string password, int brandId)
        {
            try
            {
                var storeUserApi = new StoreUserApi();
                var userApi = new AspNetUserApi();
                var user = userApi.GetUserByUsername(username);
                if (!Crypto.VerifyHashedPassword(user.PasswordHash, password))
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_WRONG_USERNAME_OR_PASSWORD,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                var roles = user.AspNetRoles.ToList();
                if (user != null)
                {
                    var storeUsers = storeUserApi.GetStoresFromUser(user.UserName);
                   
                    if (storeUsers.Count() == 0 && !(roles.Any(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString())))
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = false,
                                message = ConstantManager.MES_NO_STORE__ASSIGNED_USERNAME,
                                status = ConstantManager.STATUS_SUCCESS
                            },
                            data = new { }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    var newToken = GenerateToken(user.Id.ToString());

                    if (roles.Any(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString() || q.Id == ((int)RoleTypeEnum.StoreReportViewerMobile).ToString()))
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = true,
                                message = ConstantManager.MES_LOGIN_SUCCESS,
                                status = ConstantManager.STATUS_SUCCESS
                            },
                            data = new
                            {
                                data = new
                                {
                                    access_token = newToken,
                                    user_id = user.Id,
                                    user_name = user.UserName,
                                    full_name = user.FullName,
                                    list_roles = roles.Select(q => q.Id).ToList()
                                }
                            },
                        }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_INVALID_LOGIN_ATTEMP,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_WRONG_USERNAME_OR_PASSWORD,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_LOGIN_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get list store user can view
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetStoreUsersJson(string accessToken)
        {
            var userId = getIdFromToken(accessToken);
            var storeUsers = GetStoreUsers(userId);
            var storeApi = new StoreApi();
            var result = new List<StoreJson>();
            foreach (var storeUser in storeUsers)
            {
                StoreJson newStore = new StoreJson();
                var store = storeApi.GetStoreById(storeUser.StoreId);
                newStore.store_id = storeUser.StoreId;
                newStore.store_name = store.ShortName;
                newStore.address = store.Address;
                result.Add(newStore);
            }
            return Json(new
            {
                status = new
                {
                    success = false,
                    message = ConstantManager.MES_SUCCESS,
                    status = ConstantManager.STATUS_SUCCESS
                },
                data = new
                {
                    data = result
                }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get hour time report of selected date
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetHourReport(int brandId, string startTime, string endTime, int selectedStoreId, string accessToken)
        {
            //Get user from accessToken
            string userId = getIdFromToken(accessToken);
            var userApi = new AspNetUserApi();
            var user = userApi.GetUserById(userId);
            //check permission
            if (!checkRolesPermission(user, selectedStoreId, brandId))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_CHECK_PERMISSION_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var paymentApi = new PaymentApi();
                var storeApi = new StoreApi();
                var dateReportApi = new DateReportApi();
                var hourReport = new List<HourReportApiModel>();
                for (int i = 6; i < 23; i++)
                {
                    hourReport.Add(new HourReportApiModel()
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
                if (selectedStoreId > 0)
                {
                    //rents = orderAPI.GetOrdersByTimeRange(selectedStoreId, startDate, endDate, brandId)
                    //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    rents = orderAPI.GetOrdersByTimeRange(selectedStoreId, startDate, endDate, brandId)
                            .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    //rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                    //    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                    if (user.AspNetRoles.Where(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString()).Any())
                    {
                        rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
                    }
                    else
                    {
                        var listStoreId = GetStoreUsers(user.Id).Select(q => q.StoreId).ToList();
                        rents = orderAPI.GetOrdersByTimeRangeAndListStore(listStoreId, startDate, endDate, brandId)
                            .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);
                    }
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

                    var orderCard = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.OrderCard && r.OrderTime == item.StartTime);
                    item.OrderCard = (orderCard == null) ? 0 : orderCard.TotalOrder;
                    item.PriceOrderCard = (orderCard == null) ? 0 : orderCard.Money;

                    var orderDiscount = result.Where(r => r.OrderType >= 4 && r.OrderType <= 6 && r.OrderTime == item.StartTime);

                    item.Discount = orderDiscount.Sum(a => a.Discount);
                    item.TotalQuantity = item.TakeAway + item.AtStore + item.Delivery;
                    item.FinalPrice = item.PriceTakeAway + item.PriceAtStore + item.PriceDelivery;
                    item.TotalPrice = item.Discount + item.FinalPrice;
                }

                //var _Time = hourReport.Select(a => a.StartTime + ":00 - " + a.EndTime + ":00").ToList();
                var _Time = hourReport.Select(a => a.StartTime + ":00").ToList();
                var _TotalQuantity = hourReport.Select(a => a.TotalQuantity).ToList();
                var _FinalPrice = hourReport.Select(a => a.FinalPrice).ToList();

                //report payment transaction
                var resultPayment = new OverviewPaymentDashboard();
                resultPayment.TotalTransactionPayment = paymentApi.CountPaymentByTimeRange(selectedStoreId, startDate, endDate, brandId);
                resultPayment.TotalTransactionPaymentBuyCard = paymentApi.CountTotalPaymentByTimeRangeWithPayCard(selectedStoreId, startDate, endDate, brandId);
                resultPayment.TotalPayment = resultPayment.TotalTransactionPayment > 0 ? paymentApi.SumTotalPaymentByTimeRange(selectedStoreId, startDate, endDate, brandId) : 0;
                resultPayment.TotalTransactionPaymentCard = paymentApi.CountTotalPaymentByTimeRangeWithPayType(selectedStoreId, startDate, endDate, (int)PaymentTypeEnum.MemberPayment, brandId);
                var transPaymentCash = paymentApi.CountTotalPaymentByTimeRangeWithPaySale(selectedStoreId, startDate, endDate, brandId);
                resultPayment.TotalPaymentForSales = (transPaymentCash > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPaySale(selectedStoreId, startDate, endDate, brandId) : 0);
                resultPayment.TotalPaymentCard = resultPayment.TotalTransactionPaymentCard > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayType(selectedStoreId, startDate, endDate, (int)PaymentTypeEnum.MemberPayment, brandId) : 0;
                resultPayment.TotalPaymentBuyCard = resultPayment.TotalTransactionPaymentBuyCard > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayCard(selectedStoreId, startDate, endDate, brandId) : 0;

                //report top store data
                StoreReportViewModel topStore = null;
                List<StoreReportViewModel> listTopStore = GetTopStoreReportViewModel(brandId, startDate, endDate, 0, 0, 1, user, true);
                if (listTopStore != null)
                {
                    topStore = listTopStore.FirstOrDefault();
                }

                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            data_chart = new
                            {
                                time = _Time,
                                quantity = _TotalQuantity,
                                price = _FinalPrice,
                            },
                            total_revenue = new
                            {
                                total = hourReport.Sum(q => q.TotalPrice) + hourReport.Sum(q => q.PriceOrderCard),
                                total_real = hourReport.Sum(q => q.TotalPrice),
                                discount = hourReport.Sum(q => q.Discount),
                                final = hourReport.Sum(q => q.FinalPrice),
                                total_order_card = hourReport.Sum(q => q.PriceOrderCard),
                                total_at_store = hourReport.Sum(q => q.PriceAtStore),
                                total_take_away = hourReport.Sum(q => q.PriceTakeAway),
                                total_delivery = hourReport.Sum(q => q.PriceDelivery)
                            },
                            total_quantity = new
                            {
                                total_order = hourReport.Sum(q => q.TotalQuantity) + hourReport.Sum(q => q.OrderCard),
                                at_store = hourReport.Sum(q => q.AtStore),
                                take_away = hourReport.Sum(q => q.TakeAway),
                                delivery = hourReport.Sum(q => q.Delivery),
                                order_card = hourReport.Sum(q => q.OrderCard)
                            },
                            total_payment = new
                            {
                                total_payment = resultPayment.TotalPayment,
                                total_payment_cash = resultPayment.TotalPaymentForSales,
                                total_payment_ordercard = resultPayment.TotalPaymentBuyCard,
                                total_payment_card = resultPayment.TotalPaymentCard
                            },
                            total_payment_transaction = new
                            {
                                total_transaction = resultPayment.TotalTransactionPayment,
                                order_card_transaction = resultPayment.TotalTransactionPaymentBuyCard,
                                card_transaction = resultPayment.TotalTransactionPaymentCard
                            },
                            top_store_revenue = (topStore == null) ? null : new
                            {
                                store_name = storeApi.GetStoreById(topStore.StoreId).ShortName,
                                total_store_order = topStore.TotalOrder - topStore.TotalOrderCard,
                                total_store_revenue = topStore.FinalAmount - topStore.FinalCard
                            }
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get day report of selected date span
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDateReport(int brandId, string startTime, string endTime, int selectedStoreId, string accessToken)
        {
            //Get user from accessToken
            string userId = getIdFromToken(accessToken);
            var userApi = new AspNetUserApi();
            var user = userApi.GetUserById(userId);
            //check permission
            if (!checkRolesPermission(user, selectedStoreId, brandId))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_CHECK_PERMISSION_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                int countOrder = 0;
                var dateReportApi = new DateReportApi();
                var orderApi = new OrderApi();
                var storeApi = new StoreApi();
                var paymentApi = new PaymentApi();
                var sTime = startTime.ToDateTime().GetStartOfDate();
                var eTime = endTime.ToDateTime().GetEndOfDate();
                var form = DateTime.Now.GetStartOfDate();
                var to = DateTime.Now.GetEndOfDate();
                IEnumerable<Order> orders = orderApi.GetOrdersByTimeRange(selectedStoreId, form, to, brandId);

                //emptyList chart
                var emptyListChart = new List<DateReportApiModel>();
                for (DateTime i = sTime; i < eTime; i = i.AddDays(1))
                {
                    emptyListChart.Add(new DateReportApiModel
                    {
                        Date = i.ToString("dd/MM/yyyy"),
                        TotalQuantity = 0,
                        TotalPrice = 0,
                        FinalPrice = 0
                    });
                }

                //lay du lieu tu database table datereport
                var dateReport = new List<DateReport>();
                if (selectedStoreId == 0)
                {
                    if (user.AspNetRoles.Where(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString()).Any())
                    {
                        dateReport = dateReportApi.GetDateReportTimeRange(sTime, eTime).ToList();
                    }
                    else
                    {
                        var listStoreId = GetStoreUsers(user.Id).Select(q => q.StoreId).ToList();
                        dateReport = dateReportApi.GetDateReportTimeRangeAndListStore(sTime, eTime, listStoreId).ToList();
                        orders = orderApi.GetOrdersByTimeRangeAndListStore(listStoreId, sTime, eTime, brandId);
                    }
                }
                else
                {
                    dateReport = dateReportApi.GetDateReportTimeRangeAndStore(sTime, eTime, selectedStoreId).ToList();
                }

                //group theo ngày chart
                var DateReportResult = dateReport.GroupBy(a => a.Date.ToString("dd/MM/yyyy")).
                    Select(b => new DateReportApiModel
                    {
                        Date = b.Key,
                        AtStore = b.Sum(c => c.TotalOrderAtStore),
                        PriceAtStore = b.Sum(c => c.FinalAmountAtStore) ?? 0,
                        TakeAway = b.Sum(c => c.TotalOrderTakeAway),
                        PriceTakeAway = b.Sum(c => c.FinalAmountTakeAway) ?? 0,
                        Delivery = b.Sum(c => c.TotalOrderDelivery),
                        PriceDelivery = b.Sum(c => c.FinalAmountDelivery) ?? 0,
                        TotalQuantity = b.Sum(c => c.TotalOrder - c.TotalOrderCard),
                        OrderCard = b.Sum(c => c.TotalOrderCard),
                        PriceOrderCard = b.Sum(c => c.FinalAmountCard) ?? 0,
                        Discount = b.Sum(c => c.Discount + c.DiscountOrderDetail) ?? 0,
                        TotalPrice = b.Sum(c => c.TotalAmount - c.FinalAmountCard) ?? 0,
                        FinalPrice = b.Sum(c => c.FinalAmount - c.FinalAmountCard) ?? 0
                    });

                //ngay hom nay
                if (eTime == to)
                {
                    if (orders.Count() > 0)
                    {
                        //So bill va doanh thu hom nay
                        int OrdTakeAway = 0, OrdAtStore = 0, OrdDelivery = 0, OrdCard = 0;
                        double PriceOrdTakeAway = 0, PriceOrdAtStore = 0, PriceOrdDelivery = 0, PriceOrdCard = 0, discount = 0;
                        foreach (var item in orders)
                        {
                            if (item != null && item.OrderStatus == 2)
                            {

                                if ((item.OrderType == (int)OrderTypeEnum.TakeAway) || (item.OrderType == (int)OrderTypeEnum.AtStore)
                                    || (item.OrderType == (int)OrderTypeEnum.Delivery))
                                {
                                    countOrder++;
                                    discount += (item.Discount + item.DiscountOrderDetail);
                                }

                                OrdTakeAway += (item.OrderType == (int)OrderTypeEnum.TakeAway) ? 1 : 0;
                                PriceOrdTakeAway += (item.OrderType == (int)OrderTypeEnum.TakeAway) ? item.FinalAmount : 0;

                                OrdAtStore += (item.OrderType == (int)OrderTypeEnum.AtStore) ? 1 : 0;
                                PriceOrdAtStore += (item.OrderType == (int)OrderTypeEnum.AtStore) ? item.FinalAmount : 0;

                                OrdDelivery += (item.OrderType == (int)OrderTypeEnum.Delivery) ? 1 : 0;
                                PriceOrdDelivery += (item.OrderType == (int)OrderTypeEnum.Delivery) ? item.FinalAmount : 0;

                                OrdCard += (item.OrderType == (int)OrderTypeEnum.OrderCard) ? 1 : 0;
                                PriceOrdCard += (item.OrderType == (int)OrderTypeEnum.OrderCard) ? item.FinalAmount : 0;
                            }
                        }

                        var todayData = new DateReportApiModel()
                        {
                            Date = to.ToString("dd/MM/yyyy"),
                            AtStore = OrdAtStore,
                            PriceAtStore = PriceOrdAtStore,
                            TakeAway = OrdTakeAway,
                            PriceTakeAway = PriceOrdTakeAway,
                            Delivery = OrdDelivery,
                            PriceDelivery = PriceOrdDelivery,
                            OrderCard = OrdCard,
                            PriceOrderCard = PriceOrdCard,
                            Discount = discount,
                            TotalQuantity = countOrder,
                            TotalPrice = PriceOrdTakeAway + PriceOrdAtStore + PriceOrdDelivery,
                            FinalPrice = PriceOrdTakeAway + PriceOrdAtStore + PriceOrdDelivery - discount
                        };
                        List<DateReportApiModel> list = new List<DateReportApiModel>();
                        list.Add(todayData);
                        DateReportResult = DateReportResult.Concat(list);
                    }
                }

                //merge voi emptyListChart
                if (DateReportResult != null) DateReportResult = emptyListChart.Concat(DateReportResult);
                else DateReportResult = emptyListChart;
                DateReportResult = DateReportResult.GroupBy(a => a.Date).
                    Select(b => new DateReportApiModel
                    {
                        Date = b.Key,
                        AtStore = b.Sum(c => c.AtStore),
                        PriceAtStore = b.Sum(c => c.PriceAtStore),
                        TakeAway = b.Sum(c => c.TakeAway),
                        PriceTakeAway = b.Sum(c => c.PriceTakeAway),
                        Delivery = b.Sum(c => c.Delivery),
                        PriceDelivery = b.Sum(c => c.PriceDelivery),
                        OrderCard = b.Sum(c => c.OrderCard),
                        PriceOrderCard = b.Sum(c => c.PriceOrderCard),
                        Discount = b.Sum(c => c.Discount),
                        TotalQuantity = b.Sum(c => c.TotalQuantity),
                        TotalPrice = b.Sum(c => (c.TotalPrice)),
                        FinalPrice = b.Sum(c => (c.FinalPrice)),
                    });

                //json chart
                var _DateName = DateReportResult.Select(a => a.Date).ToArray();
                var _TotalQuantity = DateReportResult.Select(a => a.TotalQuantity).ToArray();
                var _TotalPrice = DateReportResult.Select(a => a.TotalPrice).ToArray();
                var _FinalPrice = DateReportResult.Select(a => a.FinalPrice).ToArray();

                //Payment Report
                var resultPayment = new OverviewPaymentDashboard();
                resultPayment.TotalTransactionPayment = paymentApi.CountPaymentByTimeRange(selectedStoreId, sTime, eTime, brandId);
                resultPayment.TotalTransactionPaymentBuyCard = paymentApi.CountTotalPaymentByTimeRangeWithPayCard(selectedStoreId, sTime, eTime, brandId);
                resultPayment.TotalPayment = resultPayment.TotalTransactionPayment > 0 ? paymentApi.SumTotalPaymentByTimeRange(selectedStoreId, sTime, eTime, brandId) : 0;
                resultPayment.TotalTransactionPaymentCard = paymentApi.CountTotalPaymentByTimeRangeWithPayType(selectedStoreId, sTime, eTime, (int)PaymentTypeEnum.MemberPayment, brandId);
                var transPaymentCash = paymentApi.CountTotalPaymentByTimeRangeWithPaySale(selectedStoreId, sTime, eTime, brandId);
                resultPayment.TotalPaymentForSales = (transPaymentCash > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPaySale(selectedStoreId, sTime, eTime, brandId) : 0);
                resultPayment.TotalPaymentCard = resultPayment.TotalTransactionPaymentCard > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayType(selectedStoreId, sTime, eTime, (int)PaymentTypeEnum.MemberPayment, brandId) : 0;
                resultPayment.TotalPaymentBuyCard = resultPayment.TotalTransactionPaymentBuyCard > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayCard(selectedStoreId, sTime, eTime, brandId) : 0;

                //report top store data
                //report top store data
                StoreReportViewModel topStore = null;
                List<StoreReportViewModel> listTopStore = GetTopStoreReportViewModel(brandId, sTime, eTime, 0, 0, 1, user, true);
                if (listTopStore != null)
                {
                    topStore = listTopStore.FirstOrDefault();
                }

                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            data_chart = new
                            {
                                date_name = _DateName,
                                quantity = _TotalQuantity,
                                price = _FinalPrice,
                            },
                            total_revenue = new
                            {
                                total = DateReportResult.Sum(q => q.FinalPrice) + DateReportResult.Sum(q => q.PriceOrderCard),
                                total_real = DateReportResult.Sum(q => q.TotalPrice),
                                discount = DateReportResult.Sum(q => q.Discount),
                                final = DateReportResult.Sum(q => q.FinalPrice),
                                total_order_card = DateReportResult.Sum(q => q.PriceOrderCard),
                                total_at_store = DateReportResult.Sum(q => q.PriceAtStore),
                                total_take_away = DateReportResult.Sum(q => q.PriceTakeAway),
                                total_delivery = DateReportResult.Sum(q => q.PriceDelivery)
                            },
                            total_quantity = new
                            {
                                total_order = DateReportResult.Sum(q => q.TotalQuantity) + DateReportResult.Sum(q => q.OrderCard),
                                at_store = DateReportResult.Sum(q => q.AtStore),
                                take_away = DateReportResult.Sum(q => q.TakeAway),
                                delivery = DateReportResult.Sum(q => q.Delivery),
                                order_card = DateReportResult.Sum(q => q.OrderCard)
                            },
                            total_payment = new
                            {
                                total_payment = resultPayment.TotalPayment,
                                total_payment_cash = resultPayment.TotalPaymentForSales,
                                total_payment_ordercard = resultPayment.TotalPaymentBuyCard,
                                total_payment_card = resultPayment.TotalPaymentCard
                            },
                            total_payment_transaction = new
                            {
                                total_transaction = resultPayment.TotalTransactionPayment,
                                order_card_transaction = resultPayment.TotalTransactionPaymentBuyCard,
                                card_transaction = resultPayment.TotalTransactionPaymentCard
                            },
                            top_store_revenue = (topStore == null) ? null : new
                            {
                                store_name = storeApi.GetStoreById(topStore.StoreId).ShortName,
                                total_store_order = topStore.TotalOrder - topStore.TotalOrderCard,
                                total_store_revenue = topStore.FinalAmount - topStore.FinalCard
                            }
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get top store report
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="selectedStoreId"></param>
        /// <param name="accessToken"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetTopStoreReport(int brandId, string startTime, string endTime, int selectedStoreId, string accessToken, int skip, int take, bool orderByDes)
        {
            //Get user from accessToken
            string userId = getIdFromToken(accessToken);
            var userApi = new AspNetUserApi();
            var user = userApi.GetUserById(userId);
            //check permission
            if (!checkRolesPermission(user, selectedStoreId, brandId))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_CHECK_PERMISSION_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                var storeApi = new StoreApi();
                //report top store data
                var topStore = GetTopStoreReportViewModel(brandId, startDate, endDate, 0, skip, take, user, orderByDes);
                IEnumerable<TopStoreReportJson> topStore_data = null;
                if (topStore != null)
                {
                    topStore_data = topStore.Select(q => new TopStoreReportJson
                    {
                        store_name = storeApi.GetStoreById(q.StoreId).ShortName,
                        total_store_order = q.TotalOrder - q.TotalOrderCard,
                        total_store_revenue = q.FinalAmount - q.FinalCard
                    });
                }

                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            top_store_revenue = (topStore == null) ? null : topStore_data
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<StoreReportViewModel> GetTopStoreReportViewModel(int brandId, DateTime startDate, DateTime endDate, int selectedStoreId, int skip, int take, AspNetUser user,bool orderByDes)
        {
            var storeApi = new StoreApi();
            var dateReportApi = new DateReportApi();
            if (selectedStoreId == 0)
            {
                if (user.AspNetRoles.Where(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString()).Any())
                {
                    //Get top store report for brand
                    var listStore = storeApi.GetStoresByBrandId(brandId).Select(q => new { StoreName = q.ShortName, ID = q.ID }).ToList();
                    var dateReports = dateReportApi.GetAllDateReportByTimeRange(startDate, endDate, selectedStoreId, brandId).AsEnumerable().GroupBy(q => q.StoreID);
                    var tmpDataReport = dateReports.Select(q => new StoreReportViewModel
                    {
                        StoreId = q.Key,
                        FinalAmount = q.Sum(i => i.FinalAmount.GetValueOrDefault()),
                        FinalCard = q.Sum(i => i.FinalAmountCard.GetValueOrDefault()),
                        TotalOrder = q.Sum(i => i.TotalOrder),
                        TotalOrderCard = q.Sum(i => i.TotalOrderCard),
                        TotalOrderTakeAway = q.Sum(i => i.TotalOrderTakeAway),
                        TotalOrderDelivery = q.Sum(i => i.TotalOrderDelivery),
                    }).ToList();

                    if (tmpDataReport.Count() == 0)
                    {
                        foreach (var item in listStore)
                        {
                            var tmp = new StoreReportViewModel
                            {
                                StoreId = item.ID,
                                FinalAmount = 0,
                                FinalCard = 0,
                                TotalOrder = 0,
                                TotalOrderCard = 0,
                                TotalOrderTakeAway = 0,
                                TotalOrderDelivery = 0,
                            };
                            tmpDataReport.Add(tmp);
                        }
                    }
                    var result = new List<StoreReportViewModel>();
                    if (orderByDes) {
                        result = tmpDataReport.OrderByDescending(q => q.FinalAmount - q.FinalCard).Skip(skip).Take(take).ToList();
                    }
                    else {
                        result = tmpDataReport.OrderBy(q => q.FinalAmount - q.FinalCard).Skip(skip).Take(take).ToList();
                    }
                    return result;
                }
                else if (user.AspNetRoles.Where(q => q.Id == ((int)RoleTypeEnum.StoreReportViewerMobile).ToString()).Any())
                {
                    var storeUsers = GetStoreUsers(user.Id);
                    if (storeUsers.Count == 0)
                    {
                        return null;
                    }
                    var listStoreId = storeUsers.Select(q => q.StoreId).ToList();
                    var dateReports = dateReportApi.GetDateReportTimeRangeAndListStore(startDate, endDate, listStoreId).AsEnumerable().GroupBy(q => q.StoreID);
                    var tmpDataReport = dateReports.Select(q => new StoreReportViewModel
                    {
                        StoreId = q.Key,
                        FinalAmount = q.Sum(i => i.FinalAmount.GetValueOrDefault()),
                        FinalCard = q.Sum(i => i.FinalAmountCard.GetValueOrDefault()),
                        TotalOrder = q.Sum(i => i.TotalOrder),
                        TotalOrderCard = q.Sum(i => i.TotalOrderCard),
                        TotalOrderTakeAway = q.Sum(i => i.TotalOrderTakeAway),
                        TotalOrderDelivery = q.Sum(i => i.TotalOrderDelivery),
                    }).ToList();

                    if (tmpDataReport.Count() == 0)
                    {
                        foreach (var item in listStoreId)
                        {
                            var tmp = new StoreReportViewModel
                            {
                                StoreId = item,
                                FinalAmount = 0,
                                FinalCard = 0,
                                TotalOrder = 0,
                                TotalOrderCard = 0,
                                TotalOrderTakeAway = 0,
                                TotalOrderDelivery = 0,
                            };
                            tmpDataReport.Add(tmp);
                        }
                    }
                    var result = new List<StoreReportViewModel>();
                    if (orderByDes)
                    {
                        result = tmpDataReport.OrderByDescending(q => q.FinalAmount - q.FinalCard).Skip(skip).Take(take).ToList();
                    }
                    else
                    {
                        result = tmpDataReport.OrderBy(q => q.FinalAmount - q.FinalCard).Skip(skip).Take(take).ToList();
                    }
                    return result;
                }
            }
            else
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Get product report of selected date span
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductReport(int brandId, string startTime, string endTime, int selectedStoreId, string accessToken, int skip, int take, int orderByEnum)
        {
            //Get user from accessToken
            string userId = getIdFromToken(accessToken);
            var userApi = new AspNetUserApi();
            var user = userApi.GetUserById(userId);
            //check permission
            if (!checkRolesPermission(user, selectedStoreId, brandId))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_CHECK_PERMISSION_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var orderDetailApi = new OrderDetailApi();
                var dateProductApi = new DateProductApi();
                var productApi = new ProductApi();

                var startDate = startTime.ToDateTime().GetStartOfDate();
                var endDate = endTime.ToDateTime().GetEndOfDate();
                var today = HmsService.Models.Utils.GetCurrentDateTime().GetEndOfDate();
                var time = startDate.GetStartOfDate();
                var products = productApi.BaseService.GetActiveByBrandId(brandId).ToList();
                //lay du lieu tu database table datereport
                var productReports = new List<DateProduct>();
                if (selectedStoreId == 0)
                {
                    if (user.AspNetRoles.Where(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString()).Any())
                    {
                        productReports = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId).ToList();
                    }
                    else
                    {
                        var listStoreId = GetStoreUsers(user.Id).Select(q => q.StoreId).ToList();
                        productReports = dateProductApi.GetDateProductByTimeRangeAndListStore(startDate, endDate, listStoreId).ToList();
                    }
                }
                else
                {
                    productReports = dateProductApi.GetDateProductByTimeRangeAndStore(startDate, endDate, selectedStoreId).ToList();
                }
                var totalProductsRecords = productReports.Select(q => new DateProductForDashBoard
                {
                    ID = q.ID,
                    ProductID = q.ProductId,
                    StoreID = q.StoreID,
                    CateID = q.CategoryId_,
                    ProductName = q.ProductName_,
                    StoreName = q.Store.Name,
                    Date = q.Date,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount,
                    TotalQuantity = q.Quantity,

                    QuantityAtStore = (q.QuantityAtStore == null) ? 0 : q.QuantityAtStore.Value,
                    QuantityDelivery = (q.QuantityDelivery == null) ? 0 : q.QuantityDelivery.Value,
                    QuantityTakeAway = (q.QuantityTakeAway == null) ? 0 : q.QuantityTakeAway.Value,
                }).ToList();
                if (endDate == today)
                {
                    var todayProductsRecords = new List<DateProductForDashBoard>();
                    if (selectedStoreId == 0 && !user.AspNetRoles.Where(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString()).Any())
                    {
                        var listStoreId = GetStoreUsers(user.Id).Select(q => q.StoreId).ToList();
                        foreach (var storeId in listStoreId)
                        {
                            var tmpTodayProductsRecords = orderDetailApi.GetAllTotalDate(today.GetStartOfDate(), today, selectedStoreId, brandId).GroupBy(q => q.ProductID)
                            .Select(q => new DateProductForDashBoard
                            {
                                ProductID = q.Key,
                                TotalQuantity = q.Sum(a => a.TotalOrderDetails),
                                TotalAmount = q.Sum(a => a.TotalAmount),
                                FinalAmount = q.Sum(a => a.FinalAmount)
                            }).ToList(); //storeid 0 ung voi brand
                            todayProductsRecords.AddRange(tmpTodayProductsRecords);
                        }
                    }
                    else
                    {
                        todayProductsRecords = orderDetailApi.GetAllTotalDate(today.GetStartOfDate(), today, selectedStoreId, brandId).GroupBy(q => q.ProductID)
                            .Select(q => new DateProductForDashBoard
                            {
                                ProductID = q.Key,
                                TotalQuantity = q.Sum(a => a.TotalOrderDetails),
                                TotalAmount = q.Sum(a => a.TotalAmount),
                                FinalAmount = q.Sum(a => a.FinalAmount)
                            }).ToList(); //storeid 0 ung voi brand
                    }
                    totalProductsRecords.AddRange(todayProductsRecords);
                }

                var dateProducts = totalProductsRecords.GroupBy(q => q.ProductID);

                switch (orderByEnum)
                {
                    case (int)ProductReportOrderByEnum.Revenue:
                        dateProducts = dateProducts.OrderByDescending(q => q.Sum(a => a.FinalAmount));
                        break;
                    case (int)ProductReportOrderByEnum.Name:
                        dateProducts = dateProducts.OrderBy(q => 
                        q.Select(a => !String.IsNullOrEmpty(a.ProductName) ? a.ProductName : 
                        products.Where(b => b.ProductID == a.ProductID).FirstOrDefault().ProductName).FirstOrDefault());
                        break;
                    default:
                        dateProducts = dateProducts.OrderByDescending(q => q.Sum(a => a.TotalQuantity));
                        break;
                }
                var queryProducts = dateProducts.Skip(skip).Take(take).ToList();
                var queryResult = queryProducts
                    .Select(q => new ProductReportApiModel
                    {
                        productId = q.Key,
                        productName = products.FirstOrDefault(a => q.Key == a.ProductID).ProductName,
                        totalQty = q.Sum(a => a.TotalQuantity),
                        totalAmount = q.Sum(a => a.TotalAmount),
                        finalAmount = q.Sum(a => a.FinalAmount),
                        PicURL = products.FirstOrDefault(a => q.Key == a.ProductID).PicURL
                    }).ToList();

                //paging
                var totalQuantityAll = queryResult.Sum(q => q.totalQty);

                var productData = queryResult.Select(q => new
                {
                    name = q.productName,
                    ratio = Math.Round((double)q.totalQty / (double)totalQuantityAll * 100, 2),
                    quantity = q.totalQty,
                    total_amount = q.totalAmount,
                    discout = q.totalAmount - q.finalAmount,
                    final_amount = q.finalAmount,
                    pic_url = q.PicURL
                }).ToList();

                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            product_data = productData
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }
        private static string GenerateToken(string customerID)
        {
            var payload = customerID + ":" + DateTime.Now.Ticks.ToString();

            return JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
        }

        public static bool IsTokenValid(string token)
        {
            bool result = false;
            try
            {
                string key = JWT.Decode(token, secretKey);

                result = true;
            }
            catch
            {
            }
            return result;
        }

        private string getIdFromToken(string token)
        {
            string key = JWT.Decode(token, secretKey);
            string[] parts = key.Split(new char[] { ':' });
            return parts[0];
        }

        public List<StoreUserViewModel> GetStoreUsers(string userId)
        {
            var storeUserApi = new StoreUserApi();
            var userApi = new AspNetUserApi();
            var user = userApi.GetUserById(userId);
            var storeUsers = storeUserApi.GetStoresFromUser(user.UserName).ToList();
            return storeUsers;
        }

        private bool checkRolesPermission(AspNetUser user, int selectedStoreId, int brandId)
        {
            try
            {
                var storeApi = new StoreApi();
                if (user.BrandId != brandId)
                {
                    return false;
                }
                //Check store - brand relationship
                var storeIds = storeApi.GetAllStoreByBrandId(brandId).Select(q => q.ID).ToList();
                if (selectedStoreId != 0 && !storeIds.Contains(selectedStoreId))
                {
                    return false;
                }
                //Get all user role id
                var userRoles = user.AspNetRoles.ToList();
                //Check permission for system report view
                if (!userRoles.Where(q => q.Id == ((int)RoleTypeEnum.BrandReportViewerMobile).ToString()).Any())
                {
                    //check permission for store report view
                    if (!userRoles.Where(q => q.Id == ((int)RoleTypeEnum.StoreReportViewerMobile).ToString()).Any())
                    {
                        return false;
                    }
                    //deny if selectedStoreId != 0 and not include in stores assign for this user
                    var listStoreUser = GetStoreUsers(user.Id);
                    if (selectedStoreId != 0 && !listStoreUser.Select(q => q.StoreId).Contains(selectedStoreId))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    class StoreJson
    {
        public int store_id { get; set; }
        public string store_name { get; set; }
        public string address { get; set; }
        public string district { get; set; }
        public string province { get; set; }
    }
    public class TopStoreReportJson
    {
        public string store_name { get; set; }
        public int total_store_order { get; set; }
        public double total_store_revenue { get; set; }
    }

    public class HourReportApiModel
    {
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public double TakeAway { get; set; }
        public double PriceTakeAway { get; set; }
        public double AtStore { get; set; }
        public double PriceAtStore { get; set; }
        public double Delivery { get; set; }
        public double PriceDelivery { get; set; }
        public double OrderCard { get; set; }
        public double PriceOrderCard { get; set; }
        public double Discount { get; set; }
        public double TotalQuantity { get; set; }
        public double TotalPrice { get; set; }
        public double FinalPrice { get; set; }

    }

    public class DateReportApiModel
    {
        public string Date { get; set; }
        public int TakeAway { get; set; }
        public double PriceTakeAway { get; set; }
        public int AtStore { get; set; }
        public double PriceAtStore { get; set; }
        public int Delivery { get; set; }
        public double PriceDelivery { get; set; }
        public int OrderCard { get; set; }
        public double PriceOrderCard { get; set; }
        public double Discount { get; set; }
        public double TotalQuantity { get; set; }
        public double TotalPrice { get; set; }
        public double FinalPrice { get; set; }
    }

    public class ProductReportApiModel
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public int totalQty { get; set; }
        public double totalAmount { get; set; }
        public double finalAmount { get; set; }
        public string PicURL { get; set; }
    }
}
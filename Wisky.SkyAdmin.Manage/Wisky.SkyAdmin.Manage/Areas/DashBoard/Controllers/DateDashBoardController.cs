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
using System.Data.Entity;

namespace Wisky.SkyAdmin.Manage.Areas.DashBoard.Controllers
{
    [Authorize(Roles = "BrandManager, Reception, StoreManager, StoreReportViewer, Manager")]
    public class DateDashBoardController : DomainBasedController
    {
        // GET: DashBoard/DashBoard
        public ActionResult Index()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            return View();
        }//End Index

        public ActionResult DashBoardDateReport(int? storeId)
        {
            ViewBag.storeId = storeId.Value;
            return PartialView("_DashBoardDateReport");
        }//End DashBoardDateReport

        DashBoardProductQtyViewModel GetProductQty(IEnumerable<DateProductForDashBoard> productReports)
        {
            int qtyTotalProduct = productReports.Sum(q => q.TotalQuantity);
            int qtyProductAtStore = productReports.Sum(q => q.QuantityAtStore);
            int qtyProductDelivery = productReports.Sum(q => q.QuantityDelivery);
            int qtyProductTakeAway = productReports.Sum(q => q.QuantityTakeAway);
            return new DashBoardProductQtyViewModel
            {
                qtyTotalProduct = qtyTotalProduct,
                qtyProductAtStore = qtyProductAtStore,
                qtyProductDelivery = qtyProductDelivery,
                qtyProductTakeAway = qtyProductTakeAway
            };
        }

        DashBoardProductQtyViewModel GetProductQty(IEnumerable<OrderForDashBoard> dateOrders)
        {
            int qtyTotalProduct = dateOrders.Sum(q => q.TotalOrderDetails);
            int qtyProductAtStore = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.AtStore).Sum(q => q.TotalOrderDetails);
            int qtyProductDelivery = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.Delivery).Sum(q => q.TotalOrderDetails);
            int qtyProductTakeAway = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.TakeAway).Sum(q => q.TotalOrderDetails);
            int qtyProductOrderCard = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard).Sum(q => q.TotalOrderDetails);
            return new DashBoardProductQtyViewModel
            {
                qtyTotalProduct = qtyTotalProduct,
                qtyProductAtStore = qtyProductAtStore,
                qtyProductDelivery = qtyProductDelivery,
                qtyProductTakeAway = qtyProductTakeAway,
                qtyProductOrderCard = qtyProductOrderCard,
            };
        }

        DashBoardProductAvgViewModel GetProductAvg(DashBoardProductQtyViewModel productQty, DashBoardReceiptQtyViewModel receiptQty)
        {
            double avgTotalProductPerReceipt = (receiptQty.totalReceipt - receiptQty.qtyOrderCard != 0) ? ((double)(productQty.qtyTotalProduct) / (receiptQty.totalReceipt - receiptQty.qtyOrderCard)) : 0;
            double avgProductAtStorePerReceipt = (receiptQty.qtyAtStore != 0) ? ((double)productQty.qtyProductAtStore / receiptQty.qtyAtStore) : 0;
            double avgProductDeliveryPerReceipt = (receiptQty.qtyDelivery != 0) ? ((double)productQty.qtyProductDelivery / receiptQty.qtyDelivery) : 0;
            double avgProductTakeAwayPerReceipt = (receiptQty.qtyTakeAway != 0) ? ((double)productQty.qtyProductTakeAway / receiptQty.qtyTakeAway) : 0;
            double avgProductOrderCardPerReceipt = (receiptQty.qtyOrderCard != 0) ? ((double)productQty.qtyProductOrderCard / receiptQty.qtyOrderCard) : 0;
            return new DashBoardProductAvgViewModel
            {
                avgTotalProductPerReceipt = avgTotalProductPerReceipt,
                avgProductAtStorePerReceipt = avgProductAtStorePerReceipt,
                avgProductDeliveryPerReceipt = avgProductDeliveryPerReceipt,
                avgProductTakeAwayPerReceipt = avgProductTakeAwayPerReceipt,
                avgProductOrderCardPerReceipt = avgProductOrderCardPerReceipt
            };
        }

        DashBoardReceiptAvgViewModel GetRecepitAvg(DashBoardReceiptQtyViewModel receiptQty, DashboardFinalRevenueViewModel finalRevenue)
        {
            return new DashBoardReceiptAvgViewModel
            {
                avgFinalReceipt = ((receiptQty.totalReceipt - receiptQty.qtyOrderCard) != 0) ? ((finalRevenue.finalAmount - finalRevenue.finalOrderCard) / (receiptQty.totalReceipt - receiptQty.qtyOrderCard)) : 0,
                avgFinalAtStore = (receiptQty.qtyAtStore != 0) ? (finalRevenue.finalAtStore / receiptQty.qtyAtStore) : 0,
                avgFinalDelivery = (receiptQty.qtyDelivery != 0) ? (finalRevenue.finalDelivery / receiptQty.qtyDelivery) : 0,
                avgFinalTakeAway = (receiptQty.qtyTakeAway != 0) ? (finalRevenue.finalTakeAway / receiptQty.qtyTakeAway) : 0,
                avgFinalOrderCard = (receiptQty.qtyOrderCard != 0) ? (finalRevenue.finalOrderCard / receiptQty.qtyOrderCard) : 0,
            };
        }

        DashBoardReceiptQtyViewModel GetReceiptQty(IEnumerable<DateReportForDashBoard> dateReports)
        {
            int totalReceipt = dateReports.Sum(q => q.TotalOrder);
            int qtyAtStore = dateReports.Sum(q => q.TotalOrderAtStore);
            var qtyDelivery = dateReports.Sum(q => q.TotalOrderDelivery);
            var qtyTakeAway = dateReports.Sum(q => q.TotalOrderTakeAway);
            var qtyOrderCard = dateReports.Sum(q => q.TotalOrderCard);
            return new DashBoardReceiptQtyViewModel
            {
                totalReceipt = totalReceipt,
                qtyAtStore = qtyAtStore,
                qtyDelivery = qtyDelivery,
                qtyTakeAway = qtyTakeAway,
                qtyOrderCard = qtyOrderCard
            };
        }

        DashBoardReceiptQtyViewModel GetReceiptQty(IEnumerable<OrderForDashBoard> dateOrders)
        {
            dateOrders = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish);
            int totalReceipt = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Count();
            int qtyAtStore = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.AtStore).Count();
            int qtyDelivery = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.Delivery).Count();
            int qtyTakeAway = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.TakeAway).Count();
            int qtyOrderCard = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard).Count();
            return new DashBoardReceiptQtyViewModel
            {
                totalReceipt = totalReceipt,
                qtyAtStore = qtyAtStore,
                qtyDelivery = qtyDelivery,
                qtyTakeAway = qtyTakeAway,
                qtyOrderCard = qtyOrderCard
            };
        }

        DashBoardDiscountRevenueViewModel GetDiscountRevenue(IEnumerable<DateReportForDashBoard> dateReports)
        {
            var totalAmount = dateReports.Sum(q => q.TotalAmount.Value);
            var finalAmountCash = dateReports.Sum(q => (q.FinalAmount.Value - q.FinalAmountCard.Value));
            var finalAmountCard = dateReports.Sum(q => (q.FinalAmountCard.Value));
            var totalDiscount = dateReports.Sum(q => q.Discount.Value + q.DiscountOrderDetail.Value);
            return new DashBoardDiscountRevenueViewModel
            {
                totalAmount = totalAmount,
                finalAmountCash = finalAmountCash,
                finalAmountCard = finalAmountCard,
                totalDiscount = totalDiscount
            };
        }

        DashBoardDiscountRevenueViewModel GetDiscountRevenue(IEnumerable<OrderForDashBoard> dateOrders)
        {
            dateOrders = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish && q.OrderType != (int)OrderTypeEnum.DropProduct);
            var totalAmount = dateOrders.Sum(q => q.TotalAmount.Value);
            var finalAmountCash = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard).Sum(q => q.FinalAmount.Value);
            var finalAmountCard = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard).Sum(q => q.FinalAmount.Value);
            var totalDiscount = dateOrders.Sum(q => q.Discount.Value + q.DiscountOrderDetail.Value);
            return new DashBoardDiscountRevenueViewModel
            {
                totalAmount = totalAmount,
                finalAmountCash = finalAmountCash,
                finalAmountCard = finalAmountCard,
                totalDiscount = totalDiscount
            };
        }

        DashboardFinalRevenueViewModel GetFinalRevenue(IEnumerable<OrderForDashBoard> dateOrders)
        {
            dateOrders = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish);
            var finalAmount = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Sum(q => q.FinalAmount.Value);
            var finalAtStore = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.AtStore).Sum(q => q.FinalAmount.Value);
            var finalDelivery = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.Delivery).Sum(q => q.FinalAmount.Value);
            var finalTakeAway = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.TakeAway).Sum(q => q.FinalAmount.Value);
            var finalOrderCard = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard).Sum(q => q.FinalAmount.Value);
            return new DashboardFinalRevenueViewModel
            {
                finalAmount = finalAmount,
                finalAtStore = finalAtStore,
                finalDelivery = finalDelivery,
                finalTakeAway = finalTakeAway,
                finalOrderCard = finalOrderCard
            };
        }

        DashboardFinalRevenueViewModel GetFinalRevenue(IEnumerable<DateReportForDashBoard> dateReports)
        {
            var finalAmount = dateReports.Sum(q => q.FinalAmount.Value);
            var finalAtStore = dateReports.Sum(q => q.FinalAmountAtStore.HasValue ? q.FinalAmountAtStore.Value : 0 );
            var finalDelivery = dateReports.Sum(q => q.FinalAmountDelivery.HasValue ? q.FinalAmountDelivery.Value : 0);
            var finalTakeAway = dateReports.Sum(q => q.FinalAmountTakeAway.HasValue ? q.FinalAmountTakeAway.Value : 0);
            var finalOrderCard = dateReports.Sum(q => q.FinalAmountCard.HasValue ? q.FinalAmountCard.Value : 0);
            return new DashboardFinalRevenueViewModel
            {
                finalAmount = finalAmount,
                finalAtStore = finalAtStore,
                finalDelivery = finalDelivery,
                finalTakeAway = finalTakeAway,
                finalOrderCard = finalOrderCard
            };
        }

        DashBoardCanceledReceiptViewModel GetCanceledReceipt(IEnumerable<DateReportForDashBoard> dateReports)
        {
            //So luong hoa don huy
            int qtyReceiptCancel = dateReports.Sum(q => q.TotalOrderCanceled.HasValue ? q.TotalOrderCanceled.Value : 0);
            int qtyReceiptPreCancel = dateReports.Sum(q => q.TotalOrderPreCanceled.HasValue ? q.TotalOrderPreCanceled.Value : 0);
            int qtyReceiptTotalCancel = qtyReceiptCancel + qtyReceiptPreCancel;

            //Gia tri hoa don huy
            double finalReceiptCancel = dateReports.Sum(q => q.FinalAmountCanceled.HasValue ? q.FinalAmountCanceled.Value : 0);
            double finalReceiptPreCancel = dateReports.Sum(q => q.FinalAmountPreCanceled.HasValue ? q.FinalAmountPreCanceled.Value : 0);
            double finalReceiptTotalCancel = finalReceiptCancel + finalReceiptPreCancel;

            return new DashBoardCanceledReceiptViewModel
            {
                qtyReceiptCancel = qtyReceiptCancel,
                qtyReceiptPreCancel = qtyReceiptPreCancel,
                qtyReceiptTotalCancel = qtyReceiptTotalCancel,
                finalReceiptCancel = finalReceiptCancel,
                finalReceiptPreCancel = finalReceiptPreCancel,
                finalReceiptTotalCancel = finalReceiptTotalCancel
            };
        }

        DashBoardCanceledReceiptViewModel GetCanceledReceipt(IEnumerable<OrderForDashBoard> dateOrders)
        {
            //So luong hoa don huy
            int qtyReceiptCancel = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel).Count();
            int qtyReceiptPreCancel = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel).Count();
            int qtyReceiptTotalCancel = qtyReceiptCancel + qtyReceiptPreCancel;

            //Gia tri hoa don huy
            double finalReceiptCancel = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel).Sum(q => q.FinalAmount.Value);
            double finalReceiptPreCancel = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel).Sum(q => q.FinalAmount.Value);
            double finalReceiptTotalCancel = finalReceiptCancel + finalReceiptPreCancel;

            return new DashBoardCanceledReceiptViewModel
            {
                qtyReceiptCancel = qtyReceiptCancel,
                qtyReceiptPreCancel = qtyReceiptPreCancel,
                qtyReceiptTotalCancel = qtyReceiptTotalCancel,
                finalReceiptCancel = finalReceiptCancel,
                finalReceiptPreCancel = finalReceiptPreCancel,
                finalReceiptTotalCancel = finalReceiptTotalCancel
            };
        }

        DashboardStoreDateViewModel GetDateStore(IEnumerable<DateReportForDashBoard> dateReports, int brandId)
        {
            var storeApi = new StoreApi();
            var storeList = storeApi
                .GetListStoreByBrandId(brandId)
                .Select(q => new
                {
                    ID = q.ID,
                    Name = q.ShortName
                });
            var storeData = new Dictionary<string, DashboardStoreDataDateViewModel>();

            foreach (var store in storeList)
            {
                double finalRevenue = dateReports.Sum(q => q.FinalAmount ?? 0);
                storeData.Add(
                    store.ID.ToString(),
                    new DashboardStoreDataDateViewModel
                    {
                        storeName = store.Name,
                        finalRevenue = finalRevenue
                    });
            }
            return new DashboardStoreDateViewModel
            {
                storeIdList = storeList.Select(q => q.ID.ToString()).ToList(),
                storeData = storeData
            };
        }

        DashboardStoreDateViewModel GetDateStore(IEnumerable<OrderForDashBoard> dateOrders, int brandId)
        {
            var storeApi = new StoreApi();
            var storeList = storeApi
                .GetListStoreByBrandId(brandId)
                .Select(q => new
                {
                    ID = q.ID,
                    Name = q.ShortName
                });
            var storeData = new Dictionary<string, DashboardStoreDataDateViewModel>();

            foreach (var store in storeList)
            {
                double finalRevenue = dateOrders.Sum(q => q.FinalAmount ?? 0);
                storeData.Add(
                    store.ID.ToString(),
                    new DashboardStoreDataDateViewModel
                    {
                        storeName = store.Name,
                        finalRevenue = finalRevenue
                    });
            }
            return new DashboardStoreDateViewModel
            {
                storeIdList = storeList.Select(q => q.ID.ToString()).ToList(),
                storeData = storeData
            };

        }

        DashBoardMonthOverViewModel GetMonthOverview(IEnumerable<OrderForDashBoard> monthOrders, List<DateTime> dateList)
        {
            monthOrders = monthOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish);
            var finalList = new List<double>();
            var totalReceiptList = new List<int>();
            var startDate = dateList.FirstOrDefault().GetStartOfDate();
            var endDate = dateList.LastOrDefault().GetEndOfDate();
            //for (int i = 0; i < 4; i++)
            //{
            //    var date = startDate;
            //    var dateOrders = monthOrders.Where(q => q.Date >= date && q.Date <= date.AddDays(6).GetEndOfDate());
            //    if (date.AddDays(14) > endDate)
            //    {
            //        dateOrders = monthOrders.Where(q => q.Date >= date && q.Date <= endDate);
            //    }
            //    var finalAmount = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Sum(q => q.FinalAmount.Value);
            //    var totalReceipt = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Count();

            //    finalList.Add(finalAmount);
            //    totalReceiptList.Add(totalReceipt);


            //    startDate = startDate.AddDays(7);
            //}
            foreach (var date in dateList)
            {
                var dateOrders = monthOrders.Where(q => q.Date >= date.GetStartOfDate() && q.Date <= date.GetEndOfDate());
                var finalAmount = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Sum(q => q.FinalAmount.Value);
                var totalReceipt = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Count();
                finalList.Add(finalAmount);
                totalReceiptList.Add(totalReceipt);
            }
            return new DashBoardMonthOverViewModel
            {
                revenueFinalList = finalList,
                receiptQtyTotalList = totalReceiptList,
            };
        }

        DashBoardMonthOverViewModel GetMonthOverview(IEnumerable<DateReportForDashBoard> monthReports, DateTime startDate, DateTime endDate)
        {
            var finalList = new List<double>();
            var totalReceiptList = new List<int>();
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();
            var date = startDate;
            //for (int i = 0; i < 4; i++)
            //{
            //    var date = startDate;
            //    var dateOrders = monthOrders.Where(q => q.Date >= date && q.Date <= date.AddDays(6).GetEndOfDate());
            //    if (date.AddDays(14) > endDate)
            //    {
            //        dateOrders = monthOrders.Where(q => q.Date >= date && q.Date <= endDate);
            //    }
            //    var finalAmount = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Sum(q => q.FinalAmount.Value);
            //    var totalReceipt = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct).Count();

            //    finalList.Add(finalAmount);
            //    totalReceiptList.Add(totalReceipt);


            //    startDate = startDate.AddDays(7);
            //}
            while (date >= startDate && date <= endDate)
            {
                var dateReports = monthReports.Where(q => q.Date >= date.GetStartOfDate() && q.Date <= date.GetEndOfDate());
                var finalAmount = dateReports.Sum(q => q.FinalAmount.Value);
                var totalReceipt = dateReports.Sum(q => q.TotalOrder);
                finalList.Add(finalAmount);
                totalReceiptList.Add(totalReceipt);
                date = date.AddDays(1);
            }
            return new DashBoardMonthOverViewModel
            {
                revenueFinalList = finalList,
                receiptQtyTotalList = totalReceiptList,
            };
        }
        DashboardCardPaymentDateViewModel GetPaymentNumber(IQueryable<HmsService.Models.Entities.Payment> payments)
        {
            DashboardCardPaymentDateViewModel model = new DashboardCardPaymentDateViewModel();
            payments = payments.Where(q =>
                (q.ToRentID != null && q.Order.OrderType != (int) OrderTypeEnum.DropProduct &&
                 q.Order.OrderStatus == (int) OrderStatusEnum.Finish) ||
                (q.CostID != null && q.Cost.CostStatus == (int) CostStatusEnum.Approved));
            //model.totalPayment = 0;
            //model.totalCashPayment = 0;
            //model.totalCashPaymentOrderCard = 0;
            //model.totalMemberPayment = 0;
            //model.totalMasterCardPayment = 0;
            //model.totalVisaCardPayment = 0;
            //model.totalMomoPayment = 0;
            //model.totalOtherPayment = 0;
            //model.numberTotalPayment = 0;
            //model.numberCashPayment = 0;
            //model.numberCashPaymentOrderCard = 0;
            //model.numberMemberPayment = 0;
            //model.numberMasterCardPayment = 0;
            //model.numberVisaCardPayment = 0;
            //model.numberMomoPayment = 0;
            //model.numberOtherPayment = 0;

            //payments = payments.Where(q => q.Order.OrderType != (int)OrderTypeEnum.DropProduct && q.Order.OrderStatus == (int)OrderStatusEnum.Finish);

            //var sumCash = payments.Where(q => q.Type == 1 || q.Type == 9).Sum(a => a.Amount);
            //var sumMember = payments.Where(q => q.Type == 1 || q.Type == 9).Sum(a => a.Amount);
            //var sumMaster = payments.Where(q => q.Type == 1 || q.Type == 9).Sum(a => a.Amount);
            //var sumVisa = payments.Where(q => q.Type == 1 || q.Type == 9).Sum(a => a.Amount);
            //var sum = payments.Where(q => q.Type == 1 || q.Type == 9).Sum(a => a.Amount);
            //foreach (var payment in payments)
            //{
            //    //if (payment.Order.OrderType != (int)OrderTypeEnum.DropProduct && payment.Order.OrderStatus == (int)OrderStatusEnum.Finish)
            //    {
            //        model.numberTotalPayment++;

            //        if (payment.Type == (int)PaymentTypeEnum.Cash)
            //        {
            //            if (payment.ToRentID != null)
            //            {
            //                if (payment.Order.OrderType == (int) OrderTypeEnum.OrderCard)
            //                {
            //                    model.numberCashPaymentOrderCard++;
            //                    model.totalCashPaymentOrderCard += payment.Amount;
            //                }
            //                else
            //                {
            //                    model.numberCashPayment++;
            //                    model.totalCashPayment += payment.Amount;
            //                }
            //            }
            //            else
            //            {
            //                model.numberCashPayment++;
            //                model.totalCashPayment += payment.Amount;
            //            }
            //            model.totalPayment += payment.Amount;
            //        }
            //        else if (payment.Type == (int)PaymentTypeEnum.MemberPayment)
            //        {
            //            model.numberMemberPayment++;
            //            model.totalMemberPayment += payment.Amount;
            //            model.totalPayment += payment.Amount;
            //        }
            //        else if (payment.Type == (int)PaymentTypeEnum.MasterCard)
            //        {
            //            model.numberMasterCardPayment++;
            //            model.totalMasterCardPayment += payment.Amount;
            //            model.totalPayment += payment.Amount;
            //        }
            //        else if (payment.Type == (int)PaymentTypeEnum.VisaCard)
            //        {
            //            model.numberVisaCardPayment++;
            //            model.totalVisaCardPayment += payment.Amount;
            //            model.totalPayment += payment.Amount;
            //        }
            //        else if (payment.Type == (int)PaymentTypeEnum.ExchangeCash)
            //        {
            //            model.totalPayment += payment.Amount;
            //            model.totalCashPayment += payment.Amount;
            //            model.numberTotalPayment--;
            //        } else if (payment.Type == (int) PaymentTypeEnum.ThirdParty)
            //        {
            //            model.totalPayment += payment.Amount;
            //            model.totalMomoPayment += payment.Amount;
            //            model.numberMomoPayment++;
            //        }
            //        else
            //        {
            //            model.numberOtherPayment++;
            //            model.totalOtherPayment += payment.Amount;
            //            model.totalPayment += payment.Amount;
            //        }
            //    }
            //}

            var cashPayments = payments.Where(q =>
                (q.Type == (int) PaymentTypeEnum.Cash || q.Type == (int) PaymentTypeEnum.ExchangeCash) &&
                ((q.ToRentID != null && q.Order.OrderType != (int) OrderTypeEnum.OrderCard) || (q.CostID != null)));
            model.numberCashPayment = cashPayments.Count() - payments.Where(q => q.Type == (int) PaymentTypeEnum.ExchangeCash).Count();
            model.totalCashPayment = model.numberCashPayment > 0 ? cashPayments.Sum(q => q.Amount) : 0;

            var cashPaymentsOrderCard = payments.Where(q =>
                q.Type == (int) PaymentTypeEnum.Cash && q.ToRentID != null &&
                q.Order.OrderType == (int) OrderTypeEnum.OrderCard);
            model.numberCashPaymentOrderCard = cashPaymentsOrderCard.Count();
            model.totalCashPaymentOrderCard =
                model.numberCashPaymentOrderCard > 0 ? cashPaymentsOrderCard.Sum(q => q.Amount) : 0;

            var memberPayments = payments.Where(q => q.Type == (int) PaymentTypeEnum.MemberPayment);
            model.numberMemberPayment = memberPayments.Count();
            model.totalMemberPayment = model.numberMemberPayment > 0 ? memberPayments.Sum(q => q.Amount) : 0;

            var thirdPartyPayments = payments.Where(q => q.Type == (int)PaymentTypeEnum.MoMo);
            model.numberMomoPayment = thirdPartyPayments.Count();
            model.totalMomoPayment = model.numberMomoPayment > 0 ? thirdPartyPayments.Sum(q => q.Amount) : 0;

            var visaCardPayments = payments.Where(q => q.Type == (int)PaymentTypeEnum.VisaCard);
            model.numberVisaCardPayment = visaCardPayments.Count();
            model.totalVisaCardPayment = model.numberVisaCardPayment > 0 ? visaCardPayments.Sum(q => q.Amount) : 0;

            var masterCardPayments = payments.Where(q => q.Type == (int)PaymentTypeEnum.MasterCard);
            model.numberMasterCardPayment = masterCardPayments.Count();
            model.totalMasterCardPayment = model.numberMasterCardPayment > 0 ? masterCardPayments.Sum(q => q.Amount) : 0;

            var otherPayments = payments.Where(q =>
                q.Type != (int)PaymentTypeEnum.Cash && q.Type != (int)PaymentTypeEnum.ExchangeCash &&
                q.Type != (int)PaymentTypeEnum.MasterCard && q.Type != (int)PaymentTypeEnum.VisaCard &&
                q.Type != (int)PaymentTypeEnum.MemberPayment && q.Type != (int)PaymentTypeEnum.MoMo);
            model.numberOtherPayment = otherPayments.Count();
            model.totalOtherPayment = model.numberOtherPayment > 0 ? otherPayments.Sum(q => q.Amount) : 0;

            model.numberTotalPayment = payments.Count();
            model.totalPayment = model.numberTotalPayment > 0 ? payments.Sum(q => q.Amount) : 0;
            
            return model;
        }

        //DashBoardReceiptMonthViewModel GetMonthReceipt(IEnumerable<DateReportForDashBoard> monthReports, List<DateTime> dateList)
        //{
        //    var receiptAtStoreList = new List<int>();
        //    var receiptDeliveryList = new List<int>();
        //    var receiptTakeAwayList = new List<int>();
        //    var startDate = dateList.FirstOrDefault().GetStartOfDate();
        //    var endDate = dateList.LastOrDefault().GetEndOfDate();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        var date = startDate;
        //        var dateOrders = monthReports.Where(q => q.Date >= date && q.Date <= date.AddDays(6).GetEndOfDate());
        //        if (date.AddDays(14) > endDate)
        //        {
        //            dateOrders = monthReports.Where(q => q.Date >= date && q.Date <= endDate);
        //        }

        //        var qtyReceiptAtStore = dateOrders.Sum(q => q.TotalOrderAtStore);
        //        var qtyReceiptDelivery = dateOrders.Sum(q => q.TotalOrderDelivery);
        //        var qtyRecepitTakeAway = dateOrders.Sum(q => q.TotalOrderTakeAway);

        //        receiptAtStoreList.Add(qtyReceiptAtStore);
        //        receiptDeliveryList.Add(qtyReceiptDelivery);
        //        receiptTakeAwayList.Add(qtyRecepitTakeAway);

        //        startDate = startDate.AddDays(7);
        //    }
        //    return new DashBoardReceiptMonthViewModel
        //    {
        //        receiptQtyAtStoreList = receiptAtStoreList,
        //        receiptQtyDeliveryList = receiptDeliveryList,
        //        receiptQtyTakeAwayList = receiptTakeAwayList
        //    };
        //}

        //DashBoardRevenueMonthViewModel GetMonthRevenue(IEnumerable<OrderForDashBoard> monthOrders, List<DateTime> dateList)
        //{
        //    var revenueAtStoreList = new List<double>();
        //    var revenueDeliveryList = new List<double>();
        //    var revenueTakeAwayList = new List<double>();

        //    var startDate = dateList.FirstOrDefault().GetStartOfDate();
        //    var endDate = dateList.LastOrDefault().GetEndOfDate();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        var date = startDate;
        //        var dateOrders = monthOrders.Where(q => q.Date >= date && q.Date <= date.AddDays(6).GetEndOfDate());
        //        if (date.AddDays(14) > endDate)
        //        {
        //            dateOrders = monthOrders.Where(q => q.Date >= date && q.Date <= endDate);
        //        }
        //        var revenueAtStore = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.AtStore).Sum(q => q.FinalAmount ?? 0);
        //        var revenueDelivery = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.Delivery).Sum(q => q.FinalAmount ?? 0);
        //        var revenueTakeAway = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.TakeAway).Sum(q => q.FinalAmount ?? 0);

        //        revenueAtStoreList.Add(revenueAtStore);
        //        revenueDeliveryList.Add(revenueDelivery);
        //        revenueTakeAwayList.Add(revenueTakeAway);

        //        startDate = startDate.AddDays(7);
        //    }

        //    return new DashBoardRevenueMonthViewModel
        //    {
        //        revenueAtStoreList = revenueAtStoreList,
        //        revenueDeliveryList = revenueDeliveryList,
        //        revenueTakeAwayList = revenueTakeAwayList
        //    };
        //}

        //DashBoardProductMonthViewModel GetMonthProduct(IEnumerable<DateProductForDashBoard> productReports, List<DateTime> dateList)
        //{
        //    var productAtStoreList = new List<int>();
        //    var productDeliveryList = new List<int>();
        //    var productTakeAwayList = new List<int>();
        //    var startDate = dateList.FirstOrDefault().GetStartOfDate();
        //    var endDate = dateList.LastOrDefault().GetEndOfDate();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        var date = startDate;
        //        var dateOrders = productReports.Where(q => q.Date >= date && q.Date <= date.AddDays(6).GetEndOfDate());
        //        if (date.AddDays(14) > endDate)
        //        {
        //            dateOrders = productReports.Where(q => q.Date >= date && q.Date <= endDate);
        //        }

        //        var productAtStore = dateOrders.Sum(q => q.QuantityAtStore);
        //        var productDelivery = dateOrders.Sum(q => q.QuantityDelivery);
        //        var productTakeAway = dateOrders.Sum(q => q.QuantityTakeAway);

        //        productAtStoreList.Add(productAtStore);
        //        productDeliveryList.Add(productDelivery);
        //        productTakeAwayList.Add(productTakeAway);

        //        startDate = startDate.AddDays(7);
        //    }
        //    return new DashBoardProductMonthViewModel
        //    {
        //        productQtyAtStoreList = productAtStoreList,
        //        productQtyDeliveryList = productDeliveryList,
        //        productQtyTakeAwayList = productTakeAwayList
        //    };
        //}

        //DashboardStoreMonthViewModel GetMonthStore(IEnumerable<DateReportForDashBoard> dateReports, List<DateTime> dateList, int brandId)
        //{
        //    var storeApi = new StoreApi();
        //    var storeList = storeApi
        //        .GetListStoreByBrandId(brandId)
        //        .Select(q => new
        //        {
        //            ID = q.ID,
        //            Name = q.ShortName
        //        });
        //    var startDate = dateList.FirstOrDefault().GetStartOfDate();
        //    var endDate = dateList.LastOrDefault().GetEndOfDate();
        //    var storeData = new Dictionary<string, DashboardStoreDataMonthViewModel>();

        //    foreach (var store in storeList)
        //    {
        //        var storeReport = dateReports.Where(q => q.StoreID == store.ID);
        //        var totalReceiptQtyList = new List<int>();
        //        var finalRevenueList = new List<double>();
        //        for (int i = 0; i < 4; i++)
        //        {
        //            var date = startDate;
        //            var dateOrders = storeReport.Where(q => q.Date >= date && q.Date <= date.AddDays(6).GetEndOfDate());
        //            if (date.AddDays(14) > endDate)
        //            {
        //                dateOrders = storeReport.Where(q => q.Date >= date && q.Date <= endDate);
        //            }

        //            var finalAmount = dateOrders.Sum(q => q.FinalAmount ?? 0);

        //            finalRevenueList.Add(finalAmount);
        //            startDate = startDate.AddDays(7);
        //        }
        //        storeData.Add(store.ID.ToString(), new DashboardStoreDataMonthViewModel
        //        {
        //            storeName = store.Name,
        //            finalRevenueList = finalRevenueList
        //        });
        //    }
        //    return new DashboardStoreMonthViewModel
        //    {
        //        storeIdList = storeList.Select(q => q.ID.ToString()).ToList(),
        //        storeData = storeData
        //    };
        //}

        public JsonResult DateData(int storeId, int brandId, string _startDate, string _endDate)
        {
            var startDate = Utils.ToDateTime(_startDate).GetStartOfDate();
            var endDate = Utils.ToDateTime(_endDate).GetEndOfDate();
            var today = Utils.GetCurrentDateTime().GetEndOfDate();
            var dateReportApi = new DateReportApi();
            var orderApi = new OrderApi();
            var dateProductApi = new DateProductApi();
            DashboardFinalRevenueViewModel finalRevenue;
            DashBoardReceiptQtyViewModel receiptQuantity;
            DashBoardDiscountRevenueViewModel discountRevenue;
            DashBoardCanceledReceiptViewModel canceledReceipt;
            DashBoardReceiptAvgViewModel receiptAverage;
            DashBoardProductQtyViewModel productQuantity;
            DashBoardProductAvgViewModel productAverage;
            DashBoardMonthOverViewModel monthOverviewList = null;
            IEnumerable<OrderForDashBoard> dateOrders;
            IEnumerable<DateReportForDashBoard> dateReports;
            IEnumerable<DateProductForDashBoard> productReports;

            var dateList = new List<DateTime>();
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                dateList.Add(d);
            }

            //dateOrders = orderApi.GetOrderForDashboard(startDate, endDate, brandId, storeId);
            //finalRevenue = GetFinalRevenue(dateOrders);
            //canceledReceipt = GetCanceledReceipt(dateOrders);

            if (endDate != today)
            {
                dateReports = dateReportApi.GetDateReportForDashboard(startDate, endDate, brandId, storeId);
                discountRevenue = GetDiscountRevenue(dateReports);
                receiptQuantity = GetReceiptQty(dateReports);
                finalRevenue = GetFinalRevenue(dateReports);
                canceledReceipt = GetCanceledReceipt(dateReports);
                productReports = dateProductApi.GetDateProductForDashBoard(startDate, endDate, brandId, storeId);
                productQuantity = GetProductQty(productReports);
                if (dateList.Count > 1)
                {
                    monthOverviewList = GetMonthOverview(dateReports, startDate, endDate);

                }
            }
            else
            {
                if (dateList.Count > 1)
                {
                    //Get reports upto yesterday
                    dateReports = dateReportApi.GetDateReportForDashboard(startDate, endDate.AddDays(-1), brandId, storeId);
                    var discountRevenueTmp = GetDiscountRevenue(dateReports);
                    var receiptQuantityTmp = GetReceiptQty(dateReports);
                    var finalRevenueTmp = GetFinalRevenue(dateReports);
                    var canceledReceiptTmp = GetCanceledReceipt(dateReports);
                    monthOverviewList = GetMonthOverview(dateReports, startDate, endDate.AddDays(-1));

                    productReports = dateProductApi.GetDateProductForDashBoard(startDate, endDate.AddDays(-1), brandId, storeId);
                    var productQuantityTmp = GetProductQty(productReports);

                    //Todays data
                    var todayOrders = orderApi.GetOrderForDashboard(today.GetStartOfDate(), today.GetEndOfDate(), brandId, storeId);
                    var todayDiscountRevenue = GetDiscountRevenue(todayOrders);
                    var todayReceiptQuantity = GetReceiptQty(todayOrders);
                    var todayProductQuantity = GetProductQty(todayOrders);
                    var todayFinalRevenue = GetFinalRevenue(todayOrders);
                    var todayCanceledReceipt = GetCanceledReceipt(todayOrders);

                    //Summary
                    discountRevenue = new DashBoardDiscountRevenueViewModel
                    {
                        totalAmount = discountRevenueTmp.totalAmount + todayDiscountRevenue.totalAmount,
                        finalAmountCash = discountRevenueTmp.finalAmountCash + todayDiscountRevenue.finalAmountCash,
                        finalAmountCard = discountRevenueTmp.finalAmountCard + todayDiscountRevenue.finalAmountCard,
                        totalDiscount = discountRevenueTmp.totalDiscount + todayDiscountRevenue.totalDiscount
                    };

                    receiptQuantity = new DashBoardReceiptQtyViewModel
                    {
                        totalReceipt = receiptQuantityTmp.totalReceipt + todayReceiptQuantity.totalReceipt,
                        qtyAtStore = receiptQuantityTmp.qtyAtStore + todayReceiptQuantity.qtyAtStore,
                        qtyDelivery = receiptQuantityTmp.qtyDelivery + todayReceiptQuantity.qtyDelivery,
                        qtyTakeAway = receiptQuantityTmp.qtyTakeAway + todayReceiptQuantity.qtyTakeAway,
                        qtyOrderCard = receiptQuantityTmp.qtyOrderCard + todayReceiptQuantity.qtyOrderCard
                    };

                    productQuantity = new DashBoardProductQtyViewModel
                    {
                        qtyTotalProduct = productQuantityTmp.qtyTotalProduct + todayProductQuantity.qtyTotalProduct - todayProductQuantity.qtyProductOrderCard,
                        qtyProductAtStore = productQuantityTmp.qtyProductAtStore + todayProductQuantity.qtyProductAtStore,
                        qtyProductDelivery = productQuantityTmp.qtyProductDelivery + todayProductQuantity.qtyProductDelivery,
                        qtyProductTakeAway = productQuantityTmp.qtyProductTakeAway + todayProductQuantity.qtyProductTakeAway,
                        qtyProductOrderCard = productQuantityTmp.qtyProductOrderCard + todayProductQuantity.qtyProductOrderCard
                    };

                    finalRevenue = new DashboardFinalRevenueViewModel
                    {
                        finalAmount = finalRevenueTmp.finalAmount + todayFinalRevenue.finalAmount,
                        finalAtStore = finalRevenueTmp.finalAtStore + todayFinalRevenue.finalAtStore,
                        finalTakeAway = finalRevenueTmp.finalTakeAway + todayFinalRevenue.finalTakeAway,
                        finalDelivery = finalRevenueTmp.finalDelivery + todayFinalRevenue.finalDelivery,
                        finalOrderCard = finalRevenueTmp.finalOrderCard + todayFinalRevenue.finalOrderCard,
                    };

                    canceledReceipt = new DashBoardCanceledReceiptViewModel
                    {
                        qtyReceiptCancel = canceledReceiptTmp.qtyReceiptCancel + todayCanceledReceipt.qtyReceiptCancel,
                        qtyReceiptPreCancel = canceledReceiptTmp.qtyReceiptPreCancel + todayCanceledReceipt.qtyReceiptPreCancel,
                        qtyReceiptTotalCancel = canceledReceiptTmp.qtyReceiptTotalCancel + todayCanceledReceipt.qtyReceiptTotalCancel,

                        finalReceiptCancel = canceledReceiptTmp.finalReceiptCancel + todayCanceledReceipt.finalReceiptCancel,
                        finalReceiptPreCancel = canceledReceiptTmp.finalReceiptPreCancel + todayCanceledReceipt.finalReceiptPreCancel,
                        finalReceiptTotalCancel = canceledReceiptTmp.finalReceiptTotalCancel + todayCanceledReceipt.finalReceiptTotalCancel,

                    };

                    monthOverviewList.revenueFinalList.Add(todayDiscountRevenue.finalAmountCash + todayDiscountRevenue.finalAmountCard);
                    monthOverviewList.receiptQtyTotalList.Add(todayReceiptQuantity.totalReceipt);
                }
                else
                {
                    var todayOrders = orderApi.GetOrderForDashboard(today.GetStartOfDate(), today.GetEndOfDate(), brandId, storeId);
                    discountRevenue = GetDiscountRevenue(todayOrders);
                    receiptQuantity = GetReceiptQty(todayOrders);
                    productQuantity = GetProductQty(todayOrders);
                    finalRevenue = GetFinalRevenue(todayOrders);
                    canceledReceipt = GetCanceledReceipt(todayOrders);
                    //if (storeId == 0)
                    //{
                    //    storeDateList = GetDateStore(todayOrders, brandId);
                    //}
                }
            }

            receiptAverage = GetRecepitAvg(receiptQuantity, finalRevenue);

            productAverage = GetProductAvg(productQuantity, receiptQuantity);
            return Json(new
            {
                success = true,
                finalRevenue = finalRevenue,
                discountRevenue = discountRevenue,
                receipt = new DashBoardReceiptViewModel
                {
                    receiptQty = receiptQuantity,
                    receiptAvg = receiptAverage
                },
                canceledReceipt = canceledReceipt,
                product = new DashBoardProductViewModel
                {
                    productQty = productQuantity,
                    productAvg = productAverage
                },
                monthOverviewList = monthOverviewList,
                dateList = dateList.Select(q => q.ToString("dd/MM"))
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PaymentData(int storeId, int brandId, string _startDate, string _endDate)
        {
            var startDate = Utils.ToDateTime(_startDate).GetStartOfDate();
            var endDate = Utils.ToDateTime(_endDate).GetEndOfDate();
            var orderApi = new OrderApi();
            var paymentApi = new PaymentApi();
            var payments = paymentApi.GetStorePaymentByDateRange(storeId, startDate, endDate, brandId);
            //var resultData = new DashboardCardPaymentDateViewModel();
            //var cashType = (int) PaymentTypeEnum.Cash;
            //var memberType = (int) PaymentTypeEnum.MemberPayment;
            //var masterType = (int)PaymentTypeEnum.MasterCard;
            //resultData.totalCashPayment = payments.Where(q =>
            //    (q.Type == cashType || q.Type == (int)PaymentTypeEnum.ExchangeCash) && q.Order.OrderType != (int)OrderTypeEnum.OrderCard ).Sum(q=>q.Amount);

            //resultData.totalCashPaymentOrderCard = payments.Where(q =>
            //    (q.Type == cashType || q.Type ==(int)PaymentTypeEnum.ExchangeCash) && q.Order.OrderType == (int)OrderTypeEnum.OrderCard).Sum(q => q.Amount);

            //resultData.totalMemberPayment = payments.Where(q =>
            //    q.Type == memberType).Sum(q => q.Amount);
            
            //resultData.totalMasterCardPayment = payments.Where(q =>
            //    q.Type == masterType).Sum(q => q.Amount);

            //resultData.totalVisaCardPayment = payments.Where(q =>
            //    q.Type == (int)PaymentTypeEnum.VisaCard).Sum(q => q.Amount);

            //resultData.totalMomoPayment = payments.Where(q =>
            //    q.Type == (int)PaymentTypeEnum.ThirdParty).Sum(q => q.Amount);
            //resultData.totalMomoPayment = 0;

            //resultData.totalOtherPayment = payments.Where(q =>
            //    q.Type == (int)PaymentTypeEnum.Other).Sum(q => q.Amount);

            //resultData.totalPayment = payments.Sum(q => q.Amount);

            DashboardCardPaymentDateViewModel cardPaymentQuantity;
            cardPaymentQuantity = GetPaymentNumber(payments);
            return Json(new
            {
                success = true,
                cardPaymentQuantity = cardPaymentQuantity
            });
        }
        //public JsonResult DateData(int storeId, int brandId, string _startDate, string _endDate)
        //{
        //    var orderService = this.Service<IOrderService>();
        //    var dateReportService = this.Service<IDateReportService>();
        //    var orderApi = new OrderApi();
        //    var orderDetailApi = new OrderDetailApi();
        //    var dateReportApi = new DateReportApi();
        //    DateTime startDate, endDate;
        //    if(!_startDate.IsNullOrWhiteSpace())
        //    {
        //        startDate = _startDate.ToDateTime();
        //        startDate = startDate.GetStartOfDate();
        //    }
        //    else
        //    {
        //        startDate = DateTime.Now.GetStartOfDate();
        //    }

        //    if (!_endDate.IsNullOrWhiteSpace())
        //    {
        //        endDate = _endDate.ToDateTime();
        //        endDate = endDate.GetEndOfDate();
        //    }
        //    else
        //    {
        //        endDate = DateTime.Now.GetEndOfDate();
        //    }

        //    var fromDate = startDate.GetStartOfDate();
        //    var toDate = endDate.GetEndOfDate();

        //    //Get date list
        //    var _dateList = new List<string>();
        //    //if (_startDate != null && _endDate != null && !_startDate.Equals(_endDate))
        //    //{
        //    //    var s = (endDate - startDate).ToString();
        //    //    var dateRange = Int32.Parse(s.Substring(0, s.IndexOf(".")));
        //    //    for (int i = 0; i <= dateRange; i++)
        //    //    {
        //    //        var nextDate = startDate.AddDays(i).ToString("dd/MM/yyyy");
        //    //        //.Substring(0, 10)
        //    //        _dateList.Add(nextDate);
        //    //    }
        //    //}

        //    //Get genaral information
        //    //if (fromDate == Utils.GetCurrentDateTime().GetStartOfDate())
        //    //{
        //        IEnumerable<Order> report;
        //        IEnumerable<Order> reportCancel;
        //        IEnumerable<Order> reportPreCancel;
        //        IEnumerable<OrderDetail> reportOrderCancel;

        //        #region List All Order
        //        if (storeId > 0)
        //        {
        //            report = orderApi.GetRentsByTimeRange2(storeId, fromDate, toDate)
        //                .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
        //        }
        //        else
        //        {
        //            report = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
        //                .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
        //        }
        //        #endregion

        //        #region List Cancel order
        //        if ((int)storeId > 0)
        //        {
        //            reportCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
        //                .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel || a.OrderStatus == (int)OrderStatusEnum.PosCancel).ToList();
        //        }
        //        else
        //        {
        //            reportCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
        //                .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel || a.OrderStatus == (int)OrderStatusEnum.PosCancel).ToList();
        //        }
        //        #endregion

        //        #region List Pre Cancel order

        //        if ((int)storeId > 0)
        //        {
        //            reportPreCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
        //                .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel || a.OrderStatus == (int)OrderStatusEnum.PosPreCancel).ToList();
        //        }
        //        else
        //        {
        //            reportPreCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
        //                .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel || a.OrderStatus == (int)OrderStatusEnum.PosPreCancel).ToList();
        //        }
        //        #endregion

        //        #region List Cancel OrderDetail
        //        if ((int)storeId > 0)
        //        {
        //            reportOrderCancel = orderDetailApi.GetOrderDetailsByTimeRange(DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate(), (int)storeId)
        //                .Where(a => a.Status == (int)OrderStatusEnum.PosPreCancel || a.Status == (int)OrderStatusEnum.PosCancel).ToList();
        //        }
        //        else
        //        {                
        //            reportOrderCancel = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate())
        //                 .Where(a => a.Status == (int)OrderStatusEnum.PosPreCancel || a.Status == (int)OrderStatusEnum.PosCancel).ToList(); 
        //        }
        //        #endregion
        //        var _paymentCash = report.Sum(item => item.FinalAmount);
        //        var _paymentUserCard = 0;
        //        var _paymentCreditCard = 0;
        //        //Total amount
        //        var _totalAmount = report.Sum(item => item.TotalAmount);
        //        //Total discount
        //        var _totalDiscount = report.Sum(a => a.Discount) + report.Sum(a => a.DiscountOrderDetail);
        //        //Total amount after discount
        //        var _finalAmount = report.Sum(item => item.FinalAmount);
        //        //Total amount Cancel
        //        //var _totalCancel = reportCancel.Sum(item => item.TotalAmount);
        //        var _totalCancel = reportCancel.Count();
        //        //Total amount Pre Cancel
        //        //var _totalPreCancel = reportPreCancel.Sum(item => item.TotalAmount);
        //        var _totalPreCancel = reportPreCancel.Count();
        //        // Total amount Order Cancel
        //        //var _totalOrderCancel = reportOrderCancel.Sum(item => item.TotalAmount);
        //        var _totalOrderCancel = reportOrderCancel.Count();


        //        //Total bill
        //        var _totalBill = report.Count();
        //        //Total bill at store
        //        var _totalBillAtStore = report.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
        //        var _totalRevenueAtStore = report.Where(a => a.OrderType == (int)OrderTypeEnum.AtStore).Sum(a => a.FinalAmount);
        //        //Total bill takeaway
        //        var _totalBillTakeAway = report.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
        //        var _totalRevenueTakeAway = report.Where(a => a.OrderType == (int)OrderTypeEnum.TakeAway).Sum(a => a.FinalAmount);
        //        //Total bill delivery
        //        var _totalBillDelivery = report.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
        //        var _totalRevenueDelivery = report.Where(a => a.OrderType == (int)OrderTypeEnum.Delivery).Sum(a => a.FinalAmount);

        //        return Json(new
        //        {
        //            success = true,
        //            msg = "Báo cáo chạy thành công",
        //            dataAmount = new
        //            {
        //                totalAmount = _totalAmount,
        //                totalDiscount = _totalDiscount,
        //                finalAmount = _finalAmount,
        //                totalCancel = _totalCancel,
        //                totalPreCancel = _totalPreCancel,
        //                totalOrderCancel = _totalOrderCancel,
        //            },
        //            dataBill = new
        //            {
        //                totalBill = _totalBill,
        //                totalBillAtStore = _totalBillAtStore,
        //                totalBillTakeAway = _totalBillTakeAway,
        //                totalBillDelivery = _totalBillDelivery,
        //                totalRevenueAtStore = _totalRevenueAtStore,
        //                totalRevenueTakeAway = _totalRevenueTakeAway,
        //                totalRevenueDelivery = _totalRevenueDelivery

        //            },
        //            dataPayment = new
        //            {
        //                payment = _finalAmount,
        //                paymentCash = _paymentCash,
        //                paymentUserCard = _paymentUserCard,
        //                paymentCreditCard = _paymentCreditCard,
        //                dateList = _dateList,
        //            },
        //            dateList = _dateList,
        //        }, JsonRequestBehavior.AllowGet);
        //}
        //else
        //{
        //    IEnumerable<DateReport> report;
        //    IEnumerable<Order> reportCancel;
        //    IEnumerable<Order> reportPreCancel;
        //    IEnumerable<OrderDetail> reportOrderCancel;
        //    if (storeId > 0)
        //    {
        //        report = dateReportApi.GetDateReportTimeRangeAndStore(fromDate, toDate, storeId)
        //            .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();
        //    }
        //    else
        //    {
        //        report = dateReportApi.GetDateReportTimeRangeAndBrand(fromDate, toDate, brandId)
        //            .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();
        //    }
        //    #region List Cancel order
        //    if ((int)storeId > 0)
        //    {
        //        reportCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
        //            .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel).ToList();
        //    }
        //    else
        //    {
        //        reportCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
        //            .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel).ToList();
        //    }
        //    #endregion

        //    #region List Pre Cancel order

        //    if ((int)storeId > 0)
        //    {
        //        reportPreCancel = orderApi.GetRentsByTimeRange2((int)storeId, fromDate, toDate)
        //            .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel).ToList();
        //    }
        //    else
        //    {
        //        reportPreCancel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
        //            .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel).ToList();
        //    }
        //    #endregion

        //    #region List Cancel OrderDetail
        //    if ((int)storeId > 0)
        //    {
        //        reportOrderCancel = orderDetailApi.GetAllCanceledOrderDetailByStore((int)storeId, fromDate, toDate);
        //    }
        //    else
        //    {
        //        reportOrderCancel = orderDetailApi.GetAllCanceledOrderDetailByBrand(fromDate, toDate, brandId);
        //    }
        //    #endregion

        //    var _paymentCash = report.Sum(item => item.TotalCash);
        //    var _paymentUserCard = 0;
        //    var _paymentCreditCard = 0;
        //    //Total amount
        //    var _totalAmount = report.Sum(item => item.TotalAmount);
        //    //Total discount
        //    var _totalDiscount = report.Sum(a => a.Discount) + report.Sum(a => a.DiscountOrderDetail);
        //    //Total amount after discount
        //    var _finalAmount = report.Sum(item => item.FinalAmount);
        //    //Total amount Cancel
        //    //var _totalCancel = reportCancel.Sum(item => item.TotalAmount);
        //    var _totalCancel = reportCancel.Count();
        //    //Total amount Pre Cancel
        //    //var _totalPreCancel = reportPreCancel.Sum(item => item.TotalAmount);
        //    var _totalPreCancel = reportPreCancel.Count();
        //    // Total amount Order Cancel
        //    //var _totalOrderCancel = reportOrderCancel.Sum(item => item.TotalAmount);
        //    var _totalOrderCancel = reportOrderCancel.Count();

        //    //Total bill
        //    //var _totalBill = report.Sum(a => a.TotalOrder);
        //    var _totalBill = report.Count();
        //    //Total bill at store
        //    //var _totalBillAtStore = report.Sum(a => a.TotalOrderAtStore);
        //    var _totalBillAtStore = report.Sum(a => a.TotalOrderAtStore);

        //    //Total bill takeaway
        //    var _totalBillTakeAway = report.Sum(a => a.TotalOrderTakeAway);
        //    //Total bill delivery
        //    var _totalBillDelivery = report.Sum(a => a.TotalOrderDelivery);


        //    return Json(new
        //    {
        //        success = true,
        //        msg = "Báo cáo chạy thành công",
        //        dataAmount = new
        //        {
        //            totalAmount = _totalAmount,
        //            totalDiscount = _totalDiscount,
        //            finalAmount = _finalAmount,
        //            totalCancel = _totalCancel,
        //            totalPreCancel = _totalPreCancel,
        //            totalOrderCancel = _totalOrderCancel,
        //        },
        //        dataBill = new
        //        {
        //            totalBill = _totalBill,
        //            totalBillAtStore = _totalBillAtStore,
        //            totalBillTakeAway = _totalBillTakeAway,
        //            totalBillDelivery = _totalBillDelivery
        //        },
        //        dataPayment = new
        //        {
        //            payment = _finalAmount,
        //            paymentCash = _paymentCash,
        //            paymentUserCard = _paymentUserCard,
        //            paymentCreditCard = _paymentCreditCard,
        //            dateList = _dateList,
        //        },
        //        dateList = _dateList,
        //    }, JsonRequestBehavior.AllowGet);
        //}
        //}//End DateData

        public async Task<JsonResult> StoreData(string _startDate, string _endDate, int brandId)
        {
            var startDate = _startDate.ToDateTime().GetStartOfDate();
            var endDate = _endDate.ToDateTime().GetEndOfDate();
            var today = Utils.GetCurrentDateTime().GetEndOfDate();
            var dateReportApi = new DateReportApi();
            var storeApi = new StoreApi();

            //var dateReports = dateReportApi
            //    .GetBrandDateReportForDashBoard(startDate, endDate, brandId)
            //    .GroupBy(q => q.StoreID)
            //    .Select(q => new DashboardStoreViewModel
            //    {
            //        storeId = q.Key,
            //        storeName = q.Select(a => a.StoreName).FirstOrDefault(),
            //        storeShortName = q.Select(a => a.StoreAbbr).FirstOrDefault(),
            //        totalOrderQty = q.Sum(a => a.TotalOrder),
            //        finalRevenue = q.Sum(a => a.FinalAmount)
            //    }).ToList();

            //if (dateReports.Count() == 0)
            //{
            var dateReports = await storeApi.GetStoreEntitiesByBrand(brandId).Select(q => new DashboardStoreViewModel
            {
                storeId = q.ID,
                storeName = q.Name,
                storeShortName = q.ShortName,
                totalOrderQty = 0,
                totalOrderDetails = 0,
                finalRevenue = new DashboardFinalRevenueViewModel
                {
                    finalAmount = 0,
                    finalAtStore = 0,
                    finalDelivery = 0,
                    finalTakeAway = 0
                }
            }).ToListAsync();
            //}

            //if (!string.IsNullOrWhiteSpace(param.sSearch))
            //{
            //    dateReports = dateReports.Where(q => q.storeName != null && q.storeName.Contains(param.sSearch));
            //}

            var queryResult = dateReports;
            //.Skip(param.iDisplayStart)
            //.Take(param.iDisplayLength)                   

            //if (endDate == today)
            //{
            var orderApi = new OrderApi();
            var storesOrdersList = orderApi
                .GetQueryOrderForDashBoard(startDate, endDate, brandId, 0)
                .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish)
            .GroupBy(q => q.StoreID);
            var storesOrders = await storesOrdersList
            .Select(q => new DashboardStoreViewModel
            {
                storeId = q.Key,
                totalOrderQty = q.Count(),
                totalOrderDetails = q.Sum(a => a.TotalOrderDetails),
                finalRevenue = new DashboardFinalRevenueViewModel
                {
                    finalAmount = q.Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct).Sum(a => a.FinalAmount ?? 0),
                    finalAtStore = q.Where(a => a.OrderType == (int)OrderTypeEnum.AtStore).Sum(a => a.FinalAmount) ?? 0,
                    finalDelivery = q.Where(a => a.OrderType == (int)OrderTypeEnum.Delivery).Sum(a => a.FinalAmount) ?? 0,
                    finalTakeAway = q.Where(a => a.OrderType == (int)OrderTypeEnum.TakeAway).Sum(a => a.FinalAmount) ?? 0,
                    finalOrderCard =  q.Where(a => a.OrderType == (int)OrderTypeEnum.OrderCard).Sum(a=>a.FinalAmount) ??0
                }
            }).ToListAsync();
            var storeIdList = storesOrders.Select(q => q.storeId);
            foreach (var storeId in storeIdList)
            {
                var AllStoreData = queryResult.FirstOrDefault(q => q.storeId == storeId);
                var StoreWithOrdersData = storesOrders.FirstOrDefault(q => q.storeId == storeId);
                AllStoreData.totalOrderQty = StoreWithOrdersData.totalOrderQty;
                AllStoreData.totalOrderDetails = StoreWithOrdersData.totalOrderDetails;
                AllStoreData.finalRevenue = StoreWithOrdersData.finalRevenue;
            }
            int count = 0;

            var result = queryResult.Select(q => new IConvertible[] {
                        ++count,
                        q.storeName,
                        q.totalOrderQty,
                        //q.totalOrderQty == 0 ? 0 : (double) q.totalOrderDetails / q.totalOrderQty, //San pham moi hoa don
                        q.totalOrderQty == 0 ? 0 : q.finalRevenue.finalAmount / q.totalOrderQty, //Doanh thu moi hoa don
                        q.finalRevenue.finalAmount- q.finalRevenue.finalOrderCard, // Doanh thu bán hàng
                        q.finalRevenue.finalOrderCard,
                        q.finalRevenue.finalAmount,
                        q.finalRevenue.finalAmount == 0 ? "N/A"
                                : Math.Max(q.finalRevenue.finalAtStore, Math.Max(q.finalRevenue.finalDelivery, q.finalRevenue.finalTakeAway))
                                == q.finalRevenue.finalAtStore ? "Tại cửa hàng"
                                : q.finalRevenue.finalDelivery > q.finalRevenue.finalTakeAway ? "Giao hàng" : "Mang về",
                        q.storeId
                    });

            Dictionary<string, dynamic> chartDict = queryResult.Select(q => new KeyValuePair<string, dynamic>
                (
                    q.storeId.ToString(),
                    new
                    {
                        storeName = q.storeShortName,
                        finalRevenue = q.finalRevenue.finalAmount,
                        revenueAtStore = q.finalRevenue.finalAtStore,
                        revenueDelivery = q.finalRevenue.finalDelivery,
                        revenueTakeAway = q.finalRevenue.finalTakeAway
                    }
                )
            ).ToDictionary(a => a.Key, a => a.Value);

            return Json(new
            {
                success = true,
                data = result,
                chart = chartDict
            }, JsonRequestBehavior.AllowGet);


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
            }
            else
            {
                startDate = DateTime.Now.GetStartOfDate();
            }
            if (!_endDate.Equals(""))
            {
                endDate = _endDate.ToDateTime();
            }
            else
            {
                endDate = DateTime.Now.GetEndOfDate();
            }
            var time = startDate.GetStartOfDate();
            if (time == Utils.GetCurrentDateTime().GetStartOfDate())
            {

                IQueryable<OrderDetail> filteredListItems;

                var total = 0;
                // Search.
                var totalQuery = 0;
                if (storeId > 0)
                {
                    filteredListItems = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
                        .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                else
                {
                    filteredListItems = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
                        .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
                }
                var totalFinal5List = filteredListItems.GroupBy(x => x.Product.ProductName)
                       .Select(g => new
                       {
                           ProductName = g.Key,
                           Quantity = g.Sum(z => z.Quantity),
                           FinalAmount = g.Sum(z => z.FinalAmount)
                       })
                       .OrderByDescending(g => g.FinalAmount).Take(5).ToList();
                total = totalFinal5List.Count();
                totalQuery = totalFinal5List.Count();
                List<string> names = new List<string>();
                List<int> quantities = new List<int>();
                List<double> amounts = new List<double>();
                foreach (var item in totalFinal5List)
                {
                    names.Add(item.ProductName);
                    quantities.Add(item.Quantity);
                    amounts.Add(item.FinalAmount);
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
            }
            else
            {
                IQueryable<DateProduct> filteredListItems;

                startDate = _startDate.ToDateTime();
                startDate = startDate.GetStartOfDate();


                endDate = _endDate.ToDateTime();
                endDate = endDate.GetEndOfDate();
                var total = 0;
                var totalQuery = 0;

                if (storeId > 0)
                {
                    filteredListItems = dateProductService.GetDateProductByTimeRange(startDate, endDate, storeId);
                    //.OrderBy(a => a.ProductName_);
                }
                else
                {
                    filteredListItems = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId);
                    //.OrderBy(a => a.ProductName_);
                }


                var totalFinal5List = filteredListItems.GroupBy(x => x.Product.ProductName)
                       .Select(g => new
                       {
                           ProductName = g.Key,
                           Quantity = g.Sum(z => z.Quantity),
                           FinalAmount = g.Sum(z => z.FinalAmount)
                       })
                       .OrderByDescending(g => g.FinalAmount).Take(5).ToList();
                total = totalFinal5List.Count();
                totalQuery = totalFinal5List.Count();
                List<string> names = new List<string>();
                List<int> quantities = new List<int>();
                List<double> amounts = new List<double>();
                foreach (var item in totalFinal5List)
                {
                    names.Add(item.ProductName);
                    quantities.Add(item.Quantity);
                    amounts.Add(item.FinalAmount);
                }
                return Json(new
                {
                    dataChart = new
                    {
                        nameArray = names,
                        quantityArray = quantities,
                        amountArray = amounts,
                    }

                }, JsonRequestBehavior.AllowGet);
            }
        }

        private JsonResult TotalAmountChartData(int storeId, int brandId, List<string> _dateList)
        {
            var dateReportService = this.Service<IDateReportService>();
            var dateReportApi = new DateReportApi();
            var totalAmountList = new List<double>();
            var finalAmountList = new List<double>();


            if (storeId > 0)
            {
                for (int i = 0; i < _dateList.Count; i++)
                {
                    var nextDate = _dateList[i].ToDateTime();

                    var startDate = nextDate.GetStartOfDate();
                    var endDate = nextDate.GetEndOfDate();


                    var report = dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, storeId)
                        .Where(a => a.Status == (int)DateReportStatusEnum.Approved).ToList();

                    totalAmountList.Add((double)report.Sum(a => a.TotalAmount));
                    finalAmountList.Add((double)report.Sum(a => a.FinalAmount));

                }
            }
            else
            {
                for (int i = 0; i < _dateList.Count; i++)
                {
                    var nextDate = _dateList[i].ToDateTime();

                    var startDate = nextDate.GetStartOfDate();
                    var endDate = nextDate.GetEndOfDate();

                    //if (startDate == today)
                    //{
                    //    var report = _rentService.GetAllRentByDate(startDate, endDate)
                    //        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                    //    totalAmountList.Add(report.Sum(a => a.TotalAmount));
                    //    finalAmountList.Add(report.Sum(a => a.FinalAmount));
                    //}
                    //else
                    //{
                    var report = dateReportApi.GetDateReportTimeRangeAndBrand(startDate, endDate, brandId)
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
            //var today = Utils.GetCurrentDateTime().GetStartOfDate();
            List<string> _newDateList = new List<string>();

            if (storeId > 0)
            {
                for (int i = 0; i < _dateList.Count; i++)
                {
                    var nextDate = _dateList[i].ToDateTime();

                    var startDate = nextDate.GetStartOfDate();
                    var endDate = nextDate.GetEndOfDate();
                    var report = dateReportApi.GetDateReportTimeRangeAndStore(startDate, endDate, storeId)
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

                    var startDate = nextDate.GetStartOfDate();
                    var endDate = nextDate.GetEndOfDate();

                    var report = dateReportApi.GetDateReportTimeRangeAndBrand(startDate, endDate, brandId)
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

        public JsonResult ProductData(JQueryDataTableParamModel param, string _startDate, string _endDate, int storeId, int brandId)
        {
            var orderDetailApi = new OrderDetailApi();
            var dateProductApi = new DateProductApi();

            var startDate = _startDate.ToDateTime().GetStartOfDate();
            var endDate = _endDate.ToDateTime().GetEndOfDate();
            var today = Utils.GetCurrentDateTime().GetEndOfDate();
            var time = startDate.GetStartOfDate();
            var totalProductsRecords = dateProductApi.GetQueryDateProductForDashBoard(startDate, endDate, brandId, storeId);// store = 0 lay theo brand

            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                totalProductsRecords = totalProductsRecords.Where(q => q.ProductName.Contains(param.sSearch));
            }

            var dateProducts = totalProductsRecords.GroupBy(q => q.ProductID);


            var totalQuery = dateProducts.Count();

            var queryProducts = dateProducts.OrderByDescending(q => q.Sum(a => a.TotalQuantity))
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength)
                                .ToList();

            var queryResult = queryProducts
                .Select(q => new DashboardProductDataViewModel
                {
                    productId = q.Key,
                    productName = q.Select(a => a.ProductName).FirstOrDefault(),
                    totalQty = q.Sum(a => a.TotalQuantity),
                    totalAmount = q.Sum(a => a.TotalAmount),
                    finalAmount = q.Sum(a => a.FinalAmount)
                });

            if (endDate == today)
            {
                if (startDate.GetEndOfDate() != endDate)
                {
                    //var totalTodayOrders = orderDetailApi.GetQueryDateOrderDetails(today, brandId, storeId);
                    //// store = 0 lay theo brand
                    //if (!string.IsNullOrWhiteSpace(param.sSearch))
                    //{
                    //    totalTodayOrders = totalTodayOrders.Where(q => q.ProductName.Contains(param.sSearch));
                    //}

                    //var todayOrders = totalTodayOrders.GroupBy(q => q.ProductID);

                    //totalQuery = todayOrders.Count();

                    //var queryOrdersProducts = todayOrders
                    //                    .OrderBy(q => q.Key)
                    //                    .Skip(param.iDisplayStart)
                    //                    .Take(param.iDisplayLength)
                    //                    .ToList()
                    //                    .Select(q => new DashboardProductDataViewModel
                    //                    {
                    //                        productId = q.Key,
                    //                        productName = q.Select(a => a.ProductName).FirstOrDefault(),
                    //                        totalQty = q.Sum(a => a.TotalOrderDetails),
                    //                        totalAmount = q.Sum(a => a.TotalAmount),
                    //                        finalAmount = q.Sum(a => a.FinalAmount)
                    //                    });


                    //var totalProductID = queryOrdersProducts.Select(q => q.productId);
                    //foreach (var productId in totalProductID)
                    //{
                    //    var productData = queryResult.Where(q => q.productId == productId).FirstOrDefault();
                    //    var todayProductData = queryOrdersProducts.Where(q => q.productId == productId).FirstOrDefault();
                    //    productData.totalQty += todayProductData.totalQty;
                    //    productData.totalAmount += todayProductData.totalAmount;
                    //    productData.finalAmount += todayProductData.finalAmount;
                    //}
                    var queryResultIDs = queryResult.Select(q => q.productId);
                    foreach (var productId in queryResultIDs)
                    {
                        var todayProductResult = queryResult.Where(q => q.productId == productId).FirstOrDefault();
                        IEnumerable<TodayOrderDetail> todayOrdersByProduct;
                        if (storeId == 0) //lay theo brand
                        {
                            todayOrdersByProduct = orderDetailApi.GetTodayOrderDetailByProduct(brandId, productId);
                        }
                        else
                        {
                            todayOrdersByProduct = orderDetailApi.GetStoreTodayOrderDetailByProduct(storeId, productId);
                        }
                        todayProductResult.totalQty += todayOrdersByProduct.Sum(q => q.TotalOrderDetails);
                        todayProductResult.totalAmount += todayOrdersByProduct.Sum(q => q.TotalAmount);
                        todayProductResult.finalAmount += todayOrdersByProduct.Sum(q => q.FinalAmount);
                    }
                    var totalTodayProductIDs = orderDetailApi.GetQueryDateOrderDetails(today, brandId, storeId)
                                                            .Select(q => q.ProductID)
                                                            .Distinct()
                                                            .ToList();
                    var dateProductIDs = dateProducts.Select(q => q.Key);

                    var exceptProductIds = totalTodayProductIDs.Except(dateProductIDs);
                    if (exceptProductIds.Count() > 0)
                    {
                        foreach (var productId in exceptProductIds)
                        {
                            var todayProductResult = queryResult.Where(q => q.productId == productId).FirstOrDefault();
                            IEnumerable<TodayOrderDetail> todayOrdersByProduct;
                            if (storeId == 0) //lay theo brand
                            {
                                todayOrdersByProduct = orderDetailApi.GetTodayOrderDetailByProduct(brandId, productId);
                            }
                            else
                            {
                                todayOrdersByProduct = orderDetailApi.GetStoreTodayOrderDetailByProduct(storeId, productId);
                            }
                            var onlyTodayProducts = todayOrdersByProduct.Select(q => new DateProductForDashBoard
                            {
                                ProductID = q.ProductID,
                                ProductName = q.ProductName,
                                TotalQuantity = q.TotalOrderDetails,
                                TotalAmount = q.TotalAmount,
                                FinalAmount = q.FinalAmount
                            });
                            totalProductsRecords = totalProductsRecords.Concat(onlyTodayProducts);
                        }

                        dateProducts = totalProductsRecords.GroupBy(q => q.ProductID);

                        try
                        {
                            totalQuery = dateProducts.Count();
                        }
                        catch (Exception)
                        {
                            return Json(new
                            {
                                sEcho = param.sEcho,
                                iTotalRecords = 0,
                                iTotalDisplayRecords = 0,
                                aaData = new List<DateProductForDashBoard>()
                            }, JsonRequestBehavior.AllowGet);
                        }

                        queryProducts = dateProducts.OrderByDescending(q => q.Sum(a => a.TotalQuantity))
                                            .Skip(param.iDisplayStart)
                                            .Take(param.iDisplayLength)
                                            .ToList();

                        queryResult = queryProducts
                            .Select(q => new DashboardProductDataViewModel
                            {
                                productId = q.Key,
                                productName = q.Select(a => a.ProductName).FirstOrDefault(),
                                totalQty = q.Sum(a => a.TotalQuantity),
                                totalAmount = q.Sum(a => a.TotalAmount),
                                finalAmount = q.Sum(a => a.FinalAmount)
                            });
                    }

                }
                else
                {
                    var todayOrders = orderDetailApi.GetQueryDateOrderDetails(today, brandId, storeId)// store = 0 lay theo brand
                                       .GroupBy(q => q.ProductID);

                    totalQuery = todayOrders.Count();

                    var todayProducts = todayOrders.OrderByDescending(q => q.Sum(a => a.TotalOrderDetails))
                                        .Skip(param.iDisplayStart)
                                        .Take(param.iDisplayLength)
                                        .ToList();

                    queryResult = todayProducts
                        .Select(q => new DashboardProductDataViewModel
                        {
                            productId = q.Key,
                            productName = q.Select(a => a.ProductName).FirstOrDefault(),
                            totalQty = q.Sum(a => a.TotalOrderDetails),
                            totalAmount = q.Sum(a => a.TotalAmount),
                            finalAmount = q.Sum(a => a.FinalAmount)
                        });
                    ////dateProducts = todayOrders;
                    //queryResult = todayOrders
                    //    .OrderByDescending(q => q.totalQty)
                    //    .Skip(param.iDisplayStart)
                    //    .Take(param.iDisplayLength)
                    //    .ToList();
                }
            }

            int count = param.iDisplayStart;
            var result = queryResult.Select(a => new IConvertible[]
                    {
                        ++count,
                        a.productName,
                        a.totalQty,
                        a.totalAmount,
                        a.finalAmount
                    });

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = dateProducts.Count(),
                iTotalDisplayRecords = dateProducts.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
            //if (time == Utils.GetCurrentDateTime().GetStartOfDate())
            //{
            //    IEnumerable<OrderDetail> filteredListItems;
            //    var dataNow = Utils.GetCurrentDateTime();
            //    startDate = dataNow.GetStartOfDate();
            //    endDate = dataNow.GetEndOfDate();
            //    var total = 0;
            //    // Search.
            //    var totalQuery = 0;
            //    if (!string.IsNullOrEmpty(param.sSearch))
            //    {
            //        if (storeId > 0)
            //        {
            //            filteredListItems = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
            //                .Where(a => (a.Product.ProductName != null && a.Product.ProductName.ToLower().Contains(param.sSearch.ToLower()))
            //                && a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
            //                .OrderBy(a => a.Product.ProductName);
            //        }
            //        else
            //        {
            //            filteredListItems = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
            //                .Where(a => (a.Product.ProductName != null && a.Product.ProductName.ToLower().Contains(param.sSearch.ToLower()))
            //                && a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
            //                .OrderBy(a => a.Product.ProductName);
            //        }
            //    }
            //    else
            //    {
            //        if (storeId > 0)
            //        {
            //            filteredListItems = orderDetailApi.GetOrderDetailsByTimeRange(startDate, endDate, storeId)
            //                .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
            //                .OrderBy(a => a.Product.ProductName);
            //        }
            //        else
            //        {
            //            filteredListItems = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, endDate)
            //                .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
            //                .OrderBy(a => a.Product.ProductName);
            //        }
            //    }

            //    var totalFinalList = filteredListItems.GroupBy(x => x.Product.ProductName)
            //            .Select(g => new
            //            {
            //                ProductName = g.Key,
            //                Quantity = g.Sum(z => z.Quantity),
            //                TotalAmount = g.Sum(z => z.TotalAmount),
            //                FinalAmount = g.Sum(z => z.FinalAmount)
            //            })
            //            .OrderByDescending(g => g.Quantity);

            //    var finalList = totalFinalList.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            //    total = totalFinalList.Count();
            //    totalQuery = totalFinalList.Count();

            //    int count = param.iDisplayStart;
            //    var listProduct = finalList.Select(a => new IConvertible[]
            //        {
            //            ++count,
            //            a.ProductName,
            //            a.Quantity,
            //            a.TotalAmount,
            //            a.FinalAmount
            //        });

            //    return Json(new
            //    {
            //        sEcho = param.sEcho,
            //        iTotalRecords = total,
            //        iTotalDisplayRecords = totalQuery,
            //        aaData = listProduct
            //    }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    IEnumerable<DateProduct> filteredListItems;

            //    startDate = _startDate.ToDateTime();
            //    startDate = startDate.GetStartOfDate();

            //    //var endDate = _endDate.ToDateTime().GetEndOfDate();
            //    endDate = _endDate.ToDateTime();
            //    endDate = endDate.GetEndOfDate();
            //    var total = 0;
            //    var totalQuery = 0;

            //    if (!string.IsNullOrEmpty(param.sSearch))
            //    {
            //        if (storeId > 0)
            //        {
            //            filteredListItems = dateProductApi.GetDateProductByTimeRange(startDate, endDate, storeId)
            //                .Where(
            //                d => (d.ProductName_ != null && d.ProductName_.ToLower().Contains(param.sSearch.ToLower()))
            //            ).OrderBy(a => a.ProductName_);
            //        }
            //        else
            //        {
            //            filteredListItems = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId)
            //                .Where(
            //                d => (d.ProductName_ != null && d.ProductName_.ToLower().Contains(param.sSearch.ToLower()))
            //            ).OrderBy(a => a.ProductName_);
            //        }
            //    }
            //    else
            //    {
            //        if (storeId > 0)
            //        {
            //            filteredListItems = dateProductService.GetDateProductByTimeRange(startDate, endDate, storeId)
            //                .OrderBy(a => a.ProductName_);
            //        }
            //        else
            //        {
            //            filteredListItems = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId)
            //                .OrderBy(a => a.ProductName_);
            //        }
            //    }

            //    var totalFinalList = filteredListItems.GroupBy(x => x.ProductName_)
            //            .Select(g => new
            //            {
            //                ProductName = g.Key,
            //                Quantity = g.Sum(z => z.Quantity),
            //                TotalAmount = g.Sum(z => z.TotalAmount),
            //                FinalAmount = g.Sum(z => z.FinalAmount)
            //            })
            //            .OrderByDescending(g => g.Quantity);

            //    var finalList = totalFinalList.Skip(param.iDisplayStart).Take(param.iDisplayLength);

            //    total = totalFinalList.Count();
            //    totalQuery = totalFinalList.Count();

            //    int count = param.iDisplayStart;
            //    var listProduct = finalList.Select(a => new IConvertible[]
            //    {
            //        ++count,
            //        a.ProductName,
            //        a.Quantity,
            //        //a.TotalAmount.ToString("N0")
            //        string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.TotalAmount),
            //        string.Format(CultureInfo.InvariantCulture,
            //            "{0:0,0}", a.FinalAmount),
            //    });

            //    return Json(new
            //    {
            //        sEcho = param.sEcho,
            //        iTotalRecords = total,
            //        iTotalDisplayRecords = totalQuery,
            //        aaData = listProduct
            //    }, JsonRequestBehavior.AllowGet);


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
            var time = _startDate.ToDateTime();
            var sTime = _startDate.ToDateTime().ToString("dd-MM-yyyy");
            var eTime = _endDate.ToDateTime().ToString("dd-MM-yyyy");
            var dateRange = "(" + sTime + (sTime == eTime ? "" : " - " + eTime) + ")";
            var storeName = "";
            if (storeId > 0)
            {
                storeName = storeService.Get(storeId).Name;
            }
            else
            {
                storeName = "Service";
            }

            if (time == Utils.GetCurrentDateTime())
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
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.FinalAmount;
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
                    var fileDownloadName = "TổngQuanSảnPhẩm " + _startDate.Replace("/", "-") + " đến " + _endDate.Replace("/", "-") + "_" + storeName + ".xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
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
                    var fileDownloadName = "TổngQuanSảnPhẩm " + _startDate.Replace("/", "-") + " đến " + _endDate.Replace("/", "-") + "_" + storeName + ".xlsx";
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
            var orderApi = new OrderApi();
            var _aspNetUserService = this.Service<IAspNetUserService>();

            var fromDate = _startDate.ToDateTime();
            fromDate = fromDate.GetStartOfDate();

            //var endDate = _endDate.ToDateTime().GetEndOfDate();
            var toDate = _endDate.ToDateTime();
            toDate = toDate.GetEndOfDate();

            var totalModel = orderApi.GetQueryOrderForDashBoard(fromDate, toDate, brandId, storeId) // nếu storeId = 0 thì lấy theo brand
                .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && (a.OrderStatus == (int)OrderStatusEnum.Finish || a.OrderStatus == (int)OrderStatusEnum.Cancel || a.OrderStatus == (int)OrderStatusEnum.PreCancel))
                .GroupBy(a => a.CheckInPerson)
                .OrderBy(q => q.Key);

            var modelCount = totalModel.Count();

            var cashiersData = totalModel
            .Skip(param.iDisplayStart)
            .Take(param.iDisplayLength).ToList()
                .Select(a => new DashboardCashierViewModel
                {
                    userName = a.Key,
                    totalOrderAM = a.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish).Where(q => q.CheckInHour < 12).Count(),
                    finalAmountAM = a.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish).Where(q => q.CheckInHour < 12).Sum(q => q.FinalAmount),
                    totalOrderPM = a.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish).Where(q => q.CheckInHour >= 12).Count(),
                    finalAmountPM = a.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish).Where(q => q.CheckInHour >= 12).Sum(q => q.FinalAmount),
                    totalCancelOrder = a.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel).Count(),
                    totalPreCancelOrder = a.Where(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel).Count()
                });


            //int i = param.iDisplayStart;
            var list = cashiersData

            .Select(a => new IConvertible[]
            {
                //++i,
                //a.Key == null ? "N/A" : _aspNetUserService.GetUserByUsernameSync(a.Key).FullName,
                //a.Key == null ? "N/A" : a.Key,
                //a.Count(),
                //a.Sum(b => b.FinalAmount)
                a.userName == null ? "N/A" : (_aspNetUserService.GetUserByUsernameSync(a.userName).FullName ?? a.userName),
                //a.userName,
                //a.totalOrderAM,
                //a.finalAmountAM ?? 0,
                //a.totalOrderPM,
                //a.finalAmountPM ?? 0,
                a.totalOrderAM + a.totalOrderPM,
                a.totalCancelOrder + a.totalPreCancelOrder,
                a.totalOrderAM + a.totalOrderPM - (a.totalCancelOrder + a.totalPreCancelOrder),
                a.finalAmountAM + a.finalAmountPM
            });

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecord = modelCount,
                iTotalDisplayRecords = modelCount,
                aaData = list
            }, JsonRequestBehavior.AllowGet);

            //else
            //{
            //    var totalModel = orderApi.GetAllOrderByDate2(fromDate, toDate, brandId)
            //        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
            //        .GroupBy(a => a.CheckInPerson);

            //    var modelCount = totalModel.Count();

            //    int i = param.iDisplayStart;
            //    var list = totalModel.ToList().Select(a => new IConvertible[]
            //    {
            //        ++i,
            //        a.Key == null ? "N/A" : _aspNetUserService.GetUserByUsernameSync(a.Key).FullName,
            //        a.Key == null ? "N/A" : a.Key,
            //        a.Count(),
            //        a.Sum(b => b.FinalAmount)
            //    }).Skip(param.iDisplayStart).Take(param.iDisplayLength);

            //    return Json(new
            //    {
            //        sEcho = param.sEcho,
            //        iTotalRecord = modelCount,
            //        iTotalDisplayRecords = modelCount,
            //        aaData = list
            //    }, JsonRequestBehavior.AllowGet);
            //}
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
            var dateRange = "(" + sTime + (sTime == eTime ? "" : " - " + eTime) + ")";
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
                    var fileDownloadName = "TổngQuanNhânViên_" + storeName + "_TổngQuanNgày" + dateRange + ".xlsx";
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
                    var fileDownloadName = "TổngQuanNhânViên_" + storeName + "_TổngQuanNgày" + dateRange + ".xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion
            }
        }//End ExportCashierTableToExcel


    }
    public class DashboardProductDataViewModel
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public int totalQty { get; set; }
        public double totalAmount { get; set; }
        public double finalAmount { get; set; }
    }

    public class DashboardCashierViewModel
    {
        public string userName { get; set; }
        public int totalOrderAM { get; set; }
        public double? finalAmountAM { get; set; }
        public int totalOrderPM { get; set; }
        public double? finalAmountPM { get; set; }
        public int totalCancelOrder { get; set; }
        public int totalPreCancelOrder { get; set; }
    }

    public class DashboardStoreViewModel
    {
        public int storeId { get; set; }
        public string storeName { get; set; }
        public string storeShortName { get; set; }
        public int totalOrderQty { get; set; }
        public int totalOrderDetails { get; set; }
        public DashboardFinalRevenueViewModel finalRevenue { get; set; }
    }

    public class DashboardFinalRevenueViewModel
    {
        public double finalAmount { get; set; }
        public double finalAtStore { get; set; }
        public double finalDelivery { get; set; }
        public double finalTakeAway { get; set; }
        public double finalOrderCard { get; set; }
    }

    public class DashBoardDiscountRevenueViewModel
    {
        public double finalAmountCash { get; set; }
        public double finalAmountCard { get; set; }
        public double finalAmountAtStore { get; set; }
        public double finalAmountTakeAway { get; set; }
        public double finalAmountDelivery { get; set; }
        public double finalAmountCanceled { get; set; }
        public double totalAmount { get; set; }
        public double totalDiscount { get; set; }
        public double totalDiscountOrder { get; set; }
        public double totalDiscountOrderDetail { get; set; }
    }

    public class DashBoardCanceledReceiptViewModel
    {
        public int qtyReceiptCancel { get; set; }
        public int qtyReceiptPreCancel { get; set; }
        public int qtyReceiptTotalCancel { get; set; }
        public double finalReceiptCancel { get; set; }
        public double finalReceiptPreCancel { get; set; }
        public double finalReceiptTotalCancel { get; set; }
    }

    public class DashBoardReceiptQtyViewModel
    {
        public int totalReceipt { get; set; }
        public int qtyAtStore { get; set; }
        public int qtyDelivery { get; set; }
        public int qtyTakeAway { get; set; }
        public int qtyOrderCard { get; set; }
    }

    public class DashBoardReceiptAvgViewModel
    {
        public double avgFinalReceipt { get; set; }
        public double avgFinalAtStore { get; set; }
        public double avgFinalDelivery { get; set; }
        public double avgFinalTakeAway { get; set; }
        public double avgFinalOrderCard { get; set; }
    }

    public class DashBoardReceiptViewModel
    {
        public DashBoardReceiptQtyViewModel receiptQty { get; set; }
        public DashBoardReceiptAvgViewModel receiptAvg { get; set; }
    }

    public class DashBoardProductQtyViewModel
    {
        public int qtyTotalProduct { get; set; }
        public int qtyProductAtStore { get; set; }
        public int qtyProductDelivery { get; set; }
        public int qtyProductTakeAway { get; set; }
        public int qtyProductOrderCard { get; set; }
    }

    public class DashBoardProductAvgViewModel
    {
        public double avgTotalProductPerReceipt { get; set; }
        public double avgProductAtStorePerReceipt { get; set; }
        public double avgProductDeliveryPerReceipt { get; set; }
        public double avgProductTakeAwayPerReceipt { get; set; }
        public double avgProductOrderCardPerReceipt { get; set; }
    }

    public class DashBoardProductViewModel
    {
        public DashBoardProductQtyViewModel productQty { get; set; }
        public DashBoardProductAvgViewModel productAvg { get; set; }
    }

    public class DashBoardMonthOverViewModel
    {
        public List<double> revenueFinalList { get; set; }
        public List<int> receiptQtyTotalList { get; set; }
    }

    public class DashBoardReceiptMonthViewModel
    {
        public List<int> receiptQtyAtStoreList { get; set; }
        public List<int> receiptQtyDeliveryList { get; set; }
        public List<int> receiptQtyTakeAwayList { get; set; }
    }

    public class DashBoardRevenueMonthViewModel
    {
        public List<double> revenueAtStoreList { get; set; }
        public List<double> revenueDeliveryList { get; set; }
        public List<double> revenueTakeAwayList { get; set; }
    }

    public class DashBoardProductMonthViewModel
    {
        public List<int> productQtyAtStoreList { get; set; }
        public List<int> productQtyDeliveryList { get; set; }
        public List<int> productQtyTakeAwayList { get; set; }
    }

    public class DashboardStoreDataMonthViewModel
    {
        public string storeName { get; set; }
        public List<double> finalRevenueList { get; set; }
    }

    public class DashboardStoreMonthViewModel
    {
        public List<string> storeIdList { get; set; }
        public Dictionary<string, DashboardStoreDataMonthViewModel> storeData { get; set; }
    }

    public class DashboardStoreDataDateViewModel
    {
        public string storeName { get; set; }
        public double finalRevenue { get; set; }
    }

    public class DashboardStoreDateViewModel
    {
        public List<string> storeIdList { get; set; }
        public Dictionary<string, DashboardStoreDataDateViewModel> storeData { get; set; }
    }
    public class DashboardCardPaymentDateViewModel
    {
        public int numberTotalPayment { get; set; }
        public int numberCashPayment { get; set; }
        public int numberCashPaymentOrderCard { get; set; }
        public int numberMemberPayment { get; set; }
        public int numberMasterCardPayment { get; set; }
        public int numberVisaCardPayment { get; set; }
        public int numberMomoPayment { get; set; }
        public int numberOtherPayment { get; set; }
        
        public double totalPayment { get; set; }
        public double totalCashPayment { get; set; }
        public double totalCashPaymentOrderCard { get; set; }
        public double totalMemberPayment { get; set; }
        public double totalMasterCardPayment { get; set; }
        public double totalVisaCardPayment { get; set; }
        public double totalMomoPayment { get; set; }
        public double totalOtherPayment { get; set; }

    }
}
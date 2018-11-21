using HmsService.Models;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Areas.DashBoard.Controllers;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers
{
    [Authorize(Roles = "BrandManager")]
    public class RevenueController : DomainBasedController
    {
        // GET: SystemReport/Revenue
        public ActionResult Index(int brandId, int storeId)
        {
            ViewBag.storeId = storeId;
            ViewBag.brandId = brandId;
            return View();
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

        [HttpPost]
        public JsonResult GetData(int storeId, int brandId, string sDate, string eDate)
        {
            //start, end,today
            var startDate = Utils.ToDateTime(sDate).GetStartOfDate();
            var endDate = Utils.ToDateTime(eDate).GetEndOfDate();
            var today = Utils.GetCurrentDateTime().GetEndOfDate();

            //Create instance API
            var orderApi = new OrderApi();
            var dateReportApi = new DateReportApi();
            var costApi = new CostApi();

            //Create variable
            IQueryable<HmsService.Models.Entities.Order> dateOrders;
            IQueryable<HmsService.Models.Entities.Cost> dateCosts;
            RevenueViewModel2 revenueModel2 = new RevenueViewModel2();
            IEnumerable<OrderForDashBoard> dateOrder;
            DashBoardDiscountRevenueViewModel discountRevenue;
            IEnumerable<DateReportForDashBoard> dateReports;
            RevenueMemberCard revenueMemberCard = new RevenueMemberCard();
            dateOrder = orderApi.GetOrderForDashboard(startDate, endDate, brandId, storeId);
            //Create date list from start date to end date
            var dateList = new List<DateTime>();
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                dateList.Add(d);
            }


            if (endDate != today)
            {
                dateReports = dateReportApi.GetDateReportForDashboard(startDate, endDate, brandId, storeId);
                discountRevenue = GetDiscountRevenue(dateReports);
            }
            else
            {
                if (dateList.Count > 1)
                {

                    dateReports = dateReportApi.GetDateReportForDashboard(startDate, endDate.AddDays(-1), brandId, storeId);
                    var discountRevenueTmp = GetDiscountRevenue(dateReports);
                    var todayOrders = orderApi.GetOrderForDashboard(today.GetStartOfDate(), today.GetEndOfDate(), brandId, storeId);
                    var todayDiscountRevenue = GetDiscountRevenue(todayOrders);

                    discountRevenue = new DashBoardDiscountRevenueViewModel
                    {
                        totalAmount = discountRevenueTmp.totalAmount + todayDiscountRevenue.totalAmount,
                        finalAmountCash = discountRevenueTmp.finalAmountCash + todayDiscountRevenue.finalAmountCash,

                        finalAmountCard = discountRevenueTmp.finalAmountCard + todayDiscountRevenue.finalAmountCard,
                        finalAmountAtStore = discountRevenueTmp.finalAmountAtStore + todayDiscountRevenue.finalAmountAtStore,
                        finalAmountTakeAway = discountRevenueTmp.finalAmountTakeAway + todayDiscountRevenue.finalAmountTakeAway,
                        finalAmountDelivery = discountRevenueTmp.finalAmountDelivery + todayDiscountRevenue.finalAmountDelivery,
                        finalAmountCanceled = discountRevenueTmp.finalAmountCanceled + todayDiscountRevenue.finalAmountCanceled,

                        totalDiscount = discountRevenueTmp.totalDiscount + todayDiscountRevenue.totalDiscount
                    };
                }
                else
                {
                    var todayOrders = orderApi.GetOrderForDashboard(today.GetStartOfDate(), today.GetEndOfDate(), brandId, storeId);
                    discountRevenue = GetDiscountRevenue(todayOrders);
                }
            }


            dateOrders = orderApi.GetOrderForRevenueReport(startDate, endDate, brandId, storeId);
            dateCosts = costApi.GetCostByRangeTimeStoreBrand(startDate, endDate, storeId, brandId);
            revenueModel2 = GetPayment(dateOrders, dateCosts, storeId, brandId, startDate, endDate);
            revenueMemberCard = GetMemberBalanceDate(dateOrders, storeId, brandId, startDate, endDate);

            return Json(new
            {
                revenueModel = revenueModel2,
                discountRevenue = discountRevenue,
                revenueMemberCard = revenueMemberCard
            }, JsonRequestBehavior.AllowGet);
        }

        private RevenueMemberCard GetMemberBalanceDate(IQueryable<HmsService.Models.Entities.Order> dateOrders, int storeId, int brandId, DateTime sDate, DateTime eDate)
        {
            var accountApi = new AccountApi();
            var accounts = accountApi.GetBrandActiveAccounts(brandId, storeId).ToList();

            var paymentApi = new PaymentApi();
            var payment = paymentApi.GetEntityStorePaymentInDateRange(storeId, sDate, eDate, brandId).Join(dateOrders,
                p => p.ToRentID,
                o => o.RentID,
                (p, o) => new { p, o.OrderType}
                ).ToList();

            var balance = Convert.ToDouble(accounts.Sum(a => a.Balance.Value));
            var totalOrderCard = payment.Where(p => p.OrderType == (int)OrderTypeEnum.OrderCard).Sum(o => o.p.Amount);
            var totalUseCard = payment.Where(p => p.p.Type == (int)PaymentTypeEnum.MemberPayment).Sum(p => p.p.Amount);
            var balanceBefore = balance - totalUseCard + totalOrderCard;

            return new RevenueMemberCard
            {
                balanceBefore = balanceBefore,
                totalOrderCard = totalOrderCard,
                totalUseCard = totalUseCard,
                balanceAfter = balance
            };
        }


        RevenueViewModel GetDiscountRevenue(IQueryable<HmsService.Models.Entities.Order> dateOrders, int brandId)
        {
            //Get promotion list code
            var promotionApi = new PromotionApi();
            var promotion = promotionApi.GetPromotionByBrandId(brandId)/*.Where(q => q.PromotionType == (int)PromotionTypeEnum.Bill)*/.Select(q => q.PromotionCode).ToList();

            //get order without ordercard and pay by cash
            var dateOrdersNoOrderCard = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard
                                        && q.Payments.FirstOrDefault().Type == (int)PaymentTypeEnum.Cash).ToList();
            var cashRevenueFinalAmount = dateOrdersNoOrderCard.Sum(q => q.FinalAmount);
            var cashRevenueTotalAmount = dateOrdersNoOrderCard.Sum(q => q.TotalAmount);
            var cashRevenueTotalDiscount = dateOrdersNoOrderCard.Sum(q => q.Discount + q.DiscountOrderDetail);

            //get order pay by passio account
            var passioAccountOrder = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard &&
                                        q.Payments.FirstOrDefault().Type == (int)PaymentTypeEnum.MemberPayment).ToList();
            var passioAccountRevenueFinalAmount = passioAccountOrder.Sum(q => q.FinalAmount);
            var passioAccountRevenueTotalAmount = passioAccountOrder.Sum(q => q.TotalAmount);
            var passioAccountRevenueTotalDiscount = passioAccountOrder.Sum(q => q.Discount + q.DiscountOrderDetail);

            //Get order with promotion 'thẻ kí bill'            
            var billOrder = dateOrders.ToList().Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard &&
                                        q.Att1 != null && promotion.Contains(q.Att1.Split(':').GetValue(0)));
            var billRevenueFinalAmount = billOrder.Sum(q => q.FinalAmount);
            var billRevenueTotalAmount = billOrder.Sum(q => q.TotalAmount);
            var billRevenueTotalDiscount = billOrder.Sum(q => q.Discount + q.DiscountOrderDetail);

            //Get order not order card and payment type not cash, passio account
            var otherOrder = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard &&
                             q.Payments.FirstOrDefault().Type != (int)PaymentTypeEnum.Cash &&
                             q.Payments.FirstOrDefault().Type != (int)PaymentTypeEnum.MemberPayment).ToList()
                             .Where(a => (a.Att1 == null || !promotion.Contains(a.Att1.Split(':').GetValue(0))));
            var otherRevenueFinalAmount = otherOrder.Sum(q => q.FinalAmount);
            var otherRevenueTotalAmount = otherOrder.Sum(q => q.TotalAmount);
            var otherRevenueTotalDiscount = otherOrder.Sum(q => q.Discount);

            var totalDiscount = cashRevenueTotalDiscount - billRevenueTotalDiscount;

            var totalOrderCard = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard).ToList().Sum(q => q.FinalAmount);

            var totalRevenue = cashRevenueFinalAmount + passioAccountRevenueFinalAmount + billRevenueFinalAmount + totalDiscount;

            var totalCash = cashRevenueFinalAmount + totalOrderCard;

            return new RevenueViewModel
            {
                cashRevenueFinalAmount = cashRevenueFinalAmount,
                cashRevenueTotalAmount = cashRevenueTotalAmount,
                cashRevenueTotalDiscount = cashRevenueTotalDiscount,
                passioAccountRevenueFinalAmount = passioAccountRevenueFinalAmount,
                passioAccountRevenueTotalAmount = passioAccountRevenueTotalAmount,
                passioAccountRevenueTotalDiscount = passioAccountRevenueTotalDiscount,
                billRevenueFinalAmount = billRevenueFinalAmount,
                billRevenueTotalAmount = billRevenueTotalAmount,
                billRevenueTotalDiscount = billRevenueTotalDiscount,
                otherRevenueFinalAmount = otherRevenueFinalAmount,
                otherRevenueTotalAmount = otherRevenueTotalAmount,
                otherRevenueTotalDiscount = otherRevenueTotalDiscount,
                totalDiscount = totalDiscount,
                totalOrderCard = totalOrderCard,
                totalRevenue = totalRevenue,
                totalCash = totalCash
            };
        }
        RevenueViewModel2 GetPayment(IQueryable<HmsService.Models.Entities.Order> dateOrders, IQueryable<HmsService.Models.Entities.Cost> dateCosts, int storeId, int brandId, DateTime sDate, DateTime eDate)
        {
            //Get all payment in time range with order status is finished
            var paymentApi = new PaymentApi();

            var paymentOrders = paymentApi.GetEntityStorePaymentInDateRange(storeId, sDate, eDate, brandId).Join(dateOrders,
                p => p.ToRentID,
                o => o.RentID,
                (p, o) => new { p, o.OrderStatus}
                ).ToList();

            var paymentCosts = storeId == 0 ? paymentApi.BaseService.GetPaymentByTimeRange(sDate, eDate).Join(dateCosts,
                    p => p.CostID,
                    c => c.CostID,
                    (p, c) => new { p, c }
                ).ToList()
                :
                paymentApi.BaseService.GetPaymentByTimeRange(sDate, eDate).Join(dateCosts.Where(c => (c.StoreId ?? 0) == storeId),
                    p => p.CostID,
                    c => c.CostID,
                    (p, c) => new { p, c }
                ).ToList();

            //Get Cash Amount
            var cashPaymentFinalAmount = paymentOrders.Where(q => q.p.Type == (int)PaymentTypeEnum.Cash && q.OrderStatus != (int)OrderStatusEnum.Cancel && q.OrderStatus != (int)OrderStatusEnum.PreCancel)
                .Sum(q => q.p.Amount);
            //Get Exchange Cash
            var exchangeCashPaymentFinalAmount = paymentOrders.Where(q => q.p.Type == (int)PaymentTypeEnum.ExchangeCash && q.OrderStatus != (int)OrderStatusEnum.Cancel && q.OrderStatus != (int)OrderStatusEnum.PreCancel)
                .Sum(q => q.p.Amount);
            //Get MasterCard
            var masterCardPaymentFinalAmount = paymentOrders.Where(q => q.p.Type == (int)PaymentTypeEnum.MasterCard && q.OrderStatus != (int)OrderStatusEnum.Cancel && q.OrderStatus != (int)OrderStatusEnum.PreCancel)
                .Sum(q => q.p.Amount);
            //Get VisaCard
            var visaCardPaymentFinalAmount = paymentOrders.Where(q => q.p.Type == (int)PaymentTypeEnum.VisaCard && q.OrderStatus != (int)OrderStatusEnum.Cancel && q.OrderStatus != (int)OrderStatusEnum.PreCancel)
                .Sum(q => q.p.Amount);
            //Get member payment
            var memberPaymentFinalAmount = paymentOrders.Where(q => q.p.Type == (int)PaymentTypeEnum.MemberPayment && q.OrderStatus != (int)OrderStatusEnum.Cancel && q.OrderStatus != (int)OrderStatusEnum.PreCancel)
                .Sum(q => q.p.Amount);
            //Get voucher
            var voucherPaymentFinalAmount = paymentOrders.Where(q => q.p.Type == (int)PaymentTypeEnum.Voucher && q.OrderStatus != (int)OrderStatusEnum.Cancel && q.OrderStatus != (int)OrderStatusEnum.PreCancel)
                .Sum(q => q.p.Amount);
            //Get Debt
            var debtPaymentFinalAmount = paymentOrders.Where(q => q.p.Type == (int)PaymentTypeEnum.Debt).Sum(q => q.p.Amount);
            //Get Cash Final Amount
            var cashFinalAmount = cashPaymentFinalAmount + exchangeCashPaymentFinalAmount;
            //Get Canceled and PreCanceled Final Amount
            var canceledFinalAmount = paymentOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel || q.OrderStatus == (int)OrderStatusEnum.PreCancel).Sum(q => q.p.Amount);

            //Get Cash from Costs
            var cashCostReceive = paymentCosts.Where(q => q.c.CostStatus == (int)CostStatusEnum.Approved && (q.c.CostType == (int)CostTypeEnum.ReceiveCost || q.c.CostType == (int)CostTypeEnum.ReceiveCostTranferOut) && q.p.Type == (int)PaymentTypeEnum.Cash).Sum(q => q.p.Amount);
            var cashCostSpend = paymentCosts.Where(q => q.c.CostStatus == (int)CostStatusEnum.Approved && (q.c.CostType == (int)CostTypeEnum.SpendingCost || q.c.CostType == (int)CostTypeEnum.SpendingCostTranferIn) && q.p.Type == (int)PaymentTypeEnum.Cash).Sum(q => q.p.Amount);

            var totalOrderCard = GetTotalOrderCard(dateOrders);
            var totalUseCard = memberPaymentFinalAmount;
            var totalPayment = cashFinalAmount + masterCardPaymentFinalAmount + visaCardPaymentFinalAmount
                                + memberPaymentFinalAmount + voucherPaymentFinalAmount + debtPaymentFinalAmount;
            var totalCash = cashPaymentFinalAmount + exchangeCashPaymentFinalAmount;
            var totalCashAll = totalCash + cashCostReceive - cashCostSpend;
            return new RevenueViewModel2
            {
                cashFinalAmount = cashFinalAmount,
                cashPaymentFinalAmount = cashPaymentFinalAmount,
                exchangeCashPaymentFinalAmount = exchangeCashPaymentFinalAmount,
                bankPaymentFinalAmount = masterCardPaymentFinalAmount + visaCardPaymentFinalAmount,
                masterCardPaymentFinalAmount = masterCardPaymentFinalAmount,
                visaCardPaymentFinalAmount = visaCardPaymentFinalAmount,
                memberPaymentFinalAmount = memberPaymentFinalAmount,
                voucherPaymentFinalAmount = voucherPaymentFinalAmount,
                debtPaymentFinalAmount = debtPaymentFinalAmount,
                totalOrderCard = totalOrderCard,
                totalUseCard = totalUseCard,
                totalPayment = totalPayment,
                totalCash = totalCash,
                canceledFinalAmount = canceledFinalAmount,
                cashCostReceive = cashCostReceive,
                cashCostSpend = cashCostSpend,
                totalCashAll = totalCashAll
            };
        }

        double GetTotalOrderCard(IQueryable<HmsService.Models.Entities.Order> dateOrders)
        {
            var totalOrderCard = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard).ToList().Sum(q => q.FinalAmount);
            return totalOrderCard;
        }

        public class RevenueViewModel
        {
            public double cashRevenueFinalAmount { get; set; }
            public double cashRevenueTotalAmount { get; set; }
            public double cashRevenueTotalDiscount { get; set; }

            public double passioAccountRevenueFinalAmount { get; set; }
            public double passioAccountRevenueTotalAmount { get; set; }
            public double passioAccountRevenueTotalDiscount { get; set; }


            public double billRevenueFinalAmount { get; set; }
            public double billRevenueTotalAmount { get; set; }
            public double billRevenueTotalDiscount { get; set; }

            public double otherRevenueFinalAmount { get; set; }
            public double otherRevenueTotalAmount { get; set; }
            public double otherRevenueTotalDiscount { get; set; }

            public double totalDiscount { get; set; }
            public double totalOrderCard { get; set; }
            public double totalRevenue { get; set; }
            public double totalCash { get; set; }


        }

        public class RevenueViewModel2
        {
            public double cashFinalAmount { get; set; }
            public double cashPaymentFinalAmount { get; set; }
            public double exchangeCashPaymentFinalAmount { get; set; }
            public double bankPaymentFinalAmount { get; set; }
            public double masterCardPaymentFinalAmount { get; set; }
            public double visaCardPaymentFinalAmount { get; set; }
            public double memberPaymentFinalAmount { get; set; }
            public double voucherPaymentFinalAmount { get; set; }
            public double debtPaymentFinalAmount { get; set; }
            public double totalOrderCard { get; set; }
            public double totalUseCard { get; set; }
            public double totalPayment { get; set; }
            public double totalCash { get; set; }
            public double canceledFinalAmount { get; set; }
            public double cashCostReceive { get; set; }
            public double cashCostSpend { get; set; }
            public double totalCashAll { get; set; }
        }

        DashBoardDiscountRevenueViewModel GetDiscountRevenue(IEnumerable<DateReportForDashBoard> dateReports)
        {
            var totalAmount = dateReports.Sum(q => q.TotalAmount.Value);
            var finalAmountCash = dateReports.Sum(q => q.FinalAmount.Value - q.FinalAmountCard.Value);

            var finalAmountCard = dateReports.Sum(q => q.FinalAmountCard.Value);
            var finalAmountAtStore = dateReports.Sum(q => q.FinalAmountAtStore.Value);
            var finalAmountTakeAway = dateReports.Sum(q => q.FinalAmountTakeAway.Value);
            var finalAmountDelivery = dateReports.Sum(q => q.FinalAmountDelivery.Value);
            var finalAmountCanceled = dateReports.Sum(q => q.FinalAmountCanceled.Value + q.FinalAmountPreCanceled.Value);

            var totalDiscount = dateReports.Sum(q => q.Discount.Value + q.DiscountOrderDetail.Value);
            var totalDiscountOrder = dateReports.Sum(q => q.Discount.Value);
            var totalDiscountOrderDetail = dateReports.Sum(q => q.DiscountOrderDetail.Value);
            return new DashBoardDiscountRevenueViewModel
            {
                totalAmount = totalAmount,
                finalAmountCash = finalAmountCash,

                finalAmountCard = finalAmountCard,
                finalAmountAtStore = finalAmountAtStore,
                finalAmountCanceled = finalAmountCanceled,
                finalAmountDelivery = finalAmountDelivery,
                finalAmountTakeAway = finalAmountTakeAway,

                totalDiscountOrder = totalDiscountOrder,
                totalDiscountOrderDetail = totalDiscountOrderDetail,
                totalDiscount = totalDiscount
            };
        }

        DashBoardDiscountRevenueViewModel GetDiscountRevenue(IEnumerable<OrderForDashBoard> dateOrders)
        {
            dateOrders = dateOrders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish && q.OrderType != (int)OrderTypeEnum.DropProduct);
            var totalAmount = dateOrders.Where(q => q.OrderType!=(int)OrderTypeEnum.OrderCard).Sum(q => q.TotalAmount.Value);
            var finalAmountCash = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard).Sum(q => q.FinalAmount.Value);

            var finalAmountCard = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard).Sum(q => q.FinalAmount.Value);
            var finalAmountAtStore = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.AtStore).Sum(q => q.FinalAmount.Value);
            var finalAmountTakeAway = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.TakeAway).Sum(q => q.FinalAmount.Value);
            var finalAmountDelivery = dateOrders.Where(q => q.OrderType == (int)OrderTypeEnum.Delivery).Sum(q => q.FinalAmount.Value);
            var finalAmountCanceled = 0;

            var totalDiscount = dateOrders.Where(q => q.OrderType!=(int)OrderTypeEnum.OrderCard).Sum(q => q.Discount.Value + q.DiscountOrderDetail.Value);
            var totalDiscountOrder = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard).Sum(q => q.Discount.Value);
            var totalDiscountOrderDetail = dateOrders.Where(q => q.OrderType != (int)OrderTypeEnum.OrderCard).Sum(q => q.DiscountOrderDetail.Value);
            return new DashBoardDiscountRevenueViewModel
            {
                totalAmount = totalAmount,
                finalAmountCash = finalAmountCash,

                finalAmountCard = finalAmountCard,
                finalAmountAtStore = finalAmountAtStore,
                finalAmountCanceled = finalAmountCanceled,
                finalAmountDelivery = finalAmountDelivery,
                finalAmountTakeAway = finalAmountTakeAway,

                totalDiscountOrder = totalDiscountOrder,
                totalDiscountOrderDetail = totalDiscountOrderDetail,
                totalDiscount = totalDiscount
            };
        }

        public class RevenueMemberCard
        {
            public double balanceBefore { get; set; }
            public double totalOrderCard { get; set; }
            public double totalUseCard { get; set; }
            public double balanceAfter { get; set; }
        }
    }
}
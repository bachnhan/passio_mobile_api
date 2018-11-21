using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class ReRunDateReportController : DomainBasedController
    {
        // GET: Admin/ReRunDateReport
        public ActionResult Index(int brandId)
        {
            var storeApi = new StoreApi();
            var stores = new List<SelectListItem>();
            stores.Add(new SelectListItem { Value = "0", Text = "Tất cả cửa hàng" });
            stores.AddRange(storeApi.GetActiveStoreByBrandId(brandId)
                                .Select(q => new SelectListItem { Value = q.ID.ToString(), Text = q.Name }).ToList());
            ViewBag.Stores = stores;
            return this.View();
        }

        bool CheckDateReport(DateReport oldReport, DateReport checkReport)
        {
            return oldReport != null &&
                !(oldReport.TotalOrder != checkReport.TotalOrder
                || oldReport.FinalAmount != checkReport.FinalAmount
                || oldReport.Discount != checkReport.Discount
                || oldReport.TotalAmount != checkReport.TotalAmount
                || oldReport.TotalCash != checkReport.TotalCash
                || oldReport.TotalOrderAtStore != checkReport.TotalOrderAtStore
                || oldReport.TotalOrderDelivery != checkReport.TotalOrderDelivery
                || oldReport.TotalOrderTakeAway != checkReport.TotalOrderTakeAway
                || oldReport.FinalAmountAtStore != checkReport.FinalAmountAtStore
                || oldReport.FinalAmountTakeAway != checkReport.FinalAmountTakeAway
                || oldReport.FinalAmountDelivery != checkReport.FinalAmountDelivery
                || oldReport.FinalAmountCard != checkReport.FinalAmountCard
                || oldReport.TotalOrderCanceled != checkReport.TotalOrderCanceled
                || oldReport.TotalOrderPreCanceled != checkReport.TotalOrderPreCanceled
                || oldReport.FinalAmountCanceled != checkReport.FinalAmountCanceled
                || oldReport.FinalAmountPreCanceled != checkReport.FinalAmountPreCanceled);
        }

        bool CheckPaymentReport(PaymentReport oldReport, PaymentReport checkReport)
        {
            return oldReport != null &&
                !(oldReport.BankAmount != checkReport.BankAmount
                || oldReport.CashAmount != checkReport.CashAmount
                || oldReport.DebtAmount != checkReport.DebtAmount
                || oldReport.MemberCardAmount != checkReport.MemberCardAmount
                || oldReport.OtherAmount != checkReport.OtherAmount
                || oldReport.PayDebtAmount != checkReport.PayDebtAmount
                || oldReport.ReceiptAmount != checkReport.ReceiptAmount
                || oldReport.SpendAmount != checkReport.SpendAmount
                || oldReport.VoucherAmount != checkReport.VoucherAmount);
        }

        public ActionResult ViewReport(string sDate, string eDate, /*string date,*/ int brandId, int storeId)
        {
            try
            {
                var paymentReportService = DependencyUtils.Resolve<IPaymentReportService>();
                //var dateSearch = Utils.ToDateTime(date);
                var paymentService = DependencyUtils.Resolve<IPaymentService>();
                var reportDateApi = new DateReportApi();
                var orderApi = new OrderApi();
                var fromDate = Utils.ToDateTime(sDate).GetStartOfDate();
                var toDate = Utils.ToDateTime(eDate).GetEndOfDate();
                var reports = reportDateApi.GetDateReportByTimeRange(fromDate, toDate, brandId, storeId);
                var paymentQuery = paymentReportService.Get(q => q.Date >= fromDate && q.Date <= toDate);
                if(storeId == 0)
                {
                    paymentQuery = paymentQuery.Where(q => q.Store.BrandId == brandId);
                }
                else
                {
                    paymentQuery = paymentQuery.Where(q => q.StoreID == storeId);
                }
                var paymentReports = paymentQuery.ToList();
                var allOrders = orderApi.GetOrdersByTimeRange(storeId, fromDate, toDate, brandId);
                var allPayments = paymentService.GetStorePaymentByTimeRange(storeId, brandId, fromDate, toDate).ToList();
                List<DateCheckReport> listResults = new List<DateCheckReport>();
                foreach (var item in reports)
                {
                    var orders = allOrders.Where(q => q.CheckInDate == null ? false :
                                        q.CheckInDate.Value.GetEndOfDate() == item.Date.GetEndOfDate() && q.StoreID == item.StoreID).ToList();
                    var checkReport = GetCheckReport(orders, item.Store.Type, item.StoreID, item.Date);
                    var payments = allPayments.Where(q => q.PayTime >= item.Date.GetStartOfDate()
                        && q.PayTime <= item.Date.GetEndOfDate()
                        && ((q.Order != null && q.Order.StoreID == item.StoreID)
                             || (q.Cost != null && q.Cost.StoreId == item.StoreID)));

                    var oldPaymentReport = paymentReports.FirstOrDefault(d =>
                                d.Date.Year == item.Date.Year && d.Date.Month == item.Date.Month && d.Date.Day == item.Date.Day &&
                                d.StoreID == item.StoreID);
                    
                    var checkPaymentReport = GetPaymentReport(payments, item.Date, item.StoreID);
                    var status = CheckDateReport(item, checkReport) && CheckPaymentReport(oldPaymentReport, checkPaymentReport);
                    var resultReport = new DateCheckReport
                    {
                        ID = item.ID,
                        Date = item.Date,
                        Status = status,
                        Revenue = item.FinalAmount.Value,
                        RealRevenue = checkReport.FinalAmount.Value,
                        TotalOrders = item.TotalOrder,
                        RealTotalOrders = checkReport.TotalOrder,
                        TotalPayments = oldPaymentReport == null ? 0 : oldPaymentReport.BankAmount + oldPaymentReport.CashAmount + oldPaymentReport.DebtAmount + (double)oldPaymentReport.PayDebtAmount + oldPaymentReport.MemberCardAmount + oldPaymentReport.OtherAmount + (double)oldPaymentReport.ReceiptAmount + (double)oldPaymentReport.SpendAmount,
                        RealTotalPayments = checkPaymentReport.BankAmount + checkPaymentReport.CashAmount + checkPaymentReport.DebtAmount + (double)checkPaymentReport.PayDebtAmount + checkPaymentReport.MemberCardAmount + checkPaymentReport.OtherAmount + (double)checkPaymentReport.ReceiptAmount + (double)checkPaymentReport.SpendAmount,
                        StoreId = item.StoreID,
                        StoreName = item.Store.Name
                    };
                    listResults.Add(resultReport);
                }

                var returnResult = listResults.OrderBy(q => q.Status).ThenBy(q => q.Date).Select(q => new
                {
                    Id = q.ID,
                    Date = q.Date.ToString("dd/MM/yyyy"),
                    Status = q.Status,
                    Revenue = string.Format("{0:n0}", q.Revenue),
                    RealRevenue = string.Format("{0:n0}", q.RealRevenue),
                    TotalOrders = string.Format("{0:n0}", q.TotalOrders),
                    RealTotalOrders = string.Format("{0:n0}", q.RealTotalOrders),
                    TotalPayments = string.Format("{0:n0}", q.TotalPayments),
                    RealTotalPayments = string.Format("{0:n0}", q.RealTotalPayments),
                    StoreId = q.StoreId,
                    StoreName = q.StoreName
                }).ToList();

                //var listReport = reportDateApi.GetDateReportByDateAndBrand(dateSearch, brandId).ToList();

                //var returnResult = listReport.Select(q => new IConvertible[]{
                //    count++,
                //    q.StoreID,
                //    q.TotalOrder,
                //    orderApi.CountFinishOrderByDate(dateSearch, q.StoreID),
                //    0,  //true-false
                //    q.ID
                //}).ToList();
                return Json(new { success = true, listReport = returnResult }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        DateReport GetCheckReport(IEnumerable<Order> orders, int storeType, int storeID, DateTime reportDate)
        {
            IEnumerable<Order> finishedOrders;
            if (storeType == (int)StoreTypeEnum.Hotel)
            {
                finishedOrders = orders.Where(a => a.RentStatus == (int)RentStatusEnum.Leave);
            }
            else
            {
                finishedOrders = orders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct
                                        && q.OrderStatus == (int)OrderStatusEnum.Finish);
            }
            var dateReport = new DateReport();
            if (storeType == (int)StoreTypeEnum.Hotel)
            {
                dateReport.StoreID = storeID;
                dateReport.CreateBy = "system";
                dateReport.Status = (int)DateReportStatusEnum.Approved;
                dateReport.Date = reportDate;
                dateReport.Discount = finishedOrders.Sum(a => a.Discount);
                dateReport.DiscountOrderDetail = finishedOrders.Sum(a => a.DiscountOrderDetail);
                dateReport.TotalAmount = finishedOrders.Sum(a => a.TotalAmount);
                dateReport.FinalAmount = finishedOrders.Sum(a => a.FinalAmount);
                dateReport.TotalCash = 0;
                dateReport.TotalOrder = finishedOrders.Count();
                dateReport.TotalOrderAtStore = 0;
                dateReport.TotalOrderTakeAway = 0;
                dateReport.TotalOrderDelivery = 0;
                dateReport.TotalOrderDetail = finishedOrders.Sum(a => a.OrderDetails.Sum(b => b.FinalAmount));
                dateReport.TotalOrderFeeItem = (int)finishedOrders.Sum(a => a.OrderFeeItems.Sum(b => b.TotalAmount));
                dateReport.TotalOrderCard = 0;
            }
            else
            {
                dateReport.StoreID = storeID;
                dateReport.CreateBy = "system";
                dateReport.Status = (int)DateReportStatusEnum.Approved;
                dateReport.Date = reportDate;
                dateReport.Discount = finishedOrders.Sum(a => a.Discount);
                dateReport.DiscountOrderDetail = finishedOrders.Sum(a => a.DiscountOrderDetail);
                dateReport.TotalAmount = finishedOrders.Sum(a => a.TotalAmount);
                dateReport.FinalAmount = finishedOrders.Sum(a => a.FinalAmount);
                dateReport.TotalCash = 0;
                dateReport.TotalOrder = finishedOrders.Count();
                dateReport.TotalOrderAtStore = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                dateReport.TotalOrderTakeAway = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                dateReport.TotalOrderDelivery = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                dateReport.TotalOrderCard = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.OrderCard);
                dateReport.TotalOrderDetail = 0;
                dateReport.TotalOrderFeeItem = 0;
                dateReport.FinalAmountAtStore = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.AtStore)
                    .Sum(a => a.FinalAmount);
                dateReport.FinalAmountTakeAway = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.TakeAway)
                    .Sum(a => a.FinalAmount);
                dateReport.FinalAmountDelivery = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.Delivery)
                    .Sum(a => a.FinalAmount);
                dateReport.FinalAmountCard = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard)
                    .Sum(a => a.FinalAmount);
                dateReport.TotalOrderCanceled = orders.Count(q => q.OrderStatus == (int)OrderStatusEnum.Cancel);

                dateReport.TotalOrderPreCanceled = orders.Count(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel);
                dateReport.FinalAmountCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel)
                    .Sum(a => a.FinalAmount);
                dateReport.FinalAmountPreCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel)
                    .Sum(a => a.FinalAmount);
            }
            return dateReport;
        }

        public ActionResult ReCreateReport(string date, int reportId, int storeId)
        {
            try
            {
                var reportDate = Utils.ToDateTime(date).GetEndOfDate();
                var storeService = DependencyUtils.Resolve<IStoreService>();
                var orderService = DependencyUtils.Resolve<IOrderService>();
                var orderDetailService = DependencyUtils.Resolve<IOrderDetailService>();
                var productService = DependencyUtils.Resolve<IProductService>();
                var productItemService = DependencyUtils.Resolve<IProductItemService>();
                var inventoryReceiptService = DependencyUtils.Resolve<IInventoryReceiptService>();
                var reportService = DependencyUtils.Resolve<IReportService>();
                var dateReportApi = new DateReportApi();
                var dateProductApi = new DateProductApi();
                var paymentService = DependencyUtils.Resolve<IPaymentService>();
                var paymentReportService = DependencyUtils.Resolve<IPaymentReportService>();

                //Set date
                var fromDate = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day).GetStartOfDate();
                var toDate = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day).GetEndOfDate();

                //Get store
                var store = storeService.GetStoreById(storeId);

                //Get orders
                IEnumerable<Order> orders;
                IEnumerable<Order> finishedOrders;
                if (store.Type == (int)StoreTypeEnum.Hotel)
                {
                    orders = orderService.GetAllHotelOrdersByCheckOutDate(fromDate, toDate, store.ID).ToList();
                    finishedOrders = orders.Where(a => a.RentStatus == (int)RentStatusEnum.Leave);
                }
                else
                {
                    orders = orderService.GetOrdersByTimeRange(store.ID, fromDate, toDate).ToList();
                    finishedOrders = orders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct
                                            && q.OrderStatus == (int)OrderStatusEnum.Finish);
                }

                ////Get order details
                //var orderDetails =
                //    orderDetailService.GetOrderDetailsByTimeRange(fromDate, toDate, store.ID)
                //    .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish).ToList();

                ////Get products
                //var dateProducts =
                //    orderDetails.GroupBy(a => a.ProductID)
                //        .Join(productService.GetAllProducts(), a => a.Key, a => a.ProductID, (a, b) => new DateProduct()
                //        {
                //            ProductId = a.Key,
                //            StoreID = store.ID,
                //            Quantity = a.Sum(c => c.Quantity),
                //            Date = reportDate,
                //            TotalAmount = a.Sum(c => c.TotalAmount),
                //            FinalAmount = a.Sum(c => c.FinalAmount),
                //            Discount = a.Sum(c => c.Discount),
                //            ProductName_ = b.ProductName,
                //            Product = b,
                //            CategoryId_ = b.ProductCategory.CateID
                //        }).ToList();

                //Create DateReport
                var dateReport = new DateReport();
                if (store.Type == (int)StoreTypeEnum.Hotel)
                {
                    dateReport.StoreID = store.ID;
                    dateReport.CreateBy = "system";
                    dateReport.Status = (int)DateReportStatusEnum.Approved;
                    dateReport.Date = reportDate;
                    dateReport.Discount = finishedOrders.Sum(a => a.Discount);
                    dateReport.DiscountOrderDetail = finishedOrders.Sum(a => a.DiscountOrderDetail);
                    dateReport.TotalAmount = finishedOrders.Sum(a => a.TotalAmount);
                    dateReport.FinalAmount = finishedOrders.Sum(a => a.FinalAmount);
                    dateReport.TotalCash = 0;
                    dateReport.TotalOrder = finishedOrders.Count();
                    dateReport.TotalOrderAtStore = 0;
                    dateReport.TotalOrderTakeAway = 0;
                    dateReport.TotalOrderDelivery = 0;
                    dateReport.TotalOrderDetail = finishedOrders.Sum(a => a.OrderDetails.Sum(b => b.FinalAmount));
                    dateReport.TotalOrderFeeItem = (int)finishedOrders.Sum(a => a.OrderFeeItems.Sum(b => b.TotalAmount));
                    dateReport.TotalOrderCard = 0;
                }
                else
                {
                    dateReport.StoreID = store.ID;
                    dateReport.CreateBy = "system";
                    dateReport.Status = (int)DateReportStatusEnum.Approved;
                    dateReport.Date = reportDate;
                    dateReport.Discount = finishedOrders.Sum(a => a.Discount);
                    dateReport.DiscountOrderDetail = finishedOrders.Sum(a => a.DiscountOrderDetail);
                    dateReport.TotalAmount = finishedOrders.Sum(a => a.TotalAmount);
                    dateReport.FinalAmount = finishedOrders.Sum(a => a.FinalAmount);
                    dateReport.TotalCash = 0;
                    dateReport.TotalOrder = finishedOrders.Count();
                    dateReport.TotalOrderAtStore = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                    dateReport.TotalOrderTakeAway = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                    dateReport.TotalOrderDelivery = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                    dateReport.TotalOrderCard = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.OrderCard);
                    dateReport.TotalOrderDetail = 0;
                    dateReport.TotalOrderFeeItem = 0;
                    dateReport.FinalAmountAtStore = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.AtStore)
                        .Sum(a => a.FinalAmount);
                    dateReport.FinalAmountTakeAway = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.TakeAway)
                        .Sum(a => a.FinalAmount);
                    dateReport.FinalAmountDelivery = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.Delivery)
                        .Sum(a => a.FinalAmount);
                    dateReport.FinalAmountCard = finishedOrders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard)
                        .Sum(a => a.FinalAmount);
                    dateReport.TotalOrderCanceled = orders.Count(q => q.OrderStatus == (int)OrderStatusEnum.Cancel);

                    dateReport.TotalOrderPreCanceled = orders.Count(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel);
                    dateReport.FinalAmountCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel)
                        .Sum(a => a.FinalAmount);
                    dateReport.FinalAmountPreCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel)
                        .Sum(a => a.FinalAmount);
                }

                var oldDateReport = dateReportApi.GetDateReportById(reportId);
                var listDateProduct = dateProductApi.GetDateProductByTimeRange(reportDate, storeId);

                var payments = paymentService.GetStorePaymentByTimeRange(store.ID, (int)store.BrandId, fromDate, toDate).ToList();

                var paymentReport = GetPaymentReport(payments, reportDate, store.ID);


                var oldPaymentReport = paymentReportService.FirstOrDefault(d =>
                            d.Date.Year == reportDate.Year && d.Date.Month == reportDate.Month && d.Date.Day == reportDate.Day &&
                            d.StoreID == store.ID);

                //var reportTracking = new ReportTracking()
                //{
                //    Date = Utils.GetCurrentDateTime(),
                //    IsUpdate = false,
                //    UpdatePerson = "system",
                //    StoreId = store.ID,
                //};
                //var compositionsStatistic = dateProducts.SelectMany(a => a.Product.ProductItemCompositionMappings.Select(b => new Tuple<ProductItemCompositionMapping, int>(b, a.Quantity)))
                //    .GroupBy(a => a.Item1.ItemID);
                //var dateItemProduct = compositionsStatistic.Join(productItemService.GetProductItems(), a => a.Key, a => a.ItemID, (a, b) => new DateProductItem
                //{
                //    StoreId = store.ID,
                //    Date = reportDate,
                //    ProductItemID = a.Key,
                //    ProductItemName = b.ItemName,
                //    Quantity = (int)a.Sum(c => c.Item2 * c.Item1.Quantity),
                //    Unit = b.Unit
                //}).AsQueryable();
                //var inventoryReceipts = inventoryReceiptService.GetInventoryReceiptByTimeRange(store.ID, fromDate, toDate).AsQueryable();

                string user = "";
                try
                {
                    user = (System.Web.HttpContext.Current == null) ? "quanly" : System.Web.HttpContext.Current.User.Identity.Name;
                }
                catch (Exception)
                {
                    user = "noname";
                }

                reportService.UpdateDateReportOnly(oldDateReport, dateReport, oldPaymentReport, paymentReport, store);


            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        PaymentReport GetPaymentReport(IEnumerable<HmsService.Models.Entities.Payment> payments, DateTime reportDate, int storeID)
        {
            var paymentReport = new PaymentReport()
            {
                StoreID = storeID,
                Date = reportDate,
                Status = (int)PaymentReportStatusEnum.Approved,
                CreateBy = "system",
                CashAmount = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash
                                              || q.Type == (int)PaymentTypeEnum.ExchangeCash)
                                            .Sum(q => q.Amount),
                MemberCardAmount = payments.Where(a => a.Type == (int)PaymentTypeEnum.MemberPayment).Sum(a => a.Amount),
                VoucherAmount = payments.Where(a => a.Type == (int)PaymentTypeEnum.Voucher).Sum(a => a.Amount),
                BankAmount = payments.Where(a => a.Type == (int)PaymentTypeEnum.MasterCard || a.Type == (int)PaymentTypeEnum.VisaCard)
                                            .Sum(a => a.Amount),
                DebtAmount = payments.Where(a => a.Type == (int)PaymentTypeEnum.Debt).Sum(a => a.Amount),
                OtherAmount = payments.Where(a => a.Type == (int)PaymentTypeEnum.Other).Sum(a => a.Amount),
                PayDebtAmount = payments.Where(a => a.Type == (int)PaymentTypeEnum.PayDebt).Sum(a => a.Amount),
                ReceiptAmount = payments.Where(a => a.Cost != null && a.Cost.CostType == (int)CostTypeEnum.ReceiveCost).Sum(a => a.Amount),
                SpendAmount = payments.Where(a => a.Cost != null && a.Cost.CostType == (int)CostTypeEnum.SpendingCost).Sum(a => a.Amount)
            };
            return paymentReport;
        }

        public ActionResult ReRunReport(int[] incorrectReports)
        {
            try
            {
                var dateReportApi = new DateReportApi();
                var reportService = DependencyUtils.Resolve<IReportService>();
                var orderService = DependencyUtils.Resolve<IOrderService>();
                var paymentService = DependencyUtils.Resolve<IPaymentService>();
                var paymentReportService = DependencyUtils.Resolve<IPaymentReportService>();
                var reports = dateReportApi.BaseService.Get(q => incorrectReports.Contains(q.ID)).ToList();
                foreach (var oldReport in reports)
                {
                    var reportDate = oldReport.Date.GetEndOfDate();
                    var storeId = oldReport.StoreID;
                    var orders = orderService.GetOrdersByTimeRange(storeId, reportDate.GetStartOfDate(), reportDate).ToList();
                    var checkDateReport = GetCheckReport(orders, oldReport.Store.Type, oldReport.StoreID, oldReport.Date);
                    var payments = paymentService.GetPaymentStoreByTimeRange(reportDate.GetStartOfDate(), reportDate, storeId).ToList();
                    var oldPaymentReport = paymentReportService.FirstOrDefault(d =>
                                d.Date.Year == reportDate.Year && d.Date.Month == reportDate.Month && d.Date.Day == reportDate.Day &&
                                d.StoreID == storeId);
                    var checkPaymentReport = GetPaymentReport(payments, reportDate, storeId);
                    var success = reportService.UpdateDateReportOnly(oldReport, checkDateReport, oldPaymentReport, checkPaymentReport, oldReport.Store);
                    if (!success)
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }

    
}
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Integration.Mvc;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Repositories;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

namespace Wisky.SkyAdmin.Manage.Automation
{
    public class DateReportExecuter
    {

        public string Url { get; set; }

        public DateReportExecuter(string url)
        {
            this.Url = url;
        }

        public void Start()
        {
            var t = new Thread(Loop);
            t.Start();
        }

        private void Loop()
        {
            while (true)
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadData(this.Url);
                    }
                }
                catch
                {

                }
               Thread.Sleep(5400000);
            }
        }

        public static void PerformWork()
        {
            //var storeApi = DependencyUtils.Resolve<StoreApi>();
            //var stores = storeApi.GetStoresByBrandId(1);
            //var storeService = DependencyUtils.Resolve<IStoreService>();
            //var stores = storeService.GetStoreByBrandId(1);
            var storeApi = new StoreApi();
            var stores = storeApi.GetStores();
            foreach (var store in stores)
            {
                if (store.RunReport == true)
                {
                    if (!store.ReportDate.HasValue)
                    {
                        if (store.Orders.Count > 0)
                        {
                            ReCalculate(store.ID);
                        }
                    }
                    else
                    {
                        Calculate(store.ID);
                    }
                }
            }
        }

        public static void Calculate(int storeId)
        {
            var storeService = DependencyUtils.Resolve<IStoreService>();
            var store = storeService.GetStoreById(storeId);
            //var storeApi = DependencyUtils.Resolve<StoreApi>();
            //var store = storeApi.GetStoreById(storeId);
            if (store.ReportDate.Value.AddDays(1) < Utils.GetCurrentDateTime())
            {
                var time = Utils.GetCurrentDateTime();
                var days = (time - store.ReportDate.Value).Days;
                for (int i = 0; i < days; i++)
                {
                    //UpdateDateReport(store);
                    StoreCalculate(store);
                }
            }
        }

        private static void ReCalculate(int storeId)
        {
            var time = Utils.GetCurrentDateTime();
            //var storeApi = DependencyUtils.Resolve<StoreApi>();
            //var store = storeApi.GetStoreById(storeId);
            var storeService = DependencyUtils.Resolve<IStoreService>();
            var store = storeService.GetStoreById(storeId);
            if (store.Orders.Count <= 0)
            {
                return;
            }
            var firstOrderDate = store.Orders.Where(a => a.CheckInDate.HasValue).Min(a => a.CheckInDate.Value);
            store.ReportDate = firstOrderDate.GetEndOfDate().AddDays(-1);
            var timeCount = (time - firstOrderDate).Days;
            for (int i = 0; i < timeCount; i++)
            {
                //UpdateDateReport(store);
                StoreCalculate(store);
            }
        }

        private static void UpdateDateReport(Store store)
        {
            Debug.WriteLine("Start update store {0} in date {1} - {2}", store.Name,
                store.ReportDate.Value.ToString("dd/MM/yyyy"), Utils.GetCurrentDateTime().ToString("HH:mm:ss"));
            var reportDate = store.ReportDate.Value.AddDays(1);
            var orderService = DependencyUtils.Resolve<IOrderService>();
            var reportService = DependencyUtils.Resolve<IReportService>();
            var dateReportService = DependencyUtils.Resolve<IDateReportService>();
            var paymentService = DependencyUtils.Resolve<IPaymentService>();
            var paymentReportService = DependencyUtils.Resolve<IPaymentReportService>();
            var fromDate = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day).GetStartOfDate();
            var toDate = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day).GetEndOfDate();
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
                dateReport.FinalAmountCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel).ToList()
                    .Sum(a => a.FinalAmount);
                dateReport.FinalAmountPreCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel).ToList()
                    .Sum(a => a.FinalAmount);
            }
            var payments = paymentService.GetStorePaymentByTimeRange(store.ID, (int)store.BrandId, fromDate, toDate).ToList();

            var paymentReport = new PaymentReport()
            {
                Date = reportDate,
                Status = (int)PaymentReportStatusEnum.Approved,
                StoreID = store.ID,
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
            };
            var oldDateReport = dateReportService.FirstOrDefault(d =>
                        d.Date.Year == reportDate.Year && d.Date.Month == reportDate.Month && d.Date.Day == reportDate.Day &&
                        d.StoreID == store.ID);

            var oldPaymentReport = paymentReportService.FirstOrDefault(d =>
                        d.Date.Year == reportDate.Year && d.Date.Month == reportDate.Month && d.Date.Day == reportDate.Day &&
                        d.StoreID == store.ID);

            reportService.UpdateDateReportOnly(oldDateReport, dateReport, oldPaymentReport, paymentReport, store);

        }



        public static void StoreCalculate(Store store)
        {
            Debug.WriteLine("Start update store {0} in date {1} - {2}", store.Name,
                store.ReportDate.Value.ToString("dd/MM/yyyy"), Utils.GetCurrentDateTime().ToString("HH:mm:ss"));
            var reportDate = store.ReportDate.Value.AddDays(1);
            var orderDetailService = DependencyUtils.Resolve<IOrderDetailService>();
            var orderService = DependencyUtils.Resolve<IOrderService>();
            var dateProductService = DependencyUtils.Resolve<IDateProductService>();
            var dateProductItemService = DependencyUtils.Resolve<IDateProductItemService>();
            var dateReportService = DependencyUtils.Resolve<IDateReportService>();
            var storeService = DependencyUtils.Resolve<IStoreService>();
            var productService = DependencyUtils.Resolve<IProductService>();
            var productItemService = DependencyUtils.Resolve<IProductItemService>();
            var dateProductRepo = DependencyUtils.Resolve<IDateProductRepository>();
            var dateProductItemRepo = DependencyUtils.Resolve<IDateProductItemRepository>();
            var reportTrackingService = DependencyUtils.Resolve<IReportTrackingService>();
            var reportService = DependencyUtils.Resolve<IReportService>();
            var inventoryReceiptService = DependencyUtils.Resolve<IInventoryReceiptService>();
            var paymentService = DependencyUtils.Resolve<IPaymentService>();
            var uow = DependencyUtils.Resolve<IUnitOfWork>();

            var fromDate = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day).GetStartOfDate();
            var toDate = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day).GetEndOfDate();
            //Get OrderDetail
            var orderDetails =
                orderDetailService.GetOrderDetailsByTimeRange(fromDate, toDate, store.ID)
                .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
            //Get Rent
            List<Order> orders;
            List<Order> finishedOrders;
            //Get Payments
            var payments = paymentService.GetStorePaymentByTimeRange(store.ID, (int)store.BrandId, fromDate, toDate).ToList();
            if (store.Type == (int)StoreTypeEnum.Hotel)
            {
                orders = orderService.GetAllHotelOrdersByCheckOutDate(fromDate, toDate, store.ID).ToList();
                finishedOrders = orders.Where(a => a.RentStatus == (int)RentStatusEnum.Leave).ToList();
            }
            else
            {
                orders = orderService.GetOrdersByTimeRange(store.ID, fromDate, toDate).ToList();
                finishedOrders = orders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct
                                        && q.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
            }

            var dateProducts =
                orderDetails.GroupBy(a => a.ProductID)
                    .Join(productService.GetAllProducts(), a => a.Key, a => a.ProductID, (a, b) => new DateProduct()
                    {
                        ProductId = a.Key,
                        StoreID = store.ID,
                        Quantity = a.Sum(c => c.Quantity),
                        Date = reportDate,
                        TotalAmount = a.Sum(c => c.TotalAmount),
                        FinalAmount = a.Sum(c => c.FinalAmount),
                        Discount = a.Sum(c => c.Discount),
                        ProductName_ = b.ProductName,
                        Product = b,
                        CategoryId_ = b.ProductCategory.CateID
                    }).ToList();

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
               
                dateReport.FinalAmountCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Cancel).ToList()
                    .Sum(a => a.FinalAmount);
                dateReport.FinalAmountPreCanceled = orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.PreCancel).ToList()
                    .Sum(a => a.FinalAmount);
            }


            var reportTracking = new ReportTracking()
            {
                Date = Utils.GetCurrentDateTime(),
                IsUpdate = false,
                UpdatePerson = "system",
                StoreId = store.ID,
            };
            var compositionsStatistic = dateProducts.SelectMany(a => a.Product.ProductItemCompositionMappings.Select(b => new Tuple<ProductItemCompositionMapping, int>(b, a.Quantity)))
                .GroupBy(a => a.Item1.ItemID);

            var dateItemProduct = compositionsStatistic.Join(productItemService.GetProductItems(), a => a.Key, a => a.ItemID, (a, b) => new DateProductItem
            {
                StoreId = store.ID,
                Date = reportDate,
                ProductItemID = a.Key,
                ProductItemName = b.ItemName,
                Quantity = (int)a.Sum(c => c.Item2 * c.Item1.Quantity),
                Unit = b.Unit
            }).AsQueryable();

            var inventoryReceipts = inventoryReceiptService.GetInventoryReceiptByTimeRange(store.ID, fromDate, toDate);

            var paymentReport = new PaymentReport()
            {
                Date = reportDate,
                Status = (int)PaymentReportStatusEnum.Approved,
                StoreID = store.ID,
                CreateBy = "system",
                CashAmount = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash 
                                              || q.Type == (int) PaymentTypeEnum.ExchangeCash)
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

            string user = "";
            try
            {
                user = (System.Web.HttpContext.Current == null) ? "quanly" : System.Web.HttpContext.Current.User.Identity.Name;
            }
            catch (Exception)
            {
                user = "noname";
            }
            //System.Security.Principal.IIdentity user = null;
            var result = reportService.CreateDateReport(dateReport, orderDetails, dateItemProduct, store, finishedOrders, inventoryReceipts, paymentReport, user);

        }
    }
    internal class ProductReportStatistic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Amount { get; set; }
    }
}
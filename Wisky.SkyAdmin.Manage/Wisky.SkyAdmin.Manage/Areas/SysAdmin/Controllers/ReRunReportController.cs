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

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class ReRunReportController : DomainBasedController
    {
        // GET: SysAdmin/ReRunReport
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadReport(string _date)
        {
            var dateReportApi = new DateReportApi();
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();

            var dateSearch = Utils.ToDateTime(_date);
            var startDate = dateSearch.GetStartOfDate();
            var endDate = dateSearch.GetEndOfDate();

            var dataReport = dateReportApi.GetReportByDate(dateSearch);

            var listStore = storeApi.BaseService.Get(q => q.isAvailable == true).Select(q => q.ID);
            var count = 0;
            var listResult = new List<CheckReport>();
            foreach (var item in listStore)
            {
                var status = true;
                //var listAllOrder = orderApi.getall
                count++;
                var orderDetails = orderDetailApi.BaseService.GetOrderDetailsByTimeRange(startDate, endDate, item)
                    .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
                //Get Rent
                var orders = orderApi.BaseService.GetOrdersByTimeRange(item, startDate, endDate).ToList();
                var finishedOrders = orders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct
                                            && q.OrderStatus == (int)OrderStatusEnum.Finish);

                var dateReport = new DateReport();

                dateReport.StoreID = item;
                dateReport.CreateBy = "system";
                dateReport.Status = (int)DateReportStatusEnum.Approved;
                dateReport.Date = dateSearch.GetEndOfDate();
                dateReport.Discount = finishedOrders.Sum(a => a.Discount);
                dateReport.DiscountOrderDetail = finishedOrders.Sum(a => a.DiscountOrderDetail);
                dateReport.TotalAmount = finishedOrders.Sum(a => a.TotalAmount);
                dateReport.FinalAmount = finishedOrders.Sum(a => a.FinalAmount);
                dateReport.TotalCash = 0;
                dateReport.TotalOrder = finishedOrders.Count();
                dateReport.TotalOrderAtStore = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                dateReport.TotalOrderTakeAway = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                dateReport.TotalOrderDelivery = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
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

                var currentDateReport = dataReport.Where(q => q.StoreID == item).FirstOrDefault();
                if (currentDateReport != null)
                {
                    if (currentDateReport.Date == dateReport.Date && currentDateReport.StoreID == dateReport.StoreID)
                    {
                        if (currentDateReport.TotalOrder != dateReport.TotalOrder
                            || currentDateReport.FinalAmount != dateReport.FinalAmount
                            || currentDateReport.Discount != dateReport.Discount
                            || currentDateReport.TotalAmount != dateReport.TotalAmount
                            || currentDateReport.TotalCash != dateReport.TotalCash
                            || currentDateReport.TotalOrderAtStore != dateReport.TotalOrderAtStore
                            || currentDateReport.TotalOrderDelivery != dateReport.TotalOrderDelivery
                            || currentDateReport.TotalOrderTakeAway != dateReport.TotalOrderTakeAway
                            || currentDateReport.FinalAmountAtStore != dateReport.FinalAmountAtStore
                            || currentDateReport.FinalAmountTakeAway != dateReport.FinalAmountTakeAway
                            || currentDateReport.FinalAmountDelivery != dateReport.FinalAmountDelivery
                            || currentDateReport.FinalAmountCard != dateReport.FinalAmountCard
                            || currentDateReport.TotalOrderCanceled != dateReport.TotalOrderCanceled
                            || currentDateReport.TotalOrderPreCanceled != dateReport.TotalOrderPreCanceled
                            || currentDateReport.FinalAmountCanceled != dateReport.FinalAmountCanceled
                            || currentDateReport.FinalAmountPreCanceled != dateReport.FinalAmountPreCanceled)
                        {

                            status = false;
                        }
                    }
                    if (status)
                    {
                        var resultReport = new CheckReport();
                        resultReport.STT = count++;
                        resultReport.Revenue = currentDateReport.TotalAmount.Value;
                        resultReport.RevenueWithoutDiscount = currentDateReport.FinalAmount.Value;
                        resultReport.Discount = currentDateReport.Discount.Value;
                        resultReport.Status = status;
                        resultReport.StoreName = storeApi.BaseService.Get(currentDateReport.StoreID).Name;
                        resultReport.TotalCash = currentDateReport.TotalCash.Value;
                        resultReport.TotalOrder = currentDateReport.TotalOrder;
                        resultReport.DiscountOrderDetail = currentDateReport.DiscountOrderDetail.Value;
                        resultReport.StoreId = currentDateReport.StoreID;
                        listResult.Add(resultReport);
                    }
                    else
                    {
                        var resultReport = new CheckReport();
                        resultReport.STT = count++;
                        resultReport.Revenue = dateReport.TotalAmount.Value;
                        resultReport.RevenueWithoutDiscount = dateReport.FinalAmount.Value;
                        resultReport.Discount = dateReport.Discount.Value;
                        resultReport.Status = status;
                        resultReport.StoreName = storeApi.BaseService.Get(dateReport.StoreID).Name;
                        resultReport.TotalCash = dateReport.TotalCash.Value;
                        resultReport.TotalOrder = dateReport.TotalOrder;
                        resultReport.DiscountOrderDetail = dateReport.DiscountOrderDetail.Value;
                        resultReport.StoreId = dateReport.StoreID;
                        listResult.Add(resultReport);
                    }
                }
            }
            var resutl = listResult.Select(q => new IConvertible[] {
                 q.STT,
                 q.StoreName,
                 q.TotalOrder,
                 q.RevenueWithoutDiscount,
                 q.Revenue,
                 q.Discount,
                 q.DiscountOrderDetail,
                 q.TotalCash,
                 q.Status,
                 q.StoreId
            });
            return Json(new { success = true, listResult = resutl }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReRunReportByDate(string _date)
        {
            var dateReportApi = new DateReportApi();
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();
            var productApi = new ProductApi();
            var dateProductApi = new DateProductApi();
            var productItemApi = new ProductItemApi();
            var inventoryReceptApi = new InventoryReceiptApi();
            var reportService = DependencyUtils.Resolve<IReportService>();

            var dateSearch = Utils.ToDateTime(_date);
            var startDate = dateSearch.GetStartOfDate();
            var endDate = dateSearch.GetEndOfDate();

            var dataReport = dateReportApi.GetReportByDate(dateSearch).ToList();
            var listStore = storeApi.BaseService.Get(q => q.isAvailable == true).Select(q => q.ID).ToList();
            var count = 0;
            var listResult = new List<CheckReport>();

            foreach (var item in listStore)
            {
                var status = true;
                //var listAllOrder = orderApi.getall
                count++;
                var orderDetails = orderDetailApi.BaseService.GetOrderDetailsByTimeRange(startDate, endDate, item)
                    .Where(a => a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
                //Get Rent
                var orders = orderApi.BaseService.GetOrdersByTimeRange(item, startDate, endDate).ToList();
                var finishedOrders = orders.Where(q => q.OrderType != (int)OrderTypeEnum.DropProduct
                                            && q.OrderStatus == (int)OrderStatusEnum.Finish);

                var dateReport = new DateReport();

                dateReport.StoreID = item;
                dateReport.CreateBy = "system";
                dateReport.Status = (int)DateReportStatusEnum.Approved;
                dateReport.Date = dateSearch.GetEndOfDate();
                dateReport.Discount = finishedOrders.Sum(a => a.Discount);
                dateReport.DiscountOrderDetail = finishedOrders.Sum(a => a.DiscountOrderDetail);
                dateReport.TotalAmount = finishedOrders.Sum(a => a.TotalAmount);
                dateReport.FinalAmount = finishedOrders.Sum(a => a.FinalAmount);
                dateReport.TotalCash = 0;
                dateReport.TotalOrder = finishedOrders.Count();
                dateReport.TotalOrderAtStore = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                dateReport.TotalOrderTakeAway = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                dateReport.TotalOrderDelivery = finishedOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
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
                var currentDateReport = dataReport.Where(q => q.StoreID == item).FirstOrDefault();
                if (currentDateReport != null)
                {
                    if (currentDateReport.Date == dateReport.Date && currentDateReport.StoreID == dateReport.StoreID)
                    {
                        if (currentDateReport.TotalOrder != dateReport.TotalOrder
                            || currentDateReport.FinalAmount != dateReport.FinalAmount
                            || currentDateReport.Discount != dateReport.Discount
                            || currentDateReport.TotalAmount != dateReport.TotalAmount
                            || currentDateReport.TotalCash != dateReport.TotalCash
                            || currentDateReport.TotalOrderAtStore != dateReport.TotalOrderAtStore
                            || currentDateReport.TotalOrderDelivery != dateReport.TotalOrderDelivery
                            || currentDateReport.TotalOrderTakeAway != dateReport.TotalOrderTakeAway
                            || currentDateReport.FinalAmountAtStore != dateReport.FinalAmountAtStore
                            || currentDateReport.FinalAmountTakeAway != dateReport.FinalAmountTakeAway
                            || currentDateReport.FinalAmountDelivery != dateReport.FinalAmountDelivery
                            || currentDateReport.FinalAmountCard != dateReport.FinalAmountCard
                            || currentDateReport.TotalOrderCanceled != dateReport.TotalOrderCanceled
                            || currentDateReport.TotalOrderPreCanceled != dateReport.TotalOrderPreCanceled
                            || currentDateReport.FinalAmountCanceled != dateReport.FinalAmountCanceled
                            || currentDateReport.FinalAmountPreCanceled != dateReport.FinalAmountPreCanceled)
                        {

                            status = false;
                        }
                    }
                    if (!status)
                    {
                        currentDateReport.Discount = dateReport.Discount;
                        currentDateReport.DiscountOrderDetail = dateReport.DiscountOrderDetail;
                        currentDateReport.FinalAmount = dateReport.FinalAmount;
                        currentDateReport.TotalAmount = dateReport.TotalAmount;
                        currentDateReport.TotalCash = dateReport.TotalCash;
                        currentDateReport.TotalOrder = dateReport.TotalOrder;
                        currentDateReport.TotalOrderAtStore = dateReport.TotalOrderAtStore;
                        currentDateReport.TotalOrderDelivery = dateReport.TotalOrderDelivery;
                        currentDateReport.TotalOrderDetail = dateReport.TotalOrderDetail;
                        currentDateReport.TotalOrderFeeItem = dateReport.TotalOrderFeeItem;
                        currentDateReport.Discount = dateReport.Discount;
                        currentDateReport.TotalOrderTakeAway = dateReport.TotalOrderTakeAway;
                        currentDateReport.FinalAmountAtStore = dateReport.FinalAmountAtStore;
                        currentDateReport.FinalAmountTakeAway = dateReport.FinalAmountTakeAway;
                        currentDateReport.FinalAmountDelivery = dateReport.FinalAmountDelivery;
                        currentDateReport.FinalAmountCard = dateReport.FinalAmountCard;
                        currentDateReport.TotalOrderCanceled = dateReport.TotalOrderCanceled;
                        currentDateReport.TotalOrderPreCanceled = dateReport.TotalOrderPreCanceled;
                        currentDateReport.FinalAmountCanceled = dateReport.FinalAmountCanceled;
                        currentDateReport.FinalAmountPreCanceled = dateReport.FinalAmountPreCanceled;

                        dateReportApi.BaseService.Update(currentDateReport);
                        #region DateProduct
                        var dateProductDbs = new List<DateProduct>();
                        foreach (var orderdetailTMP in orderDetails)
                        {

                            var product = dateProductDbs.FirstOrDefault(a => a.ProductId == orderdetailTMP.ProductID);
                            if (product == null)
                            {
                                //var countRent = rents.GetMany(a => a.OrderDetails.Any(b => b.ProductID == item.ProductID)).Count();
                                var totalOrders = finishedOrders.Where(a => a.OrderDetails.Any(b => b.ProductID == orderdetailTMP.ProductID));
                                var totalOrderAtstore = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                                var totalOrderDelivery = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                                var totalOrderTakeAway = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                                product = new DateProduct
                                {
                                    CategoryId_ = orderdetailTMP.Product.CatID,
                                    Date = dateSearch.GetEndOfDate(),
                                    Discount = orderdetailTMP.Discount,
                                    FinalAmount = orderdetailTMP.FinalAmount,
                                    Product = orderdetailTMP.Product,
                                    ProductId = orderdetailTMP.ProductID,
                                    ProductName_ = orderdetailTMP.Product.ProductName,
                                    Quantity = orderdetailTMP.Quantity,
                                    StoreID = (int)orderdetailTMP.StoreId,
                                    OrderQuantity = totalOrders.Count(),
                                    TotalAmount = orderdetailTMP.TotalAmount
                                };

                                if (orderdetailTMP.Order.OrderType == (int)OrderTypeEnum.AtStore)
                                {
                                    product.QuantityAtStore = orderdetailTMP.Quantity;
                                    product.QuantityDelivery = product.QuantityDelivery == null ? 0 : product.QuantityDelivery;
                                    product.QuantityTakeAway = product.QuantityTakeAway == null ? 0 : product.QuantityTakeAway;
                                }
                                else if (orderdetailTMP.Order.OrderType == (int)OrderTypeEnum.Delivery)
                                {
                                    product.QuantityDelivery = orderdetailTMP.Quantity;
                                    product.QuantityAtStore = product.QuantityAtStore == null ? 0 : product.QuantityAtStore;
                                    product.QuantityTakeAway = product.QuantityTakeAway == null ? 0 : product.QuantityTakeAway;
                                }
                                else if (orderdetailTMP.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                                {
                                    product.QuantityTakeAway = orderdetailTMP.Quantity;
                                    product.QuantityAtStore = product.QuantityAtStore == null ? 0 : product.QuantityAtStore;
                                    product.QuantityDelivery = product.QuantityDelivery == null ? 0 : product.QuantityDelivery;
                                }

                                reportService.TimeQuantity(orderdetailTMP, product, false);
                                dateProductDbs.Add(product);
                            }
                            else
                            {
                                product.Discount += orderdetailTMP.Discount;
                                product.Quantity += orderdetailTMP.Quantity;
                                product.TotalAmount += orderdetailTMP.TotalAmount;
                                product.FinalAmount += orderdetailTMP.FinalAmount;
                                if (orderdetailTMP.Order.OrderType == (int)OrderTypeEnum.AtStore)
                                {
                                    product.QuantityAtStore += orderdetailTMP.Quantity;
                                }
                                else if (orderdetailTMP.Order.OrderType == (int)OrderTypeEnum.Delivery)
                                {
                                    product.QuantityDelivery += orderdetailTMP.Quantity;
                                }
                                else if (orderdetailTMP.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                                {
                                    product.QuantityTakeAway += orderdetailTMP.Quantity;
                                }

                                reportService.TimeQuantity(orderdetailTMP, product, true);
                            }
                        }

                        #endregion


                        var listDateProducts = dateProductApi.GetDateProductByDateAndStore(dateSearch, item).ToList();
                        foreach (var dateReportProduct in dateProductDbs)
                        {
                            var currentReportProduct = listDateProducts.Where(q => q.ProductId == dateReportProduct.ProductId && q.StoreID == dateReportProduct.StoreID && q.Date == dateReportProduct.Date).FirstOrDefault();
                            if (currentReportProduct != null)
                            {
                                //Update DateProduct
                                currentReportProduct.Discount = dateReportProduct.Discount;
                                currentReportProduct.TotalAmount = dateReportProduct.TotalAmount;
                                currentReportProduct.Quantity = dateReportProduct.Quantity;
                                currentReportProduct.FinalAmount = dateReportProduct.FinalAmount;
                                currentReportProduct.OrderQuantity = dateReportProduct.OrderQuantity;
                                currentReportProduct.QuantityAtStore = dateReportProduct.QuantityAtStore;
                                currentReportProduct.QuantityDelivery = dateReportProduct.QuantityDelivery;
                                currentReportProduct.QuantityTakeAway = dateReportProduct.QuantityTakeAway;
                                currentReportProduct.Time0Quantity = dateReportProduct.Time0Quantity;
                                currentReportProduct.Time10Quantity = dateReportProduct.Time10Quantity;
                                currentReportProduct.Time11Quantity = dateReportProduct.Time11Quantity;
                                currentReportProduct.Time12Quantity = dateReportProduct.Time12Quantity;
                                currentReportProduct.Time13Quantity = dateReportProduct.Time13Quantity;
                                currentReportProduct.Time14Quantity = dateReportProduct.Time14Quantity;
                                currentReportProduct.Time15Quantity = dateReportProduct.Time15Quantity;
                                currentReportProduct.Time16Quantity = dateReportProduct.Time16Quantity;
                                currentReportProduct.Time17Quantity = dateReportProduct.Time17Quantity;
                                currentReportProduct.Time18Quantity = dateReportProduct.Time18Quantity;
                                currentReportProduct.Time19Quantity = dateReportProduct.Time19Quantity;
                                currentReportProduct.Time1Quantity = dateReportProduct.Time1Quantity;
                                currentReportProduct.Time20Quantity = dateReportProduct.Time20Quantity;
                                currentReportProduct.Time21Quantity = dateReportProduct.Time21Quantity;
                                currentReportProduct.Time22Quantity = dateReportProduct.Time22Quantity;
                                currentReportProduct.Time23Quantity = dateReportProduct.Time23Quantity;
                                currentReportProduct.Time2Quantity = dateReportProduct.Time2Quantity;
                                currentReportProduct.Time3Quantity = dateReportProduct.Time3Quantity;
                                currentReportProduct.Time4Quantity = dateReportProduct.Time4Quantity;
                                currentReportProduct.Time5Quantity = dateReportProduct.Time5Quantity;
                                currentReportProduct.Time6Quantity = dateReportProduct.Time6Quantity;
                                currentReportProduct.Time7Quantity = dateReportProduct.Time7Quantity;
                                currentReportProduct.Time8Quantity = dateReportProduct.Time8Quantity;
                                currentReportProduct.Time9Quantity = dateReportProduct.Time9Quantity;
                                dateProductApi.BaseService.Update(currentReportProduct);
                            }
                            else
                            {
                                // Add row moi vao data
                                dateProductApi.BaseService.Create(dateReportProduct);
                            }
                        }
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }

    public class CheckReport
    {
        public int STT { get; set; }
        public string StoreName { get; set; }
        public int TotalOrder { get; set; }
        public double RevenueWithoutDiscount { get; set; }
        public double Revenue { get; set; }
        public double Discount { get; set; }
        public double TotalCash { get; set; }
        public double DiscountOrderDetail { get; set; }
        public bool Status { get; set; }
        public int StoreId { get; set; }
    }
}
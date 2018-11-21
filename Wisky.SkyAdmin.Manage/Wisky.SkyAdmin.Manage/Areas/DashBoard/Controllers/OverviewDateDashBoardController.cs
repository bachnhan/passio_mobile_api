using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.DashBoard.Controllers
{
    [Authorize(Roles = "BrandManager , StoreManager")]
    public class OverviewDateDashBoardController : DomainBasedController
    {
        // GET: DashBoard/OverviewDateDashBoard
        public ActionResult Index()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            return View();
        }

        public JsonResult GetDataDashBoard(string dateSearch, int storeId, int brandId)
        {
            //Init ResultData
            var result = new OverviewDashboard();

            //Init Api
            var orderApi = new OrderApi();
            var dateReportApi = new DateReportApi();
            var paymentApi = new PaymentApi();
            var productApi = new ProductApi();
            var dateProductApi = new DateProductApi();

            //ConfigDatetime 
            var today = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            var checkToday = false;
            if (!dateSearch.Equals(today) || Utils.GetCurrentDateTime().GetEndOfDate() < dateSearch.ToDateTime().GetEndOfDate())
            {
                checkToday = false;
            }
            else
            {
                checkToday = true;
            }
            var startDate = dateSearch.ToDateTime().GetStartOfDate();
            var endDate = dateSearch.ToDateTime().GetEndOfDate();


            var resultPayment = new OverviewPaymentDashboard();

            #region Process Data
            if (checkToday) // Lấy dữ liệu hôm nay.
            {

                result.TotalOrder = orderApi.CountOrderInStore(storeId, startDate, endDate, brandId);
                result.TotalOrderAtStore = orderApi.CountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.AtStore, brandId);
                result.TotalOrderTakeAway = orderApi.CountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.TakeAway, brandId);
                result.TotalOrderDelivery = orderApi.CountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.Delivery, brandId);
                result.TotalOrderCard = orderApi.CountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.OrderCard, brandId);
                result.TotalOrderPreCancel = orderApi.CountOrderCancelInStore(storeId, startDate, endDate, (int)OrderStatusEnum.PreCancel, brandId);
                result.TotalOrderAfterCancel = orderApi.CountOrderInStoreByType(storeId, startDate, endDate, (int)OrderStatusEnum.Cancel, brandId);

                result.TotalRevenueAtStore = result.TotalOrderAtStore > 0 ? (int)orderApi.TotalAmountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.AtStore, brandId) : 0;
                result.TotalRevenueTakeAway = result.TotalOrderTakeAway > 0 ? (int)orderApi.TotalAmountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.TakeAway, brandId) : 0;
                result.TotalRevenueDelivery = result.TotalOrderDelivery > 0 ? (int)orderApi.TotalAmountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.Delivery, brandId) : 0;
                result.TotalRevenueOrderCard = result.TotalOrderCard > 0 ? (int)orderApi.TotalAmountOrderInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.OrderCard, brandId) : 0;


                result.TotalRevenue = orderApi.TotalRevenueOrderInStore(storeId, startDate, endDate, brandId);
                result.TotalRevenueWithDiscount = result.TotalOrder > 0 ? orderApi.TotalWithoutDiscountInStore(storeId, startDate, endDate, brandId) : 0;
                result.TotalRevenueCard = result.TotalOrderCard > 0 ? result.TotalRevenueOrderCard : 0;
                result.TotalDiscount = result.TotalRevenueWithDiscount - result.TotalRevenue;
                result.TotalRevenueWithoutDiscountAndCard = result.TotalRevenue - result.TotalRevenueCard;
                result.TotalRevenueWithoutCard = result.TotalRevenue - result.TotalRevenueCard;
                result.TotalRevenuePrecancel = result.TotalOrderPreCancel > 0 ? orderApi.TotalAmounttOrderCancelInStore(storeId, startDate, endDate, (int)OrderStatusEnum.PreCancel, brandId) : 0;
                result.TotalRevenueAftercancel = result.TotalOrderAfterCancel > 0 ? orderApi.TotalAmounttOrderCancelInStore(storeId, startDate, endDate, (int)OrderStatusEnum.Cancel, brandId) : 0;

                result.AvgRevenueOrder = (result.TotalOrder - result.TotalOrderCard) > 0 ? result.TotalRevenueWithoutDiscountAndCard / (result.TotalOrder - result.TotalOrderCard) : 0;
                result.AvgProductOrder = Math.Round((result.TotalOrder - result.TotalOrderCard) > 0 ? orderApi.TotalQuantityItem(storeId, startDate, endDate, brandId) / (result.TotalOrder - result.TotalOrderCard) : 0.0, 2);
                result.AvgProductOrderAtStore = Math.Round(result.TotalOrderAtStore > 0 ? orderApi.TotalQuantityInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.AtStore, brandId) / result.TotalOrderAtStore : 0.0, 2);
                result.AvgProductOrderTakeAway = Math.Round(result.TotalOrderTakeAway > 0 ? orderApi.TotalQuantityInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.TakeAway, brandId) / result.TotalOrderTakeAway : 0.0, 2);
                result.AvgProductOrderDelivery = Math.Round(result.TotalOrderDelivery > 0 ? orderApi.TotalQuantityInStoreByType(storeId, startDate, endDate, (int)OrderTypeEnum.Delivery, brandId) / result.TotalOrderDelivery : 0.0, 2);
            }
            else
            {
                var dateReport = dateReportApi.GetAllDateReportByTimeRange(startDate, endDate, storeId, brandId).AsEnumerable();
                List<DateProduct> dateProduct = new List<DateProduct>();
                if (storeId != 0)
                {
                    dateProduct = dateProductApi.GetDateProductByTimeRangeAndStore(startDate, endDate, storeId).Where(q => q.Product.ProductType != (int)ProductTypeEnum.CardPayment).ToList();
                }
                else
                {
                    dateProduct = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId).ToList();
                }
                if (dateReport.Count() > 0)
                {
                    result.TotalOrder = dateReport.Sum(q => q.TotalOrder);
                    result.TotalOrderDelivery = dateReport.Sum(q => q.TotalOrderDelivery);
                    result.TotalOrderAtStore = dateReport.Sum(q => q.TotalOrderAtStore);
                    result.TotalOrderTakeAway = dateReport.Sum(q => q.TotalOrderTakeAway);
                    result.TotalOrderCard = dateReport.Sum(q => q.TotalOrderCard);
                    result.TotalOrderAfterCancel = dateReport.Where(q => q.TotalOrderCanceled.HasValue).Count() > 0 ? dateReport.Where(q => q.TotalOrderCanceled.HasValue).Sum(q => q.TotalOrderCanceled.Value) : 0;
                    result.TotalOrderPreCancel = dateReport.Where(q => q.TotalOrderPreCanceled.HasValue).Count() > 0 ? dateReport.Where(q => q.TotalOrderPreCanceled.HasValue).Sum(q => q.TotalOrderPreCanceled.Value) : 0;


                    result.TotalRevenueAtStore = (int)dateReport.Sum(q => q.FinalAmountAtStore.GetValueOrDefault());
                    result.TotalRevenueTakeAway = (int)dateReport.Sum(q => q.FinalAmountTakeAway.GetValueOrDefault());
                    result.TotalRevenueDelivery = (int)dateReport.Sum(q => q.FinalAmountDelivery.GetValueOrDefault());
                    result.TotalRevenueOrderCard = (int)dateReport.Sum(q => q.FinalAmountCard.GetValueOrDefault());


                    result.TotalRevenue = (int)dateReport.Sum(q => q.FinalAmount.GetValueOrDefault());
                    result.TotalRevenueWithDiscount = (int)dateReport.Sum(q => q.FinalAmount.GetValueOrDefault()) + dateReport.Sum(q => q.Discount.GetValueOrDefault()) + dateReport.Sum(q => q.DiscountOrderDetail.GetValueOrDefault());
                    result.TotalRevenueCard = (int)dateReport.Sum(q => q.FinalAmountCard.GetValueOrDefault());
                    result.TotalDiscount = dateReport.Sum(q => q.Discount.GetValueOrDefault()) + dateReport.Sum(q => q.DiscountOrderDetail.GetValueOrDefault());
                    result.TotalRevenueWithoutDiscountAndCard = dateReport.Sum(q => q.FinalAmount.GetValueOrDefault()) - dateReport.Sum(q => q.FinalAmountCard.GetValueOrDefault());
                    result.TotalRevenueWithoutCard = dateReport.Sum(q => q.FinalAmount.GetValueOrDefault()) - dateReport.Sum(q => q.FinalAmountCard.GetValueOrDefault());
                    result.TotalRevenuePrecancel = dateReport.Sum(q => q.FinalAmountPreCanceled.GetValueOrDefault());
                    result.TotalRevenueAftercancel = dateReport.Sum(q => q.FinalAmountCanceled.GetValueOrDefault());

                    result.AvgRevenueOrder = (result.TotalOrder - result.TotalOrderCard) > 0 ? result.TotalRevenueWithoutDiscountAndCard / (result.TotalOrder - result.TotalOrderCard) : 0;
                    result.AvgProductOrder = Math.Round((result.TotalOrder - result.TotalOrderCard) > 0 ? (double)(dateProduct.Count() > 0 ? dateProduct.Sum(q => q.OrderQuantity.GetValueOrDefault()) : 0) / (result.TotalOrder - result.TotalOrderCard) : 0.0, 2);
                    result.AvgProductOrderAtStore = Math.Round(result.TotalOrderAtStore > 0 ? (double)(dateProduct.Count() > 0 ? dateProduct.Sum(q => q.QuantityAtStore.GetValueOrDefault()) : 0) / result.TotalOrderAtStore : 0.0, 2);
                    result.AvgProductOrderTakeAway = Math.Round(result.TotalOrderTakeAway > 0 ? (double)(dateProduct.Count() > 0 ? dateProduct.Sum(q => q.QuantityTakeAway.GetValueOrDefault()) : 0) / result.TotalOrderTakeAway : 0.0, 2);
                    result.AvgProductOrderDelivery = Math.Round(result.TotalOrderDelivery > 0 ? (double)(dateProduct.Count() > 0 ? dateProduct.Sum(q => q.QuantityDelivery.GetValueOrDefault()) : 0) / result.TotalOrderDelivery : 0.0, 2);
                }
            }
            #endregion

            resultPayment.TotalTransactionPayment = paymentApi.CountPaymentByTimeRange(storeId, startDate, endDate, brandId);
            resultPayment.TotalTransactionPaymentBank = paymentApi.CountTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.MasterCard, brandId) + paymentApi.CountTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.VisaCard, brandId);
            resultPayment.TotalTransactionPaymentCard = paymentApi.CountTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.MemberPayment, brandId);
            //Momo +  Gift talk
            resultPayment.TotalTransactionPaymentE_Wallet_Momo = paymentApi.CountTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.MoMo, brandId);
            resultPayment.TotalTransactionPaymentE_Wallet_GiftTalk = paymentApi.CountTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.GiftTalk, brandId);
            resultPayment.TotalTransactionPaymentE_Wallet = resultPayment.TotalTransactionPaymentE_Wallet_Momo + resultPayment.TotalTransactionPaymentE_Wallet_GiftTalk;
            var transPaymentCash = paymentApi.CountTotalPaymentByTimeRangeWithPaySale(storeId, startDate, endDate, brandId);
            resultPayment.TotalTransactionPaymentForSales = transPaymentCash;
            resultPayment.TotalTransactionPaymentOther = paymentApi.CountTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.Other, brandId);
            resultPayment.TotalTransactionPaymentBuyCard = paymentApi.CountTotalPaymentByTimeRangeWithPayCard(storeId, startDate, endDate, brandId);

            resultPayment.TotalPayment = resultPayment.TotalTransactionPayment > 0 ? paymentApi.SumTotalPaymentByTimeRange(storeId, startDate, endDate, brandId) : 0;
            resultPayment.TotalPaymentForSales = (transPaymentCash > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPaySale(storeId, startDate, endDate, brandId) : 0);
            resultPayment.TotalPaymentCard = resultPayment.TotalTransactionPaymentCard > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.MemberPayment, brandId) : 0;
            resultPayment.TotalPaymentBank = resultPayment.TotalTransactionPaymentBank > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.MasterCard, brandId) + paymentApi.SumTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.VisaCard, brandId) : 0;
            resultPayment.TotalPaymentE_Wallet_Momo = paymentApi.SumTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.MoMo, brandId);
            resultPayment.TotalPaymentE_Wallet_GiftTalk = paymentApi.SumTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.GiftTalk, brandId);

            resultPayment.TotalPaymentE_Wallet = resultPayment.TotalTransactionPaymentE_Wallet > 0 ? resultPayment.TotalPaymentE_Wallet_Momo + resultPayment.TotalPaymentE_Wallet_GiftTalk : 0;
            resultPayment.TotalPaymentOther = resultPayment.TotalTransactionPaymentOther > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayType(storeId, startDate, endDate, (int)PaymentTypeEnum.Other, brandId) : 0;
            resultPayment.TotalPaymentBuyCard = resultPayment.TotalTransactionPaymentBuyCard > 0 ? paymentApi.SumTotalPaymentByTimeRangeWithPayCard(storeId, startDate, endDate, brandId) : 0;

            return Json(new { OverviewData = result, PaymentData = resultPayment }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDataStore(string dateSearch, int storeId, int BrandId)
        {

            var today = Utils.GetCurrentDateTime();

            var startDate = dateSearch.ToDateTime().GetStartOfDate();
            var endDate = dateSearch.ToDateTime().GetEndOfDate();

            var startDateToday = today.GetStartOfDate();
            var endDateToday = today.GetEndOfDate();

            var checkToday = false;
            if (startDate <= today && endDate >= today)
            {
                checkToday = true;
            }
            else
            {
                checkToday = false;
            }

            var dateReportApi = new DateReportApi();
            var dateProductApi = new DateProductApi();
            var orderApi = new OrderApi();
            var storeApi = new StoreApi();
            var result = new List<StoreDetailDashboard>();

            var listStore = storeApi.GetStoresByBrandId(BrandId).Select(q => new { StoreName = q.ShortName, ID = q.ID }).ToList();
            var dateReports = dateReportApi.GetAllDateReportByTimeRange(startDate, endDate, storeId, BrandId).AsEnumerable().GroupBy(q => q.StoreID);

            var dateProduct = dateProductApi.GetDateProductByTimeRangeAndBrand(startDate, endDate, BrandId).GroupBy(q => q.StoreID);
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

            var tmpTotalQuantity = dateProduct.Select(q => new
            {
                StoreId = q.Key,
                TotalQuantity = q.Count() > 0 ? q.Sum(k => k.Quantity) : 0,
            });

            if (checkToday)
            {
                var orders = orderApi.OrderGroup(storeId, startDateToday, endDateToday, BrandId).GroupBy(q => q.StoreID);
                var tmpDataToday = orders.Select(q => new
                {
                    StoreId = q.Key,
                    FinalAmount = q.Count() > 0 ? q.Sum(i => i.FinalAmount) : 0,
                    FinalCard = q.Where(k => k.OrderType == (int)OrderTypeEnum.OrderCard).Count() > 0 ? q.Where(k => k.OrderType == (int)OrderTypeEnum.OrderCard).Sum(k => k.FinalAmount) : 0,
                    TotalOrder = q.Count(),
                    TotalOrderCard = q.Where(k => k.OrderType == (int)OrderTypeEnum.OrderCard).Count(),
                    TotalOrderTakeAway = q.Where(k => k.OrderType == (int)OrderTypeEnum.TakeAway).Count(),
                    TotalOrderDelivery = q.Where(k => k.OrderType == (int)OrderTypeEnum.Delivery).Count(),
                    TotalQuantity = q.Sum(k => k.OrderDetailsTotalQuantity.Value)
                }).ToList();
                foreach (var item in tmpDataReport)
                {
                    var tmpData = new StoreDetailDashboard();
                    var toDateData = tmpDataToday.FirstOrDefault(q => q.StoreId == item.StoreId);
                    var quantityData = tmpTotalQuantity.FirstOrDefault(q => q.StoreId == item.StoreId);

                    tmpData.StoreName = storeApi.Get(item.StoreId).ShortName;
                    tmpData.TotalOrder = item.TotalOrder + (toDateData != null ? toDateData.TotalOrder : 0);
                    tmpData.TotalOrderCard = item.TotalOrderCard + (toDateData != null ? toDateData.TotalOrderCard : 0);
                    tmpData.TotalProduct = (quantityData != null ? quantityData.TotalQuantity : 0) + (toDateData != null ? toDateData.TotalQuantity : 0);
                    tmpData.TotalRevenueAll = (int)item.FinalAmount + (int)(toDateData != null ? toDateData.FinalAmount : 0);
                    tmpData.TotalRevenueCard = (int)item.FinalCard + (int)(toDateData != null ? toDateData.FinalCard : 0);
                    tmpData.TotalRevenue = tmpData.TotalRevenueAll - tmpData.TotalRevenueCard;
                    result.Add(tmpData);
                }
            }
            else
            {
                foreach (var item in tmpDataReport)
                {
                    var tmpData = new StoreDetailDashboard();
                    var quantityData = tmpTotalQuantity.FirstOrDefault(q => q.StoreId == item.StoreId);
                    tmpData.StoreName = storeApi.Get(item.StoreId).ShortName;
                    tmpData.TotalOrder = item.TotalOrder;
                    tmpData.TotalOrderCard = item.TotalOrderCard;
                    tmpData.TotalProduct = (quantityData != null ? quantityData.TotalQuantity : 0);
                    tmpData.TotalRevenueAll = (int)item.FinalAmount;
                    tmpData.TotalRevenueCard = (int)item.FinalCard;
                    tmpData.TotalRevenue = tmpData.TotalRevenueAll - tmpData.TotalRevenueCard;
                    result.Add(tmpData);
                }
            }
            var count = 1;
            var returnResult = result.Select(q => new IConvertible[] {
                count++,
                q.StoreName,
                q.TotalOrder - q.TotalOrderCard,
                q.TotalOrderCard,
                q.TotalProduct,
                q.TotalRevenue,
                q.TotalRevenueCard,
                q.TotalRevenueAll
            }).OrderByDescending(q => q[7]);
            return Json(new { data = returnResult }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CashierData(string dateSearch,int selectedStore, int brandId)
        {
            var paymentApi = new PaymentApi();
            var aspNetUserApi = new AspNetUserApi();

            var fromDate = dateSearch.ToDateTime().GetStartOfDate();
            var toDate = dateSearch.ToDateTime().GetEndOfDate();
            var totalModel = paymentApi.PaymentByTimeRange(selectedStore, fromDate, toDate, brandId)
                .GroupBy(a => a.Cashier);
            var totalDisplay = totalModel.Count();
            var cashiersData = totalModel
            .Select(a => new
            {
                Name = a.Key,
                TotalCash = a.Where(q => q.Type == (int)PaymentTypeEnum.Cash || q.Type == (int)PaymentTypeEnum.ExchangeCash).Count() > 0 ? a.Where(q => q.Type == (int)PaymentTypeEnum.Cash || q.Type == (int)PaymentTypeEnum.ExchangeCash).Sum(q => q.Amount) : 0,
                TotalCard = a.Where(q => q.Type == (int)PaymentTypeEnum.MemberPayment).Count() > 0 ? a.Where(q => q.Type == (int)PaymentTypeEnum.MemberPayment).Sum(q => q.Amount) : 0,
                TotalE_Wallet = a.Where(q => q.Type == (int)PaymentTypeEnum.MoMo).Count() > 0 ? a.Where(q => q.Type == (int)PaymentTypeEnum.MoMo).Sum(q => q.Amount) : 0,
                TotalBank = a.Where(q => q.Type == (int)PaymentTypeEnum.MasterCard || q.Type == (int)PaymentTypeEnum.VisaCard).Count() > 0 ? a.Where(q => q.Type == (int)PaymentTypeEnum.MasterCard || q.Type == (int)PaymentTypeEnum.VisaCard).Sum(q => q.Amount) : 0,
                TotalOther = a.Where(q => q.Type == (int)PaymentTypeEnum.Other).Count() > 0 ? a.Where(q => q.Type == (int)PaymentTypeEnum.Other).Sum(q => q.Amount) : 0,
            }).ToList();

            var result = cashiersData.Select(q => new IConvertible[] {
                    q.Name,
                    q.TotalCash,
                    q.TotalCard,
                    q.TotalE_Wallet,
                    q.TotalBank,
                    q.TotalOther,
                });
            return Json(new
            {
                aaData = result,
            }, JsonRequestBehavior.AllowGet);
        }//End CashierData

        public JsonResult ProductData(string dateSearch, int selectedStore, int BrandId)
        {
            var orderDetailApi = new OrderDetailApi();
            var dateProductApi = new DateProductApi();

            var startDate = dateSearch.ToDateTime().GetStartOfDate();
            var endDate = dateSearch.ToDateTime().GetEndOfDate();
            var today = Utils.GetCurrentDateTime().GetEndOfDate();

            var totalProductsRecords = dateProductApi.GetQueryDateProductForDashBoard(startDate, endDate, BrandId, selectedStore);// store = 0 lay theo brand

            var dateProducts = totalProductsRecords.GroupBy(q => q.ProductID);
            var totalQuery = dateProducts.Count();

            var queryProducts = dateProducts.ToList();

            var queryResult = queryProducts
                .Select(q => new DashboardProductDataViewModel
                {
                    productId = q.Key,
                    productName = q.Select(a => a.ProductName).FirstOrDefault(),
                    totalQty = q.Sum(a => a.TotalQuantity),
                    totalAmount = q.Sum(a => a.TotalAmount),
                    finalAmount = q.Sum(a => a.FinalAmount)
                }).ToList();

            if (endDate == today)
            {
                if (startDate.GetEndOfDate() != endDate)
                {
                    var queryResultIDs = queryResult.Select(q => q.productId);
                    foreach (var productId in queryResultIDs)
                    {
                        var todayProductResult = queryResult.Where(q => q.productId == productId).FirstOrDefault();
                        IEnumerable<TodayOrderDetail> todayOrdersByProduct;
                        if (selectedStore == 0) //lay theo brand
                        {
                            todayOrdersByProduct = orderDetailApi.GetTodayOrderDetailByProduct(BrandId, productId);
                        }
                        else
                        {
                            todayOrdersByProduct = orderDetailApi.GetStoreTodayOrderDetailByProduct(selectedStore, productId);
                        }
                        todayProductResult.totalQty += todayOrdersByProduct.Sum(q => q.TotalOrderDetails);
                        todayProductResult.totalAmount += todayOrdersByProduct.Sum(q => q.TotalAmount);
                        todayProductResult.finalAmount += todayOrdersByProduct.Sum(q => q.FinalAmount);
                    }
                    var totalTodayProductIDs = orderDetailApi.GetQueryDateOrderDetails(today, BrandId, selectedStore)
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
                            if (selectedStore == 0) //lay theo brand
                            {
                                todayOrdersByProduct = orderDetailApi.GetTodayOrderDetailByProduct(BrandId, productId);
                            }
                            else
                            {
                                todayOrdersByProduct = orderDetailApi.GetStoreTodayOrderDetailByProduct(selectedStore, productId);
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
                                aaData = new List<DateProductForDashBoard>()
                            }, JsonRequestBehavior.AllowGet);
                        }

                        queryProducts = dateProducts.OrderByDescending(q => q.Sum(a => a.TotalQuantity))
                                            .ToList();

                        queryResult = queryProducts
                            .Select(q => new DashboardProductDataViewModel
                            {
                                productId = q.Key,
                                productName = q.Select(a => a.ProductName).FirstOrDefault(),
                                totalQty = q.Sum(a => a.TotalQuantity),
                                totalAmount = q.Sum(a => a.TotalAmount),
                                finalAmount = q.Sum(a => a.FinalAmount)
                            }).ToList();
                    }

                }
                else
                {
                    var todayOrderDetails = orderDetailApi.GetAllTotalDate(startDate, endDate, selectedStore, BrandId).GroupBy(q => q.ProductID).ToDictionary(q => q.Key, q => q.ToList());// store = 0 lay theo brand
                    var productApi = new ProductApi();
                    var products = productApi.BaseService.GetActiveByBrandId(BrandId).ToList();

                    totalQuery = todayOrderDetails.Keys.Count();

                    var listTmp = new List<DashboardProductDataViewModel>();
                    var dataFinal = todayOrderDetails.ToList();
                    totalQuery = dataFinal.Count();

                    foreach (var item in todayOrderDetails.Keys)
                    {
                        var tmpData = new DashboardProductDataViewModel();
                        tmpData.productId = item;
                        tmpData.productName = products.FirstOrDefault(a => item == a.ProductID).ProductName;
                        tmpData.totalQty = todayOrderDetails[item].Sum(q => q.TotalOrderDetails);
                        tmpData.totalAmount = todayOrderDetails[item].Sum(q => q.TotalAmount);
                        tmpData.finalAmount = todayOrderDetails[item].Sum(q => q.FinalAmount);
                        queryResult.Add(tmpData);
                    }
                    queryResult.OrderBy(q => q.finalAmount);
                }
            }

            int count = 1;
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
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
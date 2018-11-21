using HmsService.Models.Entities.Repositories;
using HmsService.ViewModels;
using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HmsService.Models.Entities.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Get Total Cost of month.
        /// </summary>
        double GetCostOfMonth(DateTime nowDate);


        /// <summary>
        /// Get Total Order of month.
        /// </summary>
        int GetOrderOfMonth(DateTime nowDate);


        /// <summary>
        /// Get Total Payment of month.
        /// </summary>
        double GetPaymentOfMonth(DateTime nowDate);


        /// <summary>
        /// Get Total soldProduct of month.
        /// </summary>
        int GetSoldProductOfMonth(DateTime nowDate);


        /// <summary>
        /// Sum quantity each product of month.
        /// </summary>
        List<ProductSoldModel> GetAllProductSoldMonth(DateTime nowDate);


        /// <summary>
        /// Sum Quantity each product in top 5 and bot 5 of month.
        /// </summary>
        List<ProductSoldModel> GetTopBotTenProductMonth(DateTime nowDate);


        int GetSoldProductOfMonthByCategory(DateTime nowDate, int cateId);


        /// <summary>
        /// Get Total soldProduct of date.
        /// </summary>
        int GetSoldProductOfDate(DateTime nowDate);


        /// <summary>
        /// Get Total Order of date.
        /// </summary>
        int GetOrderOfDate(DateTime nowDate);


        /// <summary>
        /// Get Total Payment of date.
        /// </summary>
        double GetPaymentOfDate(DateTime nowDate);


        /// <summary>
        /// Get Total Cost of date.
        /// </summary>
        double GetCostOfDate(DateTime nowDate);


        /// <summary>
        /// Sum quantity each product of date.
        /// </summary>
        List<ProductSoldModel> GetAllProductSold(DateTime nowDate);


        /// <summary>
        /// Sum Quantity each product in top 5 and bot 5 of date.
        /// </summary>
        List<ProductSoldModel> GetTopBotTenProduct(DateTime nowDate);


        int GetSoldProductOfDateByCategory(DateTime nowDate, int cateId);

        bool CreateDateReport(DateReport dateReport, IEnumerable<OrderDetail> orderDetails, IEnumerable<DateProductItem> dateProductItems,
            Store store, List<Order> orders, IEnumerable<InventoryReceipt> inventoryReceipts, PaymentReport paymentReport, string currentUser);

        bool ReCreateReportDate(DateReport dateReport, IEnumerable<OrderDetail> orderDetails, IEnumerable<DateProductItem> dateProductItems,
            Store store, IEnumerable<Order> orders);

        bool ReUpdateDateReport(DateReport oldDateReport, DateReport dateReport, List<DateProduct> listDateProduct, IEnumerable<OrderDetail> orderDetails, 
            Store store, List<Order> orders, string currentUser);

        bool UpdateDateReportOnly(DateReport oldDateReport, DateReport dateReport, PaymentReport oldPaymentReport, PaymentReport paymentReport, Store store);

        void SaveChange();

        bool ReRunDateReportRangeTime(DateTime startTime, DateTime endTime, int storeId);
        bool CheckExistDateReport(DateTime reportDate, int storeId);
        void TimeQuantity(OrderDetail orderDetail, DateProduct dateProduct, bool exist);
    }
    public class ReportService : IReportService
    {
        #region Field
        private readonly ICostService _costService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;
        private readonly IInventoryDateReportService _inventoryDateReportService;
        private readonly IProductItemService _productItemService;
        //Repository
        private readonly IDateReportRepository _dateReportRepository;
        private readonly IDateProductRepository _dateProductRepository;
        private readonly IDateProductItemRepository _dateProductItemRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IOrderRepository _rentRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInventoryReceiptRepository _iInventoryReceiptRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IPaymentReportRepository _paymentReportRepository;
        private readonly IUnitOfWork _unitOfWork;


        #endregion

        #region Ctor
        public ReportService(ICostService costService, IProductItemService productItemService, IOrderService orderService, IOrderDetailService orderDetailService, IInventoryDateReportService inventoryDateReportService,
            IProductService productService, IDateReportRepository dateReportRepository, IDateProductRepository dateProductRepository,
            IDateProductItemRepository dateProductItemRepository, IStoreRepository storeRepository, IOrderRepository orderRepository, IUnitOfWork unitOfWork,
            IPaymentRepository paymentRepository, IInventoryReceiptRepository iInventoryReceiptRepository, IOrderDetailRepository orderDetailRepository, IPaymentReportRepository paymentReportRepository)
        {
            _costService = costService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _productService = productService;
            _dateReportRepository = dateReportRepository;
            _dateProductRepository = dateProductRepository;
            _dateProductItemRepository = dateProductItemRepository;
            _storeRepository = storeRepository;
            _rentRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _iInventoryReceiptRepository = iInventoryReceiptRepository;
            _inventoryDateReportService = inventoryDateReportService;
            _productItemService = productItemService;
            _orderDetailRepository = orderDetailRepository;
            _paymentReportRepository = paymentReportRepository;
            _unitOfWork = unitOfWork;
        }
        #endregion

        public double GetCostOfMonth(DateTime nowDate)
        {
            double cost = (from costDatenow in _costService.GetCosts()
                           where costDatenow.CostDate.Month.Equals(nowDate.Month)
                           && costDatenow.CostDate.Year.Equals(nowDate.Year)
                           select costDatenow.Amount).ToList().Sum();
            return cost;
        }

        public int GetOrderOfMonth(DateTime nowDate)
        {
            int order = (from orderNowDate in _orderService.GetOrders().Where(c => c.RentStatus != (int)RentStatusEnum.DeletePermanent
                && c.RentStatus != (int)RentStatusEnum.Disabled
                && c.RentStatus != (int)RentStatusEnum.WaitDisabled)
                         where orderNowDate.CheckInDate.Value.Month.Equals(nowDate.Month)
                         && orderNowDate.CheckInDate.Value.Year.Equals(nowDate.Year)
                         select orderNowDate).Count();
            return order;
        }

        public double GetPaymentOfMonth(DateTime nowDate)
        {
            double payment = (from payDateNow in _orderService.GetOrders().Where(c => c.RentStatus != (int)RentStatusEnum.DeletePermanent
                && c.RentStatus != (int)RentStatusEnum.Disabled
                && c.RentStatus != (int)RentStatusEnum.WaitDisabled)
                              where payDateNow.CheckInDate.Value.Month.Equals(nowDate.Month)
                                   && payDateNow.CheckInDate.Value.Year.Equals(nowDate.Year)
                              select payDateNow.FinalAmount).ToList().Sum();

            return payment;
        }

        public int GetSoldProductOfMonth(DateTime nowDate)
        {
            int soldProduct = (from orderDetail in _orderDetailService.GetOrderDetails().Where(c => c.Order.OrderStatus != (int)RentStatusEnum.Disabled
                && c.Order.OrderStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.OrderStatus != (int)RentStatusEnum.DeletePermanent)
                               where orderDetail.OrderDate.Month.Equals(nowDate.Month)
                               && orderDetail.OrderDate.Year.Equals(nowDate.Year)
                               select orderDetail.Quantity).ToList().Sum();
            return soldProduct;
        }

        public List<ProductSoldModel> GetAllProductSoldMonth(DateTime nowDate)
        {
            List<ProductSoldModel> totalProductSold = new List<ProductSoldModel>();
            var result = (from temp in _orderDetailService.GetOrderDetails().Where(c => c.Order.OrderStatus != (int)RentStatusEnum.Disabled
                && c.Order.OrderStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.OrderStatus != (int)RentStatusEnum.DeletePermanent)
                          where temp.OrderDate.Month.Equals(nowDate.Month)
                          && temp.OrderDate.Year.Equals(nowDate.Year)
                          group temp by temp.Product.ProductID
                              into g
                          select new
                          {
                              ProductID = g.Key,
                              TotalQuantity = g.Sum(temp => temp.Quantity)
                          }).OrderByDescending(i => i.TotalQuantity);

            foreach (var temp in result)
            {
                ProductSoldModel productSold = new ProductSoldModel
                {
                    ProductName = "",
                    Quantity = temp.TotalQuantity,
                    ProductImage = ""
                };

                //var resultImage = (from tempImage in _productService.GetProducts()
                //                   where tempImage.ProductName.Equals(temp.ProductName)
                //                   select tempImage.PicURL).FirstOrDefault();
                //productSold.ProductImage = resultImage;

                var product = _productService.GetProductById(temp.ProductID);
                if (product != null)
                {
                    productSold.ProductName = product.ProductName;
                    productSold.ProductImage = product.PicURL;
                }
                totalProductSold.Add(productSold);
            }
            return totalProductSold;
        }

        public List<ProductSoldModel> GetTopBotTenProductMonth(DateTime nowDate)
        {
            List<ProductSoldModel> totalProductSold = new List<ProductSoldModel>();

            var result = (from temp in _orderDetailService.GetOrderDetails().Where(c => c.Order.OrderStatus != (int)RentStatusEnum.Disabled
                && c.Order.OrderStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.OrderStatus != (int)RentStatusEnum.DeletePermanent)
                          where temp.OrderDate.Month.Equals(nowDate.Month)
                          && temp.OrderDate.Year.Equals(nowDate.Year)
                          group temp by temp.Product.ProductName
                              into g
                          select new
                          {
                              ProductName = g.Key,
                              TotalQuantity = g.Sum(temp => temp.Quantity)
                          }).ToList();

            var resultTop5 = result.OrderByDescending(i => i.TotalQuantity).Take(5);

            var resultBot5 = result.OrderBy(i => i.TotalQuantity).Take(5);

            foreach (var temp in resultTop5)
            {
                ProductSoldModel productSold = new ProductSoldModel
                {
                    ProductName = temp.ProductName,
                    Quantity = temp.TotalQuantity
                };
                totalProductSold.Add(productSold);
            }

            foreach (var temp in resultBot5)
            {
                ProductSoldModel productSold = new ProductSoldModel
                {
                    ProductName = temp.ProductName,
                    Quantity = temp.TotalQuantity
                };
                totalProductSold.Add(productSold);
            }

            return totalProductSold;
        }

        public int GetSoldProductOfMonthByCategory(DateTime nowDate, int cateId)
        {
            int soldProduct = (from orderDetail in _orderDetailService.GetOrderDetails().Where(c => c.Order.OrderStatus != (int)RentStatusEnum.Disabled
                && c.Order.OrderStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.OrderStatus != (int)RentStatusEnum.DeletePermanent)
                               where orderDetail.Product.CatID == cateId
                               && orderDetail.OrderDate.Month.Equals(nowDate.Month)
                               && orderDetail.OrderDate.Year.Equals(nowDate.Year)
                               select orderDetail).Count();
            return soldProduct;
        }

        public int GetSoldProductOfDate(DateTime nowDate)
        {
            int soldProduct = (from orderDetail in _orderDetailService.GetOrderDetails() where orderDetail.OrderDate.Date.Equals(nowDate.Date) select orderDetail.Quantity).ToList().Sum();
            return soldProduct;
        }

        public int GetOrderOfDate(DateTime nowDate)
        {
            int order = (from orderNowDate in _orderService.GetOrders() where orderNowDate.CheckInDate.Value.Date.Equals(nowDate.Date) select orderNowDate).Count();
            return order;
        }

        public double GetPaymentOfDate(DateTime nowDate)
        {
            double payment =
                (from payDateNow in _orderService.GetOrders()
                 where payDateNow.CheckInDate.Value.Date.Equals(nowDate.Date)
                 select payDateNow.FinalAmount).ToList().Sum();

            return payment;
        }

        public double GetCostOfDate(DateTime nowDate)
        {
            double cost = (from costDatenow in _costService.GetCosts()
                           where costDatenow.CostDate.Date.Equals(nowDate.Date)
                           select costDatenow.Amount).ToList().Sum();
            return cost;
        }

        public List<ProductSoldModel> GetAllProductSold(DateTime nowDate)
        {
            List<ProductSoldModel> totalProductSold = new List<ProductSoldModel>();
            var orders = _orderDetailService.GetOrderDetails().Where(c => c.OrderDate.Day.Equals(nowDate.Day)
                && c.OrderDate.Month.Equals(nowDate.Month) && c.OrderDate.Year.Equals(nowDate.Year))
                .Where(c => c.Order.OrderStatus != (int)RentStatusEnum.Disabled && c.Order.OrderStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.OrderStatus != (int)RentStatusEnum.DeletePermanent);

            var result = (from temp in orders
                          group temp by temp.Product.ProductID
                              into g
                          select new
                          {
                              Id = g.Key,
                              TotalQuantity = g.Sum(temp => temp.Quantity)
                          }).OrderByDescending(i => i.TotalQuantity);

            foreach (var temp in result.ToList())
            {
                ProductSoldModel productSold = new ProductSoldModel
                {
                    ProductName = "None",
                    Quantity = temp.TotalQuantity,
                    ProductImage = ""
                };

                //var resultImage = (from tempImage in _productService.GetProducts()
                //                   where tempImage.ProductName.Equals(temp.ProductName)
                //                   select tempImage.PicURL).FirstOrDefault();
                //productSold.ProductImage = resultImage;

                var product = _productService.GetProductById(temp.Id);
                if (product != null)
                {
                    productSold.ProductName = product.ProductName;
                    productSold.ProductImage = product.PicURL;
                }
                totalProductSold.Add(productSold);
            }
            return totalProductSold;
        }

        public List<ProductSoldModel> GetTopBotTenProduct(DateTime nowDate)
        {
            List<ProductSoldModel> totalProductSold = new List<ProductSoldModel>();

            var result = from temp in _orderDetailService.GetOrderDetails().Where(c => c.Order.OrderStatus != (int)RentStatusEnum.Disabled
                && c.Order.OrderStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.OrderStatus != (int)RentStatusEnum.DeletePermanent)
                         where temp.OrderDate.Date.Equals(nowDate.Date)
                         group temp by temp.Product.ProductName
                             into g
                         select new
                         {
                             ProductName = g.Key,
                             TotalQuantity = g.Sum(temp => temp.Quantity)
                         };

            var resultTop5 = result.OrderByDescending(i => i.TotalQuantity).Take(5);

            var resultBot5 = result.OrderBy(i => i.TotalQuantity).Take(5);

            foreach (var temp in resultTop5)
            {
                ProductSoldModel productSold = new ProductSoldModel
                {
                    ProductName = temp.ProductName,
                    Quantity = temp.TotalQuantity
                };
                totalProductSold.Add(productSold);
            }

            foreach (var temp in resultBot5)
            {
                ProductSoldModel productSold = new ProductSoldModel
                {
                    ProductName = temp.ProductName,
                    Quantity = temp.TotalQuantity
                };
                totalProductSold.Add(productSold);
            }

            return totalProductSold;
        }

        public int GetSoldProductOfDateByCategory(DateTime nowDate, int cateId)
        {
            //var soldProduct = from orderDetail in _orderDetailService.GetOrderDetails()
            //                  where orderDetail.Product.CatID == cateId &&
            //                        orderDetail.OrderDate.Date.Equals(nowDate.Date)
            //                  group orderDetail by orderDetail.Product.ProductName
            //                      into g
            //                      select new
            //                      {
            //                          ProductName = g.Key,
            //                          TotalQuantity = g.Sum(temp => temp.Quantity)
            //                      };
            //int result = 0;
            //foreach (var temp in soldProduct)
            //{
            //    result += temp.TotalQuantity;
            //}
            int result = 0;
            var details = _orderDetailService.GetOrderDetails().Where(c => c.OrderDate.Day.Equals(nowDate.Day)
                && c.OrderDate.Month.Equals(nowDate.Month) && c.Product.CatID == cateId).Where(c => c.Order.OrderStatus != (int)RentStatusEnum.Disabled
                    && c.Order.OrderStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.OrderStatus != (int)RentStatusEnum.DeletePermanent);
            if (details != null) result = details.Sum(c => c.Quantity);
            return result;
        }

        public bool CreateDateReport(DateReport dateReport, IEnumerable<OrderDetail> orderDetails, IEnumerable<DateProductItem> dateProductItems,
            Store store, List<Order> orders, IEnumerable<InventoryReceipt> inventoryReceipts, PaymentReport paymentReport, string currentUser)
        {
            try
            {

                Stopwatch sw = new Stopwatch();
                sw.Start();
                var reportDate = store.ReportDate.Value.AddDays(1);

                if (!this.CheckExistDateReport(dateReport.Date, dateReport.StoreID))
                {
                    //Create Inventory
                    CreateInventory(currentUser, store.ID, dateProductItems, inventoryReceipts, dateReport.Date);

                    _dateReportRepository.Add(dateReport);
                    _paymentReportRepository.Add(paymentReport);

                    #region DateProduct
                    var dateProductDbs = new List<DateProduct>();
                    foreach (var item in orderDetails)
                    {

                        var product = dateProductDbs.FirstOrDefault(a => a.ProductId == item.ProductID);
                        if (product == null)
                        {
                            //var countRent = rents.GetMany(a => a.OrderDetails.Any(b => b.ProductID == item.ProductID)).Count();
                            var totalOrders = orders.Where(a => a.OrderDetails.Any(b => b.ProductID == item.ProductID));
                            var totalOrderAtstore = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                            var totalOrderDelivery = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                            var totalOrderTakeAway = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                            product = new DateProduct
                            {
                                CategoryId_ = item.Product.CatID,
                                Date = reportDate,
                                Discount = item.Discount,
                                FinalAmount = item.FinalAmount,
                                Product = item.Product,
                                ProductId = item.ProductID,
                                ProductName_ = item.Product.ProductName,
                                Quantity = item.Quantity,
                                StoreID = (int)item.StoreId,
                                OrderQuantity = totalOrders.Count(),
                                TotalAmount = item.TotalAmount
                            };

                            if (item.Order.OrderType == (int)OrderTypeEnum.AtStore)
                            {
                                product.QuantityAtStore = item.Quantity;
                                product.QuantityDelivery = product.QuantityDelivery == null ? 0 : product.QuantityDelivery;
                                product.QuantityTakeAway = product.QuantityTakeAway == null ? 0 : product.QuantityTakeAway;
                            }
                            else if (item.Order.OrderType == (int)OrderTypeEnum.Delivery)
                            {
                                product.QuantityDelivery = item.Quantity;
                                product.QuantityAtStore = product.QuantityAtStore == null ? 0 : product.QuantityAtStore;
                                product.QuantityTakeAway = product.QuantityTakeAway == null ? 0 : product.QuantityTakeAway;
                            }
                            else if (item.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                            {
                                product.QuantityTakeAway = item.Quantity;
                                product.QuantityAtStore = product.QuantityAtStore == null ? 0 : product.QuantityAtStore;
                                product.QuantityDelivery = product.QuantityDelivery == null ? 0 : product.QuantityDelivery;
                            }

                            TimeQuantity(item, product, false);
                            dateProductDbs.Add(product);
                        }
                        else
                        {
                            product.Discount += item.Discount;
                            product.Quantity += item.Quantity;
                            product.TotalAmount += item.TotalAmount;
                            product.FinalAmount += item.FinalAmount;
                            if (item.Order.OrderType == (int)OrderTypeEnum.AtStore)
                            {
                                product.QuantityAtStore += item.Quantity;
                            }
                            else if (item.Order.OrderType == (int)OrderTypeEnum.Delivery)
                            {
                                product.QuantityDelivery += item.Quantity;
                            }
                            else if (item.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                            {
                                product.QuantityTakeAway += item.Quantity;
                            }

                            TimeQuantity(item, product, true);
                        }
                    }

                    foreach (var item in dateProductDbs)
                    {
                        _dateProductRepository.Add(item);
                    }

                    #endregion
                    #region DateProductItem
                    foreach (var item in dateProductItems)
                    {
                        _dateProductItemRepository.Add(item);
                    }

                    #endregion
                }
                store.ReportDate = reportDate;
                _storeRepository.Edit(store);
                SaveChange();
                Debug.WriteLine("Save date product item: {0}", sw.ElapsedMilliseconds);
                sw.Stop();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool ReCreateReportDate(DateReport dateReport, IEnumerable<OrderDetail> orderDetails, IEnumerable<DateProductItem> dateProductItems,
            Store store, IEnumerable<Order> orders)
        {
            try
            {
                #region DateProduct
                var dateProductDbs = new List<DateProduct>();
                foreach (var item in orderDetails)
                {
                    var product = dateProductDbs.FirstOrDefault(a => a.ProductId == item.ProductID);
                    if (product == null)
                    {
                        //var countRent = rents.GetMany(a => a.OrderDetails.Any(b => b.ProductID == item.ProductID)).Count();
                        var totalOrders = orders.Where(a => a.OrderDetails.Any(b => b.ProductID == item.ProductID));
                        var totalOrderAtstore = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                        var totalOrderDelivery = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                        var totalOrderTakeAway = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                        product = new DateProduct
                        {
                            CategoryId_ = item.Product.CatID,
                            Date = dateReport.Date.GetEndOfDate(),
                            Discount = item.Discount,
                            FinalAmount = item.FinalAmount,
                            Product = item.Product,
                            ProductId = item.ProductID,
                            ProductName_ = item.Product.ProductName,
                            Quantity = item.Quantity,
                            StoreID = (int)item.StoreId,
                            OrderQuantity = totalOrders.Count(),
                            TotalAmount = item.TotalAmount
                        };

                        if (item.Order.OrderType == (int)OrderTypeEnum.AtStore)
                        {
                            product.QuantityAtStore = item.Quantity;
                            product.QuantityDelivery = product.QuantityDelivery == null ? 0 : product.QuantityDelivery;
                            product.QuantityTakeAway = product.QuantityTakeAway == null ? 0 : product.QuantityTakeAway;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.Delivery)
                        {
                            product.QuantityDelivery = item.Quantity;
                            product.QuantityAtStore = product.QuantityAtStore == null ? 0 : product.QuantityAtStore;
                            product.QuantityTakeAway = product.QuantityTakeAway == null ? 0 : product.QuantityTakeAway;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                        {
                            product.QuantityTakeAway = item.Quantity;
                            product.QuantityAtStore = product.QuantityAtStore == null ? 0 : product.QuantityAtStore;
                            product.QuantityDelivery = product.QuantityDelivery == null ? 0 : product.QuantityDelivery;
                        }

                        TimeQuantity(item, product, false);
                        dateProductDbs.Add(product);
                    }
                    else
                    {
                        product.Discount += item.Discount;
                        product.Quantity += item.Quantity;
                        product.TotalAmount += item.TotalAmount;
                        product.FinalAmount += item.FinalAmount;
                        if (item.Order.OrderType == (int)OrderTypeEnum.AtStore)
                        {
                            product.QuantityAtStore += item.Quantity;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.Delivery)
                        {
                            product.QuantityDelivery += item.Quantity;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                        {
                            product.QuantityTakeAway += item.Quantity;
                        }

                        TimeQuantity(item, product, true);
                    }
                }

                foreach (var item in dateProductDbs)
                {
                    _dateProductRepository.Add(item);
                }

                #endregion
                #region DateProductItem
                foreach (var item in dateProductItems)
                {
                    _dateProductItemRepository.Add(item);
                }

                #endregion

                store.ReportDate = dateReport.Date.GetEndOfDate();
                _dateReportRepository.Add(dateReport);
                _storeRepository.Edit(store);
                SaveChange();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool ReUpdateDateReport(DateReport oldDateReport, DateReport dateReport, List<DateProduct> listDateProduct, IEnumerable<OrderDetail> orderDetails,
            Store store, List<Order> orders, string currentUser)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var reportDate = dateReport.Date.GetEndOfDate();

                //Edit datereport
                if (oldDateReport == null)
                {
                    _dateReportRepository.Add(dateReport);
                }
                else
                {
                    //oldDateReport.Date = dateReport.Date;
                    //oldDateReport.StoreID = dateReport.StoreID;
                    oldDateReport.CreateBy = dateReport.CreateBy;
                    oldDateReport.Status = dateReport.Status;
                    oldDateReport.TotalAmount = dateReport.TotalAmount;
                    oldDateReport.FinalAmount = dateReport.FinalAmount;
                    oldDateReport.Discount = dateReport.Discount;
                    oldDateReport.DiscountOrderDetail = dateReport.DiscountOrderDetail;
                    oldDateReport.TotalCash = dateReport.TotalCash;
                    oldDateReport.TotalOrder = dateReport.TotalOrder;
                    oldDateReport.TotalOrderAtStore = dateReport.TotalOrderAtStore;
                    oldDateReport.TotalOrderTakeAway = dateReport.TotalOrderTakeAway;
                    oldDateReport.TotalOrderDelivery = dateReport.TotalOrderDelivery;
                    oldDateReport.TotalOrderDetail = dateReport.TotalOrderDetail;
                    oldDateReport.TotalOrderFeeItem = dateReport.TotalOrderFeeItem;
                    oldDateReport.FinalAmountAtStore = dateReport.FinalAmountTakeAway;
                    oldDateReport.FinalAmountTakeAway = dateReport.FinalAmountTakeAway;
                    oldDateReport.FinalAmountDelivery = dateReport.FinalAmountDelivery;
                    oldDateReport.FinalAmountCard = dateReport.FinalAmountCard;
                    oldDateReport.TotalOrderCanceled = dateReport.TotalOrderCanceled;
                    oldDateReport.TotalOrderPreCanceled = dateReport.TotalOrderPreCanceled;
                    oldDateReport.FinalAmountCanceled = dateReport.FinalAmountCanceled;
                    oldDateReport.FinalAmountPreCanceled = dateReport.FinalAmountPreCanceled;
                    _dateReportRepository.Edit(oldDateReport);
                }

                #region DateProduct
                var dateProductDbs = new List<DateProduct>();
                foreach (var item in orderDetails)
                {
                    var dproduct = dateProductDbs.FirstOrDefault(a => a.ProductId == item.ProductID);
                    if (dproduct == null)
                    {
                        //var countRent = rents.GetMany(a => a.OrderDetails.Any(b => b.ProductID == item.ProductID)).Count();
                        var totalOrders = orders.Where(a => a.OrderDetails.Any(b => b.ProductID == item.ProductID));
                        var totalOrderAtstore = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                        var totalOrderDelivery = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                        var totalOrderTakeAway = totalOrders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                        dproduct = new DateProduct
                        {
                            CategoryId_ = item.Product.CatID,
                            Date = reportDate.GetEndOfDate(),
                            Discount = item.Discount,
                            FinalAmount = item.FinalAmount,
                            Product = item.Product,
                            ProductId = item.ProductID,
                            ProductName_ = item.Product.ProductName,
                            Quantity = item.Quantity,
                            StoreID = (int)item.StoreId,
                            OrderQuantity = totalOrders.Count(),
                            TotalAmount = item.TotalAmount
                        };

                        if (item.Order.OrderType == (int)OrderTypeEnum.AtStore)
                        {
                            dproduct.QuantityAtStore = item.Quantity;
                            dproduct.QuantityDelivery = dproduct.QuantityDelivery == null ? 0 : dproduct.QuantityDelivery;
                            dproduct.QuantityTakeAway = dproduct.QuantityTakeAway == null ? 0 : dproduct.QuantityTakeAway;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.Delivery)
                        {
                            dproduct.QuantityDelivery = item.Quantity;
                            dproduct.QuantityAtStore = dproduct.QuantityAtStore == null ? 0 : dproduct.QuantityAtStore;
                            dproduct.QuantityTakeAway = dproduct.QuantityTakeAway == null ? 0 : dproduct.QuantityTakeAway;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                        {
                            dproduct.QuantityTakeAway = item.Quantity;
                            dproduct.QuantityAtStore = dproduct.QuantityAtStore == null ? 0 : dproduct.QuantityAtStore;
                            dproduct.QuantityDelivery = dproduct.QuantityDelivery == null ? 0 : dproduct.QuantityDelivery;
                        }

                        TimeQuantity(item, dproduct, false);
                        dateProductDbs.Add(dproduct);
                    }
                    else
                    {
                        dproduct.Discount += item.Discount;
                        dproduct.Quantity += item.Quantity;
                        dproduct.TotalAmount += item.TotalAmount;
                        dproduct.FinalAmount += item.FinalAmount;
                        if (item.Order.OrderType == (int)OrderTypeEnum.AtStore)
                        {
                            dproduct.QuantityAtStore += item.Quantity;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.Delivery)
                        {
                            dproduct.QuantityDelivery += item.Quantity;
                        }
                        else if (item.Order.OrderType == (int)OrderTypeEnum.TakeAway)
                        {
                            dproduct.QuantityTakeAway += item.Quantity;
                        }

                        TimeQuantity(item, dproduct, true);
                    }
                }

                //Edit date product
                foreach (var item in dateProductDbs)
                {
                    var oldDateProduct = listDateProduct.FirstOrDefault(dp => dp.ProductId == item.ProductId);
                    if (oldDateProduct == null)
                    {
                        _dateProductRepository.Add(item);
                    }
                    else
                    {
                        oldDateProduct.Date = item.Date;
                        //oldDateProduct.ProductId = oldDateProduct.ProductId;
                        //oldDateProduct.StoreID = oldDateProduct.StoreID;
                        //oldDateProduct.CategoryId_ = oldDateProduct.CategoryId_;
                        oldDateProduct.Quantity = item.Quantity;
                        oldDateProduct.TotalAmount = item.TotalAmount;
                        oldDateProduct.Discount = item.Discount;
                        oldDateProduct.FinalAmount = item.FinalAmount;
                        oldDateProduct.ProductName_ = item.ProductName_;
                        oldDateProduct.OrderQuantity = item.OrderQuantity;
                        oldDateProduct.QuantityAtStore = item.QuantityAtStore;
                        oldDateProduct.QuantityTakeAway = item.QuantityTakeAway;
                        oldDateProduct.QuantityDelivery = item.QuantityDelivery;
                        oldDateProduct.Time0Quantity = item.Time0Quantity;
                        oldDateProduct.Time1Quantity = item.Time1Quantity;
                        oldDateProduct.Time2Quantity = item.Time2Quantity;
                        oldDateProduct.Time3Quantity = item.Time3Quantity;
                        oldDateProduct.Time4Quantity = item.Time4Quantity;
                        oldDateProduct.Time5Quantity = item.Time5Quantity;
                        oldDateProduct.Time6Quantity = item.Time6Quantity;
                        oldDateProduct.Time7Quantity = item.Time7Quantity;
                        oldDateProduct.Time8Quantity = item.Time8Quantity;
                        oldDateProduct.Time9Quantity = item.Time9Quantity;
                        oldDateProduct.Time10Quantity = item.Time10Quantity;
                        oldDateProduct.Time11Quantity = item.Time11Quantity;
                        oldDateProduct.Time12Quantity = item.Time12Quantity;
                        oldDateProduct.Time13Quantity = item.Time13Quantity;
                        oldDateProduct.Time14Quantity = item.Time14Quantity;
                        oldDateProduct.Time15Quantity = item.Time15Quantity;
                        oldDateProduct.Time16Quantity = item.Time16Quantity;
                        oldDateProduct.Time17Quantity = item.Time17Quantity;
                        oldDateProduct.Time18Quantity = item.Time18Quantity;
                        oldDateProduct.Time19Quantity = item.Time19Quantity;
                        oldDateProduct.Time20Quantity = item.Time20Quantity;
                        oldDateProduct.Time21Quantity = item.Time21Quantity;
                        oldDateProduct.Time22Quantity = item.Time22Quantity;
                        oldDateProduct.Time23Quantity = item.Time23Quantity;

                        _dateProductRepository.Edit(oldDateProduct);
                    }
                }
                #endregion

                SaveChange();
                Debug.WriteLine("Save date product item: {0}", sw.ElapsedMilliseconds);
                sw.Stop();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }



        public void SaveChange()
        {
            _unitOfWork.Commit();
        }

        public void TimeQuantity(OrderDetail orderDetail, DateProduct dateProduct, bool exist)
        {
            if (orderDetail.OrderDate.Hour == 0)
            {
                if (exist == false)
                {
                    dateProduct.Time0Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time0Quantity == null)
                    {
                        dateProduct.Time0Quantity = 0;
                    }
                    dateProduct.Time0Quantity += orderDetail.Quantity;
                }

            }
            else if (orderDetail.OrderDate.Hour == 1)
            {
                if (exist == false)
                {
                    dateProduct.Time1Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time1Quantity == null)
                    {
                        dateProduct.Time1Quantity = 0;
                    }
                    dateProduct.Time1Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 2)
            {
                if (exist == false)
                {
                    dateProduct.Time2Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time2Quantity == null)
                    {
                        dateProduct.Time2Quantity = 0;
                    }
                    dateProduct.Time2Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 3)
            {
                if (exist == false)
                {
                    dateProduct.Time3Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time3Quantity == null)
                    {
                        dateProduct.Time3Quantity = 0;
                    }
                    dateProduct.Time3Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 4)
            {
                if (exist == false)
                {
                    dateProduct.Time4Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time4Quantity == null)
                    {
                        dateProduct.Time4Quantity = 0;
                    }
                    dateProduct.Time4Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 5)
            {
                if (exist == false)
                {
                    dateProduct.Time5Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time5Quantity == null)
                    {
                        dateProduct.Time5Quantity = 0;
                    }
                    dateProduct.Time5Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 6)
            {
                if (exist == false)
                {
                    dateProduct.Time6Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time6Quantity == null)
                    {
                        dateProduct.Time6Quantity = 0;
                    }
                    dateProduct.Time6Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 7)
            {
                if (exist == false)
                {
                    dateProduct.Time7Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time7Quantity == null)
                    {
                        dateProduct.Time7Quantity = 0;
                    }
                    dateProduct.Time7Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 8)
            {
                if (exist == false)
                {
                    dateProduct.Time8Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time8Quantity == null)
                    {
                        dateProduct.Time8Quantity = 0;
                    }
                    dateProduct.Time8Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 9)
            {
                if (exist == false)
                {
                    dateProduct.Time9Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time9Quantity == null)
                    {
                        dateProduct.Time9Quantity = 0;
                    }
                    dateProduct.Time9Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 10)
            {
                if (exist == false)
                {
                    dateProduct.Time10Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time10Quantity == null)
                    {
                        dateProduct.Time10Quantity = 0;
                    }
                    dateProduct.Time10Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 11)
            {
                if (exist == false)
                {
                    dateProduct.Time11Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time11Quantity == null)
                    {
                        dateProduct.Time11Quantity = 0;
                    }
                    dateProduct.Time11Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 12)
            {
                if (exist == false)
                {
                    dateProduct.Time12Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time12Quantity == null)
                    {
                        dateProduct.Time12Quantity = 0;
                    }
                    dateProduct.Time12Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 13)
            {
                if (exist == false)
                {
                    dateProduct.Time13Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time13Quantity == null)
                    {
                        dateProduct.Time13Quantity = 0;
                    }
                    dateProduct.Time13Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 14)
            {
                if (exist == false)
                {
                    dateProduct.Time14Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time14Quantity == null)
                    {
                        dateProduct.Time14Quantity = 0;
                    }
                    dateProduct.Time14Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 15)
            {
                if (exist == false)
                {
                    dateProduct.Time15Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time15Quantity == null)
                    {
                        dateProduct.Time15Quantity = 0;
                    }
                    dateProduct.Time15Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 16)
            {
                if (exist == false)
                {
                    dateProduct.Time16Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time16Quantity == null)
                    {
                        dateProduct.Time16Quantity = 0;
                    }
                    dateProduct.Time16Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 17)
            {
                if (exist == false)
                {
                    dateProduct.Time17Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time17Quantity == null)
                    {
                        dateProduct.Time17Quantity = 0;
                    }
                    dateProduct.Time17Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 18)
            {
                if (exist == false)
                {
                    dateProduct.Time18Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time18Quantity == null)
                    {
                        dateProduct.Time18Quantity = 0;
                    }
                    dateProduct.Time18Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 19)
            {
                if (exist == false)
                {
                    dateProduct.Time19Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time19Quantity == null)
                    {
                        dateProduct.Time19Quantity = 0;
                    }
                    dateProduct.Time19Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 20)
            {
                if (exist == false)
                {
                    dateProduct.Time20Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time20Quantity == null)
                    {
                        dateProduct.Time20Quantity = 0;
                    }
                    dateProduct.Time20Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 21)
            {
                if (exist == false)
                {
                    dateProduct.Time21Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time21Quantity == null)
                    {
                        dateProduct.Time21Quantity = 0;
                    }
                    dateProduct.Time21Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 22)
            {
                if (exist == false)
                {
                    dateProduct.Time22Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time22Quantity == null)
                    {
                        dateProduct.Time22Quantity = 0;
                    }
                    dateProduct.Time22Quantity += orderDetail.Quantity;
                }
            }
            else if (orderDetail.OrderDate.Hour == 23)
            {
                if (exist == false)
                {
                    dateProduct.Time23Quantity = orderDetail.Quantity;
                }
                else
                {
                    if (dateProduct.Time23Quantity == null)
                    {
                        dateProduct.Time23Quantity = 0;
                    }
                    dateProduct.Time23Quantity += orderDetail.Quantity;
                }
            }
        }

        private void CreateInventory(string currentUser, int storeId, IEnumerable<DateProductItem> dateProductItems,
            IEnumerable<InventoryReceipt> inventoryReceipts, DateTime dateReport)
        {
            try
            {
                #region Inventory

                InventoryDateReport reportForInventory = new InventoryDateReport()
                {
                    CreateDate = dateReport,
                    Creator = currentUser,//currenUser.Name,
                    StoreId = storeId,
                    Status = 1,
                };

                //Lấy ra list Item 
                var productItems = _productItemService.GetAvailableProductItems();
                var reportItems = new List<InventoryDateReportItem>();

                var lastInventoryDateReport = _inventoryDateReportService.GetLastReport(storeId);

                //Kiểm tra lastReport có tồn tại hay không

                if (lastInventoryDateReport != null)
                {
                    if (lastInventoryDateReport.CreateDate == dateReport)
                    {
                        return;
                    }
                    foreach (var item in productItems)
                    {
                        var lastItem = lastInventoryDateReport.InventoryDateReportItems.FirstOrDefault(q => q.ItemID == item.ItemID);
                        if (lastItem != null)
                        {
                            reportItems.Add(new InventoryDateReportItem
                            {
                                ReportID = reportForInventory.ReportID,
                                Quantity = 0,
                                ItemID = item.ItemID,
                                CancelAmount = 0,
                                ExportAmount = 0,
                                ChangeInventoryAmount = 0,
                                ImportAmount = 0,
                                RealAmount = lastItem.RealAmount ?? 0,
                                ReturnAmount = 0,
                                SoldAmount = 0,
                                TheoryAmount = lastItem.RealAmount ?? 0,
                                TotalExport = 0,
                                TotalImport = 0,
                                ReceivedChangeInventoryAmount = 0,
                                Price = item.Price ?? 0
                            });
                        }
                        else
                        {
                            reportItems.Add(new InventoryDateReportItem
                            {
                                ReportID = reportForInventory.ReportID,
                                Quantity = 0,
                                ItemID = item.ItemID,
                                CancelAmount = 0,
                                ExportAmount = 0,
                                ChangeInventoryAmount = 0,
                                ImportAmount = 0,
                                RealAmount = 0,
                                ReturnAmount = 0,
                                SoldAmount = 0,
                                TheoryAmount = 0,
                                TotalExport = 0,
                                TotalImport = 0,
                                ReceivedChangeInventoryAmount = 0,
                                Price = item.Price ?? 0,
                            });
                        }
                    }

                    //Tính số lượng item bán được trong ngày
                    foreach (var item in dateProductItems)
                    {
                        var DateReportItem = reportItems.FirstOrDefault(a => a.ItemID == item.ProductItemID);
                        if (DateReportItem != null)
                        {
                            DateReportItem.SoldAmount += item.Quantity;
                            DateReportItem.TheoryAmount = DateReportItem.TheoryAmount - item.Quantity;
                            DateReportItem.RealAmount -= item.Quantity;
                            DateReportItem.TotalExport -= item.Quantity;
                        }
                    }


                    if (inventoryReceipts.Count() != 0)
                    {
                        foreach (var receipt in inventoryReceipts)
                        {
                            //Tạm thời đơn hàng sẽ từ trạng thái chờ duyệt, duyệt sẽ chuyển sang trạng thái close
                            if (receipt.Status == (int)InventoryReceiptStatusEnum.New)
                            {
                                receipt.Status = (int)InventoryReceiptStatusEnum.Canceled;
                                _iInventoryReceiptRepository.Edit(receipt);
                            }

                            if (receipt.Status == (int)InventoryReceiptStatusEnum.Approved || receipt.Status == (int)InventoryReceiptStatusEnum.Closed)
                            {
                                switch (receipt.ReceiptType)
                                {
                                    //Tổng nhập
                                    case (int)ReceiptType.InInventory:
                                    case (int)ReceiptType.InChangeInventory:
                                        {
                                            foreach (var receiptItem in receipt.InventoryReceiptItems)
                                            {
                                                var item = reportItems.FirstOrDefault(q => q.ItemID == receiptItem.ItemID);
                                                if (item != null)
                                                {
                                                    item.TotalImport += receiptItem.Quantity;
                                                    //Quantity hiện tại không biết tác dụng
                                                    //item.Quantity += receiptItem.Quantity;
                                                    item.TheoryAmount = (item.RealAmount + receiptItem.Quantity);
                                                    item.RealAmount = item.TheoryAmount;
                                                    item.Price = receiptItem.Price;
                                                    if (receipt.ReceiptType == (int)ReceiptType.InInventory)
                                                    {
                                                        item.ImportAmount += receiptItem.Quantity;
                                                    }
                                                    //Tạm thời commnet( chưa có tác dụng)
                                                    //else
                                                    //{
                                                    //    item.ReceivedChangeInventoryAmount += receiptItem.Quantity;
                                                    //}
                                                }
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            foreach (var receiptItem in receipt.InventoryReceiptItems)
                                            {
                                                var item = reportItems.FirstOrDefault(q => q.ItemID == receiptItem.ItemID);
                                                if (item != null)
                                                {
                                                    if (receipt.InStoreId == null ||
                                                    (receipt.OutStoreId != null && receipt.OutStoreId == storeId))
                                                    {
                                                        item.TotalExport -= receiptItem.Quantity;
                                                        item.Price = receiptItem.Price;
                                                        //Không thấy tác dụng
                                                        //item.Quantity -= receiptItem.Quantity;
                                                        item.TheoryAmount = (item.RealAmount - receiptItem.Quantity);
                                                        item.RealAmount = item.TheoryAmount;
                                                        switch (receipt.ReceiptType)
                                                        {
                                                            //Xuất hủy
                                                            case (int)ReceiptType.DraftInventory:
                                                                {
                                                                    item.CancelAmount += receiptItem.Quantity;
                                                                    break;
                                                                }
                                                            //Chuyển kho đi
                                                            case (int)ReceiptType.OutChangeInventory:
                                                                {
                                                                    item.ChangeInventoryAmount += receiptItem.Quantity;
                                                                    break;
                                                                }
                                                            //Xuất trả
                                                            case (int)ReceiptType.OutInventory:
                                                                {
                                                                    item.ReturnAmount += receiptItem.Quantity;
                                                                    break;
                                                                }
                                                                //case (int)ReceiptType.SoldProduct:
                                                                //    {
                                                                //        item.SoldAmount += receiptItem.Quantity;
                                                                //        break;
                                                                //    }
                                                        }
                                                    }
                                                    //Nhận chuyển kho đến
                                                    else
                                                    {
                                                        //item.TotalImport += receiptItem.Quantity;
                                                        //item.Quantity += receiptItem.Quantity;
                                                        //Số lý thuyết
                                                        item.Price = receiptItem.Price;
                                                        item.TheoryAmount = (item.RealAmount + receiptItem.Quantity);
                                                        //Số thực tế
                                                        item.RealAmount = item.TheoryAmount;
                                                        //Số chuyển kho đến
                                                        item.ReceivedChangeInventoryAmount += receiptItem.Quantity;
                                                    }
                                                }

                                            }
                                            break;
                                        }
                                }
                            }
                            if (receipt.Status == (int)InventoryReceiptStatusEnum.Approved)
                            {
                                if (receipt.InStoreId == null || (receipt?.InStoreId == storeId))
                                {
                                    receipt.Status = (int)InventoryReceiptStatusEnum.Closed;
                                    _iInventoryReceiptRepository.Edit(receipt);
                                }

                            }
                        }
                    }
                }

                //Trường hợp chưa có lastReport
                else
                {
                    foreach (var item in productItems)
                    {
                        reportItems.Add(new InventoryDateReportItem
                        {
                            ReportID = reportForInventory.ReportID,
                            Quantity = 0,
                            ItemID = item.ItemID,
                            CancelAmount = 0,
                            ExportAmount = 0,
                            ChangeInventoryAmount = 0,
                            ImportAmount = 0,
                            RealAmount = 0,
                            ReturnAmount = 0,
                            SoldAmount = 0,
                            TheoryAmount = 0,
                            TotalExport = 0,
                            TotalImport = 0,
                            ReceivedChangeInventoryAmount = 0,
                            Price = item.Price ?? 0
                        });
                    }
                    //Tính số lượng item bán được trong ngày
                    foreach (var item in dateProductItems)
                    {
                        var DateReportItem = reportItems.FirstOrDefault(a => a.ItemID == item.ProductItemID);

                        DateReportItem.SoldAmount += item.Quantity;
                        DateReportItem.TheoryAmount = DateReportItem.RealAmount - item.Quantity;
                        DateReportItem.RealAmount -= item.Quantity;
                        DateReportItem.TotalExport -= item.Quantity;
                    }

                    if (inventoryReceipts.Count() != 0)
                    {
                        foreach (var receipt in inventoryReceipts)
                        {
                            //Tạm thời đơn hàng sẽ từ trạng thái chờ duyệt, duyệt sẽ chuyển sang trạng thái close
                            if (receipt.Status == (int)InventoryReceiptStatusEnum.New)
                            {
                                receipt.Status = (int)InventoryReceiptStatusEnum.Canceled;
                                _iInventoryReceiptRepository.Edit(receipt);
                            }

                            if (receipt.Status == (int)InventoryReceiptStatusEnum.Approved)
                            {
                                switch (receipt.ReceiptType)
                                {
                                    //Tổng nhập
                                    case (int)ReceiptType.InInventory:
                                    case (int)ReceiptType.InChangeInventory:
                                        {
                                            foreach (var receiptItem in receipt.InventoryReceiptItems)
                                            {
                                                var item = reportItems.FirstOrDefault(q => q.ItemID == receiptItem.ItemID);
                                                if (item != null)
                                                {
                                                    item.TotalImport += receiptItem.Quantity;
                                                    //Quantity hiện tại không biết tác dụng
                                                    //item.Quantity += receiptItem.Quantity;
                                                    item.TheoryAmount = (item.RealAmount + receiptItem.Quantity);
                                                    item.RealAmount = item.TheoryAmount;
                                                    item.Price = receiptItem.Price;
                                                    if (receipt.ReceiptType == (int)ReceiptType.InInventory)
                                                    {
                                                        item.ImportAmount += receiptItem.Quantity;
                                                    }
                                                    //Tạm thời commnet( chưa có tác dụng)
                                                    //else
                                                    //{
                                                    //    item.ReceivedChangeInventoryAmount += receiptItem.Quantity;
                                                    //}
                                                }
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            foreach (var receiptItem in receipt.InventoryReceiptItems)
                                            {
                                                var item = reportItems.FirstOrDefault(q => q.ItemID == receiptItem.ItemID);
                                                if (item != null)
                                                {
                                                    if (receipt.InStoreId == null ||
                                                    (receipt.OutStoreId != null && receipt.OutStoreId == storeId))
                                                    {
                                                        item.TotalExport -= receiptItem.Quantity;
                                                        item.Price = receiptItem.Price;
                                                        //Không thấy tác dụng
                                                        //item.Quantity -= receiptItem.Quantity;
                                                        item.TheoryAmount = (item.RealAmount - receiptItem.Quantity);
                                                        item.RealAmount = item.TheoryAmount;
                                                        switch (receipt.ReceiptType)
                                                        {
                                                            //Xuất hủy
                                                            case (int)ReceiptType.DraftInventory:
                                                                {
                                                                    item.CancelAmount += receiptItem.Quantity;
                                                                    break;
                                                                }
                                                            //Chuyển kho đi
                                                            case (int)ReceiptType.OutChangeInventory:
                                                                {
                                                                    item.ChangeInventoryAmount += receiptItem.Quantity;
                                                                    break;
                                                                }
                                                            //Xuất trả
                                                            case (int)ReceiptType.OutInventory:
                                                                {
                                                                    item.ReturnAmount += receiptItem.Quantity;
                                                                    break;
                                                                }
                                                                //case (int)ReceiptType.SoldProduct:
                                                                //    {
                                                                //        item.SoldAmount += receiptItem.Quantity;
                                                                //        break;
                                                                //    }
                                                        }
                                                    }
                                                    //Nhận chuyển kho đến
                                                    else
                                                    {
                                                        //item.TotalImport += receiptItem.Quantity;
                                                        //item.Quantity += receiptItem.Quantity;
                                                        //Số lý thuyết
                                                        item.Price = receiptItem.Price;
                                                        item.TheoryAmount = (item.RealAmount + receiptItem.Quantity);
                                                        //Số thực tế
                                                        item.RealAmount = item.TheoryAmount;
                                                        //Số chuyển kho đến
                                                        item.ReceivedChangeInventoryAmount += receiptItem.Quantity;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                            if (receipt.Status == (int)InventoryReceiptStatusEnum.Approved)
                            {
                                if (receipt.InStoreId == null || (receipt?.InStoreId == storeId))
                                {
                                    receipt.Status = (int)InventoryReceiptStatusEnum.Closed;
                                    _iInventoryReceiptRepository.Edit(receipt);
                                }
                            }
                        }
                    }
                }
                foreach (var item in reportItems)
                {
                    var soldItem = dateProductItems.FirstOrDefault(q => q.ProductItemID == item.ItemID);
                    //if (soldItem != null)
                    //{
                    //    item.TheoryAmount = (item.RealAmount - soldItem.Quantity);
                    //}
                    item.RealAmount = (item.TheoryAmount < 0) ? 0 : item.TheoryAmount;
                }
                reportForInventory.InventoryDateReportItems = reportItems;
                _inventoryDateReportService.Create(reportForInventory);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool EditDateReport()
        {
            try
            {
                var dateReport = _dateReportRepository.GetActive();
                foreach (var iDateReport in dateReport.ToList())
                {
                    if (iDateReport.TotalOrder == 0)
                    {
                        var orders = _orderService.GetRentsByTimeRange(iDateReport.StoreID, iDateReport.Date.GetStartOfDate(), iDateReport.Date.GetEndOfDate())
                            .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);
                        iDateReport.TotalOrder = orders.Count();
                        iDateReport.TotalOrderAtStore = orders.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore);
                        iDateReport.TotalOrderTakeAway = orders.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway);
                        iDateReport.TotalOrderDelivery = orders.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery);
                        _dateReportRepository.Edit(iDateReport);
                        SaveChange();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public bool EditDateProductReport()
        {
            throw new NotImplementedException();
        }

        public bool ReRunDateReportRangeTime(DateTime startTime, DateTime endTime, int storeId)
        {
            TimeSpan difference = endTime - startTime;

            List<DateTime> days = new List<DateTime>();

            for (int i = 0; i <= difference.Days; i++)
            {
                days.Add(startTime.AddDays(i));
            }

            foreach (var dateTime in days)
            {
                var store = _storeRepository.Get(storeId);

                ////Delete DateReport 
                //var oldDateReport = _dateReportRepository.Get(a => a.StoreID == storeId && a.Date == dateTime).Any(a.Date == dateTime);
                //if (oldDateReport != null)
                //{
                //    _dateReportRepository.Delete(oldDateReport);
                //}

                //Delete DateProduct
                var oldDateProduct = _dateProductRepository.Get(a => a.StoreID == storeId && a.Date == dateTime);
                if (oldDateProduct != null)
                {
                    foreach (var item in oldDateProduct)
                    {
                        _dateProductRepository.Delete(item);
                    }
                }

                //Delete DateProductItem
                var oldDateProductItem = _dateProductItemRepository.Get(a => a.StoreId == storeId && a.Date == dateTime);
                if (oldDateProductItem != null)
                {
                    foreach (var item in oldDateProductItem.ToList())
                    {
                        _dateProductItemRepository.Delete(item);
                    }
                }

                //Get orderDetail
                var orderDetails = _orderDetailRepository
                   .Get(a => a.OrderDate >= dateTime.GetStartOfDate() && a.OrderDate <= dateTime.GetEndOfDate() && a.StoreId == storeId
                   && a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);

                //Get rent
                var rents = _rentRepository.Get(a => a.CheckInDate >= dateTime.GetStartOfDate() && a.CheckInDate <= dateTime.GetEndOfDate() && a.StoreID == storeId
                && a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish);

                //Push DateProduct
                var dateProducts =
                orderDetails.GroupBy(a => a.ProductID)
                    .Join(_productService.GetAllProducts(), a => a.Key, a => a.ProductID, (a, b) => new DateProduct()
                    {
                        ProductId = a.Key,
                        StoreID = store.ID,
                        Quantity = a.Sum(c => c.Quantity),
                        Date = dateTime.GetEndOfDate(),
                        TotalAmount = a.Sum(c => c.TotalAmount),
                        FinalAmount = a.Sum(c => c.FinalAmount),
                        Discount = a.Sum(c => c.Discount),
                        ProductName_ = b.ProductName,
                        Product = b,
                        CategoryId_ = b.ProductCategory.CateID
                    }).ToList();
                //Push DateReport
                var dateReport = new DateReport
                {
                    StoreID = store.ID,
                    CreateBy = "system",
                    Status = (int)DateReportStatusEnum.Approved,
                    Date = dateTime.GetEndOfDate(),
                    DiscountOrderDetail = rents.Sum(a => a.DiscountOrderDetail),
                    TotalAmount = rents.Sum(a => a.TotalAmount),
                    FinalAmount = rents.Sum(a => a.FinalAmount),
                    TotalCash = 0,
                    TotalOrder = rents.Count(),
                    TotalOrderAtStore = rents.Count(a => a.OrderType == (int)OrderTypeEnum.AtStore),
                    TotalOrderTakeAway = rents.Count(a => a.OrderType == (int)OrderTypeEnum.TakeAway),
                    TotalOrderDelivery = rents.Count(a => a.OrderType == (int)OrderTypeEnum.Delivery),
                    TotalOrderDetail = 0,
                    TotalOrderFeeItem = 0
                };
                var compositionsStatistic = dateProducts.SelectMany(a => a.Product.ProductItemCompositionMappings.Select(b => new Tuple<ProductItemCompositionMapping, int>(b, a.Quantity)))
                .GroupBy(a => a.Item1.ItemID);

                //Push DateProductItem
                var dateItemProduct = compositionsStatistic.Join(_productItemService.GetProductItems(), a => a.Key, a => a.ItemID, (a, b) => new DateProductItem
                {
                    StoreId = store.ID,
                    Date = dateTime.GetEndOfDate(),
                    ProductItemID = a.Key,
                    ProductItemName = b.ItemName,
                    Quantity = (int)a.Sum(c => c.Item2 * c.Item1.Quantity),
                    Unit = b.Unit
                }).AsQueryable();


                ReCreateReportDate(dateReport, orderDetails, dateItemProduct, store, rents);
                Console.WriteLine(dateTime);
            }

            throw new NotImplementedException();
        }

        public bool CheckExistDateReport(DateTime reportDate, int storeId)
        {
            var result = _dateReportRepository.Get(q => q.Date == reportDate && q.StoreID == storeId);
            if (result.Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool UpdateDateReportOnly(DateReport oldDateReport, DateReport dateReport, PaymentReport oldPaymentReport, PaymentReport paymentReport, Store store)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var reportDate = dateReport.Date.GetEndOfDate();

                //Edit datereport
                if (oldDateReport == null)
                {
                    _dateReportRepository.Add(dateReport);
                }
                else
                {
                    oldDateReport.CreateBy = dateReport.CreateBy;
                    oldDateReport.Status = dateReport.Status;
                    oldDateReport.TotalAmount = dateReport.TotalAmount;
                    oldDateReport.FinalAmount = dateReport.FinalAmount;
                    oldDateReport.Discount = dateReport.Discount;
                    oldDateReport.DiscountOrderDetail = dateReport.DiscountOrderDetail;
                    oldDateReport.TotalCash = dateReport.TotalCash;
                    oldDateReport.TotalOrder = dateReport.TotalOrder;
                    oldDateReport.TotalOrderAtStore = dateReport.TotalOrderAtStore;
                    oldDateReport.TotalOrderTakeAway = dateReport.TotalOrderTakeAway;
                    oldDateReport.TotalOrderDelivery = dateReport.TotalOrderDelivery;
                    oldDateReport.TotalOrderDetail = dateReport.TotalOrderDetail;
                    oldDateReport.TotalOrderFeeItem = dateReport.TotalOrderFeeItem;
                    oldDateReport.FinalAmountAtStore = dateReport.FinalAmountAtStore;
                    oldDateReport.FinalAmountTakeAway = dateReport.FinalAmountTakeAway;
                    oldDateReport.FinalAmountDelivery = dateReport.FinalAmountDelivery;
                    oldDateReport.FinalAmountCard = dateReport.FinalAmountCard;
                    oldDateReport.TotalOrderCanceled = dateReport.TotalOrderCanceled;
                    oldDateReport.TotalOrderPreCanceled = dateReport.TotalOrderPreCanceled;
                    oldDateReport.FinalAmountCanceled = dateReport.FinalAmountCanceled;
                    oldDateReport.FinalAmountPreCanceled = dateReport.FinalAmountPreCanceled;
                    _dateReportRepository.Edit(oldDateReport);
                }

                if(oldPaymentReport == null)
                {
                    _paymentReportRepository.Add(paymentReport);
                }
                else
                {
                    oldPaymentReport.BankAmount = paymentReport.BankAmount;
                    oldPaymentReport.CashAmount = paymentReport.CashAmount;
                    oldPaymentReport.CreateBy = paymentReport.CreateBy;
                    oldPaymentReport.DebtAmount = paymentReport.DebtAmount;
                    oldPaymentReport.MemberCardAmount = paymentReport.MemberCardAmount;
                    oldPaymentReport.OtherAmount = paymentReport.OtherAmount;
                    oldPaymentReport.Status = paymentReport.Status;
                    oldPaymentReport.VoucherAmount = paymentReport.VoucherAmount;
                    oldPaymentReport.PayDebtAmount = paymentReport.PayDebtAmount;
                    oldPaymentReport.ReceiptAmount = paymentReport.ReceiptAmount;
                    oldPaymentReport.SpendAmount = paymentReport.SpendAmount;
                    _paymentReportRepository.Edit(oldPaymentReport);
                }

                SaveChange();
                Debug.WriteLine("Save date product item: {0}", sw.ElapsedMilliseconds);
                sw.Stop();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public class DateCheckReport
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public bool Status { get; set; }
        public double Revenue { get; set; }
        public double RealRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int RealTotalOrders { get; set; }
        public double TotalPayments { get; set; }
        public double RealTotalPayments { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
    }
}

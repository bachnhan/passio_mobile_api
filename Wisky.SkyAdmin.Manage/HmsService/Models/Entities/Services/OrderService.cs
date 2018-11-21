using SkyWeb.DatVM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.Sdk;
using AutoMapper.QueryableExtensions;
using SkyWeb.DatVM.Mvc.Autofac;
using HmsService.ViewModels;
using System.Transactions;

namespace HmsService.Models.Entities.Services
{
    public partial interface IOrderService
    {
        IQueryable<Order> GetAdminByStoreWithFilter(int storeId,
            string keyword, KeyValuePair<string, bool> sortKeyAsc);
        OrderCustomEntity GetOrderById(int storeId, int id);
        Order GetOrderByIdAndBrandId(int brandId, int id);
        Task<bool> CreateOrderAsync(Order entity);
        IQueryable<Order> GetOrderByBrand(int brandId);
        Task<Order> GetOrderByIdAsync(int id);

        bool CreateOrder(Order order);
        bool CreateOrderTransaction(Order order);
        int CreateOrderReturnId(Order order);

        #region SystemReport
        IQueryable<Order> GetAllOrderByDate(DateTime from, DateTime to, int brandId);
        IQueryable<Order> GetAllFinishedOrderByDate(DateTime from, DateTime to, int brandId);
        IQueryable<Order> GetAllFinishedOrderByDateaAndCustomer(DateTime from, DateTime to, int brandId, int customerID);
        IQueryable<Order> GetAllOrderByDateWithCard(DateTime from, DateTime to, int brandId);
        IQueryable<Order> GetAllOrderByDateByPromotionId(DateTime from, DateTime to, int brandId, int PromotionId);
        IEnumerable<Order> GetAllOrderByDateByPromotionIdAndStoreId(DateTime from, DateTime to, int brandId, int promotionId, int storeId);
        IQueryable<Order> GetAllOrderByDateDifferentFromPromotionId(DateTime from, DateTime to, int brandId, int PromotionId);
        IQueryable<Order> GetTodayOrders(int brandId);
        IQueryable<Order> GetAllOrderByDate(DateTime from, DateTime to);
        IQueryable<Order> GetRentsByTimeRange(int storeID, DateTime startTime, DateTime endTime);
        IQueryable<Order> GetRentsByTimeRange(int storeID, DateTime startTime, DateTime endTime, int brandId);
        IQueryable<Order> GetRentsByTimeRangeAndListStore(List<int> listStoreId, DateTime startTime, DateTime endTime, int brandId);
        IQueryable<Order> GetRentsFinishByTimeRange(int storeID, DateTime startTime, DateTime endTime);
        IQueryable<Order> GetRentsByTime(DateTime startTime, DateTime endTime);
        IQueryable<Order> GetAllOrderByDateByPromotionIdOrderDetail(DateTime from, DateTime to, int brandId, int PromotionId);
        IQueryable<Order> GetRentsByTimeRangeAndHour(int storeID, DateTime startTime, DateTime endTime, int brandId, int startHour, int endHour);

        #endregion
        IEnumerable<Order> GetOrders();
        IQueryable<Order> GetOrdersByTimeRange(int storeID, DateTime startTime, DateTime endTime);
        IQueryable<Order> GetAllHotelOrdersByCheckOutDate(DateTime from, DateTime to, int storeId);

        IQueryable<Order> GetOrdersByTimeRange(DateTime startTime, DateTime endTime, int brandId, int storeId);
        IQueryable<OrderForDashBoard> GetOrderForDashboard(DateTime startTime, DateTime endTime, int brandId, int storeId);
        Task UpdateOrderDeliveryAsync(int orderId, List<ProductWithExtras> listExtra);
        Task UpdateTmpDetailIdOrderDetailAsync(int orderId);
        IQueryable<int> GetRentIdsByTimeRange(int storeID, DateTime startTime, DateTime endTime, int brandId);

        IQueryable<Order> FixDB1();
        IQueryable<Order> FixDB2();
        IQueryable<Order> GetAllDebtOrder(int brandId, int storeId);
        IQueryable<Order> GetAllDebtOrderByBrand(int brandId);
        IQueryable<Order> GetAllDebtOrderByBrandAndCustomerId(int brandId, List<int> listCustomerId);
    }

    public partial class OrderService
    {
        private readonly OrderDetailService detailService;
        public IQueryable<int> GetRentIdsByTimeRange(int storeID, DateTime startTime, DateTime endTime, int brandId)
        {
            return GetRentsByTimeRange(storeID, startTime, endTime, brandId).Select(q => q.RentID);
        }
        public IQueryable<int> GetRentIdsByTimeRangeAndListStore(List<int> listStoreId, DateTime startTime, DateTime endTime, int brandId)
        {
            return GetRentsByTimeRangeAndListStore(listStoreId, startTime, endTime, brandId).Select(q => q.RentID);
        }
        public IQueryable<Order> GetAllHotelOrdersByCheckOutDate(DateTime from, DateTime to, int storeId)
        {
            var rents = this
                .Get(r => ((r.CheckOutDate >= from
                                && r.CheckOutDate <= to && r.StoreID == storeId)));
            return rents;
        }

        public IQueryable<Order> GetOrdersByTimeRange(int storeID, DateTime startTime, DateTime endTime)
        {
            var rents = this.Get(r => (r.StoreID == storeID)
                              && ((r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime)));
            return rents;
        }
        public IQueryable<Order> GetAdminByStoreWithFilter(int storeId,
            string keyword, KeyValuePair<string, bool> sortKeyAsc)
        {
            //var customerApi = new CustomerApi();
            //var customerNames = customerApi.Get().Select(r => r.Name);
            //var entities = this.GetActive(q =>
            //    q.StoreID == storeId &&
            //    (keyword == null
            //    || (!q.CustomerID.HasValue || customerNames.Contains(keyword))));

            var entities = this.GetActive(q =>
                q.StoreID == storeId &&
                (keyword == null
                || (!q.CustomerID.HasValue || q.Customer.Name.Contains(keyword))));

            entities = entities.OrderByDescending(q => q.CheckInDate);

            return entities;
        }

        public OrderCustomEntity GetOrderById(int storeId, int id)
        {
            var order = this.GetActive(a => a.StoreID == storeId && a.RentID == id).FirstOrDefault();

            return new OrderCustomEntity
            {
                Order = order,
                OrderDetails = order.OrderDetails,
                Customer = order.Customer
            };
        }

        public Order GetOrderByIdAndBrandId(int brandId, int id)
        {
            var result = this.Get(q => q.Store.BrandId == brandId && q.RentID == id).FirstOrDefault();
            return result;
        }
       
        public bool CreateOrder(Order order)
        {
            try
            {
                this.Create(order);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool CreateOrderTransaction(Order order)
        {
            try
            {
               using (TransactionScope scope =
               new TransactionScope(TransactionScopeOption.Required,
               new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }))
                {
                   
                    repository.Add(order);
                    repository.Save();

                    UpdateOrderDetailId(order);

                    repository.Edit(order);
                    repository.Save();

                    scope.Complete();
                    return true;
                }
            }
            catch(Exception e)
            {
                return false;
            }
        }
        private void UpdateOrderDetailId(HmsService.Models.Entities.Order order)
        {
            foreach (var orderDetail in order.OrderDetails)
            {
                if (orderDetail.ParentId != null && orderDetail.ParentId > -1)
                {
                    var parentOrderDetail = order.OrderDetails.FirstOrDefault(od => od.TmpDetailId == orderDetail.ParentId);
                    if (parentOrderDetail != null)
                    {
                        orderDetail.ParentId = parentOrderDetail.OrderDetailID;
                    }
                }
                //if (orderDetail.OrderPromotionMappingId != null)
                //{
                //    var mapping = order.OrderPromotionMappings.FirstOrDefault(m => m.TmpMappingId == orderDetail.OrderPromotionMappingId);
                //    if (mapping != null)
                //    {
                //        orderDetail.OrderPromotionMappingId = mapping.Id;
                //    }
                //    else
                //    {
                //        orderDetail.OrderPromotionMappingId = null;
                //    }
                //}
                //if (orderDetail.OrderDetailPromotionMappingId != null)
                //{
                //    OrderDetailPromotionMapping mapping = null;
                //    foreach (var od in order.OrderDetails)
                //    {
                //        mapping = od.OrderDetailPromotionMappings.FirstOrDefault(m => m.TmpMappingId == orderDetail.OrderDetailPromotionMappingId);
                //        if (mapping != null) break;
                //    }
                //    if (mapping != null)
                //    {
                //        orderDetail.OrderDetailPromotionMappingId = mapping.Id;
                //    }
                //    else
                //    {
                //        orderDetail.OrderDetailPromotionMappingId = null;
                //    }
                //}
            }

            //    foreach (var mapping in order.OrderPromotionMappings)
            //    {
            //        mapping.TmpMappingId = mapping.Id;
            //    }
            //    foreach (var orderDetail in order.OrderDetails)
            //    {
            //        foreach (var mapping in orderDetail.OrderDetailPromotionMappings)
            //        {
            //            mapping.TmpMappingId = mapping.Id;
            //        }
            //        orderDetail.TmpDetailId = orderDetail.OrderDetailID;
            //    }
        }
        public int CreateOrderReturnId(Order order)
        {
            try
            {
                this.Create(order);
                return order.RentID;
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        public async Task<bool> CreateOrderAsync(Order entity)
        {
            try
            {
                await this.CreateAsync(entity);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public IQueryable<Order> GetOrderByBrand(int brandId)
        {
            var result = this.Get(q => q.Store.BrandId == brandId && q.OrderType == (int)OrderTypeEnum.Delivery &&
                (q.DeliveryAddress != null || (q.InvoiceID != null)));
            return result;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var result = await this.GetAsync(id);
            return result;
        }

        #region SystemReport
        public IQueryable<Order> GetAllOrderByDate(DateTime from, DateTime to)
        {
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to)));
            return result;
        }

        public IQueryable<Order> GetTodayOrders(int brandId)
        {
            var startDate = DateTime.Now.GetStartOfDate();
            var endDate = DateTime.Now.GetEndOfDate();
            var result = this.Get(r => (r.CheckInDate >= startDate && r.CheckInDate <= endDate && r.Store.BrandId == brandId));
            return result;
        }

        public IQueryable<Order> GetRentsByTimeRangeAndHour(int storeID, DateTime startTime, DateTime endTime, int brandId, int startHour, int endHour)
        {
            var result = Get(q => q.CheckInDate >= startTime && q.CheckInDate <= endTime && q.CheckinHour >= startHour && q.CheckinHour < endHour);
            if (storeID == 0)
            {
                return result.Where(q => q.Store.BrandId == brandId && (bool)q.Store.isAvailable);
            }
            else
            {
                return result.Where(q => q.StoreID == storeID);
            }
        }

        public IQueryable<Order> GetAllOrderByDate(DateTime from, DateTime to, int brandId)
        {
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.Store.BrandId == brandId));
            return result;
        }
        public IQueryable<Order> GetAllFinishedOrderByDate(DateTime from, DateTime to, int brandId)
        {
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.Store.BrandId == brandId && r.OrderStatus == 2));
            return result;
        }
        public IQueryable<Order> GetAllDebtOrder(int brandId, int storeId)
        {
            var result = this.Get(r => r.StoreID == storeId && r.Store.BrandId == brandId && r.PaymentStatus == (int)OrderPaymentStatusEnum.Debt);
            return result;
        }
        public IQueryable<Order> GetAllDebtOrderByBrand(int brandId)
        {
            var result = this.Get(r => r.Store.BrandId == brandId && r.PaymentStatus == (int)OrderPaymentStatusEnum.Debt);
            return result;
        }
        public IQueryable<Order> GetAllDebtOrderByBrandAndCustomerId(int brandId, List<int> listCustomerId)
        {
            if (listCustomerId[0] == 0)
            {
                return this.Get(r => r.Store.BrandId == brandId && r.PaymentStatus == (int)OrderPaymentStatusEnum.Debt);
            }
            else
            {
                return this.Get(r => r.Store.BrandId == brandId && listCustomerId.Contains(r.CustomerID.Value) && r.PaymentStatus == (int)OrderPaymentStatusEnum.Debt);
            }
        }
        public IQueryable<Order> GetAllFinishedOrderByDateaAndCustomer(DateTime from, DateTime to, int brandId, int customerID)
        {
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.Store.BrandId == brandId && r.OrderStatus == 2 && r.CustomerID == customerID));
            return result;
        }
        public IQueryable<Order> GetAllOrderByDateWithCard(DateTime from, DateTime to, int brandId)
        {
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.Store.BrandId == brandId && r.Att1 != null));
            return result;
        }
        public IQueryable<Order> GetAllOrderByDateByPromotionId(DateTime from, DateTime to, int brandId, int PromotionId)
        {
            OrderPromotionMappingApi opApi = new OrderPromotionMappingApi();
            var opMapping = opApi.BaseService.GetActive(q => q.PromotionId == PromotionId).Select(q => q.OrderId);
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.Store.BrandId == brandId) && opMapping.Contains(r.RentID));


            return result;
        }
        public IEnumerable<Order> GetAllOrderByDateByPromotionIdAndStoreId(DateTime from, DateTime to, int brandId, int promotionId, int storeId)
        {
            var opService = DependencyUtils.Resolve<IOrderPromotionMappingService>();
            var opMapping = opService.GetActive(q => q.PromotionId == promotionId).Select(q => new
            {
                OrderId = q.OrderId
            }
            );

            var resultTemp = this.Get(r => ((r.CheckInDate >= from
                             && r.CheckInDate <= to) && r.Store.BrandId == brandId && r.Att1 != null))
                             .Select(r => new { Att1 = r.Att1, RentID = r.RentID, TotalAmount = r.TotalAmount, FinalAmount = r.FinalAmount, Discount = r.Discount, StoreID = r.StoreID })
                             .Join(opMapping, p => p.RentID, q => q.OrderId, (p, q) => p);

            var result = from b in resultTemp.AsEnumerable()
                         select new Order
                         {
                             Att1 = b.Att1,
                             RentID = b.RentID,
                             TotalAmount = b.TotalAmount,
                             FinalAmount = b.FinalAmount,
                             Discount = b.Discount,
                             StoreID = b.StoreID
                         };
            if (storeId > 0)
            {
                result = result.Where(r => r.StoreID == storeId);
            }

            return result;
        }
        public IQueryable<Order> GetAllOrderByDateDifferentFromPromotionId(DateTime from, DateTime to, int brandId, int PromotionId)
        {
            OrderPromotionMappingApi opApi = new OrderPromotionMappingApi();
            var opMapping = opApi.BaseService.GetActive(q => q.PromotionId == PromotionId).Select(q => q.OrderId);
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.Store.BrandId == brandId) && !opMapping.Contains(r.RentID));


            return result;
        }

        public IQueryable<Order> GetRentsByTimeRange(int storeID, DateTime startTime, DateTime endTime)
        {
            var result = this.Get(r => r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime
                                   && r.StoreID == storeID);
            return result;
        }

        public IQueryable<Order> GetOrdersByTimeRange(DateTime startTime, DateTime endTime, int brandId, int storeId)
        {
            var result = this.Get(r => (r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime));
            if (storeId != 0)
            {
                result = result.Where(r => r.StoreID == storeId);
            }
            else
            {
                result = result.Where(r => r.Store.BrandId == brandId && (bool)r.Store.isAvailable);
            }
            return result;
        }


        public IQueryable<OrderForDashBoard> GetOrderForDashboard(DateTime startTime, DateTime endTime, int brandId, int storeId)
        {
            var orders = GetOrdersByTimeRange(startTime, endTime, brandId, storeId);
            return orders.Select(q => new OrderForDashBoard
            {
                StoreID = q.StoreID.Value,
                StoreName = q.Store.Name,
                StoreAbbr = q.Store.ShortName,
                CheckInPerson = q.CheckInPerson,
                Date = q.CheckInDate.Value,
                CheckInHour = q.CheckinHour.Value,
                TotalAmount = q.TotalAmount,
                FinalAmount = q.FinalAmount,
                Discount = q.Discount,
                DiscountOrderDetail = q.DiscountOrderDetail,
                TotalOrderDetails = q.OrderDetails.Count,
                OrderType = q.OrderType,
                OrderStatus = q.OrderStatus
            });
        }
        public IQueryable<Order> GetRentsByTimeRange(int storeID, DateTime startTime, DateTime endTime, int brandId)
        {
            var result = this.Get(r => ((r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime)) && r.Store.BrandId == brandId);
            if (storeID != 0)
            {
                result = result.Where(q => q.StoreID == storeID);
            }
            else
            {
                result = result.Where(q => (q.StoreID.HasValue ? q.Store.BrandId == brandId : false));
            }
            return result;
        }

        public IQueryable<Order> GetRentsByTimeRangeAndListStore( List<int> listStoreId, DateTime startTime, DateTime endTime, int brandId)
        {
            var result = this.Get(r => ((r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime)) 
                                   && r.Store.BrandId == brandId
                                   && listStoreId.Contains(r.StoreID.Value));
            return result;
        }
        public IQueryable<Order> GetRentsByTime(DateTime startTime, DateTime endTime)
        {
            var orderDetails = detailService.GetOrderDetailsByTimeRange(startTime, endTime, 13)
                .Select(q => q.RentID)
                .Distinct();
            return this.Get(q => q.CheckInDate >= startTime && q.CheckInDate <= endTime && orderDetails.Contains(q.RentID));
        }
        public IQueryable<Order> GetRentsFinishByTimeRange(int storeID, DateTime startTime, DateTime endTime)
        {
            var result = this.Get(r => (r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime) && r.OrderStatus == (int)OrderStatusEnum.Finish);
            if (storeID != 0)
            {
                result = result.Where(r => r.StoreID == storeID);
            }
            return result;
        }

        public IQueryable<Order> GetAllOrderByDateByPromotionIdOrderDetail(DateTime from, DateTime to, int brandId, int PromotionId)
        {
            OrderDetailPromotionMappingApi opApi = new OrderDetailPromotionMappingApi();
            OrderDetailApi odApi = new OrderDetailApi();
            var mapping = opApi.BaseService.GetActive(q => q.PromotionId == PromotionId).Select(q => q.OrderDetailId);
            var orderDetails = odApi.BaseService.GetActive(q => q.OrderDate >= from && q.OrderDate <= to && mapping.Contains(q.OrderDetailID)).Select(q => q.RentID);
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.Store.BrandId == brandId) && orderDetails.Contains(r.RentID));


            return result;
        }

        #endregion

        public IEnumerable<Order> GetOrders()
        {
            return this.GetActive();
        }

        public async Task UpdateOrderDeliveryAsync(int orderId, List<ProductWithExtras> listExtra)
        {
            var productService = DependencyUtils.Resolve<IProductService>();
            var order = await this.GetAsync(orderId);
            var listOrderDetail = order.OrderDetails;

            foreach (var item in listExtra)
            {
                var parentOrderDetail = listOrderDetail.Where(q => q.TmpDetailId == item.ParentId).FirstOrDefault();
                var orderDetail = listOrderDetail.Where(q => q.ProductID == item.ProductExtraId).FirstOrDefault();
                if (parentOrderDetail != null)
                {
                    Product product = await productService.GetAsync(item.ProductExtraId);
                    if (orderDetail == null)
                    {
                        orderDetail = new OrderDetail();
                        orderDetail.Quantity = item.Quantity * parentOrderDetail.Quantity;
                        orderDetail.TotalAmount = product.Price * item.Quantity;
                        orderDetail.OrderDate = DateTime.Now;
                        orderDetail.Status = 0; // Tình trạng normal của món hàng từ website xuống POS, (thay đổi giá trị 'Hủy' tại POS)
                        orderDetail.IsAddition = false;
                        orderDetail.UnitPrice = product.Price;
                        orderDetail.Discount = (product.Price - ((product.Price != 0 && product.DiscountPrice != 0) ? product.DiscountPrice : product.Price)) * item.Quantity;
                        orderDetail.StoreId = order.StoreID;
                        orderDetail.FinalAmount = orderDetail.TotalAmount - orderDetail.Discount;
                        orderDetail.ParentId = parentOrderDetail.OrderDetailID;
                        orderDetail.ProductType = (int)ProductTypeEnum.Extra;
                        listOrderDetail.Add(orderDetail);
                    }
                    else
                    {
                        //orderDetail.ProductID = item.ProductExtraId;
                        orderDetail.Quantity = item.Quantity * parentOrderDetail.Quantity;
                        orderDetail.TotalAmount = product.Price * item.Quantity;
                        orderDetail.OrderDate = DateTime.Now;
                        orderDetail.Status = 0; // Tình trạng normal của món hàng từ website xuống POS, (thay đổi giá trị 'Hủy' tại POS)
                        orderDetail.IsAddition = false;
                        orderDetail.UnitPrice = product.Price;
                        orderDetail.Discount = (product.Price - ((product.Price != 0 && product.DiscountPrice != 0) ? product.DiscountPrice : product.Price)) * item.Quantity;
                        orderDetail.StoreId = order.StoreID;
                        orderDetail.FinalAmount = orderDetail.TotalAmount - orderDetail.Discount;
                        orderDetail.ParentId = parentOrderDetail.OrderDetailID;
                        orderDetail.ProductType = (int)ProductTypeEnum.Extra;
                        //listOrderDetail.Add(orderDetail);
                    }
                }

            }
            await this.UpdateAsync(order);
        }

        public async Task UpdateTmpDetailIdOrderDetailAsync(int orderId)
        {
            var order = await this.GetAsync(orderId);
            foreach (var item in order.OrderDetails)
            {
                item.TmpDetailId = item.OrderDetailID;
            }
            await this.UpdateAsync(order);
        }

        public IQueryable<Order> FixDB1()
        {
            return this.Get(q => q.Att1 != null
                    && (q.Discount > 0 || q.DiscountOrderDetail > 0)
                    && (q.CustomerID == null || q.CustomerID == 0));
        }

        public IQueryable<Order> FixDB2()
        {
            return this.Get(q => q.Att1 == null && q.CustomerID != null && q.CustomerID > 0);
        }
    }

    public class OrderCustomEntity : IEntity
    {
        public Order Order { get; set; }
        public IEnumerable<OrderDetail> OrderDetails { get; set; }
        public Customer Customer { get; set; }
    }

    public class OrderForDashBoard
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public string StoreAbbr { get; set; }
        public string CheckInPerson { get; set; }
        public int CheckInHour { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<double> TotalAmount { get; set; }
        public Nullable<double> FinalAmount { get; set; }
        public Nullable<double> Discount { get; set; }
        public Nullable<double> DiscountOrderDetail { get; set; }
        public int TotalOrderDetails { get; set; }
        public Nullable<int> OrderType { get; set; }
        public Nullable<int> OrderStatus { get; set; }

    }
}

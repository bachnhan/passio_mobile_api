using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class OrderApi
    {
        public PagingViewModel<OrderViewModel> GetAdminWithFilterAsync(int storeId, string keyword,
            int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var rents = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, sortKeyAsc)
                .ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            var pagedList = rents
                 .Page(currPage, pageSize);

            return new PagingViewModel<OrderViewModel>(pagedList);
        }
        public Order GetOrderById(int rentId)
        {
            return this.BaseService.Get().Where(q => q.RentID == rentId).FirstOrDefault();
        }

        public IQueryable<Order> GetOrderFinishByTimeRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            if (storeId != 0)
            {
                var result = this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != (int)OrderTypeEnum.DropProduct && q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.Finish);
                return result;
            }
            else
            {
                var result = this.BaseService.Get(q => q.OrderType != (int)OrderTypeEnum.DropProduct && q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.Finish && q.Store.BrandId == brandId);
                return result;
            }
        }

        public IEnumerable<OrderViewModel> OrderGroup(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            int typeDropProduct = (int)OrderTypeEnum.DropProduct;
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(
                    q=> new OrderViewModel
                    {
                        OrderDetailsTotalQuantity =q.OrderDetailsTotalQuantity,
                        OrderType = q.OrderType,
                        FinalAmount = q.FinalAmount,
                        StoreID = q.StoreID,
                    }
                    );
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select new OrderViewModel
                               {
                                   OrderDetailsTotalQuantity = order.OrderDetailsTotalQuantity,
                                   OrderType = order.OrderType,
                                   FinalAmount = order.FinalAmount,
                                   StoreID = order.StoreID,
                               };
                return joindata;
            }
        }


        public IQueryable<Order> GetOrderPreCancelByTimeRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            if (storeId != 0)
            {
                var result = this.BaseService.Get(q => q.StoreID == storeId && q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.PreCancel);
                return result;
            }
            else
            {
                var result = this.BaseService.Get(q => q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.PreCancel && q.Store.BrandId == brandId);
                return result;
            }
        }

        public IQueryable<Order> GetOrderCancelByTimeRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            if (storeId != 0)
            {
                var result = this.BaseService.Get(q => q.StoreID == storeId && q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.Cancel);
                return result;
            }
            else
            {
                var result = this.BaseService.Get(q => q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.Cancel && q.Store.BrandId == brandId);
                return result;
            }
        }
        public IQueryable<Order> GetOrderFinishByTimeRangeAndType(int storeId, DateTime startDate, DateTime endDate, int orderType, int brandId)
        {
            if (storeId != 0)
            {
                var result = this.BaseService.Get(q =>  q.StoreID == storeId &&  q.OrderType == orderType && q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.Finish );
                return result;
            } else
            {
                var result = this.BaseService.Get(q => q.OrderType == orderType && q.CheckInDate >= startDate && q.CheckInDate < endDate && q.OrderStatus == (int)OrderStatusEnum.Finish && q.Store.BrandId == brandId);
                return result;
            }
        }

        public IQueryable<Order> GetOrderByBrand(int brandId)
        {
            var result = this.BaseService.GetOrderByBrand(brandId);
            //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return result;
        }

        public async Task<bool> EditOrderAsync(OrderViewModel model)
        {
            try
            {
                var entity = (await this.BaseService.GetAsync(model.RentID));
                entity.DeliveryStatus = model.DeliveryStatus;
                entity.OrderStatus = model.OrderStatus;
                entity.OrderType = model.OrderType;
                entity.StoreID = model.StoreID;
                await this.BaseService.UpdateAsync(entity);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<OrderViewModel> GetOrderByIdAsync(int id)
        {
            var result = new OrderViewModel(await this.BaseService.GetOrderByIdAsync(id));
            return result;
        }

        public void Create(OrderCustomEntityViewModel model)
        {

            //model = Utils.ToExactType<RentDetailsViewModel, RentDetailsViewModel>(model);

            var entity = model.ToEntity();
            var rent = entity.Order;
            rent.OrderDetails = entity.OrderDetails.ToList();
            this.BaseService.CreateAsync(rent);
            model.Order = new OrderViewModel(rent);
            model.OrderDetails = rent.OrderDetails.AsQueryable()
                .ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig);
        }

        public int CreateOrderDelivery(OrderCustomEntityViewModel model)
        {
            var entity = model.ToEntity();
            var rent = entity.Order;
            rent.OrderDetails = entity.OrderDetails.ToList();
            this.BaseService.Create(rent);
            return rent.RentID;
        }

        public async Task<bool> CreateOrderAsync(OrderViewModel model)
        {
            return await this.BaseService.CreateOrderAsync(model.ToEntity());
        }

        public OrderCustomEntityViewModel GetOrderById(int storeId, int id)
        {
            var order = this.BaseService.GetOrderById(storeId, id);
            if (order == null)
            {
                return null;
            }
            var orderDetail = order.Order.OrderDetails.AsQueryable().ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig);
            var result = new OrderCustomEntityViewModel(order);
            result.OrderDetails = orderDetail;
            return result;
        }

        public OrderViewModel GetOrderByIdAndBrandId(int brandId, int id)
        {
            var order = this.BaseService.GetOrderByIdAndBrandId(brandId, id);
            var result = new OrderViewModel(order);
            return result;
        }

        public Order GetOrderByInvoiceId(string invoiceId)
        {
            return this.BaseService.Get(a => a.InvoiceID == invoiceId).FirstOrDefault();
        }

        public bool CreateOrder(Order order)
        {
            if (!this.BaseService.Get(q => q.InvoiceID == order.InvoiceID).Any())
            {
                return this.BaseService.CreateOrder(order);
            }

            return false;
        }

        public int CreateOrderReturnId(Order order)
        {
            if (!this.BaseService.Get(q => q.InvoiceID == order.InvoiceID).Any())
            {
                return this.BaseService.CreateOrderReturnId(order);
            }

            return -1;
        }

        public void EditOrder(Order order)
        {
            this.BaseService.Update(order);
        }

        public IEnumerable<Order> GetStoreOrdersByDate(DateTime date, int storeId)
        {
            return BaseService
                .GetOrdersByTimeRange(storeId, date.GetStartOfDate(), date.GetEndOfDate())
                .ToList();
        }

        public int CountFinishOrderByDate(DateTime date, int storeId)
        {
            var result = BaseService
                .GetOrdersByTimeRange(storeId, date.GetStartOfDate(), date.GetEndOfDate())
                .Where(o => o.OrderStatus == (int)OrderStatusEnum.Finish);
            return result.Count();
        }

        public async Task UpdateOrderDelivery(int orderId, List<ProductWithExtras> listExtra)
        {
            await this.BaseService.UpdateOrderDeliveryAsync(orderId, listExtra);
        }

        public async Task UpdateTmpDetailIdOrderDetailAsync(int orderId)
        {
            await this.BaseService.UpdateTmpDetailIdOrderDetailAsync(orderId);
        }

        public IQueryable<int> GetAllOrderIdsByTimeRange(int storeID, int brandID, DateTime startTime, DateTime endTime)
        {
            return BaseService.GetRentIdsByTimeRange(storeID, startTime, endTime, brandID);
        }

        #region Date Report
        /// <summary>
        /// Count Total Order In Store By Time Range
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public int CountOrderInStore(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            int typeDropProduct = (int)OrderTypeEnum.DropProduct;
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Count();
            } else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.RentID;
                return joindata.Count();
            }
        }

        public IQueryable<Order> OrderInStore(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            int typeDropProduct = (int)OrderTypeEnum.DropProduct;
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order;
                return joindata;
            }
        }


        public double TotalRevenueOrderInStore(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            int typeDropProduct = (int)OrderTypeEnum.DropProduct;
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q=>q.FinalAmount).Sum();
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.FinalAmount;
                return joindata.Sum();
            }
        }

        /// <summary>
        /// Sum total Quantity Item Order
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public int TotalQuantityItem(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            int typeDropProduct = (int)OrderTypeEnum.DropProduct;
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.OrderType != (int)OrderTypeEnum.OrderCard && q.OrderDetailsTotalQuantity != null && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q=>q.OrderDetailsTotalQuantity.Value).Sum();
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType != (int)OrderTypeEnum.OrderCard && q.OrderDetailsTotalQuantity != null && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.OrderDetailsTotalQuantity.Value;
                return joindata.Sum();
            }
        }


        public int SumTotalItemInStore(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            int typeDropProduct = (int)OrderTypeEnum.DropProduct;
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Count();
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.RentID;
                return joindata.Count();
            }
        }

        /// <summary>
        /// Total Amount without discount all order in store
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public double TotalWithoutDiscountInStore(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            int typeDropProduct = (int)OrderTypeEnum.DropProduct;
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q=>q.TotalAmount).Sum();
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.TotalAmount;
                return joindata.Sum();
            }
        }
        /// <summary>
        /// Count Total Order In Store By Time Range and OrderType
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public int CountOrderInStoreByType(int storeId, DateTime startTime, DateTime endTime, int orderType, int brandId)
        {
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType == orderType && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Count();
            } else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType == orderType && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.RentID;
                return joindata.Count();
            }
        }
        /// <summary>
        /// Total Quantity In Store By Type
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="orderType"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public int TotalQuantityInStoreByType(int storeId, DateTime startTime, DateTime endTime, int orderType, int brandId)
        {
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.OrderDetailsTotalQuantity != null && q.StoreID == storeId && q.OrderType == orderType && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q=>q.OrderDetailsTotalQuantity.Value).Sum();
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderDetailsTotalQuantity != null && q.OrderType == orderType && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.OrderDetailsTotalQuantity.Value;
                return joindata.Sum();
            }
        }

        /// <summary>
        /// sum total amount order in store
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="orderType"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public double TotalAmountOrderInStoreByType(int storeId, DateTime startTime, DateTime endTime, int orderType, int brandId)
        {
            int orderStatusFinish = (int)OrderStatusEnum.Finish;
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.OrderType == orderType && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q=>q.FinalAmount).Sum();
            } else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.OrderType == orderType && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.FinalAmount;
                return joindata.Sum();
            }
        }
        /// <summary>
        /// Count Total Order Cancel In Store By Time Range
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="typeCancel"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public int CountOrderCancelInStore(int storeId, DateTime startTime, DateTime endTime, int typeCancel, int brandId)
        {
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == typeCancel).Count();
            } else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q =>q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == typeCancel);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.RentID;
                return joindata.Count();
            }
        }
        /// <summary>
        /// sum total amount order cancel in store
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="typeCancel"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public double TotalAmounttOrderCancelInStore(int storeId, DateTime startTime, DateTime endTime, int typeCancel, int brandId)
        {
            if (storeId > 0)
            {
                return this.BaseService.Get(q => q.StoreID == storeId && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == typeCancel).Select(q => q.TotalAmount).Sum();
            } else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = this.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == typeCancel);
                var joindata = from order in orders
                               join store in listStore
                               on order.StoreID equals store.ID
                               select order.TotalAmount;
                return joindata.Sum();
            }
        }
        #endregion

        #region SystemReport
        public IEnumerable<OrderViewModel> GetAllRentByDate(DateTime from, DateTime to)
        {
            var allRent = this.BaseService.GetAllOrderByDate(from, to)
                .ProjectTo<OrderViewModel>(this.AutoMapperConfig)
                .ToList();
            return allRent;
        }

        public IQueryable<Order> GetAllOrdersByDate(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllOrderByDate(from, to, brandId);
            return allRent;
        }
        public IQueryable<Order> GetAllFinishedOrdersByDate(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllFinishedOrderByDate(from, to, brandId);
            return allRent;
        }
        public IQueryable<Order> GetAllFinishedOrdersByDateAndCustomer(DateTime from, DateTime to, int brandId, int customerId)
        {
            var allRent = this.BaseService.GetAllFinishedOrderByDateaAndCustomer(from, to, brandId, customerId);
            return allRent;
        }
        /// <summary>
        /// get order by customerId and brandId
        /// </summary>
        /// <param name="customerId">CustomerId</param>
        /// <param name="brandId">brandId</param>
        /// <returns>IEnumerable<Order></returns>
        public IEnumerable<Order> GetOrderByCustomerIdandBrandId(int customerId, int brandId)
        {
            return this.BaseService.Get().Where(q => q.CustomerID == customerId && q.Customer.BrandId == brandId);
        }
        public IQueryable<Order> GetAllDebtOrders(int brandId, int storeId)
        {
            var allRent = this.BaseService.GetAllDebtOrder(brandId,storeId);
            return allRent;
        }
        public IQueryable<Order> GetAllDebtOrderByBrand(int brandId)
        {
            var allRent = this.BaseService.GetAllDebtOrderByBrand(brandId);
            return allRent;
        }
        public IQueryable<Order> GetAllDebtOrderByBrandAndCustomerId(int brandId, List<int> customerId)
        {
            var allRent = this.BaseService.GetAllDebtOrderByBrandAndCustomerId(brandId,customerId);
            return allRent;
        }
        public IQueryable<Order> GetAllOrdersByDateWithCard(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllOrderByDateWithCard(from, to, brandId);
            return allRent;
        }

        public IQueryable<Order> GetAllOrderByDate(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllOrderByDate(from, to, brandId);
            //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return allRent;
        }
        public IQueryable<Order> GetAllOrderByDateByPromotionId(DateTime from, DateTime to, int brandId, int PromotionId)
        {
            var allRent = this.BaseService.GetAllOrderByDateByPromotionId(from, to, brandId, PromotionId);
            //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return allRent;
        }

        public IQueryable<Order> GetAllOrderByDateByPromotionIdOrderDetail(DateTime from, DateTime to, int brandId, int PromotionId)
        {
            var allRent = this.BaseService.GetAllOrderByDateByPromotionIdOrderDetail(from, to, brandId, PromotionId);
            //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return allRent;
        }

        public IEnumerable<Order> GetAllOrderByDateByPromotionIdAndStoreId(DateTime from, DateTime to, int brandId, int promotionId, int storeId)
        {
            return this.BaseService.GetAllOrderByDateByPromotionIdAndStoreId(from, to, brandId, promotionId, storeId);
        }

        public IQueryable<Order> GetAllOrderByDateDifferentFromPromotionId(DateTime from, DateTime to, int brandId, int PromotionId)
        {
            var allRent = this.BaseService.GetAllOrderByDateDifferentFromPromotionId(from, to, brandId, PromotionId);
            //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return allRent;
        }

        public IQueryable<Order> GetAllOrderByDate2(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllOrderByDate(from, to, brandId);
            return allRent;
        }

        public IQueryable<Order> GetRentsByTimeRange(int storeID, DateTime startTime, DateTime endTime, int brandId)
        {
            var rents = this.BaseService.GetRentsByTimeRange(storeID, startTime, endTime, brandId);
            //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return rents;
        }
        public IQueryable<Order> GetOrdersByTimeRangeAndHour(int storeID, DateTime startTime, DateTime endTime, int brandId, int startHour, int endHour)
        {
            return BaseService.GetRentsByTimeRangeAndHour(storeID, startTime, endTime, brandId, startHour, endHour);
        }

        public IEnumerable<Order> GetOrdersByTimeRange(int storeID, DateTime startTime, DateTime endTime, int brandId)
        {
            var rents = this.BaseService.GetRentsByTimeRange(storeID, startTime, endTime, brandId)
                .ToList();
            return rents;
        }

        public IEnumerable<Order> GetOrdersByTimeRangeAndListStore(List<int> listStoreId, DateTime startTime, DateTime endTime, int brandId)
        {
            var rents = this.BaseService.GetRentsByTimeRangeAndListStore(listStoreId, startTime, endTime, brandId)
                .ToList();
            return rents;
        }

        public IQueryable<Order> GetRentsByTimeRange(int storeID, DateTime startTime, DateTime endTime)
        {
            var rents = this.BaseService.GetRentsByTimeRange(storeID, startTime, endTime);
            //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            //.ToList();
            return rents;
        }
        public IEnumerable<Order> GetRentsByTimeRange2(int storeID, DateTime startTime, DateTime endTime)
        {
            var rents = this.BaseService.GetRentsByTimeRange(storeID, startTime, endTime);
            return rents;
        }

        public IEnumerable<Order> GetRentsByTime(DateTime startTime, DateTime endTime)
        {
            var rents = this.BaseService.GetRentsByTime(startTime, endTime)
                // .ProjectTo<OrderViewModel>(this.AutoMapperConfig)
                .ToList();
            return rents;
        }

        public IQueryable<Order> GetStoreOrderByDate(DateTime date, int storeId)
        {
            var starOfDate = date.GetStartOfDate();
            var endOfDate = date.GetEndOfDate();
            return BaseService.GetRentsByTimeRange(storeId, starOfDate, endOfDate);
        }

        public IEnumerable<Order> GetAllOrdersByDate(DateTime date, int brandId, int storeId)
        {
            var starOfDate = date.GetStartOfDate();
            var endOfDate = date.GetEndOfDate();
            return BaseService.GetRentsByTimeRange(storeId, starOfDate, endOfDate, brandId).ToList();
        }
        public IQueryable<Order> GetStoreOrderFinishByDate(DateTime date, int storeId)
        {
            var starOfDate = date.GetStartOfDate();
            var endOfDate = date.GetEndOfDate();
            return BaseService.GetRentsFinishByTimeRange(storeId, starOfDate, endOfDate);
        }
        public IQueryable<Order> GetTodayOrders(int brandId)
        {
            return BaseService.GetTodayOrders(brandId);
        }

        public IEnumerable<OrderForDashBoard> GetOrderForDashboard(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            return BaseService.GetOrderForDashboard(startDate, endDate, brandId, storeId).ToList();
        }
        public IQueryable<Order> GetOrderForRevenueReport(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            return BaseService.GetOrdersByTimeRange(startDate, endDate, brandId, storeId);
        }
        public IQueryable<OrderForDashBoard> GetQueryOrderForDashBoard(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            return BaseService.GetOrderForDashboard(startDate, endDate, brandId, storeId); //when storeId == 0, load all stores' reports from the chosen brand
        }
        #endregion

        #region VPOS
        public IQueryable<Order> getAllOrder()
        {
            var listOrder = this.BaseService.Get().Where(c => c.RentStatus != (int)RentStatusEnum.DeletePermanent
                && c.RentStatus != (int)RentStatusEnum.Disabled
                && c.RentStatus != (int)RentStatusEnum.WaitDisabled);
            return listOrder;
        }

        public IQueryable<Order> getAll()
        {
            var listOrder = this.BaseService.Get();
            return listOrder;
        }
        #endregion
        public IEnumerable<Order> getAllFieldthatNeedforReportByDate(DateTime fromDate, DateTime toDate)
        {
            toDate = toDate.GetEndOfDate();
            var listOrder = this.BaseService.Get(q => q.CheckInDate >= fromDate && q.CheckInDate <= toDate);
            //.Select(q => new Order
            //{
            //    RentID = q.RentID,
            //    CheckInDate = q.CheckInDate,
            //    StoreID = q.StoreID,
            //    TotalAmount = q.TotalAmount,
            //    FinalAmount = q.FinalAmount
            //}); ;


            //var tempListOrder = listOrder.AsEnumerable().Select(q => new Order
            //{
            //    RentID = q.RentID,
            //    CheckInDate = q.CheckInDate,
            //    StoreID = q.StoreID,
            //    TotalAmount = q.TotalAmount,
            //    FinalAmount = q.FinalAmount
            //});


            return listOrder;
        }

        public IEnumerable<Order> GetFinishedOrderByTime(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            return BaseService.GetOrdersByTimeRange(startDate, endDate, brandId, storeId).Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish);
        }

        #region Fix Order DB

        // Fix customerID null
        public IQueryable<Order> FixDB1()
        {
            return this.BaseService.FixDB1();
        }

        // Fix Att1 null
        public IQueryable<Order> FixDB2()
        {
            return this.BaseService.FixDB2();
        }

        public async Task UpdateOrderEntity(Order entity)
        {
            await this.BaseService.UpdateAsync(entity);
        }

        #endregion
    }
}

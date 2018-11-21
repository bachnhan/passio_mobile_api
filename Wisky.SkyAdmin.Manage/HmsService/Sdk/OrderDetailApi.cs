using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class OrderDetailApi
    {
        public OrderDetail OrderMaster(int orderDetailId)
        {
            var orderDetail = this.BaseService.GetOrderDetailMasterAsync(orderDetailId);
            return orderDetail;
        }
        public IQueryable<OrderDetail> GetOrderDetailIsExtra(int orderDetailId)
        {
            return this.BaseService.Get(q => q.ParentId == orderDetailId);
        }
        public IQueryable<OrderDetail> GetOrderDetailByTimeRange(int storeId, int BrandId, DateTime startTime, DateTime endTime)
        {
            if (storeId != 0)
            {
                return this.BaseService.Get(q => q.StoreId == storeId && q.Order.OrderStatus == (int)OrderStatusEnum.Finish && q.Order.OrderType != (int)OrderTypeEnum.DropProduct
                                                      && q.OrderDate >= startTime && q.OrderDate <= endTime);
            }
            else
            {
                return this.BaseService.Get(q => q.Order.Store.BrandId == BrandId && q.Order.OrderStatus == (int)OrderStatusEnum.Finish && q.Order.OrderType != (int)OrderTypeEnum.DropProduct
                                      && q.OrderDate >= startTime && q.OrderDate <= endTime);
            }

        }

        #region SystemReport
        public IEnumerable<DateProduct> GetProductByDate(DateTime date, int storeID)
        {
            var products = this.BaseService.GetProductByDate(date, storeID)
                //.ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)
                .ToList();
            return products;
        }
        //public IEnumerable<OrderDetail> GetAllOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime)
        //{
        //    var orderDetails = this.BaseService.GetAllOrderDetailsByTimeRange(brandId, startTime, endTime);
        //        //.ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)

        //    return orderDetails;
        //}

        public IQueryable<OrderDetail> GetAllOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime)
        {
            var orderDetails = this.BaseService.GetAllOrderDetailsByTimeRange(brandId, startTime, endTime);
            //.ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)

            return orderDetails;
        }

        public IQueryable<OrderDetail> GetAllOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime, int? startHour, int? endHour)
        {
            var orderDetails = this.BaseService.GetAllOrderDetailsByTimeRange(brandId, startTime, endTime, startHour, endHour);
            //.ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)

            return orderDetails;
        }

        public IEnumerable<OrderDetail> GetOrderDetailsByProductCategory(int brandId, DateTime startTime, DateTime endTime, int cateID)
        {
            return BaseService.GetOrderDetailsByProductCategory(brandId, startTime, endTime, cateID).ToList();
        }

        public IEnumerable<OrderDetail> GetDateOrderDetailsByProductCategory(int brandId, DateTime date, int cateID)
        {
            return BaseService.GetOrderDetailsByProductCategory(brandId, date, date, cateID).ToList();
        }

        public IEnumerable<OrderDetail> GetDateOrderDetailsByProduct(int brandId, DateTime date, int productID)
        {
            return BaseService.GetOrderDetailsByProduct(brandId, date, date, productID).ToList();
        }


        public IQueryable<OrderDetail> GetOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int storeId)
        {
            var orderDetails = this.BaseService.GetOrderDetailsByTimeRange(startTime, endTime, storeId);
            // .ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)
            // .ToList();
            return orderDetails;
        }

        public IQueryable<OrderDetail> GetOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int storeId, int? startHour, int? endHour)
        {
            var orderDetails = this.BaseService.GetOrderDetailsByTimeRange(startTime, endTime, storeId, startHour, endHour);
            // .ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)
            // .ToList();
            return orderDetails;
        }
        #endregion
        #region StoreReport
        public int CountOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int? storeId, int? excludingType)
        {
            var count = this.BaseService.CountOrderDetailsByTimeRange(startTime, endTime, storeId, excludingType);
            return count;
        }
        public IEnumerable<OrderDetail> GetOrderDetailsByRentId(int rentId)
        {
            var orderDetails = this.BaseService.GetOrderDetailsByRentId(rentId)
                //.ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)
                .ToList();
            return orderDetails;
        }

        public IQueryable<OrderDetail> getAllOrderDetail()
        {
            var orderDetails = this.BaseService.Get().Where(c => c.Order.RentStatus != (int)RentStatusEnum.Disabled
                && c.Order.RentStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.RentStatus != (int)RentStatusEnum.DeletePermanent);
            return orderDetails;
        }

        public IQueryable<OrderDetail> getAll()
        {
            var orderDetails = this.BaseService.Get();
            return orderDetails;
        }
        public IEnumerable<OrderDetail> getAllFieldthatNeedforReportByDate(DateTime fromDate, DateTime toDate)
        {
            toDate = toDate.GetEndOfDate();
            var orderDetails = this.BaseService.Get(q => q.OrderDate >= fromDate && q.OrderDate <= toDate);
            //.Select(a => new OrderDetail
            //{
            //    OrderDetailID = a.OrderDetailID,
            //    RentID = a.RentID,
            //    StoreId = a.StoreId,
            //    TotalAmount = a.TotalAmount,
            //    FinalAmount = a.FinalAmount,
            //});

            //var temporderDetails = orderDetails.AsEnumerable().Select(a => new OrderDetail
            //{
            //    OrderDetailID = a.OrderDetailID,
            //    RentID = a.RentID,
            //    StoreId = a.StoreId,
            //    TotalAmount = a.TotalAmount,
            //    FinalAmount = a.FinalAmount,
            //});


            return orderDetails;
        }

        public IQueryable<OrderDetail> GetAllOrderDetailByTimeRange(DateTime fromDate, DateTime toDate, int brandId)
        {
            return this.BaseService.GetAllOrderDetailByTimeRange(fromDate, toDate, brandId);
        }

        public IEnumerable<TodayOrderDetail> GetStoreTodayOrderDetailByProduct(int storeId, int productId)
        {
            return BaseService.GetStoreTodayOrderDetailByProduct(storeId, productId).ToList();
        }

        public IEnumerable<TodayOrderDetail> GetStoreTodayOrderDetailByProductCategory(int storeId, int cateId)
        {
            return BaseService.GetStoreTodayOrderDetailByProductCategory(storeId, cateId).ToList();
        }
        #endregion

        #region ComparisonReport
        public IEnumerable<TodayOrderDetail> GetTodayOrderDetailByProduct(int brandId, int productId)
        {
            return BaseService.GetTodayOrderDetailByProduct(brandId, productId).ToList();
        }

        public IEnumerable<TodayOrderDetail> GetTodayOrderDetailByProductCategory(int brandId, int cateId)
        {
            return BaseService.GetTodayOrderDetailByProductCategory(brandId, cateId).ToList();
        }
        #endregion

        public IEnumerable<OrderDetail> GetAllCanceledOrderDetailByStore(int storeId, DateTime startDate, DateTime endDate)
        {
            return BaseService.GetAllCanceledOrderDetailByStore(startDate, endDate, storeId).ToList();
        }

        public IEnumerable<OrderDetail> GetAllCanceledOrderDetailByBrand(DateTime startTime, DateTime endTime, int brandId)
        {
            return BaseService.GetAllCanceledOrderDetailByBrand(startTime, endTime, brandId).ToList();
        }

        public IEnumerable<TodayOrderDetail> GetDateOrderDetails(DateTime date, int brandId, int storeId)
        {
            return BaseService.GetAllDateOrderDetails(date, brandId, storeId).ToList();
        }

        public IQueryable<TodayOrderDetail> GetQueryDateOrderDetails(DateTime date, int brandId, int storeId)
        {
            return BaseService.GetAllDateOrderDetails(date, brandId, storeId);
        }

        public IQueryable<TodayOrderDetail> GetAllTotalDate(DateTime startTime, DateTime endTime, int storeId, int brandId)
        {
            var orderStatusFinish = (int)OrderStatusEnum.Finish;
            var typeDropProduct = (int)OrderTypeEnum.DropProduct;
            var orderApi = new OrderApi();

            IQueryable<int> listOrder;
            if (storeId > 0)
            {
                listOrder = orderApi.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q => q.RentID);
                var listOrderDetail = this.BaseService.Get();
                var joinData = from order in listOrder
                               join orderDetail in listOrderDetail
                               on order equals orderDetail.RentID
                               select new TodayOrderDetail
                               {
                                   StoreID = storeId,
                                   OrderDetailID = orderDetail.OrderDetailID,
                                   ProductID = orderDetail.ProductID,
                                   TotalOrderDetails = orderDetail.Quantity,
                                   TotalAmount = orderDetail.TotalAmount,
                                   FinalAmount = orderDetail.FinalAmount,
                                   Quantity = orderDetail.Quantity
                               };
                return joinData;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = orderApi.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joinOrderdata = from order in orders
                                    join store in listStore
                                    on order.StoreID equals store.ID
                                    select order.RentID;
                var listOrderDetail = this.BaseService.Get();
                var joinData = from order in joinOrderdata
                               join orderDetail in listOrderDetail
                               on order equals orderDetail.RentID
                               select new TodayOrderDetail
                               {
                                   StoreID = storeId,
                                   OrderDetailID = orderDetail.OrderDetailID,
                                   ProductID = orderDetail.ProductID,
                                   TotalOrderDetails = orderDetail.Quantity,
                                   TotalAmount = orderDetail.TotalAmount,
                                   FinalAmount = orderDetail.FinalAmount,
                                   Quantity = orderDetail.Quantity
                               };
                return joinData;
            }
        }

        public IQueryable<TodayOrderDetail> GetAllTotalDateByListStore(DateTime startTime, DateTime endTime, List<int> listStoreId, int brandId)
        {
            var orderStatusFinish = (int)OrderStatusEnum.Finish;
            var typeDropProduct = (int)OrderTypeEnum.DropProduct;
            var orderApi = new OrderApi();

            IQueryable<int> listOrder;
            listOrder = orderApi.BaseService.Get(q => q.StoreID != null && listStoreId.Contains(q.StoreID.Value) && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q => q.RentID);
            var listOrderDetail = this.BaseService.Get();
            var joinData = from order in listOrder
                           join orderDetail in listOrderDetail
                           on order equals orderDetail.RentID
                           select new TodayOrderDetail
                           {
                               StoreID = orderDetail.StoreId.Value,
                               OrderDetailID = orderDetail.OrderDetailID,
                               ProductID = orderDetail.ProductID,
                               TotalOrderDetails = orderDetail.Quantity,
                               TotalAmount = orderDetail.TotalAmount,
                               FinalAmount = orderDetail.FinalAmount,
                               Quantity = orderDetail.Quantity
                           };
            return joinData;
        }

        public IQueryable<OrderDetailForRPProduct> GetAllOrderDetail(DateTime startTime, DateTime endTime, int storeId, int brandId)
        {
            var orderStatusFinish = (int)OrderStatusEnum.Finish;
            var typeDropProduct = (int)OrderTypeEnum.DropProduct;
            var orderApi = new OrderApi();

            IQueryable<int> listOrder;
            if (storeId > 0)
            {
                listOrder = orderApi.BaseService.Get(q => q.StoreID == storeId && q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish).Select(q => q.RentID);
                var listOrderDetail = this.BaseService.Get();
                var joinData = from order in listOrder
                               join orderDetail in listOrderDetail
                               on order equals orderDetail.RentID
                               select new OrderDetailForRPProduct
                               {
                                   OrderDetailId = orderDetail.OrderDetailID,
                                   OrderId = orderDetail.RentID,
                                   ProductID = orderDetail.ProductID,
                                   Quantity = orderDetail.Quantity,
                                   Discount = orderDetail.Discount,
                                   FinalAmount = orderDetail.FinalAmount,
                                   TotalAmount = orderDetail.TotalAmount
                               };
                return joinData;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orders = orderApi.BaseService.Get(q => q.OrderType != typeDropProduct && q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == orderStatusFinish);
                var joinOrderdata = from order in orders
                                    join store in listStore
                                    on order.StoreID equals store.ID
                                    select order.RentID;
                var listOrderDetail = this.BaseService.Get();
                var joinData = from order in joinOrderdata
                               join orderDetail in listOrderDetail
                               on order equals orderDetail.RentID
                               select new OrderDetailForRPProduct
                               {
                                   OrderDetailId = orderDetail.OrderDetailID,
                                   OrderId = orderDetail.RentID,
                                   ProductID = orderDetail.ProductID,
                                   Quantity = orderDetail.Quantity,
                                   Discount = orderDetail.Discount,
                                   FinalAmount = orderDetail.FinalAmount,
                                   TotalAmount = orderDetail.TotalAmount
                               };
                return joinData;
            }
        }
    }
}

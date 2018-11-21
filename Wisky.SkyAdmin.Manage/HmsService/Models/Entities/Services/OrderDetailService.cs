using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IOrderDetailService
    {
        OrderDetail GetOrderDetailMasterAsync(int orderDetailId);
        #region SystemReport
        IQueryable<DateProduct> GetProductByDate(DateTime date, int storeID);
        IQueryable<OrderDetail> GetAllOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime);
        IQueryable<OrderDetail> GetAllOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime, int? startHour, int? endHour);
        //        IQueryable<OrderDetail> GetOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime, int storeId);
        IQueryable<OrderDetail> GetOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int storeId);
        IQueryable<OrderDetail> GetOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int storeId, int? startHour, int? endHour);
        IQueryable<OrderDetail> GetOrderDetailsByProductCategory(int brandId, DateTime startTime, DateTime endTime, int cateID);
        IQueryable<OrderDetail> GetOrderDetailsByProduct(int brandId, DateTime startTime, DateTime endTime, int productId);

        #endregion
        #region Store Report
        int CountOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int? storeId, int? excludingType);
        IQueryable<OrderDetail> GetOrderDetailsByRentId(int rentId);
        IQueryable<TodayOrderDetail> GetStoreTodayOrderDetailByProduct(int storeId, int productId);
        IQueryable<TodayOrderDetail> GetStoreTodayOrderDetailByProductCategory(int storeId, int cateId);
        IQueryable<OrderDetail> GetAllOrderDetailByTimeRange(DateTime fromDate, DateTime toDate, int brandId);
        #endregion

        #region ComparisonReport
        IQueryable<TodayOrderDetail> GetTodayOrderDetailByProduct(int brandId, int productId);
        IQueryable<TodayOrderDetail> GetTodayOrderDetailByProductCategory(int brandId, int cateId);
        #endregion
        IEnumerable<OrderDetail> GetOrderDetails();

        #region Dashboard
        IQueryable<OrderDetail> GetAllCanceledOrderDetailByStore(DateTime startTime, DateTime endTime, int storeId);
        IQueryable<OrderDetail> GetAllCanceledOrderDetailByBrand(DateTime startTime, DateTime endTime, int brandId);
        IQueryable<TodayOrderDetail> GetAllDateOrderDetails(DateTime Date, int brandId, int storeId);
        #endregion
    }
    public partial class OrderDetailService
    {
        public OrderDetail GetOrderDetailMasterAsync(int orderDetailId)
        {
            var orderDetail = Get(orderDetailId);
            return orderDetail;
        }
        #region SystemReport
        public IQueryable<DateProduct> GetProductByDate(DateTime date, int storeID)
        {
            var orderDetails = this.Get(o => o.OrderDate.Year == date.Year
                                                                   && o.OrderDate.Month == date.Month
                                                                   && o.OrderDate.Day == date.Day
                                                                   && o.Order.StoreID == storeID
                                                                   && o.Status == (int)OrderDetailStatus.Ordered);
            return orderDetails.GroupBy(a => a.ProductID).Where(w => w.Any())
                    .Select(
                        a =>
                            new DateProduct()
                            {
                                Date = date,
                                ProductId = a.FirstOrDefault().ProductID,
                                ProductName_ = a.FirstOrDefault().Product.ProductName,
                                Quantity = a.Sum(b => b.Quantity),
                                TotalAmount = a.Sum(b => b.TotalAmount),
                                FinalAmount = a.Sum(b => b.FinalAmount),
                                Discount = a.Sum(b => b.Discount),
                                Product = a.First().Product,
                                CategoryId_ = a.First().Product.CatID
                            });
        }
        public IQueryable<OrderDetail> GetAllOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime)
        {
            return this.Get(a => a.Order.Store.BrandId == brandId && (bool)a.Order.Store.isAvailable
                       && startTime <= a.OrderDate && endTime >= a.OrderDate &&
                         a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
        }


        public IQueryable<OrderDetail> GetAllOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime, int? startHour, int? endHour)
        {
            return this.Get(a => a.Order.Store.BrandId == brandId && (bool)a.Order.Store.isAvailable
                       && startTime <= a.OrderDate && endTime >= a.OrderDate
                       && a.OrderDate.Hour >= startHour && a.OrderDate.Hour <= endHour &&
                         a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
        }
        //        public IQueryable<OrderDetail> GetOrderDetailsByTimeRange(int? brandId, DateTime startTime, DateTime endTime, int storeId)
        //        {
        //            return this.Get(a => a.Order.Store.BrandId == brandId
        //                        && startTime <= a.OrderDate && endTime >= a.OrderDate &&
        //                        a.Order.StoreID == storeId &&
        //                         a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
        //        }
        public IQueryable<OrderDetail> GetOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int storeId)
        {
            return this.Get(a =>
                        startTime <= a.OrderDate && endTime >= a.OrderDate &&
                        a.Order.StoreID == storeId && (bool) a.Order.Store.isAvailable &&
                         a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
        }

        public IQueryable<OrderDetail> GetOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int storeId, int? startHour, int? endHour)
        {
            return this.Get(a =>
                        startTime <= a.OrderDate && endTime >= a.OrderDate &&
                        a.OrderDate.Hour >= startHour && a.OrderDate.Hour <= endHour &&
                        a.Order.StoreID == storeId && (bool) a.Order.Store.isAvailable &&
                         a.Order.OrderType != (int)OrderTypeEnum.DropProduct && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
        }

        public IQueryable<OrderDetail> GetOrderDetailsByProductCategory(int brandId, DateTime startTime, DateTime endTime, int cateID)
        {
            return Get(q =>
                    q.Order.Store.BrandId == brandId
                    && q.OrderDate >= startTime
                    && q.OrderDate <= endTime
                    && q.Product.CatID == cateID
                    && q.Order.OrderType == (int)OrderStatusEnum.Finish);
        }

        public IQueryable<OrderDetail> GetOrderDetailsByProduct(int brandId, DateTime startTime, DateTime endTime, int productId)
        {
            return Get(q => q.Order.Store.BrandId == brandId && q.OrderDate >= startTime && q.OrderDate <= endTime && q.ProductID == productId && q.Order.OrderStatus == (int)OrderStatusEnum.Finish);
        }

        #endregion
        #region StoreReport
        public int CountOrderDetailsByTimeRange(DateTime startTime, DateTime endTime, int? storeId, int? excludingType)
        {
            return this.Get().Count(a =>
                startTime <= a.OrderDate && endTime >= a.OrderDate &&
                (storeId == null || (a.Order.StoreID == storeId.Value && (bool)a.Order.Store.isAvailable)) &&
                (excludingType == null || a.Order.RentType != excludingType.Value));
        }
        public IQueryable<OrderDetail> GetOrderDetailsByRentId(int rentId)
        {
            return this.Get(c => c.RentID == rentId);
        }
        #endregion

        #region ComparisonReport
        public IQueryable<TodayOrderDetail> GetTodayOrderDetailByProduct(int brandId, int productId)
        {
            var startOfToday = DateTime.Today.GetStartOfDate();
            var endOfToday = DateTime.Today.GetEndOfDate();
            return GetOrderDetailsByProduct(brandId, startOfToday, endOfToday, productId)
                .Select(q => new TodayOrderDetail
                {
                    StoreID = q.StoreId.Value,
                    OrderDetailID = q.OrderDetailID,
                    ProductID = q.ProductID,
                    ProductName = q.Product.ProductName,
                    TotalOrderDetails = q.Quantity,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount,
                    Quantity = q.Quantity
                });
        }

        public IQueryable<TodayOrderDetail> GetTodayOrderDetailByProductCategory(int brandId, int cateId)
        {
            var startOfToday = DateTime.Today.GetStartOfDate();
            var endOfToday = DateTime.Today.GetEndOfDate();
            return GetOrderDetailsByProductCategory(brandId, startOfToday, endOfToday, cateId)
                .Select(q => new TodayOrderDetail
                {
                    StoreID = q.StoreId.Value,
                    OrderDetailID = q.OrderDetailID,
                    ProductID = q.ProductID,
                    ProductName = q.Product.ProductName,
                    TotalOrderDetails = q.Quantity,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount,
                    Quantity = q.Quantity
                });
        }

        public IQueryable<TodayOrderDetail> GetStoreTodayOrderDetailByProduct(int storeId, int productId)
        {
            var startOfToday = DateTime.Today.GetStartOfDate();
            var endOfToday = DateTime.Today.GetEndOfDate();
            return Get(a =>
                        startOfToday <= a.OrderDate && endOfToday >= a.OrderDate
                        && a.Order.StoreID == storeId
                        && a.ProductID == productId
                        &&(bool) a.Order.Store.isAvailable
                        && a.Order.OrderType != (int)OrderTypeEnum.DropProduct
                        && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                .Select(q => new TodayOrderDetail
                {
                    StoreID = q.StoreId.Value,
                    OrderDetailID = q.OrderDetailID,
                    ProductID = q.ProductID,
                    ProductName = q.Product.ProductName,
                    TotalOrderDetails = q.Quantity,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount
                });
        }
        public IQueryable<TodayOrderDetail> GetStoreTodayOrderDetailByProductCategory(int storeId, int cateId)
        {
            var startOfToday = DateTime.Today.GetStartOfDate();
            var endOfToday = DateTime.Today.GetEndOfDate();
            return Get(a =>
                        startOfToday <= a.OrderDate && endOfToday >= a.OrderDate
                        && a.Order.StoreID == storeId
                        && a.Product.CatID == cateId
                        && (bool) a.Order.Store.isAvailable
                        && a.Order.OrderType != (int)OrderTypeEnum.DropProduct
                        && a.Order.OrderStatus == (int)OrderStatusEnum.Finish)
                .Select(q => new TodayOrderDetail
                {
                    StoreID = q.StoreId.Value,
                    OrderDetailID = q.OrderDetailID,
                    ProductID = q.ProductID,
                    ProductName = q.Product.ProductName,
                    TotalOrderDetails = q.Quantity,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount
                });
        }
        #endregion

        public IEnumerable<OrderDetail> GetOrderDetails()
        {
            return this.GetActive();
        }

        public IQueryable<OrderDetail> GetAllCanceledOrderDetailByStore(DateTime startTime, DateTime endTime, int storeId)
        {
            return this.Get(a => a.Order.StoreID == storeId && (bool) a.Order.Store.isAvailable
                       && startTime <= a.OrderDate && endTime >= a.OrderDate && a.Status == (int)OrderDetailStatusEnum.Cancel);
        }

        public IQueryable<OrderDetail> GetAllCanceledOrderDetailByBrand(DateTime startTime, DateTime endTime, int brandId)
        {
            return this.Get(a => a.Order.Store.BrandId == brandId && (bool) a.Order.Store.isAvailable
                       && startTime <= a.OrderDate && endTime >= a.OrderDate && a.Status == (int)OrderDetailStatusEnum.Cancel);
        }
        public IQueryable<TodayOrderDetail> GetAllDateOrderDetails(DateTime Date, int brandId, int storeId)
        {
            var startOfDate = Date.GetStartOfDate();
            var endOfDate = Date.GetEndOfDate();
            var result = Get(a =>
                        startOfDate <= a.OrderDate && endOfDate >= a.OrderDate
                        && a.Order.Store.BrandId == brandId
                        && (bool) a.Order.Store.isAvailable
                        && a.Order.OrderType != (int)OrderTypeEnum.DropProduct
                        && a.Order.OrderStatus == (int)OrderStatusEnum.Finish);
            if (storeId > 0)
            {
                result = result.Where(a => a.Order.StoreID == storeId);
            }
            return result.Select(q => new TodayOrderDetail
            {
                StoreID = q.StoreId.Value,
                OrderDetailID = q.OrderDetailID,
                ProductID = q.ProductID,
                ProductName = q.Product.ProductName,
                TotalOrderDetails = q.Quantity,
                TotalAmount = q.TotalAmount,
                FinalAmount = q.FinalAmount
            });
        }

        public IQueryable<OrderDetail> GetAllOrderDetailByTimeRange(DateTime fromDate, DateTime toDate, int brandId)
        {
            var storeService = DependencyUtils.Resolve<IStoreService>();
            var stores = storeService.GetStoreByBrandId(brandId);
            return this.Get(q => q.OrderDate >= fromDate && q.OrderDate <= toDate)
                    .Join(stores, p => p.StoreId, q => q.ID, (p, q) => p);
        }

    }

    public class TodayOrderDetail
    {
        public int OrderDetailID { get; set; }
        public int StoreID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int TotalOrderDetails { get; set; }
        public double TotalAmount { get; set; }
        public double FinalAmount { get; set; }
        public int Quantity { get; set; }
    }
}

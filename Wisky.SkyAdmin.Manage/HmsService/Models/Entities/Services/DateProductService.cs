using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IDateProductService
    {
        #region SystemReport
        IQueryable<DateProduct> GetDateProductByTimeRange(DateTime startTime, DateTime endTime, int storeID);
        IQueryable<DateProduct> GetDateProductByTimeRangeAndListStore(DateTime startTime, DateTime endTime, List<int> listStoreID);
        IQueryable<DateProduct> GetDateProductByTimeRangeAndBrand(DateTime startTime, DateTime endTime, int brandId);
        IQueryable<DateProduct> GetDateProductAllStoreByTimeRange(DateTime startTime, DateTime endTime);
        IQueryable<DateProduct> GetDateProductByProductCategory(DateTime startTime, DateTime endTime, int brandID, int categoryId);
        IQueryable<DateProduct> GetDateProductByProduct(DateTime startTime, DateTime endTime, int brandID, int productId);
        #region ComparisonReport
        IQueryable<DateProductReport> GetDateProductReportByCategory(DateTime startTime, DateTime endTime, int brandID, int cateID);
        IQueryable<DateProductReport> GetDateProductReportByProduct(DateTime startTime, DateTime endTime, int brandID, int productID);
        #endregion
        #endregion
        #region StoreReport
        IQueryable<DateProduct> GetDateProductByDateAndStore(DateTime date, int storeID);
        IQueryable<DateProductReport> GetDateProductForStoreReportByProduct(DateTime startTime, DateTime endTime, int storeId, int productID);
        IQueryable<DateProductReport> GetDateProductForStoreReportByCategory(DateTime startTime, DateTime endTime, int storeId, int cateID);
        #endregion

        IQueryable<DateProductForDashBoard> GetDateProductForDashBoard(DateTime startDate, DateTime endDate, int brandId, int storeId);



    }
    public partial class DateProductService
    {
        #region SystemReport
        public IQueryable<DateProduct> GetDateProductByTimeRange(DateTime startTime, DateTime endTime, int storeID)
        {
            return this.Get(a => startTime <= a.Date && endTime >= a.Date && storeID == a.StoreID && (bool)a.Store.isAvailable);
        }
        public IQueryable<DateProduct> GetDateProductByTimeRangeAndListStore(DateTime startTime, DateTime endTime, List<int> listStoreId)
        {
            return this.Get(a => startTime <= a.Date && endTime >= a.Date && (bool)a.Store.isAvailable && listStoreId.Contains(a.StoreID));
        }
        public IQueryable<DateProduct> GetDateProductByTimeRangeAndBrand(DateTime startTime, DateTime endTime, int brandId)
        {
            return this.Get(a => startTime <= a.Date && endTime >= a.Date && a.Store.BrandId == brandId && (bool)a.Store.isAvailable);
        }
        public IQueryable<DateProduct> GetDateProductAllStoreByTimeRange(DateTime startTime, DateTime endTime)
        {
            return this.Get(a => startTime <= a.Date && endTime >= a.Date && (bool)a.Store.isAvailable);
        }
        public IQueryable<DateProduct> GetDateProductByProductCategory(DateTime startTime, DateTime endTime, int brandID, int categoryId)
        {
            return Get(q => q.Date >= startTime && q.Date <= endTime && q.Store.BrandId == brandID && q.CategoryId_.Value == categoryId);
        }
        public IQueryable<DateProduct> GetDateProductByProduct(DateTime startTime, DateTime endTime, int brandID, int productId)
        {
            return Get(q => q.Date >= startTime && q.Date <= endTime && q.Store.BrandId == brandID && q.ProductId == productId);
        }
        #endregion
        #region StoreReport
        public IQueryable<DateProduct> GetDateProductByDateAndStore(DateTime date, int storeID)
        {
            return this.Get(d => d.Date.Year == date.Year && d.Date.Month == date.Month && d.Date.Day == date.Day &&
                        d.StoreID == storeID && (bool)d.Store.isAvailable);
        }

        public IQueryable<DateProductReport> GetDateProductForStoreReportByCategory(DateTime startTime, DateTime endTime, int storeId, int cateID)
        {
            return Get(q => q.Date >= startTime && q.Date <= endTime && q.StoreID == storeId && q.CategoryId_ == cateID)
                .Select(q => new DateProductReport
                {
                    ID = q.ID,
                    ProductID = q.ProductId,
                    StoreID = q.StoreID,
                    CateID = q.CategoryId_,
                    ProductName = q.ProductName_,
                    StoreName = q.Store.Name,
                    Date = q.Date,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount
                });
        }

        public IQueryable<DateProductReport> GetDateProductForStoreReportByProduct(DateTime startTime, DateTime endTime, int storeId, int productID)
        {
            return Get(q => q.Date >= startTime && q.Date <= endTime && q.StoreID == storeId && q.ProductId == productID)
                .Select(q => new DateProductReport
                {
                    ID = q.ID,
                    ProductID = q.ProductId,
                    StoreID = q.StoreID,
                    CateID = q.CategoryId_,
                    ProductName = q.ProductName_,
                    StoreName = q.Store.Name,
                    Date = q.Date,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount
                });
        }
        #endregion

        #region ComparisonReport
        public IQueryable<DateProductReport> GetDateProductReportByCategory(DateTime startTime, DateTime endTime, int brandID, int cateID)
        {
            return GetDateProductByProductCategory(startTime, endTime, brandID, cateID)
                .Select(q => new DateProductReport
                {
                    ID = q.ID,
                    ProductID = q.ProductId,
                    StoreID = q.StoreID,
                    CateID = q.CategoryId_,
                    ProductName = q.ProductName_,
                    StoreName = q.Store.Name,
                    Date = q.Date,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount,
                    Quantity = q.Quantity
                });
        }

        public IQueryable<DateProductReport> GetDateProductReportByProduct(DateTime startTime, DateTime endTime, int brandID, int productID)
        {
            return GetDateProductByProduct(startTime, endTime, brandID, productID)
                .Select(q => new DateProductReport
                {
                    ID = q.ID,
                    ProductID = q.ProductId,
                    StoreID = q.StoreID,
                    CateID = q.CategoryId_,
                    ProductName = q.ProductName_,
                    StoreName = q.Store.Name,
                    Date = q.Date,
                    TotalAmount = q.TotalAmount,
                    FinalAmount = q.FinalAmount,
                    Quantity = q.Quantity
                });
        }
        #endregion

        public IQueryable<DateProductForDashBoard> GetDateProductForDashBoard(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            IQueryable<DateProduct> dateProductQuery;
            if (storeId != 0)
            {
                dateProductQuery = GetDateProductByTimeRange(startDate, endDate, storeId);
            }
            else
            {
                dateProductQuery = GetDateProductByTimeRangeAndBrand(startDate, endDate, brandId);
            }
            return dateProductQuery.Select(q => new DateProductForDashBoard
            {
                ID = q.ID,
                ProductID = q.ProductId,
                StoreID = q.StoreID,
                CateID = q.CategoryId_,
                ProductName = q.ProductName_,
                StoreName = q.Store.Name,
                Date = q.Date,
                TotalAmount = q.TotalAmount,
                FinalAmount = q.FinalAmount,
                TotalQuantity = q.Quantity,

                QuantityAtStore = (q.QuantityAtStore == null) ? 0 : q.QuantityAtStore.Value,
                QuantityDelivery = (q.QuantityDelivery == null) ? 0 : q.QuantityDelivery.Value,
                QuantityTakeAway = (q.QuantityTakeAway == null) ? 0 : q.QuantityTakeAway.Value,
            });
        }
    }





    public class DateProductReport
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int StoreID { get; set; }
        public int? CateID { get; set; }
        public string ProductName { get; set; }
        public string StoreName { get; set; }
        public DateTime Date { get; set; }
        public double TotalAmount { get; set; }
        public double FinalAmount { get; set; }
        public int Quantity { get; set; }
    }

    public class DateProductForDashBoard : DateProductReport
    {
        public int TotalQuantity { get; set; }
        public int QuantityAtStore { get; set; }
        public int QuantityDelivery { get; set; }
        public int QuantityTakeAway { get; set; }
    }
}

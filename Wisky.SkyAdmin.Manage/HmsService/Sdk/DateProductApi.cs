using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Models;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class DateProductApi
    {
        #region SystemReport
        public List<DateProduct> GetDateProductByTimeRange(DateTime reportDate, int storeID)
        {
            var startTime = reportDate.GetStartOfDate();
            var endTime = reportDate.AddDays(1).GetStartOfDate();
            var dateProducts = this.BaseService.GetDateProductByTimeRange(startTime, endTime, storeID)
                .ToList();
            return dateProducts;
        }

        public IEnumerable<DateProduct> GetDateProductByTimeRange(DateTime startTime, DateTime endTime, int storeID)
        {
            var dateProducts = this.BaseService.GetDateProductByTimeRange(startTime, endTime, storeID)
                //.ProjectTo<DateProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return dateProducts;
        }

        public IQueryable<DateProduct> GetDateProductByTimeRangeAndBrand(DateTime startTime, DateTime endTime, int brandId)
        {
            var dateProducts = this.BaseService.GetDateProductByTimeRangeAndBrand(startTime, endTime, brandId);
            return dateProducts;
        }
        public IQueryable<DateProduct> GetDateProductByTimeRangeAndStore(DateTime startTime, DateTime endTime, int storeId)
        {
            var dateProducts = this.BaseService.GetDateProductByTimeRange(startTime, endTime, storeId);
            return dateProducts;
        }
        public IQueryable<DateProduct> GetDateProductByTimeRangeAndListStore(DateTime startTime, DateTime endTime, List<int> listStoreId)
        {
            var dateProducts = this.BaseService.GetDateProductByTimeRangeAndListStore(startTime, endTime, listStoreId);
            return dateProducts;
        }
        //public IEnumerable<DateProduct> GetDateProductAllStoreByTimeRange(DateTime startTime, DateTime endTime)
        //{
        //    var dateProducts = this.BaseService.GetDateProductAllStoreByTimeRange(startTime, endTime)
        //        //.ProjectTo<DateProductViewModel>(this.AutoMapperConfig)
        //        .ToList();
        //    return dateProducts;
        //}

        public IQueryable<DateProduct> GetDateProductAllStoreByTimeRange(DateTime startTime, DateTime endTime)
        {
            var dateProducts = this.BaseService.GetDateProductAllStoreByTimeRange(startTime, endTime);
                //.ProjectTo<DateProductViewModel>(this.AutoMapperConfig)
                //.ToList();
            return dateProducts;
        }

        public IEnumerable<DateProduct> GetDateProductByCategory(DateTime startTime, DateTime endTime, int brandID, int cateID)
        {
            return BaseService.GetDateProductByProductCategory(startTime, endTime, brandID, cateID).ToList();
        }

        public IEnumerable<DateProduct> GetDateProductByProduct(DateTime startTime, DateTime endTime, int brandID, int productId)
        {
            return BaseService.GetDateProductByProduct(startTime, endTime, brandID, productId).ToList();
        }

        #endregion
        #region StoreReport
        public IEnumerable<DateProduct> GetDateProductByDateAndStore(DateTime date, int storeID)
        {
            var dateProducts = this.BaseService.GetDateProductByDateAndStore(date, storeID)
                //.ProjectTo<DateProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return dateProducts;
        }

        public IEnumerable<DateProductReport> GetDateProductForStoreReportByCategory(DateTime startTime, DateTime endTime, int storeId, int cateId)
        {
            return BaseService.GetDateProductForStoreReportByCategory(startTime, endTime, storeId, cateId).ToList();
        }

        public IEnumerable<DateProductReport> GetDateProductForStoreReportByProduct(DateTime startTime, DateTime endTime, int storeId, int cateId)
        {
            return BaseService.GetDateProductForStoreReportByProduct(startTime, endTime, storeId, cateId).ToList();
        }
        #endregion

        public async Task DeleteDateProductAsync(int Id)
        {
            var entity = await this.BaseService.GetAsync(Id);
            await this.BaseService.DeleteAsync(entity);
        }

        #region ComparisonReport
        public IEnumerable<DateProductReport> GetDateProductReportByCategory(DateTime startTime, DateTime endTime, int brandID, int cateID)
        {
            return BaseService.GetDateProductReportByCategory(startTime, endTime, brandID, cateID).ToList();
        }

        public IEnumerable<DateProductReport> GetDateProductReportByProduct(DateTime startTime, DateTime endTime, int brandID, int productID)
        {
            return BaseService.GetDateProductReportByProduct(startTime, endTime, brandID, productID).ToList();
        } 
        #endregion

        public IEnumerable<DateProductForDashBoard> GetDateProductForDashBoard(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            return BaseService.GetDateProductForDashBoard(startDate, endDate, brandId, storeId).ToList();
        }

        public List<DateProduct> GetDateProductByDateAndStoreId(DateTime date, int storeId)
        {
            var startDate = date.GetStartOfDate();
            var endDate = date.AddDays(1).GetEndOfDate();
            return this.BaseService.Get(q => q.StoreID == storeId && q.Date >= startDate && q.Date < endDate ).ToList();
        }

        public IQueryable<DateProductForDashBoard> GetQueryDateProductForDashBoard(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            return BaseService.GetDateProductForDashBoard(startDate, endDate, brandId, storeId);
        }
    }
}

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
    public partial class DateReportApi
    {
        #region SystemReport
        public IQueryable<DateReport> GetDateReportTimeRange(DateTime sTime, DateTime eTime)
        {
            var dateReport = this.BaseService.GetDateReportTimeRange(sTime, eTime);
            //.ProjectTo<DateReportViewModel>(this.AutoMapperConfig)
            //.ToList();
            return dateReport;
        }
        public IQueryable<DateReport> GetDateReportTimeRangeAndStore(DateTime sTime, DateTime eTime, int storeId)
        {
            var dateStoreReport = this.BaseService.GetDateReportTimeRangeAndStore(sTime, eTime, storeId);
            // .ProjectTo<DateReportViewModel>(this.AutoMapperConfig)
            //.ToList();
            return dateStoreReport;
        }

        public IQueryable<DateReport> GetDateReportTimeRangeAndListStore(DateTime sTime, DateTime eTime, List<int> listStoreId)
        {
            var dateStoreReport = this.BaseService.GetDateReportTimeRangeAndListStore(sTime, eTime, listStoreId);
            // .ProjectTo<DateReportViewModel>(this.AutoMapperConfig)
            //.ToList();
            return dateStoreReport;
        }

        public IQueryable<DateReport> GetDateReportTimeRangeAndBrand(DateTime sTime, DateTime eTime, int brandId)
        {
            var dateStoreReport = this.BaseService.GetDateReportTimeRangeAndBrand(sTime, eTime, brandId);
            // .ProjectTo<DateReportViewModel>(this.AutoMapperConfig)
            //.ToList();
            return dateStoreReport;
        }

        public async Task DeleteDateReportAsync(int Id)
        {
            var entity = await this.BaseService.GetAsync(Id);
            await this.BaseService.DeleteAsync(entity);
        }
        #endregion
        #region StoreReport
        public IEnumerable<DateReport> GetDateReport()
        {
            var dateReport = this.BaseService.GetDateReport()
                // .ProjectTo<DateReportViewModel>(this.AutoMapperConfig)
                .ToList();
            return dateReport;
        }
        public DateReport GetDateReportById(int dateReportId)
        {
            var dateReport = this.BaseService.GetDateReportById(dateReportId);
            return dateReport;
        }
        public async Task<DateReportViewModel> GetDateReportByDate(DateTime date, int storeID)
        {
            var dateReport = await this.BaseService.GetDateReportByDate(date, storeID);
            if (dateReport == null)
            {
                return null;
            }
            else
            {
                return new DateReportViewModel(dateReport);
            }
        }

        public IQueryable<DateReport> GetDateReportByDateAndBrand(DateTime reportDate, int brandId)
        {
            var startDate = reportDate.GetStartOfDate();
            var endDate = reportDate.AddDays(1).GetStartOfDate();
            var result = this.BaseService.Get(q =>
                                    q.Date >= startDate
                                    && q.Date < endDate
                                    && q.Store.BrandId == brandId);
            return result;
        }

        public IQueryable<DateReport> getAll()
        {
            var dateReports = this.BaseService.Get();
            return dateReports;
        }

        public DateReport GetStoreDateReport(DateTime date, int storeID)
        {
            return BaseService.GetStoreDateReport(date, storeID);
        }

        public IEnumerable<DateReport> GetBrandDateReport(DateTime date, int brandId)
        {
            date = date.GetEndOfDate();
            return BaseService.GetBrandReportByDate(date, brandId).ToList();
        }

        public IEnumerable<DateReport> GetStoreReportByTimeRange(DateTime startTime, DateTime endTime, int brandId, int storeId)
        {
            return BaseService.GetStoreReportByTimeRange(startTime, endTime, brandId, storeId).ToList();
        }

        public IEnumerable<DateReport> GetDateReportByTimeRange(DateTime startTime, DateTime endTime, int brandId, int storeId)
        {
            return BaseService.GetDateReportByTimeRange(startTime, endTime, brandId, storeId).ToList();
        }

        public IEnumerable<DateReport> GetStoreGroupReportByTimeRange(DateTime startTime, DateTime endTime, int brandID, int groupId)
        {
            return BaseService.GetStoreGroupReportByTimeRange(startTime, endTime, brandID, groupId).ToList();
        }

        #endregion

        public void CustomerUpdate(DateReport dateReport)
        {
            this.BaseService.Update(dateReport);
        }

        public IEnumerable<DateReport> GetReportByDate(DateTime date)
        {
            var startDate = date.GetStartOfDate();
            var endDate = date.AddDays(1).GetEndOfDate();
            var result = this.BaseService.Get(q => q.Date >= startDate && q.Date < endDate).AsEnumerable();
            return result;
        }

        public IQueryable<DateReport> GetAllDateReportByTimeRange(DateTime startTime, DateTime endTime, int storeId, int brandId)
        {
            if (storeId != 0)
            {
                return this.BaseService.Get(q => q.Date >= startTime && q.Date <= endTime && q.StoreID == storeId);
            } else
            {
                return this.BaseService.Get(q => q.Date >= startTime && q.Date <= endTime && q.Store.BrandId == brandId);
            }
        }

        public IEnumerable<DateReportForDashBoard> GetDateReportForDashboard(DateTime startDate, DateTime endDate, int brandId, int storeId)
        {
            return BaseService.GetDateReportForDashboard(startDate, endDate, brandId, storeId).ToList();
        }

        public IQueryable<DateReportForDashBoard> GetBrandDateReportForDashBoard(DateTime startDate, DateTime endDate, int brandId)
        {
            return BaseService.GetDateReportForDashboard(startDate, endDate, brandId, 0); //when storeId == 0, load all stores' reports from the chosen brand
        }
    }
}

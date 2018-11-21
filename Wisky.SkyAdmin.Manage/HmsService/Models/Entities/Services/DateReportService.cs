using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IDateReportService
    {
        #region System Report
        IQueryable<DateReport> GetDateReportTimeRange(DateTime sTime, DateTime eTime);
        IQueryable<DateReport> GetDateReportTimeRangeAndBrand(DateTime sTime, DateTime eTime, int brandId);
        IQueryable<DateReport> GetDateReportTimeRangeAndStore(DateTime sTime, DateTime eTime, int storeId);
        IQueryable<DateReport> GetDateReportTimeRangeAndListStore(DateTime sTime, DateTime eTime, List<int> listStoreId);
        IQueryable<DateReport> GetBrandReportByDate(DateTime date, int brandId);
        #endregion
        #region Store Report
        IQueryable<DateReport> GetDateReport();
        DateReport GetDateReportById(int dateReportId);
        Task<DateReport> GetDateReportByDate(DateTime date, int storeID);
        DateReport GetStoreDateReport(DateTime date, int storeID);
        IQueryable<DateReport> GetStoreReportByTimeRange(DateTime startTime, DateTime endTime, int brandID, int storeId);
        IQueryable<DateReport> GetStoreGroupReportByTimeRange(DateTime startTime, DateTime endTime, int brandID, int groupId);
        IQueryable<DateReport> GetDateReportByTimeRange(DateTime startTime, DateTime endTime, int brandID, int storeId);
        #endregion

        IQueryable<DateReportForDashBoard> GetDateReportForDashboard(DateTime startTime, DateTime endTime, int brandId, int storeId);

    }

    public partial class DateReportService
    {
        #region System Report
        public IQueryable<DateReport> GetDateReportTimeRange(DateTime sTime, DateTime eTime)
        {
            return this.Get(a => sTime <= a.Date && eTime >= a.Date);
        }
        public IQueryable<DateReport> GetDateReportTimeRangeAndStore(DateTime sTime, DateTime eTime, int storeId)
        {
            return this.Get(a => sTime <= a.Date && eTime >= a.Date && a.StoreID == storeId);
        }

        public IQueryable<DateReport> GetDateReportTimeRangeAndListStore(DateTime sTime, DateTime eTime, List<int> listStoreId)
        {
            return this.Get(a => sTime <= a.Date && eTime >= a.Date && listStoreId.Contains(a.StoreID));
        }

        public IQueryable<DateReport> GetDateReportTimeRangeAndBrand(DateTime sTime, DateTime eTime, int brandId)
        {
            return this.Get(a => sTime <= a.Date && eTime >= a.Date && a.Store.BrandId == brandId);
        }

        public IQueryable<DateReport> GetBrandReportByDate(DateTime date, int brandId)
        {
            return Get(a => a.Date == date && a.Store.BrandId == brandId);
        }
        #endregion
        #region Store Report
        public IQueryable<DateReport> GetDateReport()
        {
            return this.Get();
        }
        public DateReport GetDateReportById(int dateReportId)
        {
            return this.FirstOrDefault(dr => dr.ID == dateReportId);
        }
        public async Task<DateReport> GetDateReportByDate(DateTime date, int storeID)
        {
            return await this.FirstOrDefaultAsync(d =>
                        d.Date.Year == date.Year && d.Date.Month == date.Month && d.Date.Day == date.Day &&
                        d.StoreID == storeID);
        }
        public DateReport GetStoreDateReport(DateTime date, int storeID)
        {
            return this.FirstOrDefault(q => q.Date == date && q.StoreID == storeID);
        }
        public IQueryable<DateReport> GetDateReportByTimeRange(DateTime startTime, DateTime endTime, int brandID, int storeId)
        {
            if(storeId != 0)
            {
                return Get(q => q.Date >= startTime && q.Date <= endTime && q.StoreID == storeId);
            }
            else
            {
                return Get(q => q.Date >= startTime && q.Date <= endTime && q.Store.BrandId == brandID && (bool)q.Store.isAvailable);
            }
        }

        public IQueryable<DateReport> GetStoreReportByTimeRange(DateTime startTime, DateTime endTime, int brandID, int storeId)
        {
            return Get(q => q.Date >= startTime && q.Date <= endTime && q.Store.BrandId == brandID && q.StoreID == storeId);
        }
        public IQueryable<DateReport> GetStoreGroupReportByTimeRange(DateTime startTime, DateTime endTime, int brandID, int groupId)
        {
            var groupMappingService = DependencyUtils.Resolve<IStoreGroupMappingService>();
            var storeIds = groupMappingService.Get(q => q.StoreGroupID == groupId).Select(q => q.StoreID).ToList();

            return Get(q => q.Date >= startTime && q.Date <= endTime && q.Store.BrandId == brandID && storeIds.Contains(q.StoreID));
        }
        #endregion

        public IQueryable<DateReportForDashBoard> GetDateReportForDashboard(DateTime startTime, DateTime endTime, int brandId, int storeId)
        {
            var dateReport = GetDateReportByTimeRange(startTime, endTime, brandId, storeId).Select(q => new DateReportForDashBoard
            {
                StoreID = q.StoreID,
                StoreName = q.Store.Name,
                StoreAbbr = q.Store.ShortName,
                Date = q.Date,
                TotalAmount = q.TotalAmount,
                FinalAmount = q.FinalAmount,
                Discount = q.Discount,
                DiscountOrderDetail = q.DiscountOrderDetail,
                TotalOrder = q.TotalOrder,
                TotalOrderAtStore = q.TotalOrderAtStore,
                TotalOrderDelivery = q.TotalOrderDelivery,
                TotalOrderTakeAway = q.TotalOrderTakeAway,
                TotalOrderCard = q.TotalOrderCard,
                TotalOrderCanceled = q.TotalOrderCanceled,
                TotalOrderPreCanceled = q.TotalOrderPreCanceled,
                FinalAmountAtStore = q.FinalAmountAtStore,
                FinalAmountTakeAway = q.FinalAmountTakeAway,
                FinalAmountDelivery = q.FinalAmountDelivery,
                FinalAmountCard = q.FinalAmountCard,
                FinalAmountCanceled = q.FinalAmountCanceled,
                FinalAmountPreCanceled = q.FinalAmountPreCanceled
            });
            return dateReport;
        }
    }

    public class DateReportForDashBoard
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public string StoreAbbr { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<double> TotalAmount { get; set; }
        public Nullable<double> FinalAmount { get; set; }
        public Nullable<double> Discount { get; set; }
        public Nullable<double> DiscountOrderDetail { get; set; }
        public int TotalOrder { get; set; }
        public int TotalOrderAtStore { get; set; }
        public int TotalOrderDelivery { get; set; }
        public int TotalOrderTakeAway { get; set; }
        public int TotalOrderCard { get; set; }
        public Nullable<int> TotalOrderCanceled { get; set; }
        public Nullable<int> TotalOrderPreCanceled { get; set; }
        public Nullable<double> FinalAmountAtStore { get; set; }
        public Nullable<double> FinalAmountTakeAway { get; set; }
        public Nullable<double> FinalAmountDelivery { get; set; }
        public Nullable<double> FinalAmountCard { get; set; }
        public Nullable<double> FinalAmountCanceled { get; set; }
        public Nullable<double> FinalAmountPreCanceled { get; set; }

    }

}

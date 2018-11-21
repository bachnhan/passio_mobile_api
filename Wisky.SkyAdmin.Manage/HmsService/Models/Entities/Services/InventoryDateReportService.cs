using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IInventoryDateReportService
    {
        #region Inventory
        Task<InventoryDateReport> GetInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int storeId);
        IQueryable<InventoryDateReport> GetBrandInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int brandId);
        IQueryable<InventoryDateReport> GetListInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int storeId);
        IQueryable<InventoryDateReport> GetListInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate);
        IQueryable<InventoryDateReport> GetInventoryDateReport();
        InventoryDateReport GetLastReport(int storeId);
        IQueryable<InventoryDateReport> GetAllStoreLastReport(int brandId);
        Task<InventoryDateReport> GetReportById(int id);
        Task<InventoryDateReport> GetInventoryDateReportByDate(DateTime date, int storeId);
        IQueryable<InventoryDateReport> GetReports(DateTime date, int storeId);
       // void CreateInventoryDateReport(InventoryDateReport inventoryDateReport)
        #endregion
    }
    public partial class InventoryDateReportService
    {
        #region Inventory
        public async Task<InventoryDateReport> GetInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int storeId)
        {
            return await this.FirstOrDefaultAsync(x => x.CreateDate != null && x.CreateDate >= startDate && x.CreateDate <= endDate && x.StoreId == storeId);
        }
        public IQueryable<InventoryDateReport> GetBrandInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int brandId)
        {
            return this.Get(x => x.CreateDate != null && x.CreateDate >= startDate && x.CreateDate <= endDate && x.Store.BrandId == brandId);
        }
        public IQueryable<InventoryDateReport> GetListInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int storeId)
        {
            return this.Get(x => x.CreateDate != null && x.CreateDate >= startDate && x.CreateDate <= endDate && x.StoreId == storeId);
        }
        public IQueryable<InventoryDateReport> GetListInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate)
        {
            return this.Get(x => x.CreateDate != null && x.CreateDate >= startDate && x.CreateDate <= endDate);
        }
        public IQueryable<InventoryDateReport> GetInventoryDateReport()
        {
            return this.Get();
        }
        public InventoryDateReport GetLastReport(int storeId)
        {
            //var inventoryHistory = this.Get(x => x.StoreId == storeId).OrderByDescending(x => x.CreateDate).AsQueryable();
            //return inventoryHistory.FirstOrDefault(x => x.Status == 1);
            return this.Get(x => x.StoreId == storeId && x.Status == 1)
                .OrderByDescending(x => x.CreateDate).AsEnumerable().FirstOrDefault();
        }

        public IQueryable<InventoryDateReport> GetAllStoreLastReport(int brandId)
        {
            var query = Get(x => x.Store.BrandId == brandId && x.Status == 1).OrderByDescending(q => q.CreateDate);
            var maxDate = query.Select(q => q.CreateDate).Max();
            return query.Where(x => x.CreateDate == maxDate);
        }
        public async Task<InventoryDateReport> GetReportById(int id)
        {
            return await this.FirstOrDefaultAsync(x => x.ReportID == id);
        }
        public async Task<InventoryDateReport> GetInventoryDateReportByDate(DateTime date, int storeId)
        {
            return await this.FirstOrDefaultAsync(x => x.CreateDate != null && x.CreateDate.CompareTo(date) == 0 && x.StoreId == storeId);          
        }
        public IQueryable<InventoryDateReport> GetReports(DateTime date, int storeId)
        {
            return this.Get(x => x.StoreId == storeId && date <= x.CreateDate);
        }
        #endregion
    }
}

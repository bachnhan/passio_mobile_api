using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{
    public partial class InventoryDateReportApi
    {
        #region Inventory
        public async Task<InventoryDateReportViewModel> GetInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int storeId)
        {
            var inventoryDateReport = await this.BaseService.GetInventoryDateReportByTimeRange(startDate, endDate, storeId);
            if (inventoryDateReport == null)
            {
                return null;
            }
            else
            {
                return new InventoryDateReportViewModel(inventoryDateReport);
            }
        }

        public IEnumerable<InventoryDateReportViewModel> GetListInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int storeId)
        {
            var list = this.BaseService.GetListInventoryDateReportByTimeRange(startDate, endDate, storeId)
                .ProjectTo<InventoryDateReportViewModel>(this.AutoMapperConfig)
                .ToList();
            return list;
        }
        public IQueryable<InventoryDateReportViewModel> GetQueryInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate, int storeId, int brandId)
        {
            if (storeId == 0)
            {
                return BaseService.GetBrandInventoryDateReportByTimeRange(startDate, endDate, brandId)
                    .ProjectTo<InventoryDateReportViewModel>(AutoMapperConfig);
            }
            else
            {
                return BaseService.GetListInventoryDateReportByTimeRange(startDate, endDate, storeId)
                .ProjectTo<InventoryDateReportViewModel>(this.AutoMapperConfig);
            }
            
        }
        public IEnumerable<InventoryDateReport> GetListInventoryDateReportByTimeRangeEntity(DateTime startDate, DateTime endDate, int storeId)
        {
            var list = this.BaseService.GetListInventoryDateReportByTimeRange(startDate, endDate, storeId)
                //.ProjectTo<InventoryDateReport>(this.AutoMapperConfig)
                .ToList();
            return list;
        }
        public IEnumerable<InventoryDateReport> GetListInventoryDateReportByTimeRange(DateTime startDate, DateTime endDate)
        {
            var list = this.BaseService.GetListInventoryDateReportByTimeRange(startDate, endDate)
                //.ProjectTo<InventoryDateReport>(this.AutoMapperConfig)
                .ToList();
            return list;
        }
        public IQueryable<InventoryDateReportViewModel> GetInventoryDateReport()
        {
            var inventoryDateReports = this.BaseService.GetInventoryDateReport()
                .ProjectTo<InventoryDateReportViewModel>(this.AutoMapperConfig);
            //.ToList();
            return inventoryDateReports;
        }
        public InventoryDateReportViewModel GetLastReport(int storeId)
        {
            var lastReport = this.BaseService.GetLastReport(storeId);
            if (lastReport == null)
            {
                return null;
            }
            else
            {
                return new InventoryDateReportViewModel(lastReport);
            }
        }

        public IQueryable<InventoryDateReportViewModel> GetLastReportAllStore(int brandId)
        {
            return BaseService.GetAllStoreLastReport(brandId).ProjectTo<InventoryDateReportViewModel>(AutoMapperConfig);
        }

        public async Task<InventoryDateReportViewModel> GetReportById(int id)
        {
            var report = await this.BaseService.GetReportById(id);
            if (report == null)
            {
                return null;
            }
            else
            {
                return new InventoryDateReportViewModel(report);
            }
        }
        public async Task<InventoryDateReportViewModel> GetInventoryDateReportByDate(DateTime date, int storeId)
        {
            var inventoryDateReport = await this.BaseService.GetInventoryDateReportByDate(date, storeId);
            if (inventoryDateReport == null)
            {
                return null;
            }
            else
            {
                return new InventoryDateReportViewModel(inventoryDateReport);
            }
        }
        public IEnumerable<InventoryDateReportViewModel> GetReports(DateTime date, int storeId)
        {
            var reports = this.BaseService.GetReports(date, storeId)
                .ProjectTo<InventoryDateReportViewModel>(this.AutoMapperConfig)
                .ToList();
            return reports;
        }

        public void CreateInventoryDateReport(InventoryDateReport inventoryDateReport)
        {
            this.BaseService.Create(inventoryDateReport);
            this.BaseService.Save();
        }
        #endregion
    }
}

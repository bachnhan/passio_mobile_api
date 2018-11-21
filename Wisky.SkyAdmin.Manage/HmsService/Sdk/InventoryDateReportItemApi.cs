using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class InventoryDateReportItemApi
    {
        public IEnumerable<InventoryDateReportItemViewModel> GetItemByItemId(int itemId)
        {
            var reportItems = this.BaseService.GetItemByItemId(itemId)
                .ProjectTo<InventoryDateReportItemViewModel>(this.AutoMapperConfig)
                .ToList();
            return reportItems;
        }

        public IQueryable<InventoryDateReportItemViewModel> GetQueryItemByItemId(int itemId)
        {
            var reportItems = this.BaseService.GetItemByItemId(itemId)
                .ProjectTo<InventoryDateReportItemViewModel>(this.AutoMapperConfig);
            return reportItems;
        }

        public IQueryable<InventoryDateReportItemViewModel> GetItemByReportId(int reportId)
        {
            var reportItems = this.BaseService.GetItemByReportId(reportId)
                .ProjectTo<InventoryDateReportItemViewModel>(this.AutoMapperConfig);
                //.ToList();
            return reportItems;
        }
        public async System.Threading.Tasks.Task UpdateReportItemAsync(InventoryDateReportItemViewModel model, int itemId, int reportId)
        {
            var entity = await this.BaseService.FirstOrDefaultAsync(q => q.ItemID == itemId && q.ReportID == reportId);
            //Asign value
            entity.RealAmount = model.RealAmount;
            entity.Quantity = model.Quantity;

            await this.BaseService.UpdateReportItemAsync(entity);
        }
    }
}

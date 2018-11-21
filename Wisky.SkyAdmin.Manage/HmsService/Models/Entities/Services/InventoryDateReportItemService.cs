using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IInventoryDateReportItemService
    {
        IQueryable<InventoryDateReportItem> GetItemByItemId(int itemId);
        IQueryable<InventoryDateReportItem> GetItemByReportId(int reportId);
        System.Threading.Tasks.Task UpdateReportItemAsync(InventoryDateReportItem entity);
    }
    public partial class InventoryDateReportItemService
    {
        public IQueryable<InventoryDateReportItem> GetItemByItemId(int itemId)
        {
            return this.Get(q => q.ItemID == itemId);
        }
        public IQueryable<InventoryDateReportItem> GetItemByReportId(int reportId)
        {
            return this.Get(q => q.ReportID == reportId);
        }
        public async System.Threading.Tasks.Task UpdateReportItemAsync(InventoryDateReportItem entity)
        {
            await this.UpdateAsync(entity);
        }
    }
}

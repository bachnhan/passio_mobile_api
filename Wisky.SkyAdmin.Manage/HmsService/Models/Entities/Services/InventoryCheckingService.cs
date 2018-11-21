using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IInventoryCheckingService
    {
        #region Inventory
        IQueryable<InventoryChecking> GetInventoryChecking();
        Task<InventoryChecking> GetInventoryCheckingById(int inventoryId);
        IQueryable<InventoryChecking> GetInventoryCheckingByStoreIdAndRange(int storeId, DateTime startDate, DateTime endDate);
        System.Threading.Tasks.Task CreateInventoryChecking(InventoryChecking entity);
        #endregion
    }
    public partial class InventoryCheckingService
    {
        #region Inventory
        public IQueryable<InventoryChecking> GetInventoryChecking()
        {
            return this.Get();            
        }
        public async Task<InventoryChecking> GetInventoryCheckingById(int inventoryId)
        {
            return await this.FirstOrDefaultAsync(q => q.CheckingId == inventoryId);
        }
        public IQueryable<InventoryChecking> GetInventoryCheckingByStoreIdAndRange(int storeId, DateTime startDate, DateTime endDate)
        {
            return this.Get(q => q.StoreId == storeId && q.CheckingDate > startDate && q.CheckingDate < endDate);
        }
        public async System.Threading.Tasks.Task CreateInventoryChecking(InventoryChecking entity)
        {
            await this.CreateAsync(entity);
        }
        #endregion
    }
}

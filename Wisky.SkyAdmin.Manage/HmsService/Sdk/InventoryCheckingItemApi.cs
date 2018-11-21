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
    public partial class InventoryCheckingItemApi
    {
        #region Inventory
        public IEnumerable<InventoryCheckingItem> GetInventoryCheckingItem()
        {
            var inventoryCheckingItems = this.BaseService.GetInventoryCheckingItem();
                
            return inventoryCheckingItems;
        }

        public IEnumerable<InventoryCheckingItem> GetStoreInventoryCheckingItemByTimeRange(DateTime startTime, DateTime endTime, int storeId, int brandId)
        {
            var timeQuery = BaseService.Get(q => q.InventoryChecking.CheckingDate >= startTime && q.InventoryChecking.CheckingDate <= endTime);
            if (storeId > 0) {
                return timeQuery.Where(q => q.InventoryChecking.StoreId == storeId);
            }
            else
            {
                var stores = new StoreApi().GetActiveStoreByBrandId(brandId).Select(q => q.ID).ToList();
                return timeQuery.Where(q => stores.Contains(q.InventoryChecking.StoreId));
            }
        }

        #endregion
    }
}

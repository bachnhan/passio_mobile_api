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
    public partial class InventoryCheckingApi
    {
        #region Inventory
        public IEnumerable<InventoryChecking> GetInventoryChecking()
        {
            var inventoryChecking = this.BaseService.GetInventoryChecking()
                //.ProjectTo<InventoryCheckingViewModel>(this.AutoMapperConfig)
                .ToList();
            return inventoryChecking;
        }
        public async Task<InventoryCheckingViewModel> GetInventoryCheckingById(int inventoryId)
        {
            var inventoryChecking = await this.BaseService.GetInventoryCheckingById(inventoryId);
            if(inventoryChecking == null)
            {
                return null;
            }
            else
            {
                return new InventoryCheckingViewModel(inventoryChecking);
            }
        }
        public IEnumerable<InventoryCheckingViewModel> GetInventoryCheckingByStoreIdAndRange(int storeId, DateTime startDate, DateTime endDate)
        {
            var inventories = this.BaseService.GetInventoryCheckingByStoreIdAndRange(storeId, startDate, endDate)
                .ProjectTo<InventoryCheckingViewModel>(this.BaseService)
                .ToList();
            return inventories;
        }
        public async System.Threading.Tasks.Task CreateInventoryChecking(InventoryChecking model)
        {            
            await this.BaseService.CreateInventoryChecking(model);
        }
        #endregion
    }
}

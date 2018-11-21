using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IInventoryCheckingItemService
    {
        #region Inventory
        IQueryable<InventoryCheckingItem> GetInventoryCheckingItem();
        #endregion
    }
    public partial class InventoryCheckingItemService
    {
        #region Inventory
        public IQueryable<InventoryCheckingItem> GetInventoryCheckingItem()
        {
            return this.Get();
        }
        #endregion
    }
}

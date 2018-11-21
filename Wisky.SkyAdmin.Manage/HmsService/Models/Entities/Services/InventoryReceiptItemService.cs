using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IInventoryReceiptItemService
    {
        #region Inventory
        IQueryable<InventoryReceiptItem> GetReceiptItems();
        IQueryable<InventoryReceiptItem> GetItemReceiptById(int ReceiptId);
        IQueryable<InventoryReceiptItem> GetItemReceiptByProviderIdAndTimeRange(int ProviderId, DateTime startTime, DateTime endTime);
        #endregion
    }
    public partial class InventoryReceiptItemService
    {
        #region Inventory
        public IQueryable<InventoryReceiptItem> GetReceiptItems()
        {
            return this.Get();
        }
        public IQueryable<InventoryReceiptItem> GetItemReceiptById(int ReceiptId)
        {
            return this.Get(q => q.ReceiptID == ReceiptId);
        }
        public IQueryable<InventoryReceiptItem> GetItemReceiptByProviderIdAndTimeRange(int ProviderId, DateTime startTime, DateTime endTime )
        {
            return this.Get(q => q.InventoryReceipt.ProviderId == ProviderId && q.InventoryReceipt.ChangeDate >= startTime && q.InventoryReceipt.ChangeDate <= endTime);
        }
        #endregion
    }
}

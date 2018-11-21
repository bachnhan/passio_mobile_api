using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IInventoryReceiptService
    {
        #region Inventory
        IQueryable<InventoryReceipt> GetInventoryReceiptByTimeRange(int storeId, DateTime StartTime, DateTime EndTime);
        IQueryable<InventoryReceipt> GetInventoryReceiptByTime(DateTime StartTime, DateTime EndTime);
        IQueryable<InventoryReceipt> GetInventoryReceiptByStore(int storeId);
        IQueryable<InventoryReceipt> GetImportInventoryReceiptByStore(int storeId);
        IQueryable<InventoryReceipt> GetExportInventoryReceiptByStore(int storeId);
        IQueryable<InventoryReceipt> GetInStoreInventoryReceiptByStore(int storeId);
        IQueryable<InventoryReceipt> GetOutStoreInventoryReceiptByStore(int storeId);
        #endregion
        #region ProductInventory
        IQueryable<InventoryReceipt> GetInventoryReceipts();
        Task<InventoryReceipt> GetInventoryReceiptById(int productInventoryId);
        string GetChangeDateOfInventoryReceipt(int inventoryReceiptId);
        #endregion
    }
    public partial class InventoryReceiptService
    {
        #region Inventory
        public IQueryable<InventoryReceipt> GetInventoryReceiptByTimeRange(int storeId, DateTime StartTime, DateTime EndTime)
        {
            return this.Get(p => p.CreateDate >= StartTime && p.CreateDate <= EndTime &&
                (p.StoreId == storeId || (p.InStoreId != null && p.InStoreId == storeId)));
        }
        public IQueryable<InventoryReceipt> GetInventoryReceiptByTime(DateTime StartTime, DateTime EndTime)
        {
            return this.Get(p => p.ChangeDate >= StartTime && p.ChangeDate <= EndTime);
        }
        public IQueryable<InventoryReceipt> GetInventoryReceiptByStore(int storeId)
        {
            return this.Get(a => a.StoreId == storeId);
        }
        public IQueryable<InventoryReceipt> GetImportInventoryReceiptByStore(int storeId)
        {
            return this.Get(a => a.StoreId == storeId && a.ReceiptType == 0 && (a.ProviderId != 0 || a.ProviderId != null));
        }
        public IQueryable<InventoryReceipt> GetExportInventoryReceiptByStore(int storeId)
        {
            return this.Get(a => a.StoreId == storeId && (a.ReceiptType == 3 || a.ReceiptType == 5));
        }
        public IQueryable<InventoryReceipt> GetInStoreInventoryReceiptByStore(int storeId)
        {
            return this.Get(a => a.InStoreId != null && a.InStoreId == storeId);
        }
        public IQueryable<InventoryReceipt> GetOutStoreInventoryReceiptByStore(int storeId)
        {
            return this.Get(a => a.StoreId == storeId && a.InStoreId != null);
        }
        #endregion
        #region ProductInventory
        public IQueryable<InventoryReceipt> GetInventoryReceipts()
        {
            return this.Get();
        }
        public async Task<InventoryReceipt> GetInventoryReceiptById(int productInventoryId)
        {
            return await this.FirstOrDefaultAsync(x => x.ReceiptID == productInventoryId);
        }
        public string GetChangeDateOfInventoryReceipt(int inventoryReceiptId)
        {
            var result = this.FirstOrDefault(q => q.ReceiptID == inventoryReceiptId);
            return result.ChangeDate.ToString("dd/MM/yyyy");
        }
        #endregion
    }
}

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
    public partial class InventoryReceiptApi
    {
        #region Inventory
        public IEnumerable<InventoryReceipt> GetInventoryReceiptByTimeRange(int storeId, DateTime StartTime, DateTime EndTime)
        {
            var inventoryReceipts = this.BaseService.GetInventoryReceiptByTimeRange(storeId, StartTime, EndTime)
                //.ProjectTo<InventoryReceiptViewModel>(this.AutoMapperConfig)
                .ToList();
            return inventoryReceipts;
        }
        public IEnumerable<InventoryReceipt> GetInventoryReceiptByTime(DateTime StartTime, DateTime EndTime)
        {
            var inventoryReceipt = this.BaseService.GetInventoryReceiptByTime(StartTime, EndTime)
                .ToList();
            return inventoryReceipt;
        }
        public IEnumerable<InventoryReceipt> GetInventoryReceiptByStore(int storeId)
        {
            var inventoryReceipt = this.BaseService.GetInventoryReceiptByStore(storeId)
                //.ProjectTo<InventoryReceiptViewModel>(this.AutoMapperConfig)
                .ToList();
            return inventoryReceipt;
        }
        public IEnumerable<InventoryReceiptViewModel> GetImportInventoryReceiptByStore(int storeId)
        {
            var inventoryReceipt = this.BaseService.GetImportInventoryReceiptByStore(storeId)
                .ProjectTo<InventoryReceiptViewModel>(this.AutoMapperConfig)
                .ToList();
            return inventoryReceipt;
        }
        public IEnumerable<InventoryReceiptViewModel> GetExportInventoryReceiptByStore(int storeId)
        {
            var inventoryReceipt = this.BaseService.GetExportInventoryReceiptByStore(storeId)
                .Select(q => new InventoryReceiptViewModel()
                {
                    ReceiptID = q.ReceiptID,
                    ReceiptType = q.ReceiptType,
                    ChangeDate = q.ChangeDate,
                    Status = q.Status,
                    Notes = q.Notes,
                    Name = q.Name,
                    Creator = q.Creator,
                    StoreId = q.StoreId,
                    CreateDate = q.CreateDate,
                    Amount = q.Amount
                })
                .ToList();
            return inventoryReceipt;
        }
        public IEnumerable<InventoryReceiptViewModel> GetInStoreInventoryReceiptByStore(int storeId)
        {
            var inventoryReceipt = this.BaseService.GetInStoreInventoryReceiptByStore(storeId)
                .Select(q => new InventoryReceiptViewModel()
                {
                    ReceiptID = q.ReceiptID,
                    ReceiptType = q.ReceiptType,
                    ChangeDate = q.ChangeDate,
                    Status = q.Status,
                    Notes = q.Notes,
                    Name = q.Name,
                    OutStoreId = q.OutStoreId,
                    Creator = q.Creator,
                    StoreId = q.StoreId,
                    CreateDate = q.CreateDate,
                    Amount = q.Amount
                })
                .ToList();
            return inventoryReceipt;
        }
        public IEnumerable<InventoryReceiptViewModel> GetOutStoreInventoryReceiptByStore(int storeId)
        {
            var inventoryReceipt = this.BaseService.GetOutStoreInventoryReceiptByStore(storeId)
                .Select(q => new InventoryReceiptViewModel()
                {
                    ReceiptID = q.ReceiptID,
                    ReceiptType = q.ReceiptType,
                    ChangeDate = q.ChangeDate,
                    Status = q.Status,
                    Notes = q.Notes,
                    Name = q.Name,
                    InStoreId = q.InStoreId,
                    Creator = q.Creator,
                    StoreId = q.StoreId,
                    CreateDate = q.CreateDate,
                    Amount = q.Amount
                })
                .ToList();
            return inventoryReceipt;
        }        
        #endregion
        #region ProductInventory
        public IEnumerable<InventoryReceiptViewModel> GetInventoryReceipts()
        {
            var inventoryReceipt = this.BaseService.GetInventoryReceipts()
                .ProjectTo<InventoryReceiptViewModel>(this.AutoMapperConfig)
                .ToList();
            return inventoryReceipt;
        }
        public async Task<InventoryReceiptViewModel> GetInventoryReceiptById(int productInventoryId)
        {
            var inventoryReceipt = await this.BaseService.GetInventoryReceiptById(productInventoryId);
            if (inventoryReceipt == null)
            {
                return null;
            }
            else
            {
                return new InventoryReceiptViewModel(inventoryReceipt);
            }
        }
        public IQueryable<InventoryReceipt> GetInventoryReceiptByIdIqueryable(int receiptId)
        {
            var inventoryReceipt = this.BaseService.GetInventoryReceipts().Where(q=> q.ReceiptID == receiptId);
            if (inventoryReceipt == null)
            {
                return null;
            }
            else
            {
                return inventoryReceipt;
            }
        }
        public string GetChangeDateOfInventoryReceipt(int inventoryReceiptId)
        {
            string date = this.BaseService.GetChangeDateOfInventoryReceipt(inventoryReceiptId);
            if(date == null)
            {
                return null;
            }
            return date;
        }
        #endregion
    }
}

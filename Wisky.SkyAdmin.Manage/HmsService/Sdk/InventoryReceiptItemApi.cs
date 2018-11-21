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
    public partial class InventoryReceiptItemApi
    {
        #region Inventory
        public IQueryable<InventoryReceiptItem> GetReceiptItems()
        {
            var receiptItems = this.BaseService.GetReceiptItems();
                //.ProjectTo<InventoryReceiptItemViewModel>(this.AutoMapperConfig)
                //.ToList();
            return receiptItems;
        }
        public IEnumerable<InventoryReceiptItemViewModel> GetItemReceiptById(int ReceiptId)
        {
            var itemReceipt = this.BaseService.GetItemReceiptById(ReceiptId)
                .ProjectTo<InventoryReceiptItemViewModel>(this.AutoMapperConfig)
                .ToList();
            return itemReceipt;           
        }
        public IEnumerable<InventoryReceiptItemViewModel> GetItemReceiptByProviderIdAndTimeRange(int ProviderId, DateTime startTime, DateTime endTime)
        {
            var itemReceipt = this.BaseService.GetItemReceiptByProviderIdAndTimeRange(ProviderId, startTime, endTime)
                .ProjectTo<InventoryReceiptItemViewModel>(this.AutoMapperConfig)
                .ToList();
            return itemReceipt;
        }
        #endregion
    }
}

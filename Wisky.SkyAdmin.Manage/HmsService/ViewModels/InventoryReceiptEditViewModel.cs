using AutoMapper;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class InventoryReceiptEditViewModel : InventoryReceiptViewModel
    {
        public InventoryReceiptEditViewModel() { }
        public InventoryReceiptEditViewModel(IEnumerable<InventoryReceiptViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public InventoryReceiptEditViewModel(InventoryReceiptViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }

        public IEnumerable<InventoryReceiptItemViewModel> InventoryReceiptItem { get; set; }
        public IEnumerable<SelectListItem> AvailableProvider { get; set; }
        public IEnumerable<SelectListItem> AvailableCreator { get; set; }
        public IEnumerable<SelectListItem> AvailableItemCategory { get; set; }
        public IEnumerable<SelectListItem> AvailableStore { get; set; }
        public string OutStoreName { get; set; }
        public string InStoreName { get; set; }
    }

    public class ImportExportItemModel
    {
        public string Notes { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public string InvoiceNumber { get; set; }
        public int InStoreId { get; set; }
        public int OutStoreId { get; set; }
        public int ProviderId { get; set; }
        public int ReceiptTypeId { get; set; }
        public IEnumerable<ReceiptItem> ReceiptItems { get; set; }
        public string ExportDate { get; set; }
        public string ImportDate { get; set; }
        public double Amount { get; set; }

    }

    public class ReceiptItem
    {
        public int Id { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public string UnitVal { get; set; }
        public string ExpDate { get; set; }

    }
}
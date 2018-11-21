using AutoMapper;
using HmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class ProductItemEditViewModel : ProductItemViewModel
    {
        public ProductItemEditViewModel() : base() { }

        public ProductItemEditViewModel(ProductItemViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }

        public ProductItemViewModel Item { get; set; }
        public IEnumerable<ProductItemCategoryViewModel> ItemCateList { get; set; }
        public int? CheckedQuantity { get; set; }// So san pham o thoi diem kiem tra

        public int? HistoryId { get; set; }
        public int? Index { get; set; }
        public int? SoldQuantity { get; set; }// So san pham da ban den thoi diem
        public int? InventoryQuantity { get; set; }//So san pham nhap xuat kho
        public int? RealisticQuantity { get; set; } //So san pham thuc te
        public List<HistoryItemActivity> ListHistoryActivity { get; set; } // history of sold, import, export, checkinventory
        public IEnumerable<ProviderViewModel> Providers { get; set; }
        public IEnumerable<SelectListItem> AvailableCate { get; set; }
        public IEnumerable<SelectListItem> AvailableItemType { get; set; }
        public IEnumerable<SelectListItem> AvailableProvider { get; set; }
        public string[] SelectedProviders { get; set; }
        public ProductItemType ProductItemType { get; set; }
        public string SelectedImage { get; set; }

        public HttpPostedFileBase uploadImage { get; set; }
    }
    public class HistoryItemActivity
    {
        public string Message { get; set; }
        public int Quantity { get; set; }
        public int CurrentQuantityItem { get; set; }
        public DateTime DateActivity { get; set; }
    }
}

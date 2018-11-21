using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class InventoryReceiptViewModel: BaseEntityViewModel<InventoryReceipt>
    {
        public ProviderViewModel Provider { get; set; }
        //public ProductItemViewModel ProductItem { get; set; }
        //public InventoryReceiptItemViewModel InventoryReceiptItems { get; set; }
    }
}

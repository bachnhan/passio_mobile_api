using HmsService.Models.Entities;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class InventoryDateReportViewModel: BaseEntityViewModel<InventoryDateReport>
    {
        public IEnumerable<InventoryDateReportItemViewModel> InventoryDateReportItems { get; set; }
        public StoreViewModel Store { get; set; }
    }
}

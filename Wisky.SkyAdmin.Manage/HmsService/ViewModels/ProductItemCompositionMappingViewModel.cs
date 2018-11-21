using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class ProductItemCompositionMappingViewModel: BaseEntityViewModel<ProductItemCompositionMapping>
    {
        public ProductItemViewModel ProductItem { get; set; }
    }
}

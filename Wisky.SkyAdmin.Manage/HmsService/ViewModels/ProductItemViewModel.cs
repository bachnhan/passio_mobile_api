using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class ProductItemViewModel
    {
        public ProductItemCategoryViewModel ItemCategory { get; set; }
        public string CateName { get; set; }
        public double? Quantity { get; set; }     
    }
}

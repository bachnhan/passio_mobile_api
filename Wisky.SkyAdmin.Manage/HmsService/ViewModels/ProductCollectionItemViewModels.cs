using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    
    public partial class ProductCollectionItemMappingViewModel
    {
        public ProductViewModel Product { get; set; }
        public IEnumerable<ProductSpecificationViewModel> ProductSpecifications { get; set; }
    }

    public class ProductCollectionDetailsViewModel : BaseEntityViewModel<ProductCollectionDetails>
    {
        public ProductCollectionViewModel ProductCollection { get; set; }
        public IEnumerable<ProductCollectionItemMappingViewModel> Items { get; set; }
        
        public ProductCollectionDetailsViewModel() : base() { }
        public ProductCollectionDetailsViewModel(ProductCollectionDetails entity) : base(entity) { }
        
    }

}

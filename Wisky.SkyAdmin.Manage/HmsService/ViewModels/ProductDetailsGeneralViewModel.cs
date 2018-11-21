using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ProductDetailsGeneralViewModel
    {
        public ProductDetailsViewModel ProductDetails { get; set; }
        public IEnumerable<ProductViewModel> RelatedProducts { get; set; }
        public ImageCollectionDetailsViewModel Banner { get; set; }
    }
}

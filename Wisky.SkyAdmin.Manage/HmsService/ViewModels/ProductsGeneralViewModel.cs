using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ProductsGeneralViewModel
    {
        public ProductCategoryDetailsWithProductDetailsViewModel Category { get; set; }
        public ImageCollectionDetailsViewModel Banner { get; set; }
        public ProductCollectionDetailsViewModel Suggestions { get; set; }

        //Get all product in Category by seoname.
        public ProductCategoryDetailsViewModel CategoryDetails { get; set; }

        public IEnumerable<ProductCategoryViewModel> AllCategories { get; set; }

        public IEnumerable<ProductViewModel> AllProducts { get; set; }

        public IEnumerable<ProductCategoryTreeViewModel> AllCategoryWithoutExtra { get; set; }

    }
}

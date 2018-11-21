using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{

    public partial class ProductCategoryDetailsViewModel : BaseEntityViewModel<ProductCategoryDetails>
    {
        public ProductCategoryViewModel Category { get; set; }
        public IEnumerable<ProductViewModel> Products { get; set; }

        public ProductCategoryDetailsViewModel() : base() { }
        public ProductCategoryDetailsViewModel(ProductCategoryDetails entity) : base(entity) { }

    }

    // author: DucBM
    public partial class ProductCategoryDetailsWithProductDetailsViewModel : BaseEntityViewModel<ProductCategoryDetailsWithProductDetails>
    {
        public ProductCategoryViewModel Category { get; set; }
        public IEnumerable<ProductDetailsViewModel> Products { get; set; }

        public ProductCategoryDetailsWithProductDetailsViewModel() : base() { }
        public ProductCategoryDetailsWithProductDetailsViewModel(ProductCategoryDetailsWithProductDetails entity) : base(entity) { }

    }

    public partial class ProductCategoryTreeViewModel : BaseEntityViewModel<ProductCategoryTree>
    {

        public ProductCategoryTreeViewModel() : base() { }

        // DO NOT base on the parent's constructor, it uses AutoMapper to automap, which causes StackOverflow
        public ProductCategoryTreeViewModel(ProductCategoryTree entity) : base()
        {
            this.Category = new ProductCategoryViewModel(entity.Category);
            this.Subcategories = entity.Subcategories.Select(q => new ProductCategoryTreeViewModel(q));
        }

        public ProductCategoryViewModel Category { get; set; }
        public IEnumerable<ProductCategoryTreeViewModel> Subcategories { get; set; }

    }
}

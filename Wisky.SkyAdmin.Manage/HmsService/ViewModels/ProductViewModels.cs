using AutoMapper;
using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyWeb.DatVM.Mvc;
using HmsService.Models.Entities.Services;

namespace HmsService.ViewModels
{
    public partial class ProductViewModel: BaseEntityViewModel<Product>
    {

        public string CateName { get; set; }

        public double? DiscountPriceEdit
        {
            get
            {
                return this.DiscountPrice;
            }
            set
            {
                if (value.HasValue)
                {
                    this.DiscountPrice = value.Value;
                }
                else
                {
                    this.DiscountPrice = 0;
                }
            }
        }

        public string FormattedDiscountPrice
        {
            get
            {
                if (this.Price == 0)
                {
                    return null;
                }
                else
                {
                    return this.DiscountPrice.ToString("#,#.#");
                }
            }
        }

        public double PrimaryPrice
        {
            get
            {
                if (this.Price != 0 && this.DiscountPrice != 0)
                {
                    return this.DiscountPrice;
                }
                return this.Price;
            }
        }

        public string FormattedPrice
        {
            get
            {
                if (this.Price == 0)
                {
                    return null;
                }
                else
                {
                    return this.Price.ToString("#,#.#");
                }
            }
        }

        public ProductCategoryViewModel ProductCategory { get; set; }
        public IEnumerable<ProductItemCompositionMappingViewModel> Composition { get; set; }
        public IEnumerable<ProductViewModel> ExtraProduct { get; set; }
        public IEnumerable<ProductCategoryViewModel> ExtraGroup { get; set; }
        public IEnumerable<ProductComboDetailViewModel> ProductComboDetails { get; set; }
    }


    public partial class ProductDetailsViewModel : BaseEntityViewModel<ProductDetails>
    {
        public ProductViewModel Product { get; set; }
        public ProductViewModel GeneralProduct { get; set; }
        public ProductCategoryViewModel ProductCategory { get; set; }
        public IEnumerable<ProductImageViewModel> ProductImages { get; set; }
        public IEnumerable<ProductSpecificationViewModel> ProductSpecifications { get; set; }
        public IEnumerable<ProductCollectionViewModel> ProductCollections { get; set; }
        public IEnumerable<ProductComboDetailViewModel> ProductComboDetails { get; set; }


        public ProductDetailsViewModel() : base()
        {
            Product = new ProductViewModel();
        }
        public ProductDetailsViewModel(ProductDetails productDetail) : base(productDetail)
        {
            ProductCategory = new ProductCategoryViewModel(productDetail.Product.ProductCategory);
        }
        public ProductDetailsViewModel(ProductDetailsViewModel source)
        {
            SkyWeb.DatVM.Mvc.Autofac.DependencyUtils.Resolve<IMapper>().Map(source, this);
        }

    }

    public partial class ProductSpecViewModel
    {
        public virtual int ProductID { get; set; }
        public virtual string ProductName { get; set; }
        public virtual double Price { get; set; }
        public virtual string PicURL { get; set; }
        public virtual int CatID { get; set; }
        public string Description { get; set; }
        public virtual string SeoName { get; set; }
        public virtual string SeoKeyWords { get; set; }
        public virtual string SeoDescription { get; set; }
        public virtual Nullable<int> Position { get; set; }
        public IEnumerable<ProductSpecificationViewModel> ProductSpecifications { get; set; }

    }
}

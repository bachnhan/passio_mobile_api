using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;

namespace HmsService.Sdk
{
    public partial class ProductItemApi
    {
        #region StoreReport
        public IQueryable<ProductItem> GetAvailableProductItems()
        {
            var productItems = this.BaseService.GetAvailableProductItems();
            //.ProjectTo<ProductItemViewModel>(this.AutoMapperConfig)
            //.ToList();
            return productItems;
        }


        public IQueryable<ProductItem> GetAvailableProductItemsByBrand(int brandId)
        {
            var productItems = this.BaseService.GetAvailableProductItemsbyBrand(brandId);
            //.ProjectTo<ProductItemViewModel>(this.AutoMapperConfig)
            //.ToList();
            return productItems;
        }
        public IQueryable<ProductItemViewModel> GetListAvailableProductItemByCategoryBrandId(int brandId)
        {
            var productItems = this.BaseService.GetAvailableProductItems().Where(p => p.ProductItemCategory.BrandId == brandId).ProjectTo<ProductItemViewModel>(this.AutoMapperConfig);
            return productItems;
        }
        public IQueryable<ProductItemViewModel> GetAvailableProductItemsModelByBrand(int brandId)
        {
            var productItems = this.BaseService.GetAvailableProductItemsbyBrand(brandId)
                .ProjectTo<ProductItemViewModel>(this.AutoMapperConfig);
            return productItems;
        }
        public IQueryable<ProductItemViewModel> GetProductItems()
        {
            var productItems = this.BaseService.GetProductItems()
                .ProjectTo<ProductItemViewModel>(this.AutoMapperConfig);
            //.ToList();
            return productItems;
        }
        public IQueryable<ProductItem> GetProductItemsEntity()
        {
            var productItems = this.BaseService.GetProductItems();
            //.ProjectTo<ProductItemViewModel>(this.AutoMapperConfig);
            //.ToList();
            return productItems;
        }
        #endregion
        #region Inventory
        public async Task<ProductItemViewModel> GetProductItemById(int productItemId)
        {
            var productItem = await this.BaseService.GetProductItemById(productItemId);
            if (productItem == null)
            {
                return null;
            }
            else
            {
                return new ProductItemViewModel(productItem);
            }
        }

        public async Task EditProductItemAsync(ProductItemViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.ItemID);
            entity.ItemName = model.ItemName;
            entity.Unit = model.Unit;
            entity.Unit2 = model.Unit2;
            entity.UnitRate = model.UnitRate;
            entity.Price = model.Price;
            entity.CatID = model.CatID;
            entity.IndexPriority = model.IndexPriority;
            entity.ImageUrl = model.ImageUrl;
            entity.ItemType = model.ItemType;
            entity.ItemCode = model.ItemCode;
            await this.BaseService.UpdateAsync(entity);
        }
        #endregion
        #region Provider
        public IEnumerable<ProductItemViewModel> GetProductItemsByCategoryId(int productCategoryId)
        {
            var productItems = this.BaseService.GetProductItemsByCategoryId(productCategoryId)
                .ProjectTo<ProductItemViewModel>(this.AutoMapperConfig)
                .ToList();
            return productItems;
        }
        #endregion

        public IEnumerable<DateProductItem> GetProductItemByDate(IEnumerable<DateProduct> listProduct)
        {
            return this.BaseService.GetProductItemByDate(listProduct);
        }


        public async Task CreateProductItem(ProductItemEditViewModel model)
        {
            var entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);
            var ProviderItemMappingService = this.Service<IProviderProductItemMappingService>();
            var ProductItemCateService = this.Service<IProductItemCategoryService>();

            foreach (var provider in model.SelectedProviders)
            {
                var providerMapping = new ProviderProductItemMapping()
                {
                    ProductItemID = entity.ItemID,
                    ProviderID = int.Parse(provider),
                    Active = true,
                };
                await ProviderItemMappingService.CreateAsync(providerMapping);
            }
        }

        public IQueryable<ProductItemViewModel> GetProductItemByStore(int storeId, int brandId)
        {
            return this.BaseService.GetProductItemByStore(storeId, brandId)
                .ProjectTo<ProductItemViewModel>(this.AutoMapperConfig);
        }

    }
}

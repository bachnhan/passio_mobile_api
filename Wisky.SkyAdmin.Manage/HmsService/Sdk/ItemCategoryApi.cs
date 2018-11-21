using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class ProductItemCategoryApi
    {
        #region Inventory
        public IEnumerable<ProductItemCategoryViewModel> GetItemCategories()
        {
            var itemCategories = this.BaseService.GetItemCategories()
                .ProjectTo<ProductItemCategoryViewModel>(this.AutoMapperConfig)
                .ToList();
            return itemCategories;
        }

        public IQueryable<ProductItemCategoryViewModel> GetAllCategories()
        {
            return this.BaseService.GetItemCategories()
                .ProjectTo<ProductItemCategoryViewModel>(this.AutoMapperConfig);
        }

        //public void DeactiveItemCategory(int id)
        //{
        //    this.BaseService.DeactiveItemCategory(id);
        //}

        public void UpdateItemCategory(ProductItemCategoryViewModel model)
        {
            var entity = model.ToEntity();
            this.BaseService.UpdateItemCategory(entity);
        }
        #endregion
        #region Product-Item
        public async Task<ProductItemCategoryViewModel> GetItemCategoryById(int itemCategoryId)
        {
            var itemCategories = await this.BaseService.GetItemCategoryById(itemCategoryId);
            if(itemCategories == null)
            {
                return null;
            }
            else
            {
                return new ProductItemCategoryViewModel(itemCategories);
            }
        }

        public IQueryable<ProductItemCategoryViewModel> GetItemCategoryByBrand(int brandId)
        {
            return this.BaseService.GetItemCategoryByBrand(brandId).ProjectTo<ProductItemCategoryViewModel>(this.AutoMapperConfig);
        }
        #endregion
    }
}

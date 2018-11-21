using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IProductItemCategoryService
    {
        #region Inventory
        IQueryable<ProductItemCategory> GetItemCategories();
        //void DeactiveItemCategory(int id);
        void UpdateItemCategory(ProductItemCategory entity);
        #endregion
        #region Product-Item
        Task<ProductItemCategory> GetItemCategoryById(int itemCategoryId);
        IQueryable<ProductItemCategory> GetItemCategoryByBrand(int brandId);
        #endregion
    }
    public partial class ProductItemCategoryService
    {
        #region Inventory
        public IQueryable<ProductItemCategory> GetItemCategories()
        {
            return this.Get().Where(a=>a.Active==true);
        }
        //public void DeactiveItemCategory(int id)
        //{
        //    var entity = this.Get(id);
        //    entity.IsDisplayed = false;
        //    this.Save();
        //}
        public void UpdateItemCategory(ProductItemCategory entity)
        {
            Update(entity);
        }
        #endregion
        #region Product-Item
        public async Task<ProductItemCategory> GetItemCategoryById(int itemCategoryId)
        {
            return await this.GetAsync(itemCategoryId);
        }
        public IQueryable<ProductItemCategory> GetItemCategoryByBrand(int brandId)
        {
            return this.Get(q => q.BrandId == brandId && q.Active);
        }
        #endregion
    }
}

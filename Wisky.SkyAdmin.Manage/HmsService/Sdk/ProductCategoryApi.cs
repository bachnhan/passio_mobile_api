using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using System.Data.Entity;
using SkyWeb.DatVM.Mvc;
using HmsService.Models.Entities;
using System.Data.Entity.Validation;
using HmsService.Models;
using static HmsService.Models.Entities.Services.ProductCategoryService;

namespace HmsService.Sdk
{
    public partial class ProductCategoryApi
    {
        public async Task<IEnumerable<ProductCategoryViewModel>> GetByStoreIdAsync(int storeId)
        {
            return await this.BaseService.GetByStoreId(storeId)
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCategoryViewModel>> GetByStoreIdEditAsync(int storeId, int cateId)
        {
            return await this.BaseService.GetByStoreIdEdit(storeId, cateId)
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCategoryViewModel>> GetByBrandIdEditAsync(int brandId, int cateId)
        {
            return await this.BaseService.GetByBrandId(brandId, cateId)
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public ProductCategory GetProductCategoryEntityById(int categoryId)
        {
            var category = this.BaseService.GetProductCategoryEntityById(categoryId);

            return category;
        }
        public IEnumerable<ProductCategory> GetByBrandId(int brandId)
        {
            return this.BaseService.GetByBrandId(brandId).ToList();
        }

        //public async Task<IEnumerable<ProductCategoryViewModel>> GetByStoreIdAsync(int storeId)
        //{
        //    return await this.BaseService.GetByStoreId(storeId)
        //        .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
        //        .ToListAsync();
        //}

        public async Task<ProductCategoryDetailsViewModel> GetCategoryDetails(string seoName, int storeId)
        {
            var entity = await this.BaseService.GetDetailsBySeoNameAsync(seoName, storeId);

            var result = new ProductCategoryDetailsViewModel(entity);
            result.Products = entity.Products
                .Where(a => a.Active)
                .ProjectTo<ProductViewModel>(this.AutoMapperConfig);
            return result;
        }

        public async Task<ProductCategoryDetailsViewModel> GetCategoryDetailsById(int? id)
        {
            var entity = await this.BaseService.GetDetailsByIdAsync(id);

            var result = new ProductCategoryDetailsViewModel(entity);
            result.Products = entity.Products
                .Where(a => a.Active)
                .ProjectTo<ProductViewModel>(this.AutoMapperConfig);
            return result;
        }

        // author: DucBM
        public async Task<ProductCategoryDetailsWithProductDetailsViewModel> GetCategoryDetailsWithProductDetails(string seoName, int storeId)
        {
            var entity = await this.BaseService.GetDetailsWithProductDetailsBySeoNameAsync(seoName, storeId);

            var result = new ProductCategoryDetailsWithProductDetailsViewModel(entity);
            result.Products = entity.Products
                .Where(a => a.Product.Active)
                .ProjectTo<ProductDetailsViewModel>(this.AutoMapperConfig);
            return result;
        }

        public async Task<bool> ValidateStoreCategory(int categoryId, int storeId)
        {
            var category = await this.BaseService.GetByStoreIdAsync(categoryId, storeId);
            return category != null;
        }

        public bool ValidateBrandCategory(int categoryId, int brandId)
        {
            return this.BaseService.ValidateBrandCategory(categoryId, brandId);
        }

        public PagingViewModel<ProductCategoryViewModel> GetAdminWithFilterAsync(int brandId, string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var list = this.BaseService.GetAdminByStoreWithFilter(brandId, keyword, sortKeyAsc)
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<ProductCategoryViewModel>(list);
        }

        public async Task<ProductCategoryViewModel> GetByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ProductCategoryViewModel(entity);
            }
        }

        public async Task<ProductCategoryViewModel> GetByIdAndBrand(int id, int brandId)
        {
            var entity = await this.BaseService.GetActiveByIdAndBrandAsync(id, brandId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ProductCategoryViewModel(entity);
            }
        }

        public IEnumerable<ProductCategoryTreeViewModel> GetFullTreeByStore(int storeId)
        {
            var result = this.BaseService.GetFullTreeByStoreId(storeId).AsQueryable();
            //var result = this.BaseService.GetFullTreeByBrand(storeId).AsQueryable();

            // Map manually, do not use Auto Mapper to prevent Stack Overflow
            //return result.ProjectTo<ProductCategoryTreeViewModel>(this.AutoMapperConfig);

            return result.Select(q => new ProductCategoryTreeViewModel(q));
        }

        public IEnumerable<ProductCategoryTreeViewModel> GetFullTreeByBrandWithoutExtra(int brandId)
        {
            var result = this.BaseService.GetFullTreeByStoreIdWithoutExtra(brandId).AsQueryable();
            return result.Select(q => new ProductCategoryTreeViewModel(q));
        }

        public IEnumerable<ProductCategoryTreeViewModel> GetSubTreeByGroup(int brandId, int? groupCate)
        {
            var result = this.BaseService.GetSubTreeByGroup(brandId, groupCate).AsQueryable();
            return result.Select(q => new ProductCategoryTreeViewModel(q));
        }

        //public IEnumerable<ProductCategoryViewModel> GetFullTreeByBrandWithoutExtra(int brandId)
        //{
        //    var result = this.BaseService.GetByBrandId(brandId).Where(q => q.Type == (int)ProductCategoryType.Default)
        //        .ProjectTo<ProductCategoryViewModel>(AutoMapperConfig);

        //    return result;
        //}

        public IEnumerable<ProductCategoryDetailsViewModel> GetStoreCategoriesWithProduct(int storeId)
        {
            var categories = this.BaseService.GetDetailsByStoreId(storeId)
                .AsQueryable()
                .ProjectTo<ProductCategoryDetailsViewModel>(this.AutoMapperConfig);

            return categories;
        }

        public IEnumerable<ProductCategoryDetailsViewModel> GetSubCategories(string seoName, int storeId)
        {
            var categories = this.BaseService.GetSubCategories(seoName, storeId)
                .AsQueryable()
                .ProjectTo<ProductCategoryDetailsViewModel>(this.AutoMapperConfig);

            return categories;
        }

        public IEnumerable<ProductCategoryViewModel> GetProductCategorieExtra()
        {
            var categories = this.BaseService.GetProductCategorieExtra()
                .AsQueryable()
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig);

            return categories;
        }

        public IEnumerable<ProductViewModel> GetProductExtra(int cateId)
        {
            var product = this.BaseService.Get(cateId).Products.Where(a=>a.IsAvailable);
            return product.AsQueryable().ProjectTo<ProductViewModel>(this.AutoMapperConfig);
        }

        public void EditProductCategory(ProductCategoryEditViewModel productCategory)
        {

            try
            {
                var entity = this.BaseService.Get(productCategory.CateID);
                //productCategory.CopyToEntity(entity);

                #region CopyToEntity Manual

                entity.CateName = productCategory.CateName;
                entity.Type = productCategory.Type;
                entity.IsExtra = productCategory.IsExtra;
                entity.AdjustmentNote = productCategory.AdjustmentNote;
                entity.StoreId = productCategory.StoreId;
                entity.SeoName = productCategory.SeoName;
                entity.ImageFontAwsomeCss = productCategory.ImageFontAwsomeCss;
                entity.ParentCateId = productCategory.ParentCateId;
                entity.Active = productCategory.Active;
                entity.BrandId = productCategory.BrandId;
                entity.DisplayOrder = productCategory.DisplayOrder;
                entity.Position = productCategory.Position;
                entity.IsDisplayed = productCategory.IsDisplayed;
                entity.IsDisplayedWebsite = productCategory.IsDisplayedWebsite;
                entity.PicUrl = productCategory.PicUrl;
                entity.Description = productCategory.Description;
                entity.BannerUrl = productCategory.BannerUrl;
                entity.Att1 = productCategory.Att1;
                entity.Att2 = productCategory.Att2;
                entity.Att3 = productCategory.Att3;
                entity.Att4 = productCategory.Att4;
                entity.Att5 = productCategory.Att5;
                entity.Att6 = productCategory.Att6;
                entity.Att7 = productCategory.Att7;
                entity.Att8 = productCategory.Att8;
                entity.Att9 = productCategory.Att9;
                entity.Att10 = productCategory.Att10;

                #endregion


                this.BaseService.Update(entity);
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(ex.ToErrorsString(), ex);
            }
        }

        #region SystemReport
        //public IEnumerable<ProductCategoryViewModel> GetProductCategories(int? brandId)
        //{
        //    var productCategories = this.BaseService.Get(q => q.BrandId.Value == brandId.Value)
        //        .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
        //        .ToList();
        //    return productCategories;
        //}

        public IQueryable<ProductCategoryViewModel> GetProductCategories(int? brandId)
        {
            var productCategories = this.BaseService.Get(q => q.BrandId.Value == brandId.Value)
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig);
            return productCategories;
        }

        public IEnumerable<ProductCategoryViewModel> GetProductCategories()
        {
            var productCategories = this.BaseService.Get()
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
                .ToList();
            return productCategories;
        }

        public ProductCategory GetProductCategoryBySeo(string seoName)
        {
            var productCategory = this.BaseService.FirstOrDefaultActive(q => q.IsDisplayed == true && q.SeoName == seoName);
            //.ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
            //.ToList();
            if (productCategory == null)
            {
                return null;
            }
            else
            {
                return productCategory;
            }
        }

        public IEnumerable<ProductCategory> GetProductCategoriesByBrandId(int brandId)
        {
            var productCategories = this.BaseService.GetActive(c => c.BrandId == brandId && c.Active)
                .ToList();
            return productCategories;
        }

        public IQueryable<ProductCategory> GetActiveProductCategoriesByBrandId(int brandId)
        {
            var productCategories = this.BaseService.GetActive(q => q.BrandId == brandId);
            return productCategories;
        }

        public IEnumerable<ProductCategoryForReport> GetProductCategoriesForReport(int brandId)
        {

            var productCategories = this.BaseService.GetProductCategoriesForReport(brandId)
                .ToList();
            return productCategories;
        }

        public IQueryable<ProductCategoryForReport> GetQueryProductCategoriesForReport(int brandId)
        {

            var productCategories = this.BaseService.GetProductCategoriesForReport(brandId);
            return productCategories;
        }

        public IEnumerable<ProductCategoryForReport> GetProductCategoriesForStoreReport(int storeId)
        {

            var productCategories = this.BaseService.GetProductCategoriesForStoreReport(storeId)
                .ToList();
            return productCategories;
        }

        public async Task<ProductCategoryViewModel> GetProductCategoryById(int productCategoryId)
        {
            var productCategory = await this.BaseService.GetProductCategoryById(productCategoryId);
            if (productCategory == null)
            {
                return null;
            }
            else
            {
                //return null;
                return new ProductCategoryViewModel(productCategory);
            }
        }
        #endregion       

        #region Methods load in Brand
        public async Task<IEnumerable<ProductCategoryViewModel>> GetByBrandIdAsync(int brandId)
        {
            return await this.BaseService.GetDisplayedByBrandId(brandId)
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }
        public async Task<ProductCategoryDetailsViewModel> GetCategoryDetailsInBrand(string seoName, int brandId)
        {
            var entity = await this.BaseService.GetDetailsBySeoNameAsyncInBrand(seoName, brandId);

            var result = new ProductCategoryDetailsViewModel(entity);
            result.Products = entity.Products
                .Where(a => a.Active)
                .ProjectTo<ProductViewModel>(this.AutoMapperConfig);
            return result;
        }
        public IEnumerable<ProductCategoryTreeViewModel> GetFullTreeByBrand(int brandId)
        {
            var result = this.BaseService.GetFullTreeByBrandId(brandId).AsQueryable();
            return result.Select(q => new ProductCategoryTreeViewModel(q));
        }
        public IEnumerable<ProductCategoryDetailsViewModel> GetBrandCategoriesWithProduct(int brandId)
        {
            var categories = this.BaseService.GetDetailsByBrandId(brandId)
                .AsQueryable()
                .ProjectTo<ProductCategoryDetailsViewModel>(this.AutoMapperConfig);

            return categories;
        }
        public IEnumerable<ProductCategoryDetailsViewModel> GetSubCategoriesInBrand(string seoName, int brandId)
        {
            var categories = this.BaseService.GetSubCategoriesInBrand(seoName, brandId)
                .AsQueryable()
                .ProjectTo<ProductCategoryDetailsViewModel>(this.AutoMapperConfig);

            return categories;
        }

        #endregion
    }

}

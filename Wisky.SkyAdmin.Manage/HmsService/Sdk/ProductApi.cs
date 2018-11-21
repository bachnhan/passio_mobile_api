using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using SkyWeb.DatVM.Mvc;
using System.Linq.Expressions;
using HmsService.Models.Entities;
using HmsService.Models;

namespace HmsService.Sdk
{

    public partial class ProductApi
    {
        public async Task<IEnumerable<Product>> GetActiveByProductCategoryIdAsync(int categoryId)
        {
            var products = await this.BaseService.GetAvailableByProductCategoryId(categoryId)
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }


        public async Task<IEnumerable<Product>> GetActiveByProductCategoryAndPatternAsync(int categoryId, int storeId, string pattern)
        {
            var products = await this.BaseService.GetAvailableByProductCategoryAndPattern(categoryId, storeId, pattern)
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }

        /*
         * Author: BaoTD
         * Method: Load all products in a category and subcategories
         */
        public async Task<IEnumerable<Product>> GetAllActiveByProductCategoryAndPatternAsync(int categoryId, int brandId, string pattern)
        {
            var products = await this.BaseService.GetAvailableByProductCategoryAndPatternInBrand(categoryId, brandId, pattern)
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }

        public IEnumerable<Product> GetAllActiveByProductCategoryAndPattern(int categoryId, int brandId, string pattern)
        {
            var products = this.BaseService.GetByProductCategoryAndPatternInBrand(categoryId, brandId, pattern)
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return products;
        }

        public async Task<ProductDetailsViewModel> GetProductDetailsAsync(int productId, int storeId)
        {
            var entity = await this.BaseService.GetActiveDetailsByStoreAsync(productId, storeId);
            if (entity == null)
            {
                return null;
            }
            return new ProductDetailsViewModel(entity);
        }

        public async Task<IEnumerable<ProductViewModel>> GetActiveByStoreIdAsync(int storeId)
        {
            var products = await this.BaseService.GetActiveByStoreId(storeId)
               .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }
        public async Task<IEnumerable<ProductViewModel>> GetActiveDisplayByStoreIdAsync(int storeId)
        {
            var products = await this.BaseService.GetActiveDisplayByStoreId(storeId)
               .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }
        public async Task<IEnumerable<Product>> GetActiveDisplayByStoreIdEntityAsync(int storeId)
        {
            var products = await this.BaseService.GetActiveDisplayByStoreId(storeId)
                .ToListAsync();
            return products;
        }
        public async Task<IEnumerable<ProductViewModel>> GetActiveByStoreIdAsyncWithoutExtra(int storeId)
        {
            var products = await this.BaseService.GetActiveWithoutExtraByStoreId(storeId)
               .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }

        public async Task<IEnumerable<ProductViewModel>> GetActiveByBrandId(int brandId)
        {
            var products = await this.BaseService.GetActiveByBrandId(brandId).Where(q => q.ProductType == (int)ProductTypeEnum.Single)
              .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
               .ToListAsync();
            return products;
        }

        public IEnumerable<Product> GetActiveByStoreId(int storeId)
        {
            var products = this.BaseService.GetActiveByStoreId(storeId).ToList();
            //.ProjectTo<Product>(this.AutoMapperConfig)

            return products;
        }

        public async Task<IEnumerable<ProductDetailsViewModel>> GetActiveWithSpecsByStoreIdAsync(int storeId)
        {
            var products = await this.BaseService.GetActiveWithSpecsByStoreId(storeId)
               .ProjectTo<ProductDetailsViewModel>(this.AutoMapperConfig)
                .ToListAsync();

            return products;
        }

        public async Task<IEnumerable<ProductDetailsViewModel>> GetAllProductWithoutExtra(int storeId)
        {
            var products = await this.BaseService.GetAllProductWithoutExtraByStoreId(storeId)
               .ProjectTo<ProductDetailsViewModel>(this.AutoMapperConfig)
                .ToListAsync();

            return products;
        }

        public IEnumerable<ProductDetailsViewModel> GetActiveWithSpecsByStoreId(int storeId)
        {
            var products = this.BaseService.GetActiveWithSpecsByStoreId(storeId)
                .ProjectTo<ProductDetailsViewModel>(this.AutoMapperConfig)
                .AsQueryable();

            return products;
        }

        public async Task<IEnumerable<Product>> GetByStoreIdAsync(int storeId)
        {
            var products = await this.BaseService.GetByStoreId(storeId)
                //.ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }

        public async Task<IEnumerable<Product>> GetByProductCategoryIdAsync(int categoryId)
        {
            var products = await this.BaseService.GetByProductCategoryId(categoryId)
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }

        public async Task<ProductDetailsViewModel> GetProductDetailsAsync(string seoName, int brandId)
        {
            var entity = await this.BaseService.GetBySeoNameAsync(seoName, brandId);
            if (entity == null)
            {
                return null;
            }
            return new ProductDetailsViewModel(entity);
        }

        public PagingViewModel<ProductViewModel> GetAdminWithFilterAsync(int brandId, string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByStoreWithFilter(brandId, keyword, sortKeyAsc)
                .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            var result = new PagingViewModel<ProductViewModel>(pagedList);

            return result;
        }

        public IEnumerable<Product> GetProductByBrand(int brandId, string keyword)
        {
            return this.BaseService.GetProductByBrand(brandId, keyword);
        }

        public IEnumerable<Product> GetProductByBrand(int brandId)
        {
            return this.BaseService.GetProductByBrand(brandId).ToList();
        }

        public IEnumerable<Product> GetAllProductByBrand(int brandId)
        {
            return this.BaseService.GetAllProductByBrand(brandId).ToList();
        }

        public IEnumerable<Product> GetAllProductsByBrand(int brandId)
        {
            return this.BaseService.GetAllProductsByBrand(brandId).ToList();
        }

        public IQueryable<Product> GetAllActiveProductByBrand(int brandId)
        {
            return this.BaseService.GetAllProductsByBrand(brandId);
        }

        public IEnumerable<ProductViewModel> GetProductExtraByBrandId(int brandId)
        {
            return this.BaseService.GetProductExtraByBrandId(brandId).ProjectTo<ProductViewModel>(this.AutoMapperConfig);
        }

        public async Task<ProductDetailsViewModel> GetByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveDetailsByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ProductDetailsViewModel(entity);
            }
        }

        public ProductDetailsViewModel GetByBrandId(int id, int brandId)
        {
            var entity = this.BaseService.GetActiveDetailsByBrand(id, brandId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ProductDetailsViewModel(entity);
            }
        }

        public IEnumerable<ProductViewModel> GetMatchingCollection(int storeId, string keyword)
        {
            var products = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, new KeyValuePair<string, bool>())
               .ProjectTo<ProductViewModel>(this.AutoMapperConfig);

            if (products == null)
            {
                return null;
            }
            else
            {
                return products;
            }
        }

        public IEnumerable<ProductViewModel> GetLikelyProducts(string seoName, int storeId)
        {
            var products = this.BaseService.GetLikelyProducts(seoName, storeId)
                .ProjectTo<ProductViewModel>(this.AutoMapperConfig);

            return products;
        }

        public IEnumerable<ProductViewModel> GetProductOfCategory(string seoName, int storeId)
        {
            var products = this.BaseService.GetProductOfCategoryBySeoName(seoName, storeId)
                .ProjectTo<ProductViewModel>(this.AutoMapperConfig);
            return products;
        }

        // author: DucBM
        public IEnumerable<ProductDetailsViewModel> GetProductDetailsOfCategory(string seoName, int storeId)
        {
            var products = this.BaseService.GetProductDetailsOfCategoryBySeoName(seoName, storeId)
                .ProjectTo<ProductDetailsViewModel>(this.AutoMapperConfig);
            return products;
        }

        public IEnumerable<Product> GetAllProductGeneral(int productId)
        {
            var products = this.BaseService.GetAllProductGeneral(productId);
            // .ProjectTo<ProductViewModel>(this.AutoMapperConfig);

            return products;
        }
        public IEnumerable<Product> GetProductGeneralByProductId(int productId)
        {
            var product = this.BaseService.Get(q => q.GeneralProductId == productId);
            return product;
        }
        public ProductViewModel GetProductGeneral(int productId)
        {
            var product = this.BaseService.GetProductGeneral(productId);
            return new ProductViewModel(product); ;
        }
        public IEnumerable<Product> GetActiveProductsEntitybyBrandId(int brandId)
        {
            return this.BaseService.GetActiveProductsEntitybyBrandId(brandId);
        }

        public int CreateSync(ProductViewModel product, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs, IEnumerable<int> listStoreId)
        {
            // Make sure the variable is ProductViewModel, so AutoMapper does not map reference properties
            product = product.ToExactType<ProductViewModel, ProductViewModel>();

            var entity = product.ToEntity();
            entity.PicURL = images.FirstOrDefault();
            entity.ProductCategory = null;

            var productId = this.BaseService.CreateSync(entity, images, productCollectionIds, specs, listStoreId);

            return productId;
        }

        public async System.Threading.Tasks.Task CreateAsync(ProductViewModel product, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs)
        {
            // Make sure the variable is ProductViewModel, so AutoMapper does not map reference properties
            product = product.ToExactType<ProductViewModel, ProductViewModel>();

            //var entity = product.ToEntity();
            var entity = new Product
            {
                CatID = product.CatID,
                PicURL = images.FirstOrDefault(),
                ProductName = product.ProductName,
                Code = product.Code,
                ProductType = product.ProductType,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Description = product.Description,
                Active = product.Active,
                IsAvailable = product.IsAvailable,
                SeoDescription = product.SeoDescription,
                SeoKeyWords = product.SeoKeyWords,
                SeoName = product.SeoName,
                Att1 = product.Att1,
                Att2 = product.Att2,
                Att3 = product.Att3,
                Att4 = product.Att4,
                Att5 = product.Att5,
                Att6 = product.Att6,
                Att7 = product.Att7,
                Att8 = product.Att8,
                Att9 = product.Att9,
                Att10 = product.Att10
            };

            await this.BaseService.CreateAsync(entity, images, productCollectionIds, specs);
        }

        public void EditAsync(ProductViewModel product, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs)
        {
            // Make sure the variable is ProductViewModel, so AutoMapper does not map reference properties
            product = product.ToExactType<ProductViewModel, ProductViewModel>();

            var entity = this.BaseService.Get(product.ProductID);
            //product.CopyToEntity(entity);

            #region CopyToEntity Manual

            entity.CatID = product.CatID;
            entity.ProductName = product.ProductName;
            entity.Code = product.Code;
            entity.ProductType = product.ProductType;
            entity.Price = product.Price;
            entity.DiscountPrice = product.DiscountPrice;
            entity.DiscountPercent = product.DiscountPercent;
            entity.Description = product.Description;
            entity.Active = product.Active;
            entity.IsAvailable = product.IsAvailable;
            entity.IsMostOrdered = product.IsMostOrdered;
            entity.SeoDescription = product.SeoDescription;
            entity.SeoKeyWords = product.SeoKeyWords;
            entity.SeoName = product.SeoName;
            entity.Position = product.Position;
            entity.DisplayOrder = product.DisplayOrder;
            entity.PosX = product.PosX;
            entity.PosY = product.PosY;
            entity.Group = product.Group;
            entity.ColorGroup = product.ColorGroup;
            entity.Att1 = product.Att1;
            entity.Att2 = product.Att2;
            entity.Att3 = product.Att3;
            entity.Att4 = product.Att4;
            entity.Att5 = product.Att5;
            entity.Att6 = product.Att6;
            entity.Att7 = product.Att7;
            entity.Att8 = product.Att8;
            entity.Att9 = product.Att9;
            entity.Att10 = product.Att10;
            #endregion


            entity.PicURL = images.FirstOrDefault();
            var api = new ProductCategoryApi();
            entity.ProductCategory = api.GetProductCategoryEntityById(product.CatID);
            this.BaseService.EditSync(entity, images, productCollectionIds, specs);
        }

        public void EditAsync(ProductViewModel product, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs, int[] combos)
        {
            // Make sure the variable is ProductViewModel, so AutoMapper does not map reference properties
            product = product.ToExactType<ProductViewModel, ProductViewModel>();

            var entity = this.BaseService.Get(product.ProductID);
            //product.CopyToEntity(entity);

            #region CopyToEntity Manual

            entity.CatID = product.CatID;
            entity.ProductName = product.ProductName;
            entity.Code = product.Code;
            entity.ProductType = product.ProductType;
            entity.Price = product.Price;
            entity.DiscountPrice = product.DiscountPrice;
            entity.DiscountPercent = product.DiscountPercent;
            entity.Description = product.Description;
            entity.Active = product.Active;
            entity.IsAvailable = product.IsAvailable;
            entity.IsMostOrdered = product.IsMostOrdered;
            entity.SeoDescription = product.SeoDescription;
            entity.SeoKeyWords = product.SeoKeyWords;
            entity.SeoName = product.SeoName;
            entity.Position = product.Position;
            entity.PosX = product.PosX;
            entity.PosY = product.PosY;
            entity.Group = product.Group;
            entity.ColorGroup = product.ColorGroup;
            entity.Att1 = product.Att1;
            entity.Att2 = product.Att2;
            entity.Att3 = product.Att3;
            entity.Att4 = product.Att4;
            entity.Att5 = product.Att5;
            entity.Att6 = product.Att6;
            entity.Att7 = product.Att7;
            entity.Att8 = product.Att8;
            entity.Att9 = product.Att9;
            entity.Att10 = product.Att10;
            #endregion


            entity.PicURL = images.FirstOrDefault();
            var api = new ProductCategoryApi();
            entity.ProductCategory = api.GetProductCategoryEntityById(product.CatID);
            this.BaseService.EditSync(entity, images, productCollectionIds, specs, combos);
        }

        public IQueryable<ProductViewModel> GetChildProductGeneral(int productId)
        {
            return this.BaseService.GetChildProductGeneral(productId).ProjectTo<ProductViewModel>(this.AutoMapperConfig);
        }

        public ProductViewModel GetProductById(int productId)
        {
            var entity = this.BaseService.Get(p => p.ProductID == productId).FirstOrDefault();

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ProductViewModel(entity);
            }
        }

        //GetProductById theo Entity
        public Product GetProductByIdEntity(int productId)
        {
            var entity = this.BaseService.Get(p => p.ProductID == productId).FirstOrDefault();

            if (entity == null)
            {
                return null;
            }
            else
            {
                return entity;
            }
        }

        public void EditProductEntity(Product product)
        {
            this.BaseService.Update(product);
            this.BaseService.Save();
        }

        public Product GetProductEntityById(int productId)
        {
            return this.BaseService.Get(p => p.ProductID == productId).FirstOrDefault();
        }

        public IQueryable<Product> GetProductByBrandId(int brandId)
        {
            return this.BaseService.GetActive(p => p.ProductCategory.BrandId == brandId);
        }
        public async Task<IEnumerable<Product>> GetActiveByBrandIdAndCateIdAsyncEntity(int brandId, int cateId)
        {
            var products = await this.BaseService.GetActiveByBrandIdAndCateId(brandId, cateId)
                .ToListAsync();
            return products;
        }
        public Product GetProductByCode(string productCode)
        {
            return this.BaseService.GetProductByCode(productCode);
        }
        public Product GetProductBySeoName(string seo)
        {
            return this.BaseService.GetProductBySeo(seo);
        }

        public IEnumerable<Product> GetGeneralProductByBrandId(int brandId)
        {
            var result = this.BaseService.GetGeneralProductByBrandId(brandId).ToList();
            return result;
        }

        public IEnumerable<Product> GetProductForCombo(int brandId)
        {
            return BaseService.GetAllProductByBrand(brandId).Where(q => q.ProductType != (int)ProductTypeEnum.Combo && q.ProductType != (int)ProductTypeEnum.Extra).ToList();
        }

        #region Product load in brand
        public async Task<ProductDetailsViewModel> GetProductDetailsAsyncInBrand(string seoName, int brandId)
        {
            var entity = await this.BaseService.GetBySeoNameAsyncInBrand(seoName, brandId);
            if (entity == null)
            {
                return null;
            }
            return new ProductDetailsViewModel(entity);
        }
        public async Task<IEnumerable<ProductViewModel>> GetActiveByBrandIdAsync(int brandId)
        {
            var products = await this.BaseService.GetActiveByBrandId(brandId)
               .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }

        public IEnumerable<ProductViewModel> GetActiveByBrandIdAndProductCategoryId(int brandId, int id)
        {
            var products = this.BaseService.GetActiveByBrandId(brandId).Where(a => a.CatID == id)
               .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return products;
        }
        public IEnumerable<ProductViewModel> GetMatchingCollectionInBrand(int brandId, string keyword)
        {
            var products = this.BaseService.GetAdminByBrandWithFilter(brandId, keyword, new KeyValuePair<string, bool>())
               .ProjectTo<ProductViewModel>(this.AutoMapperConfig);

            if (products == null)
            {
                return null;
            }
            else
            {
                return products;
            }
        }
        public IEnumerable<ProductViewModel> GetLikelyProductsInBrand(string seoName, int brandId)
        {
            var products = this.BaseService.GetLikelyProductsInBrand(seoName, brandId)
                .ProjectTo<ProductViewModel>(this.AutoMapperConfig);

            return products;
        }
        public async Task<IEnumerable<ProductDetailsViewModel>> GetActiveWithSpecsByBrandIdAsync(int brandId)
        {
            var products = await this.BaseService.GetActiveWithSpecsByBrandId(brandId)
               .ProjectTo<ProductDetailsViewModel>(this.AutoMapperConfig)
                .ToListAsync();

            return products;
        }
        public async Task<IEnumerable<Product>> GetActiveByProductCategoryAndPatternInBrandAsync(int categoryId, int brandId, string pattern)
        {
            var products = await this.BaseService.GetAvailableByProductCategoryAndPatternInBrand(categoryId, brandId, pattern)
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return products;
        }

        public async Task<IEnumerable<ProductForComparisonReport>> GetAllBrandActiveProductsForReport(int brandId)
        {
            return await BaseService.GetAllBrandActiveProductsForReport(brandId).ToListAsync();
        }
        #endregion
        public void EditSeoName()
        {
            this.BaseService.EditSeoName();
        }

        #region Store Report
        public IEnumerable<Product> GetProducts()
        {
            var products = this.BaseService.GetProducts()
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return products;
        }
        public IEnumerable<Product> GetAllProducts()
        {
            var products = this.BaseService.GetAllProducts()
                //  .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return products;
        }

        public IEnumerable<Product> GetGiftProducts()
        {
            var products = this.BaseService.GetAllProducts().Where(q => q.IsAvailable == true && q.GeneralProductId == null)
                //  .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return products;
        }

        public async Task<IEnumerable<ProductForComparisonReport>> GetAllStoreActiveProductsForReport(int storeId)
        {
            return await BaseService.GetAllStoreActiveProductsForReport(storeId).ToListAsync();
        }

        public IQueryable<ProductForComparisonReport> GetQueryAllStoreActiveProductsForReport(int storeId)
        {
            return BaseService.GetAllStoreActiveProductsForReport(storeId);
        }
        #endregion
        public ProductViewModel GetProductDeliveryFee()
        {
            var entity = BaseService.Get().Where(q => q.ProductName.Equals(ConstantManager.DELIVERY)).FirstOrDefault();

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ProductViewModel(entity);
            }
             
        }
        
    }
}

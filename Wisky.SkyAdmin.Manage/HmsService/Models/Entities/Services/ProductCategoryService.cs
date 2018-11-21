using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IProductCategoryService
    {
        IQueryable<ProductCategory> GetByStoreId(int storeId);
        IQueryable<ProductCategory> GetByStoreIdEdit(int storeId, int cateId);

        Task<ProductCategory> GetByStoreIdAsync(int categoryId, int storeId);
        Task<ProductCategoryDetails> GetDetailsByIdAsync(int? id);
        Task<ProductCategory> GetIncludeProductAsync(int id);
        Task<ProductCategoryDetails> GetDetailsBySeoNameAsync(string seoName, int storeId);
        Task<ProductCategoryDetailsWithProductDetails> GetDetailsWithProductDetailsBySeoNameAsync(string seoName, int storeId);
        IQueryable<ProductCategory> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty);
        Task<ProductCategory> GetActiveByStoreAsync(int id, int storeId);
        Task<ProductCategory> GetActiveByIdAndBrandAsync(int id, int brandId);
        IEnumerable<ProductCategoryTree> GetFullTreeByStoreId(int storeId);
        IEnumerable<ProductCategoryTree> GetFullTreeByStoreIdWithoutExtra(int brandId);
        IEnumerable<ProductCategoryTree> GetSubTreeByGroup(int brandId, int? groupCate);
        IEnumerable<ProductCategoryDetails> GetDetailsByStoreId(int storeId);
        IEnumerable<ProductCategoryDetails> GetSubCategories(string seoName, int storeId);
        IEnumerable<ProductCategoryDetails> GetSubCategoriesById(int cateParentCateId, int storeId);
        IEnumerable<ProductCategory> GetProductCategorieExtra();
        IQueryable<ProductCategory> GetByBrandId(int brandId);
        IQueryable<ProductCategory> GetByBrandId(int brandId, int cateId);
        bool ValidateBrandCategory(int categoryId, int brandId);
        ProductCategory GetProductCategoryEntityById(int categoryId);
        IQueryable<ProductCategory> GetDisplayedByBrandId(int brandId);
        Task<ProductCategoryDetails> GetDetailsBySeoNameAsyncInBrand(string seoName, int brandId);
        IEnumerable<ProductCategoryTree> GetFullTreeByBrandId(int brandId);
        IEnumerable<ProductCategoryDetails> GetDetailsByBrandId(int brandId);
        IEnumerable<ProductCategoryDetails> GetSubCategoriesInBrand(string seoName, int brandId);

        #region SystemReport
        IQueryable<ProductCategory> GetProductCategories();
        Task<ProductCategory> GetProductCategoryById(int productCategoryId);
        IQueryable<ProductCategoryForReport> GetProductCategoriesForReport(int brandId);
        #endregion

        #region StoreReport
        IQueryable<ProductCategoryForReport> GetProductCategoriesForStoreReport(int storeId);
       
        #endregion
    }

    public partial class ProductCategoryService
    {
        public IQueryable<ProductCategory> GetByStoreId(int storeId)
        {
            return this.GetActive(q => q.StoreId == storeId && q.IsDisplayed == true)
                .OrderBy(q => q.CateName);
        }

        public IQueryable<ProductCategory> GetByStoreIdEdit(int storeId, int cateId)
        {
            return this.GetActive(q => q.StoreId == storeId && q.IsDisplayed == true && q.CateID != cateId)
                .OrderBy(q => q.CateName);
        }

        public IQueryable<ProductCategory> GetByBrandId(int brandId)
        {
            return this.GetActive(q => q.BrandId == brandId)
                .OrderBy(q => q.CateName);
        }

        public IQueryable<ProductCategory> GetByBrandId(int brandId, int cateId)
        {
            return this.GetActive(q => q.BrandId == brandId && q.IsDisplayed && q.CateID != cateId)
                .OrderBy(q => q.CateName);
        }

        public bool ValidateBrandCategory(int categoryId, int brandId)
        {
            var category = this.GetActive(c => c.CateID == categoryId && c.BrandId == brandId).FirstOrDefault();

            return category != null;
        }

        public async Task<ProductCategory> GetByStoreIdAsync(int categoryId, int storeId)
        {
            return await this.FirstOrDefaultActiveAsync(q => q.CateID == categoryId && q.StoreId == storeId);
        }

        public async Task<ProductCategory> GetIncludeProductAsync(int id)
        {
            return await this.GetActive(q => q.CateID == id)
                .Include(q => q.Products)
                .FirstOrDefaultAsync();
        }

        public async Task<ProductCategoryDetails> GetDetailsBySeoNameAsync(string seoName, int storeId)
        {
            var entity = await this.FirstOrDefaultActiveAsync(q => q.SeoName == seoName && q.StoreId == storeId);

            if (entity == null)
            {
                return null;
            }

            var products = entity.Products.AsQueryable()
                .Where(q => q.Active && q.IsAvailable)
                .OrderBy(q => q.ProductName);

            return new ProductCategoryDetails()
            {
                Category = entity,
                Products = products,
            };
        }

        public async Task<ProductCategoryDetails> GetDetailsByIdAsync(int? id)
        {
            var entity = await this.GetAsync(id);
            if (entity == null)
            {
                return null;
            }

            var products = entity.Products.AsQueryable()
                .Where(q => q.Active && q.IsAvailable)
                .OrderBy(q => q.ProductName);

            return new ProductCategoryDetails()
            {
                Category = entity,
                Products = products,
            };
        }

        // author: DucBM
        public async Task<ProductCategoryDetailsWithProductDetails> GetDetailsWithProductDetailsBySeoNameAsync(string seoName, int storeId)
        {
            var entity = await this.FirstOrDefaultActiveAsync(q => q.SeoName == seoName && q.StoreId == storeId);

            if (entity == null)
            {
                return null;
            }

            var products = entity.Products.AsQueryable()
                .Where(q => q.Active && q.IsAvailable)
                .OrderBy(q => q.ProductName).Select(a => new ProductDetails
                {
                    Product = a,
                    ProductCollections = a.ProductCollectionItemMappings,
                    ProductSpecifications = a.ProductSpecifications.Where(b => b.Active),
                    ProductImages = a.ProductImages
                });

            return new ProductCategoryDetailsWithProductDetails()
            {
                Category = entity,
                Products = products,
            };
        }

        /// <summary>
        /// HiepBP-PhuongTA
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="keyword"></param>
        /// <param name="orderByProperty"></param>
        /// <returns></returns>
        public IQueryable<ProductCategory> GetAdminByStoreWithFilter(int brandId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q =>
                q.BrandId == brandId &&
                (keyword == null || q.CateName.Contains(keyword)));

            ProductCategorySortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case ProductCategorySortableProperty.CateName:
                        result = result.OrderBy(q => q.CateName, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.CateID);
            }

            return result;
        }

        public async Task<ProductCategory> GetActiveByStoreAsync(int id, int storeId)
        {
            return await this.FirstOrDefaultActiveAsync(q => q.CateID == id && q.StoreId == storeId);
        }

        public async Task<ProductCategory> GetActiveByIdAndBrandAsync(int id, int brandId)
        {
            return await this.FirstOrDefaultActiveAsync(q => q.CateID == id && q.BrandId == brandId);
        }

        public IEnumerable<ProductCategoryTree> GetFullTreeByStoreId(int storeId)
        {
            var rootCategories = this.GetActive(q => q.StoreId == storeId && q.ParentCateId == null)
                .Include(q => q.ProductCategory1).OrderBy(x => x.Position);

            // ToList and run the Select statement in memory to avoid Expression error
            return rootCategories.ToList()
                .Select(q => new ProductCategoryTree()
                {
                    Category = q,
                    Subcategories = this.GetChildren(q),
                });
        }

        public IEnumerable<ProductCategoryTree> GetFullTreeByStoreIdWithoutExtra(int brandId)
        {
            var rootCategories = this.GetActive(q => q.BrandId == brandId && q.ParentCateId == null && q.IsExtra == false && q.IsDisplayedWebsite == true)
                .Include(q => q.ProductCategory1).OrderBy(x => x.Position);

            // ToList and run the Select statement in memory to avoid Expression error
            return rootCategories.ToList()
                .Select(q => new ProductCategoryTree()
                {
                    Category = q,
                    Subcategories = this.GetChildrenWithoutExtra(q),
                });
        }

        public IEnumerable<ProductCategoryTree> GetSubTreeByGroup(int brandId, int? groupCate)
        {
            var rootCategories = this.GetActive(q => q.BrandId == brandId && q.ParentCateId == null && q.IsExtra == false)
                .Include(q => q.ProductCategory1).OrderBy(x => x.Position);

            // ToList and run the Select statement in memory to avoid Expression error
            return rootCategories.ToList()
                .Select(q => new ProductCategoryTree()
                {
                    Subcategories = this.GetChildrenByGroup(q, groupCate),
                });
        }


        private IEnumerable<ProductCategoryTree> GetChildrenWithoutExtra(ProductCategory category)
        {
            var subCategories = category.ProductCategory1.Where(q => q.Active && q.IsExtra == false).OrderBy(x => x.Position);

            return subCategories.ToList()
                .Select(q => new ProductCategoryTree()
                {
                    Category = q,
                    Subcategories = GetChildren(q),
                });
        }


        private IEnumerable<ProductCategoryTree> GetChildren(ProductCategory category)
        {
            var subCategories = category.ProductCategory1.Where(q => q.Active).OrderBy(x => x.Position);

            return subCategories.ToList()
                .Select(q => new ProductCategoryTree()
                {
                    Category = q,
                    Subcategories = GetChildren(q),
                });
        }

        private IEnumerable<ProductCategoryTree> GetChildrenByGroup(ProductCategory category, int? groupCate)
        {
            //Changed from Category.Groupcate to Category.ParentCateId since Category.Groupcate is no longer exist.
            var subCategories = category.ProductCategory1.Where(q => q.Active && (q.ParentCateId == groupCate)).OrderBy(x => x.Position);

            return subCategories.ToList()
                .Select(q => new ProductCategoryTree()
                {
                    Category = q,
                    Subcategories = GetChildren(q),
                });
        }

        public IEnumerable<ProductCategoryDetails> GetDetailsByStoreId(int storeId)
        {
            var entity = this.GetActive(q => q.StoreId == storeId);


            return entity.ToList().Select(a => new ProductCategoryDetails()
            {
                Category = a,
                Products = a.Products.AsQueryable().Where(b => b.Active)
            });
        }

        public IEnumerable<ProductCategoryDetails> GetSubCategories(string seoName, int storeId)
        {
            var entity = this.GetActive(q => q.StoreId == storeId
                    && q.ParentCateId.HasValue && q.ProductCategory2.SeoName.Equals(seoName));

            return entity.ToList().Select(a => new ProductCategoryDetails()
            {
                Category = a,
                Products = a.Products.AsQueryable().Where(b => b.Active)
            });
        }
        public IEnumerable<ProductCategoryDetails> GetSubCategoriesById(int cateParentCateId, int storeId)
        {
            var entity = this.GetActive(q => q.StoreId == storeId
                    && q.ParentCateId.HasValue && q.ProductCategory2.CateID == cateParentCateId);

            return entity.ToList().Select(a => new ProductCategoryDetails()
            {
                Category = a,
                Products = a.Products.AsQueryable().Where(b => b.Active)
            });
        }
        public IEnumerable<ProductCategory> GetProductCategorieExtra()
        {
            return this.GetActive(a => a.IsExtra && a.IsDisplayed);
        }

        public ProductCategory GetProductCategoryEntityById(int categoryId)
        {
            return this.Get().FirstOrDefault(c => c.CateID == categoryId);
        }
        public IQueryable<ProductCategory> GetDisplayedByBrandId(int brandId)
        {
            return this.GetActive(q => q.BrandId == brandId && q.IsDisplayed == true)
                .OrderBy(q => q.CateName);
        }
        public async Task<ProductCategoryDetails> GetDetailsBySeoNameAsyncInBrand(string seoName, int brandId)
        {
            var entity = await this.FirstOrDefaultActiveAsync(q => q.SeoName == seoName && q.BrandId == brandId);

            if (entity == null)
            {
                return null;
            }

            var products = entity.Products.AsQueryable()
                .Where(q => q.Active && q.IsAvailable)
                .OrderBy(q => q.ProductName);

            return new ProductCategoryDetails()
            {
                Category = entity,
                Products = products,
            };
        }
        public IEnumerable<ProductCategoryTree> GetFullTreeByBrandId(int brandId)
        {
            var rootCategories = this.GetActive(q => q.BrandId == brandId && q.ParentCateId == null)
                .Include(q => q.ProductCategory1).OrderBy(x => x.Position);

            // ToList and run the Select statement in memory to avoid Expression error
            return rootCategories.ToList()
                .Select(q => new ProductCategoryTree()
                {
                    Category = q,
                    Subcategories = this.GetChildren(q),
                });
        }
        public IEnumerable<ProductCategoryDetails> GetDetailsByBrandId(int brandId)
        {
            var entity = this.GetActive(q => q.BrandId == brandId);


            return entity.ToList().Select(a => new ProductCategoryDetails()
            {
                Category = a,
                Products = a.Products.AsQueryable().Where(b => b.Active)
            });
        }
        public IEnumerable<ProductCategoryDetails> GetSubCategoriesInBrand(string seoName, int brandId)
        {
            var entity = this.GetActive(q => q.BrandId == brandId
                    && q.ParentCateId.HasValue && q.ProductCategory2.SeoName.Equals(seoName));

            return entity.ToList().Select(a => new ProductCategoryDetails()
            {
                Category = a,
                Products = a.Products.AsQueryable().Where(b => b.Active)
            });
        }

        #region SystemReport
        public IQueryable<ProductCategory> GetProductCategories()
        {
            return this.Get();
        }

        public IQueryable<ProductCategoryForReport> GetProductCategoriesForReport(int brandId)
        {
            return this.GetActive(q => q.BrandId == brandId).Select(a => new ProductCategoryForReport
            {
                CateID = a.CateID,
                CateName = a.CateName,
            });
        }
        public async Task<ProductCategory> GetProductCategoryById(int productCategoryId)
        {
            return await this.GetAsync(productCategoryId);
        }
        #endregion

       
        public IQueryable<ProductCategoryForReport> GetProductCategoriesForStoreReport(int storeId)
        {
            //Cmt => test
            var productDetailMappingService = DependencyUtils.Resolve<IProductDetailMappingService>();

            var result = productDetailMappingService.GetProductByStore(storeId)
                                .Where(q => q.Active.Value == true)
                .Select(q => new ProductCategoryForReport
                {
                    CateID = q.Product.ProductCategory.CateID == null ? default(int) : q.Product.ProductCategory.CateID,
                    CateName = q.Product.ProductCategory.CateName == null ? "" : q.Product.ProductCategory.CateName,
                }).Distinct();
            
            return result;
    }
    }


    public class ProductCategoryForReport
    {
        public int CateID { get; set; }
        public string CateName { get; set; }
    }

    public class ProductCategoryDetails : IEntity
    {
        public ProductCategory Category { get; set; }
        public IQueryable<Product> Products { get; set; }
    }

    // author: DucBM
    public class ProductCategoryDetailsWithProductDetails : IEntity
    {
        public ProductCategory Category { get; set; }
        public IQueryable<ProductDetails> Products { get; set; }
    }

    public class ProductCategoryTree : IEntity
    {
        public ProductCategory Category { get; set; }
        public IEnumerable<ProductCategoryTree> Subcategories { get; set; }
    }

    public enum ProductCategorySortableProperty
    {
        CateName,
    }

}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IBlogCategoryService
    {
        Task<BlogCategory> GetActiveByTitleAsync(string title);
        //IQueryable<BlogCategory> GetAllBlogCategoryActiveByBrandId(int brandId);
        IQueryable<BlogCategory> GetByStoreId(int storeId);
        IQueryable<BlogCategory> GetActiveByStoreId(int storeId);
        IQueryable<BlogCategory> GetAllBlogCategoryActiveByBrandId(int brandId);
        IQueryable<BlogCategory> GetAllParentBlogCategoryActiveByBrandId(int brandId);
    }
    public partial class BlogCategoryService
    {
        public IQueryable<BlogCategory> GetAllBlogCategoryActiveByBrandId(int brandId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<BlogCategory> GetAllParentBlogCategoryActiveByBrandId(int brandId)
        {
            throw new NotImplementedException();
        }

        public async Task<BlogCategory> GetActiveByTitleAsync(string title)
        {
            var blogCategory = await this.Get(m => title.Equals(m.BlogCateName))
                .FirstOrDefaultAsync();
            return blogCategory;
        }

        public IQueryable<BlogCategory> GetByStoreId(int storeId)
        {
            var blogCategories = this.Get(m => m.StoreId == storeId);
            return blogCategories;
        }

        public IQueryable<BlogCategory> GetActiveByStoreId(int storeId)
        {
            var blogCategories = this.Get(m => m.StoreId == storeId && m.IsActive == true);
            return blogCategories;
        }
        //public IQueryable<BlogCategory> GetAllBlogCategoryActiveByBrandId(int brandId)
        //{
        //    return this.Get(q => q.IsActive && q.BrandId == brandId);
        //}
    }
}

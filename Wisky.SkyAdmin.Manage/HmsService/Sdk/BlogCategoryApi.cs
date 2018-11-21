using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{
    public partial class BlogCategoryApi
    {
        public async Task<BlogCategoryViewModel> GetActiveByTitleAsync(string title)
        {
            var entity = await this.BaseService.GetActiveByTitleAsync(title);
            var blogCategory = new BlogCategoryViewModel(entity);
            return blogCategory;
        }

        public async Task<IEnumerable<BlogCategoryViewModel>> GetByStoreIdAsync(int storeId)
        {
            var blogCategories = await this.BaseService.GetByStoreId(storeId)
                .ProjectTo<BlogCategoryViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return blogCategories;
        }
        public IQueryable<BlogCategory> GetParrentCategory(int brandId)
        {
            return this.BaseService.Get(q => q.BrandId == brandId && q.ParentCateId == null && q.IsActive == true && q.IsDisplay == true);

        }
        public async Task<IEnumerable<BlogCategoryViewModel>> GetActiveByStoreIdAsync(int storeId)
        {
            var blogCategories = await this.BaseService.GetActiveByStoreId(storeId)
                .ProjectTo<BlogCategoryViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return blogCategories;
        }
        public IQueryable<BlogCategory> GetAllBlogCategoryActive()
        {
            return this.BaseService.Get(q => q.IsActive == true);
        }
        public IQueryable<BlogCategoryViewModel> GetAllBlogCategoryActiveByBrandId(int brandId)
        {
            return this.BaseService.GetAllBlogCategoryActiveByBrandId(brandId).ProjectTo<BlogCategoryViewModel>(this.AutoMapperConfig);
        }
    }
}

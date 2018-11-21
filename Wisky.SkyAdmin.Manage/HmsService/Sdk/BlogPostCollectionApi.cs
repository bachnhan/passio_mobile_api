using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{

    public partial class BlogPostCollectionApi
    {
        //Async method
        public async Task<BlogPostCollectionWithPostsViewModel> GetDetailsAsync(int id)
        {
            var entity = await this.BaseService.GetAsync(id);

            if (entity == null) { return null; }

            var result = new BlogPostCollectionWithPostsViewModel(entity);
            result.BlogPostCollectionItems = entity.BlogPostCollectionItemMappings
                .AsQueryable()
                .Where(a => a.Active && a.BlogPost.Active)
                .OrderBy(a=> a.Id)
                .ProjectTo<BlogPostCollectionItemWithBlogPostViewModel>(this.AutoMapperConfig);
            return result;
        }
        public List<BlogPostCollection> GetSelectedCollection(int[] collectionNumber)
        {
            var blogPostCollections = this.BaseService.Get(b => collectionNumber.Contains(b.Id)).ToList();
            return blogPostCollections;
        }
        public BlogPostCollectionWithPostsViewModel GetDetailBySeoname(string seoname)
        {
            var entity = this.BaseService.GetActive(q => q.Active && q.SeoName == seoname).FirstOrDefault();

            if (entity == null) { return null; }

            var result = new BlogPostCollectionWithPostsViewModel(entity);
            result.BlogPostCollectionItems = entity.BlogPostCollectionItemMappings
                .AsQueryable()
                .Where(a => a.Active && a.BlogPost.Active)
                .OrderBy(a => a.BlogPost.CreatedTime)
                .ProjectTo<BlogPostCollectionItemWithBlogPostViewModel>(this.AutoMapperConfig);
            return result;
        }

        public PagingViewModel<BlogPostCollectionDetailsViewModel> GetAdminWithFilterAsync(int storeId, string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, sortKeyAsc)
                .ProjectTo<BlogPostCollectionDetailsViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<BlogPostCollectionDetailsViewModel>(pagedList);
        }

        public async Task<IEnumerable<BlogPostCollectionViewModel>> GetActiveByStoreIdAsync(int storeId)
        {
            return await this.BaseService.GetActiveByStore(storeId)
                .ProjectTo<BlogPostCollectionViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }


        //Sync method

        public BlogPostCollectionWithPostsViewModel GetDetails(int id)
        {
            var entity = this.BaseService.Get(id);

            if (entity == null) { return null; }

            var result = new BlogPostCollectionWithPostsViewModel(entity);
            result.BlogPostCollectionItems = entity.BlogPostCollectionItemMappings
                .AsQueryable()
                .Where(a => a.Active && a.BlogPost.Active)
                .OrderBy(a => a.Id)
                .ProjectTo<BlogPostCollectionItemWithBlogPostViewModel>(this.AutoMapperConfig);
            return result;
        }

        public IEnumerable<BlogPostCollectionViewModel> GetActiveByStoreId(int storeId)
        {
            return this.BaseService.GetActiveByStore(storeId)
                .ProjectTo<BlogPostCollectionViewModel>(this.AutoMapperConfig)
                .ToList();
        }
        //public List<BlogPostCollection> GetSelectedCollection(int[] collectionNumber)
        //{
        //    var blogPostCollections = this.BaseService.Get(b => collectionNumber.Contains(b.Id)).ToList();
        //    return blogPostCollections;
        //}
    }

}

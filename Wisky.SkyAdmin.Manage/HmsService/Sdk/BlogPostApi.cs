using AutoMapper.QueryableExtensions;
using HmsService.Models;
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
    public partial class BlogPostApi
    {

        public async Task<BlogPostViewModel> GetBlogPostBySeoNameAsync(string seoName, int storeId)
        {
            var entity = await this.BaseService.GetBlogPostBySeoNameAsync(seoName, storeId);
            var blogPost = new BlogPostViewModel(entity);
            return blogPost;
        }
        //public BlogPost GetDetailBlog(int blogId)
        //{
        //    return this.BaseService.Get(blogId);
        //}
        public IEnumerable<BlogPostViewModel> GetMatchingCollection(int storeId, string keyword)
        {
            var result = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, new KeyValuePair<string, bool>())
                .Select(a => a.BlogPost)
                .ProjectTo<BlogPostViewModel>(this.AutoMapperConfig);

            return result;
        }
        //public IQueryable<BlogPost> GetAllActiveByBlogCategoryAndPattern(int categoryId, string pattern)
        //{
        //    var blogPosts = this.BaseService.GetByBlogPostCategoryAndPattern(categoryId, pattern);
        //    return blogPosts;
        //}

        public async Task<IEnumerable<BlogPostViewModel>> GetByStoreIdAsync(int storeId, bool adminGet = true)
        {
            var blogPost = this.BaseService.GetByStoreId(storeId);
            if (!adminGet)
            {
                blogPost = blogPost.Where(a => a.Active);
            }
            var result = await blogPost
                .ProjectTo<BlogPostViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return result;
        }

        public PagingViewModel<BlogPostDetailsViewModel> GetAdminWithFilterAsync(int storeId, string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc, DateTime starttime, DateTime endtime)
        {
            var pagedList = this.BaseService.GetAdminByStoreWithFilterDateTime(storeId, keyword, sortKeyAsc, starttime, endtime)
                .ProjectTo<BlogPostDetailsViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<BlogPostDetailsViewModel>(pagedList);
        }

        public async Task<BlogPostViewModel> GetByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostViewModel(entity);
            }
        }

        public async Task<BlogPostDetailsViewModel> GetDetailsBySeoNameAsync(string seoname, int storeId)
        {
            var entity = await this.BaseService.GetActiveDetailsByStoreAsync(seoname, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostDetailsViewModel(entity);
            }
        }

        public async Task<BlogPostDetailsViewModel> GetDetailsByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveDetailsByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostDetailsViewModel(entity);
            }
        }

        public async Task CreateAsync(BlogPostViewModel model, int[] blogPostCollectionIds, string[] images)
        {
            model = Utils.ToExactType<BlogPostViewModel, BlogPostViewModel>(model);

            var entity = model.ToEntity();
            entity.Image = images.FirstOrDefault();

            await this.BaseService.CreateAsync(entity, blogPostCollectionIds, images);
        }

        public async Task EditAsync(BlogPostViewModel model, int[] blogPostCollectionIds, string[] images)
        {
            model = Utils.ToExactType<BlogPostViewModel, BlogPostViewModel>(model);

            var entity = await this.BaseService.GetAsync(model.Id);
            model.CopyToEntity(entity);
            entity.Image = images.FirstOrDefault();

            await this.BaseService.UpdateAsync(entity, blogPostCollectionIds, images);
        }

        public IEnumerable<HmsService.Models.Entities.BlogPost> GetBlogPostByCollectionId(int? collectionId)
        {
            return this.BaseService.GetByCollectionId(collectionId)/*.ProjectTo<BlogPostViewModel>()*/.ToList();
        }

        public BlogPostDetailsViewModel GetDetailsByStoreId(int id, int storeId)
        {
            Console.WriteLine(id);
            Console.WriteLine(storeId);
            var entity = this.BaseService.GetActiveDetailsByStore(id, storeId);
            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostDetailsViewModel(entity);
            }
        }
        public BlogPost GetDetailBlog(int blogId)
        {
            return this.BaseService.Get(blogId);
        }
        public IQueryable<BlogPost> GetAllActiveByBlogCategoryAndPattern(int categoryId, string pattern)
        {
            var blogPosts = this.BaseService.GetByBlogPostCategoryAndPattern(categoryId, pattern);
            return blogPosts;
        }
        public async Task CreateAsync(BlogPostViewModel model, int[] blogPostCollectionIds, string[] images, int[] tags)
        {
            model = Utils.ToExactType<BlogPostViewModel, BlogPostViewModel>(model);

            var entity = model.ToEntity();
            entity.Image = images.FirstOrDefault();

            await this.BaseService.CreateAsync(entity, blogPostCollectionIds, images, tags);
        }
        public BlogPost GetBlogPostById(int id)
        {
            var blog = this.BaseService.FirstOrDefault(q => q.Id == id);
            return blog;
        }
        public BlogPostViewModel GetByStoreAsyn(int id, int storeId)
        {
            var entity = this.BaseService.Get().Where(q => q.Id == id && q.StoreId == storeId).FirstOrDefault();

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostViewModel(entity);
            }
        }
        public async Task EditAsync(BlogPostViewModel model, int[] blogPostCollectionIds, string[] images, int[] tags)
        {
            model = Utils.ToExactType<BlogPostViewModel, BlogPostViewModel>(model);

            var entity = await this.BaseService.GetAsync(model.Id);
            model.CopyToEntity(entity);
            entity.Image = images.FirstOrDefault();

            await this.BaseService.UpdateAsync(entity, blogPostCollectionIds, images, tags);
        }
        public IEnumerable<BlogPostViewModel> GetBlogPostOrderUpdateTimeAndType(int type)
        {
            var blogPost = this.Get().Where(q => q.Active == true && q.BlogType == type).OrderByDescending(q => q.UpdatedTime).Take(ConstantManager.NUMBER_BLOG_POST);
            return blogPost;
        }

    }
}

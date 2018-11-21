using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.Sdk;

namespace HmsService.Models.Entities.Services
{
    public partial interface IBlogPostService
    {
        IQueryable<BlogPost> GetAllBlogCategoryActiveByBrandId(int brandId);
        Task<BlogPost> GetBlogPostBySeoNameAsync(string seoName, int storeId);
        IQueryable<BlogPost> GetByStoreId(int storeId);
        Task CreateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images, int[] tags);
        IQueryable<BlogPost> GetByBlogPostCategoryAndPattern(int categoryId, string pattern);
        IQueryable<BlogPostDetails> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty);
        IQueryable<BlogPostDetails> GetAdminByStoreWithFilterDateTime(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty, DateTime starttime, DateTime endtime);
        Task<BlogPost> GetActiveByStoreAsync(int id, int storeId);
        BlogPost GetActiveByStore(int id, int storeId);
        Task<BlogPostDetails> GetActiveDetailsByStoreAsync(int id, int storeId);
        Task<BlogPostDetails> GetActiveDetailsByStoreAsync(string seoname, int storeId);
        BlogPostDetails GetActiveDetailsByStore(int id, int storeId);
        System.Threading.Tasks.Task CreateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images);
        System.Threading.Tasks.Task UpdateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images);
        IQueryable<BlogPost> GetByCollectionId(int? collectionId);
        Task UpdateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images, int[] neededTags);
    }

    public partial class BlogPostService
    {

        public IQueryable<BlogPost> GetAllBlogCategoryActiveByBrandId(int brandId)
        {
            return this.Get(q => q.Active && q.BrandId == brandId);
        }
        public IQueryable<BlogPost> GetByBlogPostCategoryAndPattern(int categoryId, string pattern)
        {
            IBlogCategoryService categoryService = DependencyUtils.Resolve<IBlogCategoryService>();
            List<int> categoriesList = new List<int>();
            var blogPost = this.GetActive(p =>
                (pattern == null || p.Title.Contains(pattern)) &&
                (categoryId <= 0 || p.BlogCategoryId == categoryId));
            return blogPost;
        }
        public async Task<BlogPost> GetBlogPostBySeoNameAsync(string seoName, int storeId)
        {
            var blogPost = await this.GetActive(q => q.SeoName == seoName && q.StoreId == storeId)
                .FirstOrDefaultAsync();
            return blogPost;
        }
        public async Task CreateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images, int[] tags)
        {
            // Images
            var blogPostImages = entity.BlogPostImages.ToArray();
            blogPostImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
            {
                imageEntity.ImageUrl = data;

                if (requireAddNew)
                {
                    entity.BlogPostImages.Add(imageEntity);
                }
            });

            // Collections
            foreach (var blogPostCollectionId in blogPostCollectionIds)
            {
                var connector = new BlogPostCollectionItemMapping()
                {
                    BlogPostCollectionId = blogPostCollectionId,
                    Active = true,
                };

                entity.BlogPostCollectionItemMappings.Add(connector);
            }

            this.Create(entity);

            // TagsMapping
            foreach (var tag in tags)
            {
                var tagMapping = new TagMapping()
                {
                    TagBlogId = entity.Id,
                    BlogPost = entity,
                    CategoryId = Convert.ToInt32(entity.BlogCategoryId),
                    BlogCategory = entity.BlogCategory,
                    TagId = tag,
                    //Tag = new TagApi().BaseService.Get(tag),
                };

                await (new TagMappingApi()).BaseService.CreateAsync(tagMapping);
            }
        }

        public IQueryable<BlogPost> GetByStoreId(int storeId)
        {
            var blogPosts = this.Get(q => q.StoreId == storeId);
            return blogPosts;
        }

        public IQueryable<BlogPostDetails> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var entities = this.Get(q =>
                q.StoreId == storeId &&
                (keyword == null || q.Title.Contains(keyword)));

            BlogPostSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case BlogPostSortableProperty.Id:
                        entities = entities.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case BlogPostSortableProperty.Title:
                        entities = entities.OrderBy(q => q.Title, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                entities = entities.OrderBy(q => q.Id);
            }

            var result = entities.Select(q => new BlogPostDetails()
            {
                BlogPost = q,
                BlogPostCollections = q.BlogPostCollectionItemMappings.AsQueryable()
                    .Where(p => p.Active && p.BlogPostCollection.Active)
                    .Select(p => p.BlogPostCollection),
                BlogPostImages = q.BlogPostImages.AsQueryable()
                        .Where(sq => sq.Active )
            });

            return result;
        }


        public IQueryable<BlogPostDetails> GetAdminByStoreWithFilterDateTime(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty, DateTime starttime, DateTime endtime)
        {
            var entities = this.Get(q =>
                q.StoreId == storeId &&
                (keyword == null || q.Title.Contains(keyword)) && q.CreatedTime >= starttime && q.CreatedTime <= endtime);

            BlogPostSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case BlogPostSortableProperty.Id:
                        entities = entities.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case BlogPostSortableProperty.Title:
                        entities = entities.OrderBy(q => q.Title, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                entities = entities.OrderBy(q => q.Id);
            }

            var result = entities.Select(q => new BlogPostDetails()
            {
                BlogPost = q,
                BlogPostCollections = q.BlogPostCollectionItemMappings.AsQueryable()
                    .Where(p => p.Active && p.BlogPostCollection.Active)
                    .Select(p => p.BlogPostCollection),
                BlogPostImages = q.BlogPostImages.AsQueryable()
                        .Where(sq => sq.Active)
            });

            return result;
        }
        public override void Create(BlogPost entity)
        {
            this.repository.Add(entity);
            this.Save();
        }

        public async Task<BlogPost> GetActiveByStoreAsync(int id, int storeId)
        {
            return await this.FirstOrDefaultAsync(q => q.Id == id && q.StoreId == storeId);
        }

        public BlogPost GetActiveByStore(int id, int storeId)
        {
            return this.FirstOrDefaultActive(q => q.Id == id && q.StoreId == storeId);
        }

        public async Task<BlogPostDetails> GetActiveDetailsByStoreAsync(string seoname, int storeId)
        {
            var entity = await this.GetActive(a=>a.SeoName.Equals(seoname) && storeId == a.StoreId)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostDetails()
                {
                    BlogPost = entity,
                    BlogPostCollections = entity.BlogPostCollectionItemMappings.AsQueryable()
                        .Where(q => q.Active && q.BlogPostCollection.Active)
                        .Select(q => q.BlogPostCollection),
                    BlogPostImages = entity.BlogPostImages.AsQueryable()
                        .Where(q => q.Active)
                };
            }
        }
        public async Task<BlogPostDetails> GetActiveDetailsByStoreAsync(int id, int storeId)
        {
            var entity = await this.GetActiveByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostDetails()
                {
                    BlogPost = entity,
                    BlogPostCollections = entity.BlogPostCollectionItemMappings.AsQueryable()
                        .Where(q => q.Active && q.BlogPostCollection.Active)
                        .Select(q => q.BlogPostCollection),
                    BlogPostImages = entity.BlogPostImages.AsQueryable()
                        .Where(q => q.Active)
                };
            }
        }
        public async Task UpdateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images, int[] neededTags) 
        {
            // Images
            var blogPostImages = entity.BlogPostImages.ToArray();
            blogPostImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
            {
                imageEntity.ImageUrl = data;

                if (requireAddNew)
                {
                    entity.BlogPostImages.Add(imageEntity);
                }
            });

            // Collections
            var blogPostCollectionItems = entity.BlogPostCollectionItemMappings.ToArray();

            foreach (var blogPostCollectionId in blogPostCollectionIds)
            {
                var connector = blogPostCollectionItems
                    .FirstOrDefault(q => q.BlogPostCollectionId == blogPostCollectionId);

                if (connector == null)
                {
                    connector = new BlogPostCollectionItemMapping()
                    {
                        BlogPostCollectionId = blogPostCollectionId,
                    };

                    entity.BlogPostCollectionItemMappings.Add(connector);
                } 
                connector.Active = true;
            }

            foreach (var blogPostCollectionItem in blogPostCollectionItems)
            {
                if (!blogPostCollectionIds.Contains(blogPostCollectionItem.BlogPostCollectionId))
                {
                    blogPostCollectionItem.Active = false;
                }
            }

            // Tags

            //get containing neededTags
            var oldTags = (new TagMappingApi()).GetAllMappingByBlogId(entity.Id);
            
            var deleteTags = oldTags.Where(q => !neededTags.Contains(q.TagId)).ToList();
            var addTags = neededTags.Where(q => !oldTags.Any(o => o.TagId == q));

            foreach (var tagMapping in deleteTags)
            {
                await (new TagMappingApi()).DeleteAsync(tagMapping.Id);
            }

            foreach (var addTag in addTags)
            {
                var tagMapping = new TagMapping()
                {
                    TagBlogId = entity.Id,
                    BlogPost = entity,
                    CategoryId = Convert.ToInt32(entity.BlogCategoryId),
                    BlogCategory = entity.BlogCategory,
                    TagId = addTag,
                    //Tag = new TagApi().BaseService.Get(addTag),
                };

                await (new TagMappingApi()).BaseService.CreateAsync(tagMapping);
            }

            this.Update(entity);
        }
        public BlogPostDetails GetActiveDetailsByStore(int id, int storeId)
        {
            var entity = this.GetActiveByStore(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new BlogPostDetails()
                {
                    BlogPost = entity,
                    BlogPostCollections = entity.BlogPostCollectionItemMappings.AsQueryable()
                        .Where(q => q.Active && q.BlogPostCollection.Active)
                        .Select(q => q.BlogPostCollection),
                    BlogPostImages = entity.BlogPostImages.AsQueryable()
                        .Where(q => q.Active)
                };
            }
        }
        public async System.Threading.Tasks.Task CreateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images)
        {
            // Images
            var blogPostImages = entity.BlogPostImages.ToArray();
            blogPostImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
            {
                imageEntity.ImageUrl = data;

                if (requireAddNew)
                {
                    entity.BlogPostImages.Add(imageEntity);
                }
            });

            // Collections
            foreach (var blogPostCollectionId in blogPostCollectionIds)
            {
                var connector = new BlogPostCollectionItemMapping()
                {
                    BlogPostCollectionId = blogPostCollectionId,
                    Active = true,
                };

                entity.BlogPostCollectionItemMappings.Add(connector);
            }

            await this.CreateAsync(entity);
        }

        public async System.Threading.Tasks.Task UpdateAsync(BlogPost entity, int[] blogPostCollectionIds, string[] images)
        {
            // Images
            var blogPostImages = entity.BlogPostImages.ToArray();
            blogPostImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
            {
                imageEntity.ImageUrl = data;

                if (requireAddNew)
                {
                    entity.BlogPostImages.Add(imageEntity);
                }
            });

            // Collections
            var blogPostCollectionItems = entity.BlogPostCollectionItemMappings.ToArray();

            foreach (var blogPostCollectionId in blogPostCollectionIds)
            {
                var connector = blogPostCollectionItems
                    .FirstOrDefault(q => q.BlogPostCollectionId == blogPostCollectionId);

                if (connector == null)
                {
                    connector = new BlogPostCollectionItemMapping()
                    {
                        BlogPostCollectionId = blogPostCollectionId,
                    };

                    entity.BlogPostCollectionItemMappings.Add(connector);
                }

                connector.Active = true;
            }

            foreach (var blogPostCollectionItem in blogPostCollectionItems)
            {
                if (!blogPostCollectionIds.Contains(blogPostCollectionItem.BlogPostCollectionId))
                {
                    blogPostCollectionItem.Active = false;
                }
            }

            await this.UpdateAsync(entity);
        }
        public IQueryable<BlogPost> GetByCollectionId(int? collectionId)
        {
            var rs = this.GetActive(q => q.BlogPostCollectionItemMappings.Any(a => a.BlogPostCollectionId == collectionId));
            return rs;
        }


        //HiepBP-PhuongTA
        //public IQueryable<BlogPost> GetByCollectionId(int? collectionId)
        //{
        //    var rs = this.GetActive(q => q.BlogPostCollectionItems.Any(a => a.BlogPostCollectionId == collectionId));
        //    return rs;
        //}

    }

    public class BlogPostDetails : IEntity
    {
        public BlogPost BlogPost { get; set; }
        public IQueryable<BlogPostCollection> BlogPostCollections { get; set; }
        public IEnumerable<BlogPostImage> BlogPostImages { get; set; }
    }

    public enum BlogPostSortableProperty
    {
        Id,
        Title,
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IBlogPostCollectionService
    {
        IQueryable<BlogPostCollection> GetActiveByStore(int storeId);
        IQueryable<BlogPostCollectionDetails> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty);
    }

    public partial class BlogPostCollectionService
    {

        public IQueryable<BlogPostCollectionDetails> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var list = this.GetActive(q =>
                q.StoreId == storeId &&
                (keyword == null || q.Name.Contains(keyword)));

            BlogPostCollectionSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case BlogPostCollectionSortableProperty.Id:
                        list = list.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case BlogPostCollectionSortableProperty.Name:
                        list = list.OrderBy(q => q.Name, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                list = list.OrderBy(q => q.Id);
            }

            var result = list.Select(q => new BlogPostCollectionDetails()
            {
                BlogPostCollection = q,
                PostCount = q.BlogPostCollectionItemMappings.Count(p => p.Active && p.BlogPost.Active),
            });

            return result;
        }

        public IQueryable<BlogPostCollection> GetActiveByStore(int storeId)
        {
            return this.GetActive(q => q.StoreId == storeId)
                .OrderBy(q => q.Name);
        }
    }

    public class BlogPostCollectionDetails
    {
        public BlogPostCollection BlogPostCollection { get; set; }
        public int PostCount { get; set; }
    }

    public enum BlogPostCollectionSortableProperty
    {
        Id,
        Name,
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IWebPageService
    {
        IQueryable<WebPage> GetWebPagesByStoreId(int storeId);
        IQueryable<WebPage> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty);
        Task<WebPage> GetActiveByStoreAsync(int id, int storeId);
    }
    public partial class WebPageService
    {
        public IQueryable<WebPage> GetWebPagesByStoreId(int storeId)
        {
            var webPages = this.Get(m => m.StoreId == storeId);
            return webPages;
        }

        public IQueryable<WebPage> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q =>
                q.StoreId == storeId &&
                (keyword == null || q.Title.Contains(keyword)));

            WebPageSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case WebPageSortableProperty.Id:
                        result = result.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case WebPageSortableProperty.Title:
                        result = result.OrderBy(q => q.Title, orderByProperty.Value);
                        break;
                    case WebPageSortableProperty.PageTitle:
                        result = result.OrderBy(q => q.PageTitle, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.Id);
            }

            return result;
        }

        public async Task<WebPage> GetActiveByStoreAsync(int id, int storeId)
        {
            return await this.FirstOrDefaultActiveAsync(q => q.Id == id && q.StoreId == storeId);
        }

    }

    public enum WebPageSortableProperty
    {
        Id,
        Title,
        PageTitle,
    }

}

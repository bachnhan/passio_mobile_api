using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IStoreDomainService
    {
        Task<StoreDomain> GetAsync(string protocol, string hostName, int port);
        StoreDomain Get(string protocol, string hostName, int port, string directory);
        IQueryable<StoreDomain> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty);
        void CreateStoreDomain(int StoreId);
    }

    public partial class StoreDomainService
    {

        public StoreDomain Get(string protocol, string hostName, int port, string directory)
        {
            var potentialDomains = this.GetActive(q => q.Active && q.Protocol == protocol && q.HostName == hostName && q.Port == port)
                .ToList();


            // Check for directory priority
            if (directory != null)
            {
                foreach (var domain in potentialDomains)
                {
                    if (domain.Directory?.ToLowerInvariant() == directory.ToLowerInvariant())
                    {
                        return domain;
                    }
                }
            }

            return potentialDomains.FirstOrDefault(q => q.Directory == null);
        }

        public async Task<StoreDomain> GetAsync(string protocol, string hostName, int port)
        {
            return await this
                .GetActive(q => q.Protocol == protocol && q.HostName == hostName && q.Port == port)
                .FirstOrDefaultAsync();
        }

        public IQueryable<StoreDomain> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q => q.Active && q.StoreId == storeId &&
                (keyword == null || (q.Protocol + "://" + q.HostName + ":" + q.Port).Contains(keyword)));

            StoreDomainSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case StoreDomainSortableProperty.Id:
                        result = result.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case StoreDomainSortableProperty.Protocol:
                        result = result.OrderBy(q => q.Protocol + q.HostName + q.Port, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.Id);
            }

            return result;
        }

        public void CreateStoreDomain(int StoreId)
        {
            var entity = new StoreDomain
            {
                StoreId = StoreId,
                Active = true,
                HostName = "hello",
                Port = 80,
                Protocol = "http",
            };
            this.Create(entity);
        }
    }


    public enum StoreDomainSortableProperty
    {
        Id,
        Protocol,
    }
}

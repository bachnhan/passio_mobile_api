using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IStoreGroupMappingService
    {
        Task DeleteByStoreIDAndStoreGroupIDAsync(int storeid, int storegroupid);
        IQueryable<StoreGroupMapping> GetStoreGroupMappingsByGroupID(int storeGroupID);
        Task DeleteByStoreGroupIDAsync(int storeGroupId);
    }

    public partial class StoreGroupMappingService : IStoreGroupMappingService
    {
        public async Task DeleteByStoreIDAndStoreGroupIDAsync(int storeid, int storegroupid)
        {
            var entity = this.Get(q => q.StoreID == storeid && q.StoreGroupID == storegroupid).FirstOrDefault();
            await this.DeleteAsync(entity);
        }

        public IQueryable<StoreGroupMapping> GetStoreGroupMappingsByGroupID(int storeGroupID)
        {
            return this.Get(q => q.StoreGroupID == storeGroupID);
        }

        public async Task DeleteByStoreGroupIDAsync(int storeGroupId)
        {
            var storeGroupMappings = this.Get(q => q.StoreGroupID == storeGroupId).ToList();
            foreach (var item in storeGroupMappings)
            {
                await this.DeleteAsync(item);
            }
        }
    }
}

using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class StoreGroupMappingApi
    {
        public async Task DeleteByStoreIDAndStoreGroupIDAsync(int storeid, int storegroupid)
        {
            await this.BaseService.DeleteByStoreIDAndStoreGroupIDAsync(storeid, storegroupid);
        }

        public async Task DeleteByStoreGroupIDAsync(int storeGroupId)
        {
            await this.BaseService.DeleteByStoreGroupIDAsync(storeGroupId);
        }

        public IEnumerable<StoreGroupMappingViewModel> GetStoreGroupMappingsByGroupID(int groupID)
        {
            return this.BaseService.GetStoreGroupMappingsByGroupID(groupID).ProjectTo<StoreGroupMappingViewModel>(this.AutoMapperConfig);
        }

    }
}

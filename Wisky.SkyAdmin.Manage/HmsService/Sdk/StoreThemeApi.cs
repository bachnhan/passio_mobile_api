using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class StoreThemeApi
    {
        public IQueryable<StoreThemeViewModel> GetActiveStoreThemeByStoreId(int storeId)
        {
            return this.BaseService.GetActiveStoreThemeByStoreId(storeId).ProjectTo<StoreThemeViewModel>(this.AutoMapperConfig);
        }

        public async Task CreateStoreThemeAsync(StoreThemeViewModel model)
        {
            await this.BaseService.CreateStoreThemeAsync(model.ToEntity());
        }
    }
}

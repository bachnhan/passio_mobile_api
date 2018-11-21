using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{

    public partial class StoreWebSettingApi
    {

        public IEnumerable<StoreWebSettingViewModel> GetActiveByStore(int storeId)
        {
            return this.BaseService.GetActiveByStore(storeId)
                .ProjectTo<StoreWebSettingViewModel>(this.AutoMapperConfig)
                .ToList();
        }

        public async Task MassUpdate(IEnumerable<KeyValuePair<int, string>> values, int storeId)
        {
            await this.BaseService.MassUpdate(values, storeId);
        }

    }

}

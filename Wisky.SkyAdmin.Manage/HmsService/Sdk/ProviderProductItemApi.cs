using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{
    public partial class ProviderProductItemMappingApi
    {
        #region Provider
        public IEnumerable<ProviderProductItemMappingViewModel> GetProviderProductItems()
        {
            var providerProductItems = this.BaseService.GetProviderProductItems()
                .ProjectTo<ProviderProductItemMappingViewModel>(this.AutoMapperConfig)
                .ToList();
            return providerProductItems;
        }
        public async Task<ProviderProductItemMappingViewModel> GetProviderProductItemById(int ProviderId, int ItemId)
        {
            var providerProductItem = await this.BaseService.GetProviderProductItemById(ProviderId, ItemId);
            if(providerProductItem == null)
            {
                return null;
            }
            else
            {
                return new ProviderProductItemMappingViewModel(providerProductItem);
            }
        }
        public IEnumerable<ProviderProductItemMapping> GetProviderProductItemByItemId(int ItemId)
        {
            var providerList = this.BaseService.GetProviderProductItemByItemId(ItemId)
                .ToList();
            return providerList;
        }
        #endregion
    }
}

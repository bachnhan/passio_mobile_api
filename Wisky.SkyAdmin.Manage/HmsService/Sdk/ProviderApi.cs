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
    public partial class ProviderApi
    {
        #region ProductInventory
        public IEnumerable<ProviderViewModel> GetProviders()
        {
            var providers = this.BaseService.GetProviders()
                    .ProjectTo<ProviderViewModel>(this.AutoMapperConfig)
                .ToList();
                return providers;
        }

        public IEnumerable<ProviderViewModel> GetProvidersByBrand(int brandId)
        {
            var providers = this.BaseService.GetProvidersByBrand(brandId)
                .ProjectTo<ProviderViewModel>(this.AutoMapperConfig)
                .ToList();
            return providers;
        }

        public IEnumerable<ProviderViewModel> GetProvidersActive()
        {
            var providers = (this.BaseService.GetProviders()
                .Where(q=>q.IsAvailable==true))               
                .ProjectTo<ProviderViewModel>(this.AutoMapperConfig)               
                .ToList();
            return providers;
        }
        #endregion
        #region Provider

        public async Task<ProviderViewModel> GetProviderById(int providerId)
        {
            var provider = await this.BaseService.GetProviderById(providerId);
            if(provider == null)
            {
                return null;
            }
            else
            {
                return new ProviderViewModel(provider);
            }
        }

        public async System.Threading.Tasks.Task UpdateProviderAsync(ProviderViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.Id);
            entity.ProviderName = model.ProviderName;
            entity.Address = model.Address;
            entity.Phone = model.Phone;
            entity.Email = model.Email;
            await this.BaseService.UpdateAsync(entity);
        }

        public async System.Threading.Tasks.Task DeleteAccountAsync(ProviderViewModel model)
        {
            model.IsAvailable = false;
            await this.EditAsync(model.Id, model);
        }

        public async System.Threading.Tasks.Task CreateProviderAsync(ProviderViewModel model)
        {
            var entity = new Provider();
            entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);
		}

        public string GetProviderNameByID(int id)
        {
            var providerName = this.BaseService.GetProviderNameByID(id);
            if (providerName == null)
            {
                return null;
            }
            return providerName;
        }
        #endregion
    }
}

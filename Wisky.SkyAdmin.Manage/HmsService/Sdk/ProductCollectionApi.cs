using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{

    public partial class ProductCollectionApi
    {

        public async Task<IEnumerable<ProductCollectionViewModel>> GetByStoreIdAsync(int storeId)
        {
            return await this.BaseService.GetByStoreId(storeId)
                .ProjectTo<ProductCollectionViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public async Task<ProductCollectionDetailsViewModel> GetDetails(int id)
        {
            var entity = await this.BaseService.GetDetailsAsync(id);

            var result = new ProductCollectionDetailsViewModel(entity);
            result.Items = entity.Items
                .Where(a => a.Active && a.Product.Active)
                .ProjectTo<ProductCollectionItemMappingViewModel>(this.AutoMapperConfig);
            return result;
        }

        public async Task<ProductCollectionDetailsViewModel> GetDetailsByName(string name, int brandId)
        {
            var entity = await this.BaseService.GetDetailsByNameAsync(name, brandId);

            var result = new ProductCollectionDetailsViewModel(entity);
            result.Items = entity.Items
                .Where(a => a.Active && a.Product.Active)
                .ProjectTo<ProductCollectionItemMappingViewModel>(this.AutoMapperConfig);
            return result;
        }

        public async Task<IEnumerable<ProductCollectionViewModel>> GetActiveByStoreIdAsync(int storeId)
        {
            return await this.BaseService.GetByStoreId(storeId)
                .ProjectTo<ProductCollectionViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public IEnumerable<ProductCollection> GetActiveByBrandId(int brandId)
        {
            return this.BaseService.GetActive(c => c.BrandId == brandId).ToList();
        }

        #region With Brand Id
        public async Task<IEnumerable<ProductCollectionViewModel>> GetByBrandIdAsync(int brandId)
        {
            return await this.BaseService.GetByBrandId(brandId)
                .ProjectTo<ProductCollectionViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        #endregion
    }

}

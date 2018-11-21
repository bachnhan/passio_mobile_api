using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyWeb.DatVM.Mvc;
using HmsService.Models;
using HmsService.Models.Entities.Services;

namespace HmsService.Sdk
{

    public partial class ProductImageCollectionApi
    {

        public async Task<IEnumerable<ImageCollectionViewModel>> GetByStoreIdAsync(int storeId)
        {
            return await this.BaseService.GetByStoreId(storeId)
                .ProjectTo<ImageCollectionViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public async Task EditAsync(ProductImageCollectionViewModel model, IEnumerable<ProductImageCollectionItemMappingViewModel> items)
        {

            model = Utils.ToExactType<ProductImageCollectionViewModel, ProductImageCollectionViewModel>(model);

            var entity = await this.BaseService.GetAsync(model.Id);

            await this.BaseService.UpdateAsync(entity, items.Select(a => new KeyValuePair<string, string>(a.ImageUrl, a.Title)).ToArray());
        }

        public async Task<ProductImageCollectionDetailsViewModel> GetByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ProductImageCollectionDetailsViewModel(entity);
            }
        }

        public PagingViewModel<ProductImageCollectionDetailsViewModel> GetAdminWithFilter(int storeId, string keyword,
            int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {

            var pagedList = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, sortKeyAsc)
                .ProjectTo<ProductImageCollectionDetailsViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<ProductImageCollectionDetailsViewModel>(pagedList);
        }
    }

}

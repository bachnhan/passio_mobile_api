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
using HmsService.Models.Entities;

namespace HmsService.Sdk
{

    public partial class ImageCollectionApi
    {

        public async Task<IEnumerable<ImageCollectionViewModel>> GetByStoreIdAsync(int storeId)
        {
            return await this.BaseService.GetByStoreId(storeId)
                .ProjectTo<ImageCollectionViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public IQueryable<ImageCollection> GetAllByStoreId(int storeId, int brandId)
        {
            if (storeId <= 0)
            {
                var storeApi = new StoreApi();
                var listStoreID = storeApi.GetActiveStoreByBrandId(brandId).Select(q => q.ID);
                return this.BaseService.GetActive(q => listStoreID.Contains(q.StoreId));
            }
            else
            {
                return this.BaseService.Get(q => q.StoreId == storeId && q.Active == true);
            }
        }


        public async Task EditAsync(ImageCollectionViewModel model, IEnumerable<ImageCollectionItemViewModel> items)
        {

            model = Utils.ToExactType<ImageCollectionViewModel, ImageCollectionViewModel>(model);

            var entity = await this.BaseService.GetAsync(model.Id);

            await this.BaseService.UpdateAsync(entity, items.Select(a => new KeyValuePair<string, string>(a.ImageUrl, a.Title)).ToArray());
        }

        public async Task<ImageCollectionDetailsViewModel> GetByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new ImageCollectionDetailsViewModel(entity);
            }
        }

        public PagingViewModel<ImageCollectionDetailsViewModel> GetAdminWithFilter(int storeId, string keyword,
            int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {

            var pagedList = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, sortKeyAsc)
                .ProjectTo<ImageCollectionDetailsViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<ImageCollectionDetailsViewModel>(pagedList);
        }
    }

}

using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using SkyWeb.DatVM.Mvc;

namespace HmsService.Sdk
{
    public partial class WebPageApi
    {
        public async Task<IEnumerable<WebPageViewModel>> GetWebPagesByStoreIdAsync(int storeId)
        {
            var webPages = await this.BaseService.GetWebPagesByStoreId(storeId)
                .ProjectTo<WebPageViewModel>(this.AutoMapperConfig)
                .ToListAsync();
            return webPages;
        }

        public PagingViewModel<WebPageViewModel> GetAdminWithFilterAsync(int storeId, string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, sortKeyAsc)
                .ProjectTo<WebPageViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<WebPageViewModel>(pagedList);
        }

        public async Task<WebPageViewModel> GetByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new WebPageViewModel(entity);
            }
        }
    }
}

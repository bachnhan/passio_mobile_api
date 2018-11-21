using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;

namespace HmsService.Sdk
{
    public partial class CustomerFeedbackApi
    {

        public PagingViewModel<CustomerFeedbackViewModel> GetAdminWithFilterAsync(int storeId, string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, sortKeyAsc)
                .ProjectTo<CustomerFeedbackViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<CustomerFeedbackViewModel>(pagedList);
        }
        public async Task<CustomerFeedbackViewModel> GetByStoreIdAsync(int id, int storeId)
        {
            var entity = await this.BaseService.GetActiveByStoreAsync(id, storeId);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new CustomerFeedbackViewModel(entity);
            }
        }
    }
}

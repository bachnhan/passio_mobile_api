using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{

    public partial class StoreDomainApi
    {

        public StoreDomainViewModel Get(string protocol, string hostName, int port, string directory)
        {
         var entity = this.BaseService.Get(protocol, hostName, port, directory);

            if (entity == null)
            {
                return null;
            }
            else
            {
                var result = new StoreDomainViewModel(entity);
                result.StoreInfo = new StoreViewModel(entity.Store);

                return result;
            }
        }

        public PagingViewModel<StoreDomainViewModel> GetAdminWithFilterAsync(int storeId, string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByStoreWithFilter(storeId, keyword, sortKeyAsc)
                .ProjectTo<StoreDomainViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<StoreDomainViewModel>(pagedList);
        }

        public void CreateAsync(int StoreId)
        {
            this.BaseService.CreateStoreDomain(StoreId);
        }
    }

}

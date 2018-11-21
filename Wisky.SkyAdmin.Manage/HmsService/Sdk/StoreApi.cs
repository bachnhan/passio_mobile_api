using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.Sdk
{
    public partial class StoreApi
    {
        public IQueryable<StoreViewModel> GetAllActiveStore(int brandId)
        {
            return BaseService.GetActiveStoreByBrandId(brandId).ProjectTo<StoreViewModel>(AutoMapperConfig);
        }


        public IQueryable<StoreViewModel> GetAllStore(int brandId)
        {
            return this.BaseService.GetAllStore(brandId)
                .ProjectTo<StoreViewModel>(this.AutoMapperConfig);
        }

        public IEnumerable<StoreUser> GetAllStoreUser()
        {
            return this.BaseService.GetAllStoreUser();
        }

        public IEnumerable<StoreViewModel> GetStoreArrayByID(int storeId)
        {
            return this.BaseService.Get(a => a.isAvailable == true && a.ID == storeId).ProjectTo<StoreViewModel>(this.AutoMapperConfig);
        }

        public IEnumerable<Store> GetStoresToAssign()
        {
            return this.BaseService.GetStores();
        }

        public Store GetStoreById(int storeId)
        {
            return this.BaseService.Get(storeId);
        }


        public PagingViewModel<StoreViewModel> GetAdminWithFilterAsync(string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByStoreWithFilter(keyword, sortKeyAsc)
                .ProjectTo<StoreViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<StoreViewModel>(pagedList);
        }

        public PagingViewModel<StoreViewModel> GetAdminWithStoreOfBrandFilterAsync(string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc, int brandId)
        {
            var pagedList = this.BaseService.GetAdminByStoreOfBrandWithFilter(keyword, sortKeyAsc, brandId)
                .ProjectTo<StoreViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<StoreViewModel>(pagedList);
        }

        public IQueryable<StoreViewModel> GetActiveStoreByBrandId(int brandId)
        {
            return this.BaseService.GetActiveStoreByBrandId(brandId).ProjectTo<StoreViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<StoreViewModel> GetStoreByBrandId(int brandId)
        {
            return this.BaseService.GetStoreByBrandId(brandId).ProjectTo<StoreViewModel>(this.AutoMapperConfig);
        }

        public IEnumerable<Store> GetListStoreByBrandId(int brandId)
        {
            return this.BaseService.GetStoreByBrandId(brandId).ToList();
        }

        public IQueryable<Store> GetAllStoreByBrandId(int brandId)
        {
            return this.BaseService.Get(q => q.BrandId == brandId);
        }

        public IEnumerable<StoreViewModel> GetListStoreByBrandAndNone(int brandId)
        {
            return this.BaseService.Get(q => (q.BrandId == brandId || q.BrandId == 0 || q.BrandId == null) && q.isAvailable == true)
                .ProjectTo<StoreViewModel>(this.AutoMapperConfig)
                .ToList();
        }

        public async System.Threading.Tasks.Task CreateStoreAsync(StoreViewModel model, IEnumerable<Product> listProduct)
        {
            await this.BaseService.CreateStoreAsync(model.ToEntity(), listProduct);
        }

        public async System.Threading.Tasks.Task EditStoreAsync(StoreViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.ID);
            entity.Name = model.Name;
            entity.ShortName = model.ShortName;
            entity.Email = model.Email;
            entity.Address = model.Address;
            entity.Lat = model.Lat;
            entity.Lon = model.Lon;
            entity.Phone = model.Phone;
            entity.Fax = model.Fax;
            entity.Type = model.Type;
            entity.OpenTime = model.OpenTime;
            entity.CloseTime = model.CloseTime;
            //entity.DefaultAdminPassword = model.DefaultAdminPassword;
            entity.isAvailable = model.isAvailable;
            entity.RunReport = (bool)model.RunReport ? model.RunReport : false;
            entity.StoreFeatureFilter = model.StoreFeatureFilter;
            await this.BaseService.UpdateAsync(entity);
        }

        public async System.Threading.Tasks.Task AssignStoreAsync(int id, int brandid)
        {
            var entity = await this.BaseService.GetAsync(id);
            entity.BrandId = brandid;            
            await this.BaseService.UpdateAsync(entity);
        }
        public async System.Threading.Tasks.Task UnAssignStoreAsync(int id)
        {
            var entity = await this.BaseService.GetAsync(id);
            // 
            entity.BrandId = 0;
            await this.BaseService.UpdateAsync(entity);
        }

        public async Task DeactiveStore(int id)
        {
            var entity = await this.BaseService.GetAsync(id);
            entity.isAvailable = false;
            await this.BaseService.UpdateAsync(entity);
        }

        public async System.Threading.Tasks.Task DeActiveStore(int Id)
        {
            await this.BaseService.DeActiveStore(Id);
        }

        public IQueryable<Store> GetStoreEntitiesByBrand(int brandId)
        {
            return this.BaseService.GetStoresByBrand(brandId);
        }

        public async Task<IEnumerable<StoreUserViewModel>> GetStoreUserByStoreIdAsync(int storeId)
        {
            var result = (await this.BaseService.GetStoreUserByStoreIdAsync(storeId)).ProjectTo<StoreUserViewModel>(this.AutoMapperConfig);
            return result;
        }

        public IEnumerable<StoreViewModel> GetStoresByUserApi(string user)
        {
            return this.BaseService.GetStoresByUser(user).ProjectTo<StoreViewModel>(this.AutoMapperConfig);
        }
        public IEnumerable<StoreViewModel> GetStoreByArrayStoreID(int[] stores, int brandID)
        {
            return this.BaseService.GetAllStore(brandID).Where(s => stores.ToList().Contains(s.ID))
                .ProjectTo<StoreViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<Store> GetActiveReturnEntities()
        {
            return this.BaseService.GetActive().AsQueryable();
        }

        #region SystemReport
        public IEnumerable<Store> GetStores()
        {
            var stores = this.BaseService.GetStores()
                //.ProjectTo<StoreViewModel>(this.AutoMapperConfig)
                .ToList();
            return stores;
        }

        public IEnumerable<Store> GetStoresByBrandId(int brandId)
        {
            var stores = this.BaseService.GetStoresByBrand(brandId);
            return stores;
        }


        public IEnumerable<StoreViewModel> GetStoresByStoreIds(IQueryable<PromotionStoreMappingViewModel> promotionStoreMappingList)
        {
            var stores = this.BaseService.GetActive(q => promotionStoreMappingList.Any(a => a.StoreId == q.ID)).ProjectTo<StoreViewModel>(this.AutoMapperConfig);
            return stores;
        }

        public IQueryable<Store> GetStoresByPromotionId(int promotionId, int brandId)
        {
            return this.BaseService.GetStoresByPromotionId(promotionId, brandId);
        }

        public IEnumerable<Store> GetStores(int brandId)
        {
            var stores = this.BaseService.GetStores()
                .Where(q => q.BrandId == brandId);
                //.ProjectTo<StoreViewModel>(this.AutoMapperConfig)
                
            return stores;
        }

        public IEnumerable<Store> GetStoresByBrandId(int? brandId)
        {
            var stores = this.BaseService.GetStores()
                .Where(q => q.BrandId == brandId.Value)
                //.ProjectTo<StoreViewModel>(this.AutoMapperConfig)
                .ToList();
            return stores;
        }
        public string GetStoreNameByID(int id)
        {
            var storeName = this.BaseService.GetStoreNameByID(id);
            if (storeName == null)
            {
                return null;
            }
            return storeName;
        }
        public IEnumerable<StoreViewModel> GetStoresByBrandIdAndType(int brandId, int type)
        {
            var stores = this.BaseService.GetStoresByBrandIdAndType(brandId, type)
                .ProjectTo<StoreViewModel>(this.AutoMapperConfig)
                .ToList();
            return stores;
        }
        #endregion
        #region StoreReport
        public async Task<StoreViewModel> GetStoreByID(int storeid)
        {
            var store = await this.BaseService.GetStoreByID(storeid);
            if (store == null)
            {
                return null;
            }
            else
            {
                return new StoreViewModel(store);
            }
        }

        public Store GetStoreByIdSync(int storeId)
        {
            var store = this.BaseService.Get(storeId);
            if (store == null)
            {
                return null;
            }
            else
            {
                return store;
            }
        }

        public IQueryable<Store> GetStoresByUser(string username)
        {
            return this.BaseService.GetStoresByUser(username);
        }
        #endregion
        public IEnumerable<Store> GetStoreByGroupId(int groupId)
        {
            var result = this.BaseService.GetStoreByGroupId(groupId);
            return result;
        }
    }
}

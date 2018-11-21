using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class StoreGroupApi
    {

        public IEnumerable<StoreGroupViewModel> GetAllStoreGroup()
        {
            var result = this.BaseService.Get().ProjectTo<StoreGroupViewModel>(this.AutoMapperConfig).ToList();
            return result;
        }

        public async System.Threading.Tasks.Task CreateStoreGroupAsync(StoreGroupEditViewModel model)
        {
            await this.BaseService.CreateAsync(model.ToEntity());
        }

        public async System.Threading.Tasks.Task UpdateStoreGroupAsync(StoreGroupEditViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.GroupID);
            entity.GroupName = model.GroupName;
            entity.Description = model.Description;
            await this.BaseService.UpdateAsync(entity);
        }

        public async System.Threading.Tasks.Task DeleteStoreGroupAsync(StoreGroupEditViewModel model)
        {
            var storeGroupMapping = new StoreGroupMappingApi();
            await storeGroupMapping.DeleteByStoreGroupIDAsync(model.GroupID);

            await this.DeleteAsync(model.GroupID);
        }

        public IEnumerable<StoreGroupViewModel> GetStoreGroupByBrandId(int brandId)
        {            
            return this.BaseService.GetStoreGroupByBrand(brandId).ProjectTo<StoreGroupViewModel>(AutoMapperConfig);
        }

        public StoreGroupViewModel GetStoreGroupByID(int ID)
        {
            var entity = this.BaseService.Get(ID);
            var list = new List<StoreGroup>();
            list.Add(entity);
            var modelList = list.AsQueryable().ProjectTo<StoreGroupViewModel>(AutoMapperConfig);
            return modelList.ElementAt(0);
        }

        public StoreGroup GetStoreGroupByIDEntity(int ID)
        {
            var entity = this.BaseService.Get(ID);
            return entity;
        }

        //public async Task AssignStore(StoreGroupEditViewModel model)
        //{
        //    var mappingApi = new StoreGroupMappingApi();
        //    var mappedStoreIDs = mappingApi.GetStoreGroupMappingsByGroupID(model.GroupID);
        //    var assignedStoreIDs = model.StoresInGroup.Select(q => q.ID);
        //    mappingsModel.SkipWhile(q => assignedStores.Contains(q))
        //}
    }
}

using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HmsService.Sdk
{
    public partial class PromotionStoreMappingApi
    {
        public IQueryable<PromotionStoreMappingViewModel> GetActivePromotionStoreMappingByPromotionID(int promotionId)
        {
            return this.BaseService.GetActivePromotionStoreMappingByPromotionID(promotionId).ProjectTo<PromotionStoreMappingViewModel>(this.AutoMapperConfig);
           
        }

        public IQueryable<PromotionStoreMappingViewModel> GetAllPromotionStoreMappingByPromotionID(int promotionId)
        {
            return this.BaseService.GetAllPromotionStoreMappingByPromotionID(promotionId).ProjectTo<PromotionStoreMappingViewModel>(this.AutoMapperConfig);

        }

        public IQueryable<int> GetAllActivePromotionIDByStoreID(int storeId)
        {
            return this.BaseService.GetAllActivePromotionIDByStoreID(storeId);

        }

        public async Task CreatePromotionStoreMapping(int promotionId, int storeId)
        {
            PromotionStoreMappingApi api = new PromotionStoreMappingApi();
            PromotionStoreMappingViewModel mappingModel = new PromotionStoreMappingViewModel();
                mappingModel.PromotionId = promotionId;
                mappingModel.StoreId = storeId;
                mappingModel.Active = true;
                await api.CreateAsync(mappingModel);
        }


        public IQueryable<Promotion> GetPromotionByStoreId (int storeId)
        {
            return this.BaseService.Get(q => q.StoreId == storeId && q.Active == true).Select(q=>q.Promotion);
        }


    }
}

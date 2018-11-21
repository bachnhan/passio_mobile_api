using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.Models.Entities.Services;
using HmsService.Models;

namespace HmsService.Sdk
{
    public partial class    PromotionApi
    {
        public IQueryable<PromotionViewModel> GetActivePromotion(int brandId)
        {
            return this.BaseService.GetActivePromotion()
                .Where(q => q.BrandId == brandId)
                .ProjectTo<PromotionViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<PromotionViewModel> GetAllPromotion(int brandId)
        {
            return this.BaseService.Get()
                .Where(q => q.BrandId == brandId)
                .ProjectTo<PromotionViewModel>(this.AutoMapperConfig);
        }


        public PromotionViewModel GetPromotionById(int id)
        {
            var promotion = this.BaseService.GetPromotionById(id);
            if (promotion == null)
            {
                return null;
            }
            else
            {
                return new PromotionViewModel(promotion);
            }
        }

        public async Task EditPromotionIsApplyOnce(int id, bool isApplyOnce)
        {
            var promotion = this.BaseService.GetPromotionById(id);
            promotion.IsApplyOnce = isApplyOnce;

            await this.BaseService.UpdateAsync(promotion);
        }

        public async Task CreatePromotion(Promotion entity)
        {
            await this.BaseService.CreatePromotion(entity);
        }

        public async Task<int> CreatePromotionApplyforStore(PromotionEditViewModel model, int[] storeIds)
        {
            try
            {
                var entity = model.ToEntity();
                await this.BaseService.CreatePromotion(entity);
                if (storeIds == null)
                {
                    return entity.PromotionID;
                }
                PromotionStoreMappingApi api = new PromotionStoreMappingApi();
                foreach (var item in storeIds)
                {
                    PromotionStoreMappingViewModel mappingModel = new PromotionStoreMappingViewModel();
                    mappingModel.PromotionId = entity.PromotionID;
                    mappingModel.StoreId = item;
                    mappingModel.Active = true;
                    await api.CreateAsync(mappingModel);
                }
                return entity.PromotionID;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task UpdatePromotion(Promotion entity)
        {
            await this.BaseService.UpdatePromotion(entity);
        }

        public void Update(PromotionViewModel model)
        {
            var entity = this.BaseService.Get(model.PromotionID);
            model.CopyToEntity(entity);
            this.BaseService.Update(entity);
        }

        public async Task DeactivePromotionAsync(int id)
        {
            var promotion = this.BaseService.GetPromotionById(id);
            promotion.Active = false;
            await this.BaseService.DeactivePromotionAsync(promotion);
        }
        public Promotion GetByIdBrandId(int id, int brandId)
        {
            var promotion = this.BaseService.GetByIdBrandId(id, brandId);
            if (promotion == null)
            {
                return null;
            }
            else
            {
                return promotion;
            }
        }
        public Promotion GetByPromoCode(string code)
        {
            var promotion = this.BaseService.GetByPromoCode(code);
            if (promotion == null)
            {
                return null;
            }
            else
            {
                return promotion;
            }
        }

        public IQueryable<Promotion> GetPromotionByBrandId(int brandId)
        {
            var result = this.BaseService.GetPromotionByIdTime(brandId);
            return result;
        }

        public IQueryable<PromotionViewModel> GetPromotionVMByBrandId(int brandId)
        {
            var result = this.GetPromotionByIdTime(brandId);
            return result;
        }

        public IQueryable<Promotion> GetPromotionByStoreId(int storeId)
        {
            var result = this.BaseService.GetPromotionByStoreId(storeId);
            return result;
        }

        public IQueryable<PromotionEditViewModel> GetPromotionByIdTime(int brandId)
        {
            var result = this.BaseService.GetPromotionByIdTime(brandId).ProjectTo<PromotionEditViewModel>(this.AutoMapperConfig);
            return result;
        }

        public List<ProductPromotionReport> ProductPromotionReport(DateTime startTime, DateTime endTime, int storeId, int brandId)
        {
            var allPromotionOrder = this.BaseService.Get(q => q.PromotionType == (int)PromotionApplyLevelEnum.Order);
            var allPromotionOrderDetail = this.BaseService.Get(q => q.PromotionType == (int)PromotionApplyLevelEnum.OrderDetail);
            var orderDetailApi = new OrderDetailApi();
            var orderApi = new OrderApi();
            var orderPromotionMapping = new OrderPromotionMappingApi();
            var orderDetailPromotionMapping = new OrderDetailPromotionMappingApi();
            var allMapping = orderPromotionMapping.BaseService.Get();
            var allMappingDetail = orderPromotionMapping.BaseService.Get();

            var orderPromotionMappingApi = new OrderPromotionMappingApi();
            var todayOrderDetails = orderDetailApi.GetAllOrderDetail(startTime, endTime, storeId, brandId);
            var orders = orderApi.OrderInStore(storeId, startTime, endTime, brandId);
            var joinDataOrderMapping = from order in orders
                                       join promotionMapping in allMapping
                                       on order.RentID equals promotionMapping.OrderId
                                       select promotionMapping;
            //var joinData
            //var allOrder = orderApi.order
            //var orderPromotionMapping = 
            return null;
        } 
        public Promotion GetPromotionByDateAndId(int id)
        {
            return this.BaseService.GetPromotionByIdAndDate(id);
        }
        public Promotion GetPromotionByDateAndCode(string code)
        {
            return this.BaseService.GetPromotionByDateAndCode(code);
        }
        
    }
}

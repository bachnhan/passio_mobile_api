
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IPromotionService
    {
        IQueryable<Promotion> GetActivePromotion();
        Promotion GetPromotionById(int id);
        Task CreatePromotion(Promotion entity);
        Task UpdatePromotion(Promotion entity);
        Task DeactivePromotionAsync(Promotion entity);

        Promotion GetByIdBrandId(int id, int brandId);
        Promotion GetByPromoCode(string code);
        IQueryable<Promotion> GetPromotionByBrandId(int brandId);
        IQueryable<Promotion> GetPromotionByIdTime(int brandId);
        IQueryable<Promotion> GetPromotionByStoreId(int storeId);
        Promotion GetPromotionByIdAndDate(int id);
        Promotion GetPromotionByDateAndCode(string code);
    }

    public partial class PromotionService
    {
        public IQueryable<Promotion> GetActivePromotion()
        {          
            return this.GetActive();
                
        }
        public Promotion GetPromotionByIdAndDate(int id)
        {
            var result = this.Get(q => q.Active == true && q.PromotionID == id && DateTime.Now >= q.FromDate && DateTime.Now <= q.ToDate).FirstOrDefault();
            return result;
        }
        public Promotion GetPromotionByDateAndCode(string code)
        {
            var result = this.Get(q => q.Active == true && q.PromotionCode == code && DateTime.Now >= q.FromDate && DateTime.Now <= q.ToDate).FirstOrDefault();
            return result;
        }
        public IQueryable<Promotion> GetPromotionByBrandId(int brandId)
        {
            throw new NotImplementedException();
        }
        public Promotion GetPromotionById(int id)
        {
            return this.FirstOrDefault(q => q.PromotionID == id);
        }

        public async Task CreatePromotion(Promotion entity)
        {
            await this.CreateAsync(entity);
        }

        public async Task UpdatePromotion(Promotion entity)
        {
            await this.UpdateAsync(entity);
        }

        public async Task DeactivePromotionAsync(Promotion entity)
        {
            await this.UpdateAsync(entity);
        }
        public Promotion GetByIdBrandId(int id, int brandId)
        {
            return this.FirstOrDefault(q => q.PromotionID == id && q.BrandId == brandId);
        }
        public Promotion GetByPromoCode(string code)
        {
            return this.FirstOrDefault(q => q.PromotionCode == code);
        }

        public IQueryable<Promotion> GetPromotionByBrandId(int? brandId)
        {
            return this.GetActive(q => q.BrandId == brandId);
        }

        public IQueryable<Promotion> GetPromotionByStoreId(int storeId)
        {
            var storeApi = new StoreApi();
            var brandId = storeApi.GetStoreById(storeId).BrandId;
            return this.GetPromotionByBrandId(brandId);
        }

        public IQueryable<Promotion> GetPromotionByIdTime(int brandId)
        {
            return this.GetActive(q => q.BrandId == brandId && DateTime.Now >= q.FromDate && DateTime.Now <= q.ToDate);
        }
    }
}

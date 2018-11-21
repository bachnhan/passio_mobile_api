using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class PromotionDetailApi
    {
        public IQueryable<PromotionDetail> GetDetailByCode(string code)
        {
            return this.BaseService.GetDetailByCode(code);
        }

        public PromotionDetail GetDetailByPromotionDetailCode(string code)
        {
            return this.BaseService.GetDetailByPromotionDetailCode(code);
        }

        public PromotionDetail GetDetailById(int id)
        {
            var promoDetail = this.BaseService.GetDetailById(id);
            if(promoDetail == null)
            {
                return null;
            }
            else
            {
                return promoDetail;
            }
        }

        public PromotionDetailViewModel GetDetailViewModelById(int id)
        {
            var promoDetail = this.BaseService.GetDetailById(id);
            if (promoDetail == null)
            {
                return null;
            }
            else
            {
                return new PromotionDetailViewModel(promoDetail);
            }
        }

        public async Task CreatePromotionDetail(PromotionDetail entity)
        {
            await this.BaseService.CreatePromotionDetail(entity);
        }
        public async Task DeletePromotionDetail(PromotionDetail entity)
        {
            await this.BaseService.DeletePromotionDetail(entity);
        }
        public async Task UpdatePromotionDetail(PromotionDetail entity)
        {
            //var promoDetail = this.BaseService.GetDetailById(entity.PromotionDetailID);
            await this.BaseService.UpdatePromotionDetail(entity);
        }
        public IQueryable<PromotionDetail> GetDetailListById(int id)
        {
            return this.BaseService.GetDetailListById(id);
        }

        public IEnumerable<PromotionDetail> GetPromotionDetailByAppendCode(string appendCode)
        {
            return this.BaseService.GetPromotionDetailByAppendCode(appendCode);
        }
    }
}

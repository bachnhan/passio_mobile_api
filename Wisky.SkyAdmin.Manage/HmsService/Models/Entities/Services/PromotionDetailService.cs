using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IPromotionDetailService
    {
        IQueryable<PromotionDetail> GetDetailByCode(string code);
        PromotionDetail GetDetailByPromotionDetailCode(string code);
        PromotionDetail GetDetailById(int id);
        IQueryable<PromotionDetail> GetDetailListById(int id);
        Task CreatePromotionDetail(PromotionDetail entity);
        Task DeletePromotionDetail(PromotionDetail entity);
        Task UpdatePromotionDetail(PromotionDetail entity);
        IEnumerable<PromotionDetail> GetPromotionDetailByAppendCode(string appendCode);
    }
    public partial class PromotionDetailService
    {
        public IQueryable<PromotionDetail> GetDetailByCode(string code)
        {
            return this.Get(q => q.PromotionCode == code);
        }
        public PromotionDetail GetDetailByPromotionDetailCode(string code)
        {
            return this.Get(q => q.PromotionDetailCode == code).FirstOrDefault();
        }
        public async Task CreatePromotionDetail(PromotionDetail entity)
        {
            await this.CreateAsync(entity);
        }
        public PromotionDetail GetDetailById(int id)
        {
            return this.FirstOrDefault(q => q.PromotionDetailID == id);
        }
        public async Task DeletePromotionDetail(PromotionDetail entity)
        {
            await this.DeleteAsync(entity);
        }
        public async Task UpdatePromotionDetail(PromotionDetail entity)
        {
            await this.UpdateAsync(entity);
        }
        public IQueryable<PromotionDetail> GetDetailListById(int id)
        {
            return this.Get(q => q.PromotionDetailID == id);
        }
        public IEnumerable<PromotionDetail> GetPromotionDetailByAppendCode(string appendCode)
        {
            var list = this.Get(q => q.RegExCode.Contains(appendCode)).ToList();

            return list.Select(q => new
            {
                PromotionDetail = q,
                AppendCode = q.RegExCode.Substring(3, q.RegExCode.IndexOf('_') - 3)
            }).Where(p => p.AppendCode == appendCode).Select(p => p.PromotionDetail);
        }
    }
}

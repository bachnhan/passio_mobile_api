using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;

namespace HmsService.Sdk
{
    public partial class VoucherApi
    {

        /// <summary>
        /// Trả về tập hợp auto generated code
        /// 
        /// </summary>
        /// <param name="length">Chiều dài chuỗi code</param>
        /// <param name="amount">Số lượng cần sinh</param>
        /// <returns></returns>
        public List<string> GetGeneratedCode(int length, int amount)
        {
            Random random = new Random();
            List<string> list = new List<string>();
            int posibility = (int)Math.Pow(36, length);
            int maxLength = posibility.ToString().Length;
            int trimStart = 0;
            int distance = posibility / amount;
            int trimEnd = trimStart + distance;
            //const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (int i = 0; i < amount; i++)
            {
                int a = random.Next(trimStart, trimEnd);
                string hex = a.ToString("X"); //convert to hexadecimal
                while (hex.Length < maxLength)
                {
                    hex += "0";
                }
                list.Add(hex);
                trimStart = trimEnd;
                trimEnd += distance;
            }
            return list;
        }
        public IQueryable<VoucherViewModel> GetVoucherByMembershipcardId(int membershipcardId)
        {
            var entity = BaseService.Get(q => q.MembershipCardId == membershipcardId && q.Active == true);
            if(entity.Count() > 0)
            {
                return entity.ProjectTo<VoucherViewModel>(AutoMapperConfig);
            }else
            {
                return null;
            }
        }
        public VoucherViewModel GetVoucherByCode(string code)
        {
            var entity = BaseService.Get(q => q.VoucherCode == code);
            if (entity.Count() > 0)
            {
                return new VoucherViewModel(entity.FirstOrDefault());
            }
            else
            {
                return null;
            }
        }
        public VoucherViewModel GetVoucherByCodeAndMembershipcardId(string code, int membershipcardId)
        {
            var entity = BaseService.Get(q => q.VoucherCode == code && q.MembershipCardId == membershipcardId);
            if (entity.Count() > 0)
            {
                return new VoucherViewModel(entity.FirstOrDefault());
            }
            else
            {
                return null;
            }
        }
        public VoucherViewModel GetVoucherIsNotUsedAndCode(string code)
        {
            var entity = BaseService.Get(q => (q.isUsed == null || q.isUsed.Value == false) && q.Active == true && q.VoucherCode == code);

            //return entity.ProjectTo<VoucherViewModel>(this.AutoMapperConfig);
            return new VoucherViewModel(entity.FirstOrDefault());
        }
       
        public Voucher GetEntityVoucherByCode(string code)
        {
            return BaseService.GetActive().FirstOrDefault(q => code.Equals(q.VoucherCode));
        }

        public string GetGeneratedCode(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public List<Voucher> GetByPromotionDetailId(int promotionDetailId)
        {
            return this.BaseService.Get(q => q.PromotionDetailID == promotionDetailId).ToList();
        }

        public List<Voucher> GetByPromotionId(int promotionId, int limit)
        {
            return this.BaseService.Get(q => q.PromotionID == promotionId).Take(limit).ToList();
        }


        public IQueryable<VoucherViewModel> getVoucherbyPromotionDetailId(int promotionDetailId)
        {
            return this.BaseService.Get(q => q.PromotionDetailID == promotionDetailId)
                .ProjectTo<VoucherViewModel>(this.AutoMapperConfig);

        }
        public IQueryable<VoucherViewModel> getVoucherUsedbyPromotionDetailId(int promotionDetailId)
        {
            return this.BaseService.Get(q => q.PromotionDetailID == promotionDetailId && q.Active == false)
                .ProjectTo<VoucherViewModel>(this.AutoMapperConfig);

        }

        public IQueryable<VoucherViewModel> getAllVoucherByPromotionIdByPromotionDetailID(int promotionId, int promotionDetailId)
        {
            return this.BaseService.GetActive(q => q.PromotionDetailID == promotionDetailId && q.PromotionID == promotionId).ProjectTo<VoucherViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<VoucherViewModel> getAllVoucherByPromotionId(int promotionId)
        {
            return this.BaseService.GetActive(q => q.PromotionID == promotionId).ProjectTo<VoucherViewModel>(this.AutoMapperConfig);
        }
        public bool CreateVoucher(Voucher entity)
        {
            try
            {
                this.BaseService.Create(entity);
                return true;
            }
            catch
            {
                return false;
            }
           
        }
        //public VoucherViewModel GetVoucherByCode(string voucherCode)
        //{
        //    return this.BaseService.GetActive(q => q.VoucherCode == voucherCode).ProjectTo<VoucherViewModel>(this.AutoMapperConfig).ToList().FirstOrDefault();
        //}

        public void Update(VoucherViewModel model)
        {
            var entity = this.BaseService.Get(model.VoucherID);
            model.CopyToEntity(entity);
            this.BaseService.Update(entity);
        }
    }
}

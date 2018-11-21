using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CouponCampaignApi
    {
        public IEnumerable<CouponCampaignViewModel> GetAllCouponCampaigns(int? brandId)
        {
            var CouponCampaigns = this.BaseService.GetAllCouponCampaigns(brandId)
                .ProjectTo<CouponCampaignViewModel>(this.AutoMapperConfig)
                .ToList();
            return CouponCampaigns;
        }
        public CouponCampaignViewModel GetCouponCampaignById(int id)
        {
            var CouponCampaign = this.BaseService.GetCouponCampaignById(id);
            if(CouponCampaign==null)
            {
                return null;
            }
            else
            {
                return new CouponCampaignViewModel(CouponCampaign);
            }
        }
        
        //Tạo thêm do hàm CopyToEntity bị lỗi parse mất providerId
        public async System.Threading.Tasks.Task EditCouponCampaign(CouponCampaignEditViewModel model)
        {
            var CouponCampaign = await this.BaseService.GetAsync(model.Id);
            CouponCampaign.StartDate = model.StartDate;
            CouponCampaign.EndDate = model.EndDate;
            CouponCampaign.Name = model.Name;
            CouponCampaign.Price = model.Price;
            CouponCampaign.Value = model.Value;
            CouponCampaign.ProviderId = model.ProviderId;
            CouponCampaign.Description = model.Description;
            //CouponCampaign CouponCampaign = new CouponCampaign()
            //{
            //    Id = model.Id,
            //    Description = model.Description,
            //    StartDate = model.StartDate,
            //    EndDate = model.EndDate,
            //    IsActive = model.IsActive,
            //    Name = model.Name,
            //    Price = model.Price,
            //    ProviderId = model.ProviderId,
            //    Status = model.Status,
            //    Value = model.Value,
            //};
            await this.BaseService.UpdateAsync(CouponCampaign);
        }
    }
}

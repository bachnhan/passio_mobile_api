using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICouponCampaignService
    {
        IQueryable<CouponCampaign> GetAllCouponCampaigns(int? brandId);
        CouponCampaign GetCouponCampaignById(int id);
    }
    public partial class CouponCampaignService
    {
        public IQueryable<CouponCampaign> GetAllCouponCampaigns(int? brandId)
        {
            return this.Get().Where(q => q.IsActive && q.BrandId == brandId);
        }
        public CouponCampaign GetCouponCampaignById(int id)
        {
            return this.FirstOrDefault(q => q.Id == id);
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HmsService.Models.Entities.Services
{
    using System;
    using System.Collections.Generic;
    
    
    public partial interface ICouponCampaignService : SkyWeb.DatVM.Data.IBaseService<CouponCampaign>
    {
    }
    
    public partial class CouponCampaignService : SkyWeb.DatVM.Data.BaseService<CouponCampaign>, ICouponCampaignService
    {
        public CouponCampaignService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.ICouponCampaignRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}
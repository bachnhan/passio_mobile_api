//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HmsService.ViewModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class CouponCampaignViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.CouponCampaign>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual string Name { get; set; }
    			public virtual string Description { get; set; }
    			public virtual Nullable<System.DateTime> StartDate { get; set; }
    			public virtual Nullable<System.DateTime> EndDate { get; set; }
    			public virtual int Status { get; set; }
    			public virtual decimal Price { get; set; }
    			public virtual decimal Value { get; set; }
    			public virtual Nullable<int> ProviderId { get; set; }
    			public virtual bool IsActive { get; set; }
    			public virtual Nullable<int> BrandId { get; set; }
    	
    	public CouponCampaignViewModel() : base() { }
    	public CouponCampaignViewModel(HmsService.Models.Entities.CouponCampaign entity) : base(entity) { }
    
    }
}

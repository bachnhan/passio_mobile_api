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
    
    public partial class OrderPromotionMappingViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.OrderPromotionMapping>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int OrderId { get; set; }
    			public virtual int PromotionId { get; set; }
    			public virtual Nullable<int> PromotionDetailId { get; set; }
    			public virtual decimal DiscountAmount { get; set; }
    			public virtual Nullable<double> DiscountRate { get; set; }
    			public virtual bool Active { get; set; }
    			public virtual int MappingIndex { get; set; }
    			public virtual Nullable<int> TmpMappingId { get; set; }
    	
    	public OrderPromotionMappingViewModel() : base() { }
    	public OrderPromotionMappingViewModel(HmsService.Models.Entities.OrderPromotionMapping entity) : base(entity) { }
    
    }
}

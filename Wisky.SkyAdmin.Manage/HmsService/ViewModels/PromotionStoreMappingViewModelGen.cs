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
    
    public partial class PromotionStoreMappingViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.PromotionStoreMapping>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int PromotionId { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public PromotionStoreMappingViewModel() : base() { }
    	public PromotionStoreMappingViewModel(HmsService.Models.Entities.PromotionStoreMapping entity) : base(entity) { }
    
    }
}
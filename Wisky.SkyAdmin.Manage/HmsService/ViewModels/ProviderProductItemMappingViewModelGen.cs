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
    
    public partial class ProviderProductItemMappingViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.ProviderProductItemMapping>
    {
    	
    			public virtual int ProviderProductItemId { get; set; }
    			public virtual int ProviderID { get; set; }
    			public virtual int ProductItemID { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public ProviderProductItemMappingViewModel() : base() { }
    	public ProviderProductItemMappingViewModel(HmsService.Models.Entities.ProviderProductItemMapping entity) : base(entity) { }
    
    }
}
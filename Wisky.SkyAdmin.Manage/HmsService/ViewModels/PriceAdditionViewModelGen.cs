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
    
    public partial class PriceAdditionViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.PriceAddition>
    {
    	
    			public virtual int AdditionPriceID { get; set; }
    			public virtual string EarlyHourRange { get; set; }
    			public virtual string EarlyPriceRange { get; set; }
    			public virtual string LateHourRange { get; set; }
    			public virtual string LatePriceRange { get; set; }
    			public virtual string Name { get; set; }
    	
    	public PriceAdditionViewModel() : base() { }
    	public PriceAdditionViewModel(HmsService.Models.Entities.PriceAddition entity) : base(entity) { }
    
    }
}
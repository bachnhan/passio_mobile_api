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
    
    public partial class WardViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.Ward>
    {
    	
    			public virtual int WardCode { get; set; }
    			public virtual string WardName { get; set; }
    			public virtual string WardType { get; set; }
    			public virtual int DistrictCode { get; set; }
    	
    	public WardViewModel() : base() { }
    	public WardViewModel(HmsService.Models.Entities.Ward entity) : base(entity) { }
    
    }
}

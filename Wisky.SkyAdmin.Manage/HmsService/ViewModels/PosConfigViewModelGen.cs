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
    
    public partial class PosConfigViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.PosConfig>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual string Key { get; set; }
    			public virtual string Value { get; set; }
    			public virtual int PosFileId { get; set; }
    	
    	public PosConfigViewModel() : base() { }
    	public PosConfigViewModel(HmsService.Models.Entities.PosConfig entity) : base(entity) { }
    
    }
}

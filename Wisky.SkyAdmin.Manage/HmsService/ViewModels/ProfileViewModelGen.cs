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
    
    public partial class ProfileViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.Profile>
    {
    	
    			public virtual System.Guid UserId { get; set; }
    			public virtual string PropertyNames { get; set; }
    			public virtual string PropertyValueStrings { get; set; }
    			public virtual byte[] PropertyValueBinary { get; set; }
    			public virtual System.DateTime LastUpdatedDate { get; set; }
    	
    	public ProfileViewModel() : base() { }
    	public ProfileViewModel(HmsService.Models.Entities.Profile entity) : base(entity) { }
    
    }
}

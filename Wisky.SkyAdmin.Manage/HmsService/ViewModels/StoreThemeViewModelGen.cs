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
    
    public partial class StoreThemeViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.StoreTheme>
    {
    	
    			public virtual int StoreThemeId { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual string ThemeName { get; set; }
    			public virtual int ThemeId { get; set; }
    			public virtual string StoreThemeName { get; set; }
    			public virtual string ThemeLogoUrl { get; set; }
    			public virtual string CustomThemeStyle { get; set; }
    			public virtual System.DateTime CreatedDate { get; set; }
    			public virtual bool IsUsing { get; set; }
    			public virtual string CreatedBy { get; set; }
    			public virtual System.DateTime LastModifiedDate { get; set; }
    			public virtual string LastModifiedBy { get; set; }
    			public virtual bool Active { get; set; }
    			public virtual string Description { get; set; }
    	
    	public StoreThemeViewModel() : base() { }
    	public StoreThemeViewModel(HmsService.Models.Entities.StoreTheme entity) : base(entity) { }
    
    }
}

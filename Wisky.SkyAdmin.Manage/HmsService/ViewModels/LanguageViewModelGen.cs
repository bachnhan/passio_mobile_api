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
    
    public partial class LanguageViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.Language>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual string Name { get; set; }
    			public virtual string EnglishName { get; set; }
    			public virtual bool IsFallbackLanguage { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public LanguageViewModel() : base() { }
    	public LanguageViewModel(HmsService.Models.Entities.Language entity) : base(entity) { }
    
    }
}

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
    
    public partial class WebElementTypeViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.WebElementType>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual Nullable<int> WebElementId { get; set; }
    			public virtual string Name { get; set; }
    			public virtual string Description { get; set; }
    			public virtual Nullable<int> Template { get; set; }
    			public virtual Nullable<int> Position { get; set; }
    			public virtual Nullable<bool> ShowOnContentPage { get; set; }
    			public virtual string ImageUrl { get; set; }
    			public virtual string Link { get; set; }
    			public virtual Nullable<int> BrandId { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public WebElementTypeViewModel() : base() { }
    	public WebElementTypeViewModel(HmsService.Models.Entities.WebElementType entity) : base(entity) { }
    
    }
}

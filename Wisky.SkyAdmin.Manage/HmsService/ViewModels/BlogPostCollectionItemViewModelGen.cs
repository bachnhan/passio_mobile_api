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
    
    public partial class BlogPostCollectionItemViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.BlogPostCollectionItem>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int BlogPostCollectionId { get; set; }
    			public virtual int BlogPostId { get; set; }
    			public virtual int Position { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public BlogPostCollectionItemViewModel() : base() { }
    	public BlogPostCollectionItemViewModel(HmsService.Models.Entities.BlogPostCollectionItem entity) : base(entity) { }
    
    }
}

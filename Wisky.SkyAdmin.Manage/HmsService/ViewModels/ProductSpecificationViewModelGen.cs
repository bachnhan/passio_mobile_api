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
    
    public partial class ProductSpecificationViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.ProductSpecification>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int ProductId { get; set; }
    			public virtual string Name { get; set; }
    			public virtual string Value { get; set; }
    			public virtual string ImageUrl { get; set; }
    			public virtual int Position { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public ProductSpecificationViewModel() : base() { }
    	public ProductSpecificationViewModel(HmsService.Models.Entities.ProductSpecification entity) : base(entity) { }
    
    }
}

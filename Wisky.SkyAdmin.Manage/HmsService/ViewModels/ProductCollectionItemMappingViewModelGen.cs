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
    
    public partial class ProductCollectionItemMappingViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.ProductCollectionItemMapping>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int ProductCollectionId { get; set; }
    			public virtual int ProductId { get; set; }
    			public virtual int Position { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public ProductCollectionItemMappingViewModel() : base() { }
    	public ProductCollectionItemMappingViewModel(HmsService.Models.Entities.ProductCollectionItemMapping entity) : base(entity) { }
    
    }
}

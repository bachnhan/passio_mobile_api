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
    
    public partial class ProductDetailMappingViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.ProductDetailMapping>
    {
    	
    			public virtual int ProductDetailID { get; set; }
    			public virtual Nullable<int> ProductID { get; set; }
    			public virtual Nullable<int> StoreID { get; set; }
    			public virtual Nullable<double> Price { get; set; }
    			public virtual Nullable<double> DiscountPrice { get; set; }
    			public virtual Nullable<double> DiscountPercent { get; set; }
    			public virtual Nullable<bool> Active { get; set; }
    	
    	public ProductDetailMappingViewModel() : base() { }
    	public ProductDetailMappingViewModel(HmsService.Models.Entities.ProductDetailMapping entity) : base(entity) { }
    
    }
}

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
    
    public partial class ProductItemViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.ProductItem>
    {
    	
    			public virtual int ItemID { get; set; }
    			public virtual string ItemName { get; set; }
    			public virtual string Unit { get; set; }
    			public virtual Nullable<bool> IsAvailable { get; set; }
    			public virtual string ImageUrl { get; set; }
    			public virtual Nullable<int> CatID { get; set; }
    			public virtual Nullable<double> Price { get; set; }
    			public virtual string Unit2 { get; set; }
    			public virtual Nullable<double> UnitRate { get; set; }
    			public virtual Nullable<int> IndexPriority { get; set; }
    			public virtual Nullable<System.DateTime> CreateDate { get; set; }
    			public virtual Nullable<int> ItemType { get; set; }
    			public virtual string ItemCode { get; set; }
    	
    	public ProductItemViewModel() : base() { }
    	public ProductItemViewModel(HmsService.Models.Entities.ProductItem entity) : base(entity) { }
    
    }
}

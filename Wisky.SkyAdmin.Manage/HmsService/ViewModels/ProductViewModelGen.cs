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
    
    public partial class ProductViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.Product>
    {
    	
    			public virtual int ProductID { get; set; }
    			public virtual string ProductName { get; set; }
    			public virtual string ProductNameEng { get; set; }
    			public virtual double Price { get; set; }
    			public virtual string PicURL { get; set; }
    			public virtual int CatID { get; set; }
    			public virtual bool IsAvailable { get; set; }
    			public virtual string Code { get; set; }
    			public virtual double DiscountPercent { get; set; }
    			public virtual double DiscountPrice { get; set; }
    			public virtual int ProductType { get; set; }
    			public virtual int DisplayOrder { get; set; }
    			public virtual bool HasExtra { get; set; }
    			public virtual bool IsFixedPrice { get; set; }
    			public virtual Nullable<int> PosX { get; set; }
    			public virtual Nullable<int> PosY { get; set; }
    			public virtual Nullable<int> ColorGroup { get; set; }
    			public virtual Nullable<int> Group { get; set; }
    			public virtual Nullable<int> GroupId { get; set; }
    			public virtual Nullable<bool> IsMenuDisplay { get; set; }
    			public virtual Nullable<int> GeneralProductId { get; set; }
    			public virtual string Att1 { get; set; }
    			public virtual string Att2 { get; set; }
    			public virtual string Att3 { get; set; }
    			public virtual string Att4 { get; set; }
    			public virtual string Att5 { get; set; }
    			public virtual string Att6 { get; set; }
    			public virtual string Att7 { get; set; }
    			public virtual string Att8 { get; set; }
    			public virtual string Att9 { get; set; }
    			public virtual string Att10 { get; set; }
    			public virtual Nullable<int> MaxExtra { get; set; }
    			public virtual string Description { get; set; }
    			public virtual string DescriptionEng { get; set; }
    			public virtual string Introduction { get; set; }
    			public virtual string IntroductionEng { get; set; }
    			public virtual Nullable<int> PrintGroup { get; set; }
    			public virtual string SeoName { get; set; }
    			public virtual Nullable<int> IsHomePage { get; set; }
    			public virtual string WebContent { get; set; }
    			public virtual string SeoKeyWords { get; set; }
    			public virtual string SeoDescription { get; set; }
    			public virtual bool Active { get; set; }
    			public virtual int IsDefaultChildProduct { get; set; }
    			public virtual Nullable<int> Position { get; set; }
    			public virtual Nullable<int> SaleType { get; set; }
    			public virtual bool IsMostOrdered { get; set; }
    			public virtual string Note { get; set; }
    			public virtual Nullable<System.DateTime> CreateTime { get; set; }
    			public virtual Nullable<int> RatingTotal { get; set; }
    			public virtual Nullable<int> NumOfUserVoted { get; set; }
    			public virtual Nullable<int> Status { get; set; }
    	
    	public ProductViewModel() : base() { }
    	public ProductViewModel(HmsService.Models.Entities.Product entity) : base(entity) { }
    
    }
}

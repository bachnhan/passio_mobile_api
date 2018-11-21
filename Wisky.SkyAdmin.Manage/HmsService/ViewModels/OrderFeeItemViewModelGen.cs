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
    
    public partial class OrderFeeItemViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.OrderFeeItem>
    {
    	
    			public virtual int OrderId { get; set; }
    			public virtual int RentId { get; set; }
    			public virtual Nullable<double> TotalAmount { get; set; }
    			public virtual Nullable<double> FinalAmount { get; set; }
    			public virtual System.DateTime OrderDate { get; set; }
    			public virtual string DetailDescription { get; set; }
    			public virtual Nullable<int> Status { get; set; }
    			public virtual Nullable<int> CustomerID { get; set; }
    			public virtual Nullable<int> StoreId { get; set; }
    			public virtual Nullable<System.DateTime> FromDate { get; set; }
    			public virtual Nullable<System.DateTime> ToDate { get; set; }
    			public virtual bool IsAddition { get; set; }
    			public virtual Nullable<int> ProductType { get; set; }
    			public virtual Nullable<int> RoomId { get; set; }
    			public virtual string RoomName { get; set; }
    			public virtual Nullable<int> RentMode { get; set; }
    			public virtual Nullable<int> PriceGroupId { get; set; }
    	
    	public OrderFeeItemViewModel() : base() { }
    	public OrderFeeItemViewModel(HmsService.Models.Entities.OrderFeeItem entity) : base(entity) { }
    
    }
}

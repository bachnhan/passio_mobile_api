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
    
    public partial class OrderGroupViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.OrderGroup>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual string Name { get; set; }
    			public virtual string Code { get; set; }
    			public virtual Nullable<int> CustomerID { get; set; }
    			public virtual int SourceID { get; set; }
    			public virtual System.DateTime BookingDate { get; set; }
    			public virtual Nullable<System.DateTime> GetRoomDate { get; set; }
    			public virtual string Note { get; set; }
    			public virtual Nullable<int> StoreID { get; set; }
    	
    	public OrderGroupViewModel() : base() { }
    	public OrderGroupViewModel(HmsService.Models.Entities.OrderGroup entity) : base(entity) { }
    
    }
}

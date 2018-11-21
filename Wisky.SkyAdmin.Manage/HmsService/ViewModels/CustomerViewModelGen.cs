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
    
    public partial class CustomerViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.Customer>
    {
    	
    			public virtual int CustomerID { get; set; }
    			public virtual string Name { get; set; }
    			public virtual string Address { get; set; }
    			public virtual string Phone { get; set; }
    			public virtual string Fax { get; set; }
    			public virtual string ContactPerson { get; set; }
    			public virtual string ContactPersonNumber { get; set; }
    			public virtual string Website { get; set; }
    			public virtual string Email { get; set; }
    			public virtual int Type { get; set; }
    			public virtual Nullable<int> AccountID { get; set; }
    			public virtual string IDCard { get; set; }
    			public virtual Nullable<bool> Gender { get; set; }
    			public virtual Nullable<System.DateTime> BirthDay { get; set; }
    			public virtual Nullable<int> StoreRegisterId { get; set; }
    			public virtual string District { get; set; }
    			public virtual string City { get; set; }
    			public virtual string CustomerCode { get; set; }
    			public virtual Nullable<int> CustomerTypeId { get; set; }
    			public virtual Nullable<int> BrandId { get; set; }
    			public virtual Nullable<int> deliveryInfoDefault { get; set; }
    			public virtual string picURL { get; set; }
    			public virtual string AccountPhone { get; set; }
    			public virtual string FacebookId { get; set; }
    	
    	public CustomerViewModel() : base() { }
    	public CustomerViewModel(HmsService.Models.Entities.Customer entity) : base(entity) { }
    
    }
}

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
    
    public partial class GuestViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.Guest>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual string Name { get; set; }
    			public virtual string PersonId { get; set; }
    			public virtual Nullable<int> BirthYear { get; set; }
    			public virtual string Phone { get; set; }
    			public virtual string Address { get; set; }
    			public virtual Nullable<bool> Sex { get; set; }
    			public virtual Nullable<int> RentId { get; set; }
    			public virtual Nullable<int> RentGroup { get; set; }
    			public virtual string Note { get; set; }
    	
    	public GuestViewModel() : base() { }
    	public GuestViewModel(HmsService.Models.Entities.Guest entity) : base(entity) { }
    
    }
}

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
    
    public partial class DeliveryInfoViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.DeliveryInfo>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual Nullable<int> CustomerId { get; set; }
    			public virtual string CustomerName { get; set; }
    			public virtual string Address { get; set; }
    			public virtual string Phone { get; set; }
    			public virtual Nullable<bool> Active { get; set; }
    			public virtual Nullable<int> Type { get; set; }
    			public virtual Nullable<bool> isDefaultDeliveryInfo { get; set; }
                public virtual string Lat { get; set; }
                public virtual string Lon { get; set; }

        public DeliveryInfoViewModel() : base() { }
    	public DeliveryInfoViewModel(HmsService.Models.Entities.DeliveryInfo entity) : base(entity) { }
    
    }
}

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
    
    public partial class WebCustomerFeedbackViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.WebCustomerFeedback>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual string Title { get; set; }
    			public virtual string Fullname { get; set; }
    			public virtual string Email { get; set; }
    			public virtual string Phone { get; set; }
    			public virtual string Company { get; set; }
    			public virtual string Content { get; set; }
    			public virtual bool Active { get; set; }
    			public virtual string Collection { get; set; }
    			public virtual string CustomFields { get; set; }
    	
    	public WebCustomerFeedbackViewModel() : base() { }
    	public WebCustomerFeedbackViewModel(HmsService.Models.Entities.WebCustomerFeedback entity) : base(entity) { }
    
    }
}

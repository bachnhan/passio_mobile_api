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
    
    public partial class VATOrderViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.VATOrder>
    {
    	
    			public virtual int InvoiceID { get; set; }
    			public virtual int BrandID { get; set; }
    			public virtual string VATOrderDetail { get; set; }
    			public virtual int Type { get; set; }
    			public virtual string InvoiceNo { get; set; }
    			public virtual string CheckInPerson { get; set; }
    			public virtual System.DateTime CheckInDate { get; set; }
    			public virtual string Notes { get; set; }
    			public virtual double Total { get; set; }
    			public virtual double VATAmount { get; set; }
    			public virtual int ProviderID { get; set; }
    	
    	public VATOrderViewModel() : base() { }
    	public VATOrderViewModel(HmsService.Models.Entities.VATOrder entity) : base(entity) { }
    
    }
}
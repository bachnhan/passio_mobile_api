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
    
    public partial class InventoryDateReportViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.InventoryDateReport>
    {
    	
    			public virtual int ReportID { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual System.DateTime CreateDate { get; set; }
    			public virtual string Creator { get; set; }
    			public virtual int Status { get; set; }
    	
    	public InventoryDateReportViewModel() : base() { }
    	public InventoryDateReportViewModel(HmsService.Models.Entities.InventoryDateReport entity) : base(entity) { }
    
    }
}

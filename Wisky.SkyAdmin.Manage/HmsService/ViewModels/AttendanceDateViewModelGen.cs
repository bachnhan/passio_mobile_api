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
    
    public partial class AttendanceDateViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.AttendanceDate>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual System.DateTime DateReport { get; set; }
    			public virtual int TotalEmployee { get; set; }
    			public virtual int TotalOnTimeEmployee { get; set; }
    			public virtual int TotalSession { get; set; }
    			public virtual int TotalOnTimeSession { get; set; }
    			public virtual int TotalMissSession { get; set; }
    			public virtual int TotalMissEmployee { get; set; }
    			public virtual int TotalComeLate { get; set; }
    			public virtual int TotalComeOnTime { get; set; }
    			public virtual int TotalReturnEarly { get; set; }
    			public virtual int TotalReturnOntime { get; set; }
    			public virtual System.TimeSpan TotalWorkingTime { get; set; }
    			public virtual int Status { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public AttendanceDateViewModel() : base() { }
    	public AttendanceDateViewModel(HmsService.Models.Entities.AttendanceDate entity) : base(entity) { }
    
    }
}

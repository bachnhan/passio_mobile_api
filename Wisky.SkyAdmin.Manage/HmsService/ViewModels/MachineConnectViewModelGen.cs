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
    
    public partial class MachineConnectViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.MachineConnect>
    {
    	
    			public virtual int ID { get; set; }
    			public virtual int MachineID { get; set; }
    			public virtual Nullable<System.DateTime> ConnectTime { get; set; }
    			public virtual Nullable<bool> ConnectResult { get; set; }
    			public virtual int StoreID { get; set; }
    			public virtual int BrandID { get; set; }
    			public virtual bool Active { get; set; }
    			public virtual string MachineName { get; set; }
    			public virtual string MachineCode { get; set; }
    	
    	public MachineConnectViewModel() : base() { }
    	public MachineConnectViewModel(HmsService.Models.Entities.MachineConnect entity) : base(entity) { }
    
    }
}

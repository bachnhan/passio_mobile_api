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
    
    public partial class RoomFloorViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.RoomFloor>
    {
    	
    			public virtual int FloorID { get; set; }
    			public virtual string FloorName { get; set; }
    			public virtual Nullable<int> Position { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual Nullable<bool> IsDelete { get; set; }
    	
    	public RoomFloorViewModel() : base() { }
    	public RoomFloorViewModel(HmsService.Models.Entities.RoomFloor entity) : base(entity) { }
    
    }
}
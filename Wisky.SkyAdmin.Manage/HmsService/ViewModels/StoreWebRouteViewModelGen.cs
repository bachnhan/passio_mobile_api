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
    
    public partial class StoreWebRouteViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.StoreWebRoute>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual Nullable<int> StoreWebRouteCopyId { get; set; }
    			public virtual string Path { get; set; }
    			public virtual string ViewName { get; set; }
    			public virtual string LayoutName { get; set; }
    			public virtual int Position { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public StoreWebRouteViewModel() : base() { }
    	public StoreWebRouteViewModel(HmsService.Models.Entities.StoreWebRoute entity) : base(entity) { }
    
    }
}

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
    
    public partial class ImageCollectionViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.ImageCollection>
    {
    	
    			public virtual int Id { get; set; }
    			public virtual int StoreId { get; set; }
    			public virtual string Name { get; set; }
    			public virtual bool Active { get; set; }
    	
    	public ImageCollectionViewModel() : base() { }
    	public ImageCollectionViewModel(HmsService.Models.Entities.ImageCollection entity) : base(entity) { }
    
    }
}

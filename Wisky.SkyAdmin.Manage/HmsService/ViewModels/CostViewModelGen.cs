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
    
    public partial class CostViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.Cost>
    {
    	
    			public virtual int CostID { get; set; }
    			public virtual int CatID { get; set; }
    			public virtual string CostDescription { get; set; }
    			public virtual System.DateTime CostDate { get; set; }
    			public virtual double Amount { get; set; }
    			public virtual int CostStatus { get; set; }
    			public virtual string PaidPerson { get; set; }
    			public virtual string LoggedPerson { get; set; }
    			public virtual string ApprovedPerson { get; set; }
    			public virtual Nullable<int> StoreId { get; set; }
    			public virtual Nullable<int> CostCategoryType { get; set; }
    			public virtual string CostCode { get; set; }
    			public virtual Nullable<int> CostType { get; set; }
    	
    	public CostViewModel() : base() { }
    	public CostViewModel(HmsService.Models.Entities.Cost entity) : base(entity) { }
    
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HmsService.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmployeeInStore
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int StoreId { get; set; }
        public System.DateTime AssignedDate { get; set; }
        public Nullable<int> Status { get; set; }
        public bool Active { get; set; }
    
        public virtual Employee Employee { get; set; }
        public virtual Store Store { get; set; }
    }
}
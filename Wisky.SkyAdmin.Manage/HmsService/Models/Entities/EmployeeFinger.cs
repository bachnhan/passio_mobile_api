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
    
    public partial class EmployeeFinger
    {
        public int Id { get; set; }
        public Nullable<int> EmpId { get; set; }
        public string EmpEnrollNumber { get; set; }
        public int FingerIndex { get; set; }
        public string FingerData { get; set; }
        public Nullable<int> Type { get; set; }
        public string NameEmployeeInMachine { get; set; }
        public bool Active { get; set; }
    
        public virtual Employee Employee { get; set; }
    }
}

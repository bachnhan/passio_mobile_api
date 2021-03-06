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
    
    public partial class CustomerStoreReportMapping
    {
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public int StoreID { get; set; }
        public Nullable<int> TotalOrder { get; set; }
        public Nullable<double> TotalAmount { get; set; }
        public Nullable<double> AverageAmount { get; set; }
        public Nullable<int> VisitAmount { get; set; }
        public Nullable<int> DateAmount { get; set; }
        public Nullable<double> Frequency { get; set; }
        public System.DateTime LastVisitDate { get; set; }
        public System.DateTime UpdateDate { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Store Store { get; set; }
    }
}

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
    
    public partial class InventoryDateReport
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InventoryDateReport()
        {
            this.InventoryDateReportItems = new HashSet<InventoryDateReportItem>();
        }
    
        public int ReportID { get; set; }
        public int StoreId { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Creator { get; set; }
        public int Status { get; set; }
    
        public virtual Store Store { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InventoryDateReportItem> InventoryDateReportItems { get; set; }
    }
}

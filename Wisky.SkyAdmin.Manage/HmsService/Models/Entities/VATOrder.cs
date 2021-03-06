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
    
    public partial class VATOrder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VATOrder()
        {
            this.VATOrderMappings = new HashSet<VATOrderMapping>();
        }
    
        public int InvoiceID { get; set; }
        public int BrandID { get; set; }
        public string VATOrderDetail { get; set; }
        public int Type { get; set; }
        public string InvoiceNo { get; set; }
        public string CheckInPerson { get; set; }
        public System.DateTime CheckInDate { get; set; }
        public string Notes { get; set; }
        public double Total { get; set; }
        public double VATAmount { get; set; }
        public int ProviderID { get; set; }
    
        public virtual Brand Brand { get; set; }
        public virtual Provider Provider { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VATOrderMapping> VATOrderMappings { get; set; }
    }
}

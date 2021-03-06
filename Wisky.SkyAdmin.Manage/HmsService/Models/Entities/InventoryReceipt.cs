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
    
    public partial class InventoryReceipt
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InventoryReceipt()
        {
            this.CostInventoryMappings = new HashSet<CostInventoryMapping>();
            this.InventoryReceiptItems = new HashSet<InventoryReceiptItem>();
        }
    
        public int ReceiptID { get; set; }
        public System.DateTime ChangeDate { get; set; }
        public int ReceiptType { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> InStoreId { get; set; }
        public Nullable<int> OutStoreId { get; set; }
        public Nullable<int> ProviderId { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string InvoiceNumber { get; set; }
        public Nullable<double> Amount { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CostInventoryMapping> CostInventoryMappings { get; set; }
        public virtual Provider Provider { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InventoryReceiptItem> InventoryReceiptItems { get; set; }
        public virtual Store Store { get; set; }
        public virtual Store Store1 { get; set; }
        public virtual Store Store2 { get; set; }
    }
}

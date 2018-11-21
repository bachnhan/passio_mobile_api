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
    
    public partial class InventoryReceiptItem
    {
        public int ReceiptID { get; set; }
        public int ItemID { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public Nullable<System.DateTime> DateExpired { get; set; }
        public System.DateTime ExportedDate { get; set; }
        public bool IsUnit1 { get; set; }
    
        public virtual InventoryReceipt InventoryReceipt { get; set; }
        public virtual ProductItem ProductItem { get; set; }
    }
}
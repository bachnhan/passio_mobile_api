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
    
    public partial class InventoryCheckingItem
    {
        public int InventoryCheckingID { get; set; }
        public int ItemID { get; set; }
        public int CheckingId { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public Nullable<double> Price { get; set; }
    
        public virtual InventoryChecking InventoryChecking { get; set; }
    }
}

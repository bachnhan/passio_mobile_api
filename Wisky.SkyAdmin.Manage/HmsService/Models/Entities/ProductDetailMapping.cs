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
    
    public partial class ProductDetailMapping
    {
        public int ProductDetailID { get; set; }
        public Nullable<int> ProductID { get; set; }
        public Nullable<int> StoreID { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<double> DiscountPrice { get; set; }
        public Nullable<double> DiscountPercent { get; set; }
        public Nullable<bool> Active { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual Store Store { get; set; }
    }
}

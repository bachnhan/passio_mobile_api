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
    
    public partial class Voucher
    {
        public int VoucherID { get; set; }
        public string VoucherCode { get; set; }
        public Nullable<int> PromotionDetailID { get; set; }
        public int PromotionID { get; set; }
        public int Quantity { get; set; }
        public int UsedQuantity { get; set; }
        public bool Active { get; set; }
        public Nullable<bool> IsGetted { get; set; }
        public Nullable<bool> isUsed { get; set; }
        public Nullable<int> MembershipCardId { get; set; }
    
        public virtual MembershipCard MembershipCard { get; set; }
        public virtual Promotion Promotion { get; set; }
        public virtual PromotionDetail PromotionDetail { get; set; }
    }
}

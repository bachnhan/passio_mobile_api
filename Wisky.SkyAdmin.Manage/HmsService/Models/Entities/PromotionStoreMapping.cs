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
    
    public partial class PromotionStoreMapping
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public int StoreId { get; set; }
        public bool Active { get; set; }
    
        public virtual Store Store { get; set; }
        public virtual Promotion Promotion { get; set; }
    }
}

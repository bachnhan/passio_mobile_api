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
    
    public partial class MembershipCard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MembershipCard()
        {
            this.Accounts = new HashSet<Account>();
            this.Vouchers = new HashSet<Voucher>();
        }
    
        public int Id { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public string MembershipCardCode { get; set; }
        public string CSV { get; set; }
        public bool Active { get; set; }
        public Nullable<int> Status { get; set; }
        public System.DateTime CreatedTime { get; set; }
        public int BrandId { get; set; }
        public Nullable<int> C_Level { get; set; }
        public Nullable<int> MembershipTypeId { get; set; }
        public Nullable<bool> IsSample { get; set; }
        public Nullable<int> StoreId { get; set; }
        public string ProductCode { get; set; }
        public Nullable<double> InitialValue { get; set; }
        public string CreateBy { get; set; }
        public string PhysicalCardCode { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual MembershipCardType MembershipCardType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Voucher> Vouchers { get; set; }
    }
}

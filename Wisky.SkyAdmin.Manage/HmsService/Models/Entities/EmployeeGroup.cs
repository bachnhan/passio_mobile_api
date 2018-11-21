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
    
    public partial class EmployeeGroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EmployeeGroup()
        {
            this.Employees = new HashSet<Employee>();
            this.TimeFrames = new HashSet<TimeFrame>();
        }
    
        public int Id { get; set; }
        public string CodeGroup { get; set; }
        public string NameGroup { get; set; }
        public int BrandId { get; set; }
        public bool Active { get; set; }
        public Nullable<System.TimeSpan> ExpandTime { get; set; }
        public Nullable<int> GroupPolicy { get; set; }
        public Nullable<int> GroupSecurity { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employees { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TimeFrame> TimeFrames { get; set; }
    }
}

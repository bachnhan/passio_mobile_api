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
    
    public partial class StoreWebRoute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StoreWebRoute()
        {
            this.StoreWebRoute1 = new HashSet<StoreWebRoute>();
            this.StoreWebRouteModels = new HashSet<StoreWebRouteModel>();
        }
    
        public int Id { get; set; }
        public int StoreId { get; set; }
        public Nullable<int> StoreWebRouteCopyId { get; set; }
        public string Path { get; set; }
        public string ViewName { get; set; }
        public string LayoutName { get; set; }
        public int Position { get; set; }
        public bool Active { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StoreWebRoute> StoreWebRoute1 { get; set; }
        public virtual StoreWebRoute StoreWebRoute2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StoreWebRouteModel> StoreWebRouteModels { get; set; }
        public virtual Store Store { get; set; }
    }
}

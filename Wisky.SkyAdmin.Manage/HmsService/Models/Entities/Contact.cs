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
    
    public partial class Contact
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Nullable<bool> Gender { get; set; }
        public string Email { get; set; }
        public string ProfileUrl { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public string ImageUrl { get; set; }
        public string Phone { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public bool Active { get; set; }
        public string Username { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string Address { get; set; }
        public string Organization { get; set; }
        public string Job { get; set; }
        public string Position { get; set; }
        public Nullable<int> FirstLocationId { get; set; }
        public string Fax { get; set; }
        public Nullable<int> TotalVisted { get; set; }
        public Nullable<int> TypeContactFrom { get; set; }
        public Nullable<System.DateTime> LastVisted { get; set; }
    
        public virtual Brand Brand { get; set; }
    }
}

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
    
    public partial class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Title_En { get; set; }
        public string Description { get; set; }
        public string Description_En { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string PicUrl { get; set; }
        public string Opening { get; set; }
        public string Content { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public string Creator { get; set; }
    }
}

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
    
    public partial class Favorited
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public int ProductID { get; set; }
        public bool Active { get; set; }
        public Nullable<bool> FavoriteStt { get; set; }
    
        public virtual Product Product { get; set; }
    }
}

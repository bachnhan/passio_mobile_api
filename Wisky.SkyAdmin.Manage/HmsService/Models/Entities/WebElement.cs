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
    
    public partial class WebElement
    {
        public int Id { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string ImageUrl { get; set; }
        public string Link { get; set; }
        public bool Active { get; set; }
        public Nullable<int> BrandId { get; set; }
    
        public virtual WebElementType WebElementType { get; set; }
    }
}

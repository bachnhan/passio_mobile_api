//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HmsService.ViewModels
{
    using System;
    using System.Collections.Generic;

    public partial class DeliveryInformationViewModel : SkyWeb.DatVM.Mvc.BaseEntityViewModel<HmsService.Models.Entities.DeliveryInformation>
    {

        public virtual int ID { get; set; }
        public virtual string UserId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Email { get; set; }
        public virtual string City { get; set; }
        public virtual string District { get; set; }
        public virtual string Ward { get; set; }
        public virtual Nullable<bool> TypeAddress { get; set; }
        public virtual string Address { get; set; }
        public virtual bool Active { get; set; }
        public virtual Nullable<bool> IsDefault { get; set; }
        public virtual Nullable<int> ProvinceCode { get; set; }
        public virtual Nullable<int> DistrictCode { get; set; }
        public virtual string Lat { get; set; }
        public virtual string Lon { get; set; }
        public DeliveryInformationViewModel() : base() { }
        public DeliveryInformationViewModel(HmsService.Models.Entities.DeliveryInformation entity) : base(entity) { }

    }
}
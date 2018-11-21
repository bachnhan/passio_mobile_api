using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class MembershipCardEditViewModel : BaseEntityViewModel<MembershipCard>
    {

        public int Id { get; set; }

        [Display(Name = "Customer ID")]
        public int CustomerId { get; set; }

        [Display(Name = "MembershipCardCode")]
        public string MembershipCardCode { get; set; }

        [Display(Name = "CSV")]
        public string CSV { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Create Time")]
        public DateTime CreatedTime { get; set; }

        [Display(Name = "Membership Type")]
        public int? MembershipTypeId { get; set; }
        // membershipType
        public string MembershipCardTypeName { get; set; }
        public MembershipCardTypeViewModel MembershipCardType { get; set;}
        public int Status { get; set; }
        public string TypeName { get; set; }
        public decimal? DefaultBalance { get; set; }
        public CustomerEditViewModel Customer { get; set; }
        public List<SelectListItem> ListTypeActive { get; set; }
        public List<String> ListType { get; set; }
        public IEnumerable<SelectListItem> ListTypeMembership { get; set; }
        public IEnumerable<SelectListItem> CustomerList { get; set; }
        public string AccountType { get; set; } //Cấm delete
        public string ProductCode { get; set; } //Cấm delete
        public MembershipCardEditViewModel(MembershipCard entity)
        {
            this.Id = entity.Id;
            this.CustomerId = entity.CustomerId.GetValueOrDefault();
            this.MembershipCardCode = entity.MembershipCardCode;
            this.CSV = entity.CSV;
            this.CustomerName = entity.Customer == null ? "" : entity.Customer.Name;
            this.Active = entity.Active;
            this.CreatedTime = entity.CreatedTime;
            this.MembershipTypeId = entity.MembershipCardType.Id;
            this.MembershipCardTypeName = entity.MembershipCardType.TypeName;
            this.Status = entity.Status.GetValueOrDefault();
        }

        public MembershipCardEditViewModel()
        {

        }
    }
}

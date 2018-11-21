using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class CustomerEditViewModel:CustomerViewModel
    {
        public CustomerEditViewModel() : base() { }

        public CustomerEditViewModel(Customer entity) : base(entity) { }

        public IEnumerable<SelectListItem> AvailableType { get; set; }
        public List<SelectListItem> AvailableGender { get; set; }
        public MembershipCardEditViewModel MembershipCard { get; set; }
        public int cardId { get; set; }
    }
}

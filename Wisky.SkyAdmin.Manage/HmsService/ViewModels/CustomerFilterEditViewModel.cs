using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class CustomerFilterEditViewModel:CustomerFilterViewModel
    {
        public CustomerFilterEditViewModel() : base() { }
        public CustomerFilterEditViewModel(CustomerFilter entity) : base(entity) { }
        public IEnumerable<SelectListItem> AvailableType { get; set; }
        public List<SelectListItem> AvailableGender { get; set; }
        public List<SelectListItem> AvailableBirthdayOptions { get; set; }
    }
}

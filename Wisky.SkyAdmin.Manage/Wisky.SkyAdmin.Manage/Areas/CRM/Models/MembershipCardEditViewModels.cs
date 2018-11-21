using HmsService.ViewModels;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Models
{
    public class MembershipCardEditViewModels : MembershipCardViewModel
    {
        public IEnumerable<AccountEditViewModel> listAccounts { get; set; }
        public string newAccounts { get; set; }
        public string Type { get; set; }
        public IEnumerable<SelectListItem> listType { get; set; }
    }
}
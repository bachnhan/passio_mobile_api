using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Models
{
    public class MembershipCardLevelViewModels
    {
        public int CustomerId { get; set; }
        public int CurrentCardId { get; set; }
        public MembershipCardEditViewModels CardModel { get; set; }
        public IEnumerable<SelectListItem> ListLevelUpCards { get; set; }
    }
}
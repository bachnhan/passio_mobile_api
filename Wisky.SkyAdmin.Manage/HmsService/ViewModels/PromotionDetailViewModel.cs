using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class PromotionDetailViewModel: BaseEntityViewModel<PromotionDetail>
    {
        public IEnumerable<SelectListItem> AvailableProduct { get; set; }
        public int isMember { get; set; }
        public List<MembershipCardTypeViewModel> MembershipCardTypeList { get; set; }
    }
}

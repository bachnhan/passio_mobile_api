using HmsService.Models;
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
   public partial class PromotionViewModel : BaseEntityViewModel<Promotion>
    {
        public PromotionApplyLevelEnum PromotionApplyLevelEnum { get; set; }
        public PromotionGiftTypeEnum PromotionGiftTypeEnum { get; set; }
        public ImageCssEnum ImageCssEnum { get; set; }
        public IEnumerable<SelectListItem> AvailableGroup { get; set; }
       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class CouponViewModel
    {
        public StoreViewModel Store { get; set; }
        public CouponCampaignViewModel CouponCampaign { get; set; }
    }
}

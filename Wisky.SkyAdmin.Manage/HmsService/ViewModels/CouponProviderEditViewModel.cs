using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HmsService.ViewModels;
using AutoMapper;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class CouponProviderEditViewModel: CouponProviderViewModel
    {
        public CouponProviderEditViewModel() { }
        public CouponProviderEditViewModel(IEnumerable<CouponProviderViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public CouponProviderEditViewModel(CouponProviderViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public IEnumerable<SelectListItem> AvailableProvider { get; set; }
        public IEnumerable<SelectListItem> AvailableCouponProvider { get; set; }
    }
}
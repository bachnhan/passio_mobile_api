using AutoMapper;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class CouponEditViewModel
    {
        public CouponEditViewModel() { }
        public CouponEditViewModel(IEnumerable<CouponViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public CouponViewModel CouponViewModel { get; set; }

        public CouponCampaignViewModel CouponCampaign { get; set; }
        public StoreViewModel Store { get; set; }
        public IEnumerable<SelectListItem> AvailableStore { get; set; }

        public HttpPostedFileBase imageUpload { get; set; }
        public string CouponImageUrlTEmp { get; set; }
        public string DateUse
        {
            get
            {
                if (this.CouponViewModel.DateUse.HasValue)
                {
                    return CouponViewModel.DateUse.Value.ToString("dd/MM/yyyy");
                }
                return "";
            }
            set
            {
                DateTime result = default(DateTime);
                var rs = DateTime.TryParseExact(value, "dd/MM/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out result);
                if (rs)
                {
                    this.CouponViewModel.DateUse = result;
                }
            }
        }

    }
}
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class CouponCampaignEditViewModel: CouponCampaignViewModel
    {
        public CouponCampaignEditViewModel() { }
        public CouponCampaignEditViewModel(IEnumerable<CouponCampaignViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public CouponCampaignEditViewModel(CouponCampaignViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }

        [Required(ErrorMessage ="Vui lòng nhập 'Tên chiến dịch'")]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }
        [Required(ErrorMessage ="Vui lòng nhập 'Giá trị Coupon'")]
        public override Decimal Price
        {
            get { return base.Price; }
            set { base.Price = value; }
        }
        [Required(ErrorMessage = "Vui lòng nhập 'Giá trị quy đổi'")]
        public override Decimal Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }
        public string StartDateStr
        {
            get
            {
                if (this.StartDate.HasValue)
                {
                    return StartDate.Value.ToString("dd/MM/yyyy");
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
                    this.StartDate = result;
                }
            }
        }
        public string EndDateStr
        {
            get
            {
                if (this.EndDate.HasValue)
                {
                    return EndDate.Value.ToString("dd/MM/yyyy");
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
                    this.EndDate = result;
                }
            }
        }
        public IEnumerable<SelectListItem> AvailableCouponProvider { get; set; }
        public IEnumerable<SelectListItem> AvailableProvider { get; set; }
        public Enum CouponCampaignStatus { get; set; }
    }
}
using AutoMapper;
using HmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class PromotionApiViewModel
    {
        public int PromotionID { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionName { get; set; }
        public string PromotionClassName { get; set; }
        public string Description { get; set; }
        public string ImageCss { get; set; }
        public int ApplyLevel { get; set; }
        public int GiftType { get; set; }
        public bool IsForMember { get; set; }
        public bool Active { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int PromotionType { get; set; }
        public Nullable<int> ApplyFromTime { get; set; }
        public Nullable<int> ApplyToTime { get; set; }
        public Nullable<int> BrandId { get; set; }
        public List<PromotionDetailApiViewModel> PromotionDetails { get; set; }
        public Nullable<bool> IsVoucher { get; set; }
        public Nullable<bool> IsApplyOnce { get; set; }
        public Nullable<int> FromHappyDay { get; set; }
        public Nullable<int> ToHappyDay { get; set; }
        public Nullable<int> FromHoursHappy { get; set; }
        public Nullable<int> ToHoursHappy { get; set; }
    }

    public class PromotionPromotionDetailMappingViewModel
    {
        public List<PromotionApiViewModel> Promotions { get; set; }
        public List<PromotionDetailApiViewModel> PromotionDetails { get; set; }
    }
}

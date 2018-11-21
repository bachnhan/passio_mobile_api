using HmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class PromotionDetailApiViewModel
    {
        public int PromotionDetailID { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionDetailCode { get; set; }
        public string RegExCode { get; set; }
        public Nullable<double> MinOrderAmount { get; set; }
        public Nullable<double> MaxOrderAmount { get; set; }
        public string BuyProductCode { get; set; }
        public Nullable<int> MinBuyQuantity { get; set; }
        public Nullable<int> MaxBuyQuantity { get; set; }
        public string GiftProductCode { get; set; }
        public int GiftQuantity { get; set; }
        public Nullable<double> DiscountRate { get; set; }
        public Nullable<decimal> DiscountAmount { get; set; }
        public Nullable<int> PointTrade { get; set; }
        public Nullable<int> MinPoint { get; set; }
        public Nullable<int> MaxPoint { get; set; }
    }

    public class ListPromotionDetailApiViewModel : BaseApi
    {
        public List<PromotionDetailApiViewModel> PromotionDetailApiViewModels { get; set; }

        public ListPromotionDetailApiViewModel()
        {
            if (PromotionDetailApiViewModels == null)
            {
                PromotionDetailApiViewModels = new List<PromotionDetailApiViewModel>();
            }
        }
    }
}

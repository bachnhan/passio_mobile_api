using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ProductPromotionReport
    {
        public string ProductName { get; set; }
        public int ProductID { get; set; }
        Dictionary<string ,PromotionReport> DictPromotion { get; set; }

        public ProductPromotionReport()
        {
            this.DictPromotion = new Dictionary<string, PromotionReport>();
        }
    }

    public class PromotionReport
    {
        public string PromotionCode { get; set; }
        public string PromotionName { get; set; }
        public double TotalDiscount { get; set; }
    }
}

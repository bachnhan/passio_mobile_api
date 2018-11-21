using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OrderDetailPromotionMappingModel
    {
        public int Id { get; set; }
        public int OrderDetailId { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionDetailCode { get; set; }
        public Nullable<decimal> DiscountAmount { get; set; }
        public Nullable<int> DiscountRate { get; set; }
        public int MappingIndex { get; set; }
        public Nullable<int> TmpMappingId { get; set; }
    }
}

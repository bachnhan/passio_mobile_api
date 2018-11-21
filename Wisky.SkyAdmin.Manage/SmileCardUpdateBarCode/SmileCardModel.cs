using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmileCardUpdateBarCode
{
    public class SmileCardModel
    {
        public string ProcessingCode { get; set; }
        public string TerminalId { get; set; }
        public string MerchantId { get; set; }
        public string StaffId { get; set; }
        public string DiscountPercent { get; set; }
        public string Saving { get; set; }
    }
}

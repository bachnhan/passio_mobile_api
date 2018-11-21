using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OrderDetailForRPProduct
    {
        public int OrderDetailId { get; set; }
        public int ProductID { get; set; }
        public int OrderId { get; set; }
        public double FinalAmount { get; set; }
        public double Discount { get; set; }
        public double TotalAmount { get; set; }
        public int Quantity { get; set; }
    }
}

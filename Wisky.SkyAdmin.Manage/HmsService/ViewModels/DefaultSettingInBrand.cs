using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    /// <summary>
    /// Class default setting price of product in brand
    /// </summary>
    public class DefaultSettingInBrand
    {
        public double PriceDefault { get; set; }
        public int DiscountPercentDefault { get; set; }
        public double PriceDiscountDefault { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
    }
}

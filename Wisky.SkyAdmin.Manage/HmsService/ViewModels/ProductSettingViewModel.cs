using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ProductSettingViewModel : ProductDetailMappingViewModel 
    {
        public string storeName { get; set; }
        public string productName { get; set; }
        public string productCode { get; set; }
        public string imageUrl { get; set; }
        public string productType { get; set; }
    }
}

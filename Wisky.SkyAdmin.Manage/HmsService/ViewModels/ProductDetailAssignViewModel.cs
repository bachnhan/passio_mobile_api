using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ProductDetailAssignViewModel
    {
        public int StoreId { get; set; }
        public int Price { get; set; }
        public int DiscountPrice { get; set; }
        public bool IsChecked { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OrderRequestViewModel
    {
        //customer
        public string AccessToken { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Note { get; set; }

        //order
        public string DeliveryAddress { get; set; }
        public string StoreName { get; set; }

        public List<OrderProductRequest> ProductList { get; set; }

    }

    public class OrderProductRequest
    {
        public string ProductCode { get; set; }
        public int Quantity { get; set; }

    }
}

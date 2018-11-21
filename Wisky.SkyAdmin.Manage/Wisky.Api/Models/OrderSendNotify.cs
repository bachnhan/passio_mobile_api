using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wisky.Api.Models
{
    public class CustomerNotify
    {
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string address { get; set; }

    }

    public class OrderDetailNotify
    {
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productCategory { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
    }

    public class OrderNotifyMessage
    {
        public string OrderCode { get; set; }
        public string CheckInDate { get; set; }
        public int TotalAmount { get; set; }
        public CustomerNotify AccountInfo { get; set; }
        public List<OrderDetailNotify> OrderDetail { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public class GroupCategoryReportModal
    {
        public int ProductId { get; set; }
        public string CateName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int QuantityAtStore { get; set; }
        public int QuantityTakeAway { get; set; }
        public int QuantityDelivery { get; set; }
        public int TotalPrice { get; set; }

        public int Discount { get; set; }

        public int TotalOrder { get; set; }
    }

    public class TempProductByCategory
    {
        public string CategotyName { get; set; }
        public List<string> ListProductName { get; set; }
        public List<int> ListQuantity { get; set; }
    }
}

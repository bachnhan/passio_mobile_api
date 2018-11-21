using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ItemViewModel
    {
        public ItemViewModel() { }

        public int productID { get; set; }
        public int itemID { get; set; }
        public string itemName { get; set; }
        public string itemUnit { get; set; }
        public double quantity { get; set; }
        public double price { get; set; }
        public ItemViewModel(int productID, int itemID, string name, string unit, double quantity)
        {
            this.productID = productID;
            this.itemID = itemID;
            this.itemName = name;
            this.itemUnit = unit;
            this.quantity = quantity;
        }
    }
}

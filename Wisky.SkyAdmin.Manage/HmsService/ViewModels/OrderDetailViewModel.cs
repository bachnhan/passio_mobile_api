using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class OrderDetailViewModel
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }

        public string FormatDiscount
        {
            get
            {
                if (this.Discount == 0)
                {
                    return "0";
                }
                else
                {
                    return this.Discount.ToString("#,#.#");
                }
            }
        }

        public string FormatUnitPrice
        {
            get
            {
                if (this.UnitPrice == 0)
                {
                    return "0";
                }
                else
                {
                    return this.UnitPrice.ToString("#,#.#");
                }
            }
        }

        public string FormatFinalPrice
        {
            get
            {
                if (this.FinalAmount == 0)
                {
                    return "0";
                }
                else
                {
                    return this.FinalAmount.ToString("#,#.#");
                }
            }
        }
        public ProductViewModel Product { get; set; }
    }

    public class OrderDetailModel
    {
        public int OrderDetailID { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public double FinalAmount { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public int Quantity { get; set; }
        public System.DateTime OrderDate { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; }
        public int TaxValue { get; set; }
        public double UnitPrice { get; set; }
        public int ProductType { get; set; }
        public int ParentId { get; set; }
        public int ProductOrderType { get; set; }
        public int ItemQuantity { get; set; }
        public int StoreId { get; set; }
    }
}

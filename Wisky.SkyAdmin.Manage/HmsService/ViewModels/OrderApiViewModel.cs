using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OrderApiViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public System.DateTime CheckInDate { get; set; }
        public System.DateTime CheckOutDate { get; set; }
        public System.DateTime ApproveDate { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double DiscountOrderDetail { get; set; }
        public double FinalAmount { get; set; }
        public int OrderStatus { get; set; }
        public int OrderType { get; set; }
        public string Notes { get; set; }
        public string FeeDescription { get; set; }
        public string CheckInPerson { get; set; }
        public string CheckOutPerson { get; set; }
        public string ApprovePerson { get; set; }
        public int CustomerID { get; set; }
        public int SourceID { get; set; }
        public int TableId { get; set; }
        public bool IsFixedPrice { get; set; }
        public System.DateTime LastRecordDate { get; set; }
        public string ServedPerson { get; set; }
        public string DeliveryAddress { get; set; }
        public int DeliveryStatus { get; set; }
        public string DeliveryPhone { get; set; }
        public string DeliveryCustomer { get; set; }
        public int SourceType { get; set; }
        public int TotalInvoicePrint { get; set; }
        public double VAT { get; set; }
        public double VATAmount { get; set; }
        public int NumberOfGuest { get; set; }

        public string VoucherCode { get; set; }

        public string Att1 { get; set; }
        public string Att2 { get; set; }
        public string Att3 { get; set; }
        public string Att4 { get; set; }
        public string Att5 { get; set; }

        public int GroupPaymentStatus { get; set; }

        public int StoreId { get; set; }
        public List<OrderDetailApiViewModel> OrderDetailMs { get; set; }
        public List<PaymentApiViewModel> PaymentMs { get; set; }
        public List<OrderPromotionMappingModel> OrderPromotioMappingMs { get; set; }

        public Nullable<System.DateTime> LastModifiedPayment { get; set; }
        public Nullable<System.DateTime> LastModifiedOrderDetail { get; set; }

        public OrderApiViewModel()
        {
            OrderDetailMs = new List<OrderDetailApiViewModel>();
            PaymentMs = new List<PaymentApiViewModel>();
            OrderPromotioMappingMs = new List<OrderPromotionMappingModel>();
        }
    }

    public class OrderDetailApiViewModel
    {
        public OrderDetailApiViewModel()
        {
            OrderDetailPromotioMappingMs = new List<OrderDetailPromotionMappingModel>();
        }
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
        public int? ParentId { get; set; }
        public int ProductOrderType { get; set; }
        public int ItemQuantity { get; set; }
        public int StoreId { get; set; }
        public int? TmpDetailId { get; set; }
        public List<OrderDetailPromotionMappingModel> OrderDetailPromotioMappingMs { get; set; }
        public OrderPromotionMappingModel OrderPromotionMapping { get; set; }
        public OrderDetailPromotionMappingModel OrderDetailPromotionMapping { get; set; }
        public Nullable<int> OrderPromotionMappingId { get; set; }
        public Nullable<int> OrderDetailPromotionMappingId { get; set; }
    }

    public class PaymentApiViewModel
    {
        public int PaymentID { get; set; }
        public int OrderId { get; set; }
        public int Amount { get; set; }
        public string CurrencyCode { get; set; }
        public decimal FCAmount { get; set; }
        public string Notes { get; set; }
        public System.DateTime PayTime { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string CardCode { get; set; }
    }

    public class OrderStatusApiViewModel
    {
        public int OrderStatus { get; set; }
        public int DeliveryStatus { get; set; }
        public string InvoiceId { get; set; }
        public string CheckInPerson { get; set; }
    }
}

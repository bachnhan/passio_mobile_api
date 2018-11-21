using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HmsService.ViewModels
{
    public class OrderOnlineViewModel
    {
        //Thông tin khách hàng
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public DateTime OrderDate { get; set; }
        public string Note { get; set; }
        public string Email { get; set; }

        public double Fee { get; set; }

        public string Voucher { get; set; }
        //Đơn hàng
        //public string OrderId { get; set; }
        public string DeliveryAddress { get; set; }
        public string StoreName { get; set; }
        public List<ProductList> ProductList { get; set; }
    }
    public class ProductList
    {
        //sản phẩm yêu cầu
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public double Price { get; set; }
        public double FinalAmount { get; set; }
        public int Quantity { get; set; }
        public int? TempId { get; set; }
        public Nullable<int> OrderDetailPromotionMappingId { get; set; }
        public Nullable<int> OrderPromotionMappingId { get; set; }
        public List<ProductWithExtras> ProductWithExtras { get; set; }
    }
    public class ProductWithExtras
    {
        //sản phẩm đi kèm
        public int ProductExtraId { get; set; }
        public string ProductCode { get; set; }
        public int ParentId { get; set; }
        public double Price { get; set; }
        public double FinalAmount { get; set; }
        public int Quantity { get; set; }
        public Nullable<int> OrderDetailPromotionMappingId { get; set; }
        public Nullable<int> OrderPromotionMappingId { get; set; }
    }
}

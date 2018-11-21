using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class VATOrderViewModel
    {
        public string CustomerName { get; set; }
        public string CreateTimeStr
        {
            get
            {
                if (!this.CheckInDate.ToString("dd/MM/yyyy").Equals(""))
                {
                    return this.CheckInDate.ToString("dd/MM/yyyy");
                }
                return "";
            }
        }
        public CustomerViewModel Customer { get; set; }
        public IEnumerable<OrderDetailViewModel> VATOrderMappings { get; set; }
    }

    public class VATOrderModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public System.DateTime CheckInDate { get; set; }
        public double Total { get; set; }
        public double VATAmount { get; set; }
        public int Type { get; set; }
        public string Notes { get; set; }
        public string CheckInPerson { get; set; }
        public int BrandId { get; set; }
        public int ProviderID { get; set; }
        //public List<OrderDetailModel> OrderDetailMs { get; set; }
        //public List<PaymentModel> PaymentMs { get; set; }

        //public VATOrderModel()
        //{
        //    OrderDetailMs = new List<OrderDetailModel>();
        //    PaymentMs = new List<PaymentModel>();
        //}
    }
}

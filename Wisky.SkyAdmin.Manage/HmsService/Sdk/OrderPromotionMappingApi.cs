using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class OrderPromotionMappingApi
    {

        public int GetByOrderId(int orderId)
        {
            var m = this.BaseService.GetActive(c => c.OrderId == orderId).Select(c => new { id = c.Id }).ToList();
            return m[0].id;
        }
        public OrderPromotionMapping GetOrderMappingByOrderId(int orderId)
        {
            var m = this.BaseService.GetActive(c => c.OrderId == orderId).FirstOrDefault();
            return m;
        }
        public IQueryable<int> GetOrderIdByPromoId(int promoId)
        {
            return this.BaseService.GetActive(c => c.PromotionId == promoId).Select(c => c.OrderId);
        }

        public IEnumerable<OrderPromotionMapping> GetMappingByOrderId(int orderId)
        {
            var m = this.BaseService.GetActive(c => c.OrderId == orderId).AsEnumerable();
            return m;
        }

        public int GetByOrderInvoiceId(string invoiceId)
        {
            var m = this.BaseService.GetActive(c => c.Order.InvoiceID == invoiceId).
                Select(c => new { id = c.Id }).ToList();
            return m[0].id;
        }

        public IQueryable<OrderPromotionMapping> GetActiveReturnEntities()
        {
            return this.BaseService.GetActive();
        }

        public int CreateAndReturnId(OrderPromotionMappingViewModel model)
        {
            return this.BaseService.CreateAndReturnId(model.ToEntity());
        }
    }
}

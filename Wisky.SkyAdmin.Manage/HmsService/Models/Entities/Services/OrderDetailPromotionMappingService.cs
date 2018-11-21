using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IOrderDetailPromotionMappingService
    {
        IQueryable<OrderDetailPromotionMapping> GetOrderDetailPromotionByOrderId(int orderDetailId);
        int CreateAndReturnId(OrderDetailPromotionMapping entity);
    }

    public partial class OrderDetailPromotionMappingService
    {
        public IQueryable<OrderDetailPromotionMapping> GetOrderDetailPromotionByOrderId(int orderDetailId)
        {
            return this.GetActive(q => q.OrderDetailId == orderDetailId);
        }

        public int CreateAndReturnId(OrderDetailPromotionMapping entity)
        {
            try
            {
                this.Create(entity);
                return entity.Id;
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}

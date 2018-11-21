using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class OrderDetailPromotionMappingApi
    {
        public IEnumerable<OrderDetailPromotionMapping> GetOrderDetailPromotionByOrderDetailId(int orderDetailId)
        {
            return this.BaseService.GetOrderDetailPromotionByOrderId(orderDetailId).AsEnumerable();
        }

        public IQueryable<OrderDetailPromotionMapping> GetActiveReturnEntities()
        {
            return this.BaseService.GetActive();
        }

        public int CreateAndReturnId (OrderDetailPromotionMappingViewModel model)
        {
            return this.BaseService.CreateAndReturnId(model.ToEntity());
        }
    }
}

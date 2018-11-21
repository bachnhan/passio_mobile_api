using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IOrderPromotionMappingService
    {
        int CreateAndReturnId(OrderPromotionMapping entity);
    }

    public partial class OrderPromotionMappingService
    {
        public int CreateAndReturnId(OrderPromotionMapping entity)
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

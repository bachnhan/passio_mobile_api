using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class DeliveryInfoApi
    {
        public DeliveryInfo getDeliveryById(int deliveryInfoId)
        {
            var result = this.BaseService.Get(deliveryInfoId);
            //DeliveryInfoViewModel entity = new DeliveryInfoViewModel(result);
            return result;

        }
        public IQueryable<DeliveryInfo> getDeliveryByCustomerId(int customerId)
        {
            return this.BaseService.Get(q => q.CustomerId == customerId && q.Active == true);
        }
        public void updateDeliveryInfo(DeliveryInfo model)
        {
            this.BaseService.updateDeliveryInfo(model);
        }
        public void createDeliveryInfo(DeliveryInfo model)
        {
            this.BaseService.createDeliveryInfo(model);
        }
        public void deleteDeliveryInfo(DeliveryInfo model)
        {
            this.BaseService.deleteDeliveryInfo(model);
        }
    }
}

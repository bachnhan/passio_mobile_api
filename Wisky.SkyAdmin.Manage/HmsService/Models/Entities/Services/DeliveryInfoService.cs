using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.Sdk;

namespace HmsService.Models.Entities.Services
{
    public partial interface IDeliveryInfoService
    {
        void updateDeliveryInfo(DeliveryInfo deliveryInfo);
        void createDeliveryInfo(DeliveryInfo deliveryInfo);
        void deleteDeliveryInfo(DeliveryInfo deliveryInfo);
    }
    public partial class DeliveryInfoService
    {
        public void updateDeliveryInfo(DeliveryInfo deliveryInfo)
        {
            try
            {
                this.Update(deliveryInfo);
                Save();
            }
            catch (Exception e)
            {
                var mes = e.Message;
            }
        }
        public void createDeliveryInfo(DeliveryInfo deliveryInfo)
        {
            this.Create(deliveryInfo);
            this.Save();
        }
        public void deleteDeliveryInfo(DeliveryInfo deliveryInfo)
        {
            deliveryInfo.Active = false;
            this.Update(deliveryInfo);
            this.Save();
        }
    }
}

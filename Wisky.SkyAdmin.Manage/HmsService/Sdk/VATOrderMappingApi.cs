using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class VATOrderMappingApi
    {
        public VATOrderMapping VATOrderMaster(int VATOrderMappingId)
        {
            var orderDetail = this.BaseService.GetVATOrderMappingMasterAsync(VATOrderMappingId);
            return orderDetail;
        }
        #region StoreReport
        public IEnumerable<VATOrderMapping> GetVATOrderMappingByInvoiceId(int InvoiceId)
        {
            var orderDetails = this.BaseService.GetVATOrderMappingByInvoiceId(InvoiceId)
                //.ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig)
                .ToList();
            return orderDetails;
        }

        /*public IQueryable<OrderDetail> getAllOrderDetail()
        {
            var orderDetails = this.BaseService.Get().Where(c => c.Order.RentStatus != (int)RentStatusEnum.Disabled
                && c.Order.RentStatus != (int)RentStatusEnum.WaitDisabled
                && c.Order.RentStatus != (int)RentStatusEnum.DeletePermanent);
            return orderDetails;
        }

        public IQueryable<OrderDetail> getAll()
        {
            var orderDetails = this.BaseService.Get();
            return orderDetails;
        }*/
        #endregion
    }
}

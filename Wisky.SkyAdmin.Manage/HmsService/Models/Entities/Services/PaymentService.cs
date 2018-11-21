using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IPaymentService
    {
        IQueryable<Payment> GetPaymentByTimeRange(DateTime startDate, DateTime endDate);
        IQueryable<Payment> GetPaymentStoreByTimeRange(DateTime startDate, DateTime endDate, int storeId);
        IQueryable<Payment> GetPaymentBrandByTimeRange(DateTime startDate, DateTime endDate, int brandId);
        IQueryable<Payment> GetStorePaymentByTimeRange(int storeId, int brandId, DateTime startDate, DateTime endDate);
        IQueryable<Payment> GetStorePaymentByTimeRangeAndType(int storeId, int brandId, DateTime startDate, DateTime endDate, int type);
        IQueryable<PaymentViewModel> GetStorePaymentVMByTimeRange(int storeId, int brandId, DateTime startDate, DateTime endDate);
    }

    public partial class PaymentService
    {

        public IQueryable<Payment> GetPaymentBrandByTimeRange(DateTime startDate, DateTime endDate, int brandId)
        {
            var result = Get(q => (q.Order.Store.BrandId == brandId) && (q.PayTime <= endDate) && (q.PayTime > startDate) && (q.Order.OrderStatus == (int)OrderStatusEnum.Finish));
            return result;
        }

        public IQueryable<Payment> GetPaymentByTimeRange(DateTime startDate, DateTime endDate)
        {
            var result = Get(q => q.PayTime >= startDate && q.PayTime <= endDate);
            return result;
        }
        public IQueryable<Payment> GetPaymentStoreByTimeRange(DateTime startDate, DateTime endDate, int storeId)
        {
            try
            {
                return Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Order.StoreID.Value == storeId && q.Order.OrderStatus == (int)OrderStatusEnum.Finish);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IQueryable<PaymentViewModel> GetStorePaymentVMByTimeRange(int storeId, int brandId, DateTime startDate, DateTime endDate)
        {
            var entity = Get(q => q.PayTime >= startDate && q.PayTime <= endDate);
            var _orderService = DependencyUtils.Resolve<IOrderService>();
            var orders = _orderService.GetRentIdsByTimeRange(storeId, startDate, endDate, brandId);
            return entity.Join(orders, q => q.ToRentID, p => p, (q, p) => new PaymentViewModel
            {
                PaymentID = q.PaymentID,
                ToRentID = p,
                CardCode = q.CardCode,
                Amount = q.Amount,
                CurrencyCode = q.CurrencyCode,
                FCAmount = q.FCAmount,
                Notes = q.Notes,
                PayTime = q.PayTime,
                RealAmount = q.RealAmount,
                Status = q.Status,
                Type = q.Type
            });
        }
        /// <summary>
        /// Get Payement ByTime 
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="brandId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IQueryable<Payment> GetStorePaymentByTimeRange(int storeId, int brandId, DateTime startDate, DateTime endDate)
        {
            if (storeId != 0)
            {
                //Get By Order Finish 
                var entity = Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Order.Store.BrandId == brandId && q.Order.StoreID == storeId && q.Order.OrderStatus == (int)OrderStatusEnum.Finish);
                return entity;
            }
            else
            {
                //Get By Order Finish 
                var entity = Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Order.Store.BrandId == brandId && q.Order.OrderStatus == (int)OrderStatusEnum.Finish);
                return entity;
            }
        }

        public IQueryable<Payment> GetStorePaymentByTimeRangeAndType(int storeId, int brandId, DateTime startDate, DateTime endDate, int type)
        {
            if (storeId != 0)
            {
                return Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Type == type && q.Order.StoreID == storeId);
            }
            else
            {
                return Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Type == type && q.Order.Store.BrandId == brandId);
            }
        }
    }
}
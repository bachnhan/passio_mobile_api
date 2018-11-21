using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class PaymentApi
    {
        public void CreatePayment(Payment payment)
        {
            this.BaseService.Create(payment);
        }

        public int CreatePaymentReturnId(Payment payment)
        {
            this.BaseService.Create(payment);
            return payment.PaymentID;
        }

        public IEnumerable<PaymentViewModel> GetStorePaymentInDateRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            return BaseService.GetStorePaymentByTimeRange(storeId, brandId, startDate, endDate)
                .ProjectTo<PaymentViewModel>(AutoMapperConfig).ToList();
        }

        public IQueryable<Payment> GetPaymentByTimeRange(int storeId, DateTime startDate, DateTime endDate, int brandId )
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Order.StoreID == storeId && q.Order.OrderStatus == statusOrder);
            } else
            {
                return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Order.Store.BrandId == brandId && q.Order.OrderStatus == statusOrder);
            }
        }


        public IEnumerable<PaymentViewModel> PaymentByTimeRange(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId);
                var payments = this.BaseService.Get();
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select new PaymentViewModel
                               {
                                   Cashier = order.CheckInPerson,
                                   Amount = payment.Amount,
                                   Type = payment.Type
                               };
                return joinData;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder);
                var payments = this.BaseService.Get();
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order;
                var joinData = from orderdata in joinTmpOrder
                               join payment in payments
                               on orderdata.RentID equals payment.ToRentID
                               select new PaymentViewModel
                               {
                                   Cashier = orderdata.CheckInPerson,
                                   Amount = payment.Amount,
                                   Type = payment.Type
                               };
                return joinData;
            }
        }


        public IQueryable<Payment> GetPaymentByTimeRangeAndType(int storeId, DateTime startDate, DateTime endDate, int paymentType, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                if ((int)PaymentTypeEnum.Cash == paymentType)
                {
                    return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && (q.Type == paymentType || q.Type == (int) PaymentTypeEnum.ExchangeCash) && q.Order.StoreID == storeId && q.Order.OrderStatus == statusOrder);
                }
                return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Type == paymentType && q.Order.StoreID == storeId && q.Order.OrderStatus == statusOrder);
            }
            else
            {
                if ((int)PaymentTypeEnum.Cash == paymentType)
                {
                    return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && (q.Type == paymentType || q.Type == (int)PaymentTypeEnum.ExchangeCash) && q.Order.Store.BrandId == brandId && q.Order.OrderStatus == statusOrder);
                }
                return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && q.Type == paymentType && q.Order.Store.BrandId == brandId && q.Order.OrderStatus == statusOrder);
            }
        }

        public IQueryable<Payment> GetPaymentByTimeRangeAndTypeList(int storeId, DateTime startDate, DateTime endDate, List<int> paymentType, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && paymentType.Any(k=>k == q.Type) && q.Order.StoreID == storeId && q.Order.OrderStatus == statusOrder);
            }
            else
            {
                return this.BaseService.Get(q => q.PayTime >= startDate && q.PayTime <= endDate && paymentType.Any(k => k == q.Type) && q.Order.Store.BrandId == brandId && q.Order.OrderStatus == statusOrder);
            }
        }

        public IEnumerable<Payment> GetPaymentByOrder(int orderId)
        {
            return this.BaseService.Get(q => q.ToRentID == orderId).Distinct();
        }

        public IQueryable<Payment> GetPaymentById(int paymentId)
        {
            return this.BaseService.Get(q => q.PaymentID == paymentId);
        }
        public IQueryable<Payment> GetPaymentByCostId(int costId)
        {
            return this.BaseService.Get(q => q.CostID == costId);
        }
        public IQueryable<Payment> GetPaymentByToRentAndType(int toRentId,int type)
        {
            return this.BaseService.Get(q => q.ToRentID == toRentId && q.Type == type);
        }
        /// <summary>
        /// Get payment by time range
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="brandId"></param>
        /// <returns>Entity Payment</returns>
        public IQueryable<Payment> GetStorePaymentByDateRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            return BaseService.GetStorePaymentByTimeRange(storeId, brandId, startDate, endDate);
        }



        public IQueryable<PaymentViewModel> GetQueryStorePaymentInDateRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            return BaseService.GetStorePaymentByTimeRange(storeId, brandId, startDate, endDate)
                .ProjectTo<PaymentViewModel>(AutoMapperConfig);
        }

     
        public IQueryable<Payment> GetEntityStorePaymentInDateRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            return BaseService.GetStorePaymentByTimeRange(storeId, brandId, startDate, endDate);
        }

        public IQueryable<Payment> GetStorePaymentByTimeRangeAndType(int storeId, DateTime startDate, DateTime endDate, int type, int brandId)
        {
            return BaseService.GetStorePaymentByTimeRangeAndType(storeId, brandId, startDate, endDate, type);
        }
        public int UpdatePayment(Payment payment)
        {
            var id = -1;
            try
            {
                BaseService.Update(payment);
                id = payment.PaymentID;
            }
            catch
            {
            }
            return id;
        }
        public IQueryable<PaymentViewModel> GetStorePaymentVMInDateRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            //return BaseService.GetStorePaymentVMByTimeRange(storeId, brandId, startDate, endDate);
            var entity = BaseService.GetPaymentByTimeRange(startDate, endDate);        
            var orderApi = new OrderApi();
            var orders = orderApi.GetAllOrderIdsByTimeRange(storeId, brandId, startDate, endDate);
            return entity.Join(orders, q => q.ToRentID, p => p, (q, p) => new PaymentViewModel
            {
                PaymentID = q.PaymentID,
                ToRentID = q.ToRentID,
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
        /// Get Total Payment By Time Range
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="paymentType"></param>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public int CountTotalPaymentByTimeRangeWithPayType(int storeId, DateTime startTime, DateTime endTime, int paymentType, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId);
                var payments = this.BaseService.Get(q=> q.Type == paymentType);
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            } else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder);
                var payments = this.BaseService.Get(q => q.Type == paymentType);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
        }

        public double SumTotalPaymentByTimeRangeWithPayType(int storeId, DateTime startTime, DateTime endTime, int paymentType, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId);
                var payments = this.BaseService.Get(q => q.Type == paymentType);
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.Amount;
                var returnValue = 0.0;
                try
                {
                    returnValue = joinData.Sum();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder);
                var payments = this.BaseService.Get(q => q.Type == paymentType);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.Amount;
                var returnValue = 0.0;
                try
                {
                    returnValue = joinData.Sum();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
        }

        public int CountTotalPaymentByTimeRangeWithPayCard(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            var ordercard = (int)OrderTypeEnum.OrderCard;
            var paymentCash = (int)PaymentTypeEnum.Cash;
            var paymentExchange = (int)PaymentTypeEnum.ExchangeCash;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId && q.OrderType == ordercard);
                var payments = this.BaseService.Get(q => q.Type == paymentCash || q.Type == paymentExchange);
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.OrderType == ordercard);
                var payments = this.BaseService.Get(q =>q.Type == paymentCash || q.Type == paymentExchange);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
        }

        public double SumTotalPaymentByTimeRangeWithPayCard(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            var ordercard = (int)OrderTypeEnum.OrderCard;
            var paymentCash = (int)PaymentTypeEnum.Cash;
            var paymentExchange = (int)PaymentTypeEnum.ExchangeCash;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId && q.OrderType == ordercard);
                var payments1 = this.BaseService.Get(q => q.Type == paymentCash);
                var payments2 = this.BaseService.Get(q => q.Type == paymentExchange);
                var joinData1 = from order in listOrder
                               join payment in payments1
                               on order.RentID equals payment.ToRentID
                               select payment.Amount;
                var joinData2 = from order in listOrder
                                join payment in payments2
                                on order.RentID equals payment.ToRentID
                                select payment.Amount;
                var sum1 = 0.0;
                try
                {
                    sum1 = joinData1.Sum();

                }
                catch (Exception)
                {
                }
                var sum2 = 0.0;
                try
                {
                    sum2 = joinData2.Sum();

                }
                catch (Exception)
                {
                }
                return sum1 + sum2;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.OrderType == ordercard);
                var payments1 = this.BaseService.Get(q => q.Type == paymentExchange);
                var payments2 = this.BaseService.Get(q => q.Type == paymentCash);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData1 = from idOrder in joinTmpOrder
                               join payment in payments1
                               on idOrder equals payment.ToRentID
                               select payment.Amount;
                var joinData2 = from idOrder in joinTmpOrder
                               join payment in payments2
                               on idOrder equals payment.ToRentID
                               select payment.Amount;
                var sum1 = 0.0;
                try
                {
                    sum1 = joinData1.Sum();

                }
                catch (Exception)
                {
                }
                var sum2 = 0.0;
                try
                {
                    sum2 = joinData2.Sum();

                }
                catch (Exception)
                {
                }
                return sum1 + sum2;
            }
        }

        public int CountTotalPaymentByTimeRangeWithPaySale(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            var ordercard = (int)OrderTypeEnum.OrderCard;
            var paymentCash = (int)PaymentTypeEnum.Cash;
            var paymentExchange = (int)PaymentTypeEnum.ExchangeCash;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId && q.OrderType != ordercard);
                var payments = this.BaseService.Get(q => q.Type == paymentCash);
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue1 = 0;
                var returnValue2 = 0;
                try
                {
                    returnValue1 = joinData.Count();
                }
                catch (Exception)
                {
                }
                var payments2 = this.BaseService.Get(q => q.Type == paymentCash);
                var joinData2 = from order in listOrder
                               join payment in payments2
                               on order.RentID equals payment.ToRentID
                               select payment.PaymentID;
                try
                {
                    returnValue2 = joinData2.Count();
                }
                catch (Exception)
                {
                }
                var count2 = joinData2.Count();
                return returnValue1 + returnValue2;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.OrderType != ordercard);
                var payments = this.BaseService.Get(q => q.Type == paymentCash);
                var payments2 = this.BaseService.Get(q => q.Type == paymentExchange);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData1 = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.PaymentID;
                var joinData2 = from idOrder in joinTmpOrder
                                join payment in payments2
                                on idOrder equals payment.ToRentID
                                select payment.PaymentID;
                var returnValue1 = 0;
                var returnValue2 = 0;
                try
                {
                    returnValue1 = joinData1.Count();
                }
                catch (Exception)
                {
                }
                try
                {
                    returnValue2 = joinData2.Count();
                }
                catch (Exception)
                {
                }
                return returnValue1 + returnValue2;
            }
        }

        public double SumTotalPaymentByTimeRangeWithPaySale(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            var ordercard = (int)OrderTypeEnum.OrderCard;
            var paymentCash = (int)PaymentTypeEnum.Cash;
            var paymentExchange = (int)PaymentTypeEnum.ExchangeCash;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId && q.OrderType != ordercard);
                var payments = this.BaseService.Get(q => q.Type == paymentCash || q.Type == paymentExchange);
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.Amount;
                var sum = 0.0;
                try
                {
                    sum = joinData.Sum();

                }
                catch (Exception)
                {
                }
                return sum;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.OrderType != ordercard);
                var payments = this.BaseService.Get(q =>q.Type == paymentCash || q.Type == paymentExchange);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.Amount;
                var sum = 0.0;
                try
                {
                    sum = joinData.Sum();

                }
                catch (Exception)
                {
                }
                return sum;
            }
        }


        public int CountPaymentByTimeRange(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId);
                var payments = this.BaseService.Get();
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder);
                var payments = this.BaseService.Get();
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
        }

        public double SumTotalPaymentByTimeRange(int storeId, DateTime startTime, DateTime endTime, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId);
                var payments = this.BaseService.Get();
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.Amount;
                return joinData.Sum();
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder);
                var payments = this.BaseService.Get();
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.Amount;
                return joinData.Sum();
            }
        }

        public double SumTotalPaymentByTimeRangeWithPayTypes(int storeId, DateTime startTime, DateTime endTime, int payType1, int payType2, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId);
                var payments = this.BaseService.Get(q => q.Type == payType1 || q.Type == payType2);
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.Amount;
                var sum = 0.0;
                try
                {
                    sum = joinData.Sum();

                }
                catch (Exception)
                {
                }
                return sum;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder);
                var payments = this.BaseService.Get(q => q.Type == payType1 || q.Type == payType2);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment.ToRentID
                               select payment.Amount;
                var sum = 0.0;
                try
                {
                    sum = joinData.Sum();

                }
                catch (Exception)
                {
                }
                return sum;
            }
        }
        public int CountPaymentByTimeRangeWithPayTypes(int storeId, DateTime startTime, DateTime endTime, int payType1, int payType2, int brandId)
        {
            var statusOrder = (int)OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder && q.StoreID == storeId);
                var payments = this.BaseService.Get(q=> q.Type == payType1 || q.Type == payType2);
                var joinData = from order in listOrder
                               join payment in payments
                               on order.RentID equals payment.ToRentID
                               select payment.PaymentID;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
            else
            {
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllStoreByBrandId(brandId);
                var orderApi = new OrderApi();
                var listOrder = orderApi.BaseService.Get(q => q.CheckInDate >= startTime && q.CheckInDate < endTime && q.OrderStatus == statusOrder);
                var payments = this.BaseService.Get(q => q.Type == payType1 || q.Type == payType2).Select(q => q.ToRentID);
                var joinTmpOrder = from order in listOrder
                                   join store in listStore
                                   on order.StoreID equals store.ID
                                   select order.RentID;
                var joinData = from idOrder in joinTmpOrder
                               join payment in payments
                               on idOrder equals payment
                               select payment;
                var returnValue = 0;
                try
                {
                    returnValue = joinData.Count();
                }
                catch (Exception)
                {
                }
                return returnValue;
            }
        }

    }
}

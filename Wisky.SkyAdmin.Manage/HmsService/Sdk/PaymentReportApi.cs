using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class PaymentReportApi
    {
        public IQueryable<PaymentReport> GetPaymentReportByTimeRange(int storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            var statusOrder = (int)Models.OrderStatusEnum.Finish;
            if (storeId != 0)
            {
                return this.BaseService.Get(q => q.Date >= startDate && q.Date <= endDate && q.StoreID == storeId && q.Status == statusOrder);
            }
            else
            {
                return this.BaseService.Get(q => q.Date >= startDate && q.Date <= endDate && q.Store.BrandId == brandId && q.Status == statusOrder);
            }
        }
    }
}

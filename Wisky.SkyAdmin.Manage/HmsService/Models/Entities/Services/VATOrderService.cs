using SkyWeb.DatVM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IVATOrderService
    {
        IQueryable<VATOrder> GetAdminWithFilter(string keyword, KeyValuePair<string, bool> sortKeyAsc);
        //OrderCustomEntity GetVATOrderById(int storeId, int id);
        VATOrder GetVATOrderByIdAndBrandId(int brandId, int id);
        Task<bool> CreateVATOrderAsync(VATOrder entity);
        IQueryable<VATOrder> GetVATOrderByBrand(int brandId);
        Task<VATOrder> GetVATOrderByIdAsync(int id);

        bool CreateVATOrder(VATOrder VATorder);

        #region SystemReport
        IQueryable<VATOrder> GetAllVATOrderByDate(DateTime from, DateTime to, int brandId);
        IQueryable<VATOrder> GetAllVATOrderByDate(DateTime from, DateTime to);
        IQueryable<VATOrder> GetRentsByTimeRange(DateTime startTime, DateTime endTime);
        IQueryable<VATOrder> GetRentsByTimeRange(DateTime startTime, DateTime endTime, int brandId);
        IQueryable<VATOrder> GetRentsByTime(DateTime startTime, DateTime endTime);
        #endregion
        IEnumerable<VATOrder> GetVATOrders();
        IQueryable<VATOrder> GetVATOrdersByTimeRange(DateTime startTime, DateTime endTime);
    }

    public partial class VATOrderService
    {
        private readonly VATOrderMappingService mappingService;

        public IQueryable<VATOrder> GetVATOrdersByTimeRange(DateTime startTime, DateTime endTime)
        {
            var rents = this.Get(r => ((r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime)));
            return rents;
        }

        /*public OrderCustomEntity GetOrderById(int storeId, int id)
        {
            var order = this.GetActive(a => a.StoreID == storeId && a.RentID == id).FirstOrDefault();
            return new OrderCustomEntity
            {
                Order = order,
                OrderDetails = order.OrderDetails,
                Customer = order.Customer
            };
        }*/

        public IQueryable<VATOrder> GetAdminWithFilter(string keyword, 
            KeyValuePair<string, bool> sortKeyAsc)
        {

            var entities = this.GetActive();
            entities = entities.OrderByDescending(q => q.CheckInDate);

            return entities;
        }

        public VATOrder GetVATOrderByIdAndBrandId(int brandId, int id)
        {
            var result = this.Get(q => q.BrandID == brandId && q.InvoiceID == id).FirstOrDefault();
            return result;
        }

        public bool CreateVATOrder(VATOrder VATorder)
        {
            try
            {
                this.Create(VATorder);
                return true;
            }
            catch (Exception)
            {
                return false; ;
            }
        }

        public async Task<bool> CreateVATOrderAsync(VATOrder entity)
        {
            try
            {
                await this.CreateAsync(entity);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public IQueryable<VATOrder> GetVATOrderByBrand(int brandId)
        {
            var result = this.Get(q => q.BrandID == brandId); //&& q.OrderType == (int)OrderTypeEnum.Delivery &&
                //(q.DeliveryAddress != null || (q.InvoiceID != null)));
            return result;
        }

        public async Task<VATOrder> GetVATOrderByIdAsync(int id)
        {
            var result = await this.GetAsync(id);
            return result;
        }

        #region SystemReport
        public IQueryable<VATOrder> GetAllVATOrderByDate(DateTime from, DateTime to)
        {
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to)));
            return result;
        }

        public IQueryable<VATOrder> GetAllVATOrderByDate(DateTime from, DateTime to, int brandId)
        {
            var result = this.Get(r => ((r.CheckInDate >= from
                               && r.CheckInDate <= to) && r.BrandID == brandId));
            return result;
        }

        public IQueryable<VATOrder> GetRentsByTimeRange(DateTime startTime, DateTime endTime)
        {
            var result = this.Get(r =>(r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime));
            return result;
        }

        public IQueryable<VATOrder> GetRentsByTimeRange(DateTime startTime, DateTime endTime, int brandId)
        {
            var result = this.Get(r => ((r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime)) && r.BrandID == brandId);
            return result;
        }
        public IQueryable<VATOrder> GetRentsByTime(DateTime startTime, DateTime endTime)
        {
            /*var orderDetails = mappingService.GetOrderDetailsByTimeRange(startTime, endTime, 13)
                .Select(q => q.RentID)
                .Distinct();
            return this.Get(q => q.CheckInDate >= startTime && q.CheckInDate <= endTime && orderDetails.Contains(q.RentID));
            */
            return null;
        }
        #endregion

        public IEnumerable<VATOrder> GetVATOrders()
        {
            return this.GetActive();
        }
    }
}

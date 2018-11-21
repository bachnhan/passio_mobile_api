using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class VATOrderApi
    {
        public PagingViewModel<VATOrderViewModel> GetAdminWithFilterAsync(string keyword,
            int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var rents = this.BaseService.GetAdminWithFilter(keyword, sortKeyAsc)
                .ProjectTo<VATOrderViewModel>(this.AutoMapperConfig);
            var pagedList = rents
                 .Page(currPage, pageSize);

            return new PagingViewModel<VATOrderViewModel>(pagedList);
        }

        public IQueryable<VATOrder> GetVATOrderByBrand(int brandId)
        {
            var result = this.BaseService.GetVATOrderByBrand(brandId);
                //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return result;
        }

        public async Task<bool> EditOrderAsync(VATOrderViewModel model)
        {
            try
            {
                var entity = (await this.BaseService.GetAsync(model.InvoiceID));
                /*entity.DeliveryStatus = model.DeliveryStatus;
                entity.OrderStatus = model.OrderStatus;
                entity.OrderType = model.OrderType;
                entity.StoreID = model.StoreID;*/
                await this.BaseService.UpdateAsync(entity);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<VATOrderViewModel> GetVATOrderByIdAsync(int id)
        {
            var result = new VATOrderViewModel(await this.BaseService.GetVATOrderByIdAsync(id));
            return result;
        }

        /*public void Create(OrderCustomEntityViewModel model)
        {

            //model = Utils.ToExactType<RentDetailsViewModel, RentDetailsViewModel>(model);

            var entity = model.ToEntity();
            var rent = entity.Order;
            rent.OrderDetails = entity.OrderDetails.ToList();
            this.BaseService.CreateAsync(rent);
            model.Order = new OrderViewModel(rent);
            model.OrderDetails = rent.OrderDetails.AsQueryable()
                .ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig);
        }

        public int CreateOrderDelivery(OrderCustomEntityViewModel model)
        {
            var entity = model.ToEntity();
            var rent = entity.Order;
            rent.OrderDetails = entity.OrderDetails.ToList();
            this.BaseService.Create(rent);
            return rent.RentID;
        }*/

        public async Task<bool> CreateVATOrderAsync(VATOrderViewModel model)
        {
            return await this.BaseService.CreateVATOrderAsync(model.ToEntity());
        }

        /*public OrderCustomEntityViewModel GetOrderById(int storeId, int id)
        {
            var order = this.BaseService.GetOrderById(storeId, id);
            if (order == null)
            {
                return null;
            }
            var orderDetail = order.Order.OrderDetails.AsQueryable().ProjectTo<OrderDetailViewModel>(this.AutoMapperConfig);
            var result = new OrderCustomEntityViewModel(order);
            result.OrderDetails = orderDetail;
            return result;
        }*/

        public VATOrderViewModel GetVATOrderByIdAndBrandId(int brandId, int id)
        {
            var VATorder = this.BaseService.GetVATOrderByIdAndBrandId(brandId, id);
            var result = new VATOrderViewModel(VATorder);
            return result;
        }

        public VATOrder GetVATOrderByInvoiceId(int invoiceId)
        {
            return this.BaseService.Get(a => a.InvoiceID == invoiceId).FirstOrDefault();
        }

        public bool CreateVATOrder(VATOrder VATorder)
        {
            return this.BaseService.CreateVATOrder(VATorder);
        }

        public async Task CreateVATOrder(VATOrderEditViewModel model)
        {
            var entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);
        }

        public void EditVATOrder(VATOrder VATorder)
        {
            this.BaseService.Update(VATorder);
        }

        #region SystemReport
        public IEnumerable<VATOrderViewModel> GetAllRentByDate(DateTime from, DateTime to)
        {
            var allRent = this.BaseService.GetAllVATOrderByDate(from, to)
                .ProjectTo<VATOrderViewModel>(this.AutoMapperConfig)
                .ToList();
            return allRent;
        }

        public IEnumerable<VATOrder> GetAllVATOrdersByDate(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllVATOrderByDate(from, to, brandId)
                .ToList();
            return allRent;
        }

        public IQueryable<VATOrder> GetAllVATOrderByDate(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllVATOrderByDate(from, to, brandId);
                //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
            return allRent;
        }

        public IQueryable<VATOrder> GetAllVATOrderByDate2(DateTime from, DateTime to, int brandId)
        {
            var allRent = this.BaseService.GetAllVATOrderByDate(from, to, brandId);
            return allRent;
        }

        public IQueryable<VATOrderViewModel> GetRentsByTimeRange(DateTime startTime, DateTime endTime, int brandId)
        {
            var rents = this.BaseService.GetRentsByTimeRange(startTime, endTime, brandId)
                .ProjectTo<VATOrderViewModel>(this.AutoMapperConfig);
            return rents;
        }

        public IEnumerable<VATOrder> GetVATOrdersByTimeRange(DateTime startTime, DateTime endTime, int brandId)
        {
            var rents = this.BaseService.GetRentsByTimeRange(startTime, endTime, brandId)
                .ToList();
            return rents;
        }
        public IQueryable<VATOrder> GetRentsByTimeRange(DateTime startTime, DateTime endTime)
        {
            var rents = this.BaseService.GetRentsByTimeRange(startTime, endTime);
                //.ProjectTo<OrderViewModel>(this.AutoMapperConfig);
                //.ToList();
            return rents;
        }
        public IEnumerable<VATOrder> GetRentsByTimeRange2(DateTime startTime, DateTime endTime)
        {
            var rents = this.BaseService.GetRentsByTimeRange(startTime, endTime);
            return rents;
        }

        public IEnumerable<VATOrder> GetRentsByTime(DateTime startTime, DateTime endTime)
        {
            var rents = this.BaseService.GetRentsByTime(startTime, endTime)
                // .ProjectTo<OrderViewModel>(this.AutoMapperConfig)
                .ToList();
            return rents;
        }
        #endregion

        #region VPOS
        public IQueryable<VATOrder> getAllVATOrder()
        {
            var listOrder = this.BaseService.Get();//.Where(c => c.RentStatus != (int)RentStatusEnum.DeletePermanent
                //&& c.RentStatus != (int)RentStatusEnum.Disabled
                //&& c.RentStatus != (int)RentStatusEnum.WaitDisabled);
            return listOrder;
        }

        public IQueryable<VATOrder> getAll()
        {
            var listOrder = this.BaseService.Get();
            return listOrder;
        }
        #endregion
    }
}

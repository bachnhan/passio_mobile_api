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
    public partial class CouponApi
    {
        public IEnumerable<Coupon> GetAllCoupons(int id)
        {
            var Coupons = this.BaseService.GetAllCoupons(id)
                .ToList();
            return Coupons;
        }

        public async System.Threading.Tasks.Task CreateCouponAsync(CouponEditViewModel model)
        {
            var storeApi = new StoreApi();
            //model.CouponViewModel.StoreName = (await storeApi.GetAsync(model.CouponViewModel.StoreId)).Name;
            model.CouponViewModel.IsActive = true;
            //model.Status chưa biết business
            await this.BaseService.CreateAsync(model.CouponViewModel.ToEntity());
        }

        public async System.Threading.Tasks.Task EditCouponAsync(CouponEditViewModel model)
        {
            var storeApi = new StoreApi();
            //model.CouponViewModel.StoreName = (await storeApi.GetAsync(model.CouponViewModel.StoreId)).Name;
            var Coupon = await this.BaseService.GetAsync(model.CouponViewModel.Id);
            //Asign value
            Coupon.StoreId = model.CouponViewModel.StoreId;
            Coupon.SerialNumber = model.CouponViewModel.SerialNumber;
            Coupon.DateUse = model.CouponViewModel.DateUse;
            Coupon.ImageUrl = model.CouponViewModel.ImageUrl;
            //Coupon.StoreName = model.CouponViewModel.StoreName;
            //Coupon Coupon = new Coupon()
            //{
            //    CampaginId = model.CampaginId,
            //    DateUse = model.DateUse,
            //    Id = model.Id,
            //    SerialNumber = model.SerialNumber,
            //    IsActive = model.IsActive,
            //    Status = model.Status,
            //    StoreId = model.StoreId,
            //    StoreName = model.StoreName,
            //};
            await this.BaseService.UpdateAsync(Coupon);
        }

        public async System.Threading.Tasks.Task<CouponEditViewModel> GetCouponAsync(int id)
        {
            var Coupon = (await this.BaseService.GetAsync(id))?.ToViewModel<Coupon, CouponViewModel>();
            var CouponEditViewModel = new CouponEditViewModel()
            {
                CouponViewModel = Coupon,
            };
            return CouponEditViewModel;
        }
    }
}

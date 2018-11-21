using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CouponProviderApi
    {
        public IEnumerable<CouponProviderViewModel> GetAllCouponProviders()
        {
            var CouponProviders = this.BaseService.GetAllCouponProviders()
                .ProjectTo<CouponProviderViewModel>(this.AutoMapperConfig)
                .ToList();
            return CouponProviders;
        }

        public CouponProviderViewModel GetCouponProviderById(int id)
        {
            //return this.BaseService.Get(id);
            //return this.BaseService.GetCouponProviderById(id);
            var CouponProvider =  this.BaseService.GetCouponProviderById(id);
            if (CouponProvider == null)
            {
                return null;
            }
            else
            {
                return new CouponProviderViewModel(CouponProvider);
            }
        }



        public async Task<CouponProviderViewModel> GetCouponProviderByNameActive(string name)
        {
            var CouponProvider = await this.BaseService.GetCouponProviderByNameActive(name);
            if(CouponProvider == null)
            {
                return null;
            }
            else
            {
                return new CouponProviderViewModel(CouponProvider);
            }
        }
    }
}

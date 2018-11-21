using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICouponService
    {
        IQueryable<Coupon> GetAllCoupons(int id);
        void EditCouponAsync(Coupon entity);
    }
    public partial class CouponService
    {
        public IQueryable<Coupon> GetAllCoupons(int id)
        {
            return this.Get(q => q.CampaginId == id && q.IsActive);
        }

        public void EditCouponAsync(Coupon entity)
        {
            this.repository.Edit(entity);
            this.repository.Save();
        }
    }
}

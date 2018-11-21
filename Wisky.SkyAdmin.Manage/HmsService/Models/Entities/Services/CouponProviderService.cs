using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICouponProviderService
    {
        IQueryable<CouponProvider> GetAllCouponProviders();
        Task<CouponProvider> GetCouponProviderByNameActive(string name);
        CouponProvider GetCouponProviderById(int id);
        
    }
    public partial class CouponProviderService
    {
        public IQueryable<CouponProvider> GetAllCouponProviders()
        {
            return this.Get();
        }
        public async Task<CouponProvider> GetCouponProviderByNameActive(string name)
        {
            return await this.FirstOrDefaultAsync(q => q.ProviderName == name && q.IsActive == true);
        }
        public CouponProvider GetCouponProviderById(int id)
        {
            return this.FirstOrDefault(q => q.Id == id);
        }
    }
}

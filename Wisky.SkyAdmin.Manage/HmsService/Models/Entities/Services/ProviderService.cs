using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IProviderService
    {
        #region ProductInventory
        IQueryable<Provider> GetProviders();
        IQueryable<Provider> GetProvidersByBrand(int brandId);
        #endregion
        #region Provider
        Task<Provider> GetProviderById(int providerId);
        string GetProviderNameByID(int id);
        #endregion
    }
    public partial class ProviderService
    {
        #region ProductInventory
        public IQueryable<Provider> GetProviders()
        {
            return this.Get(q => q.IsAvailable == true);
        }
        #endregion
        #region Provider
        public async Task<Provider> GetProviderById(int providerId)
        {
            return await this.GetAsync(providerId);
        }
        public string GetProviderNameByID(int id)
        {
            var rs = this.Get(a => a.Id == id).FirstOrDefault();
            return rs.ProviderName;
        }
        public IQueryable<Provider> GetProvidersByBrand(int brandId)
        {
            var result = this.Get(q => q.BrandId == brandId && (q.IsAvailable.HasValue? q.IsAvailable.Value : true));
            return result;
        }
        #endregion
    }
}

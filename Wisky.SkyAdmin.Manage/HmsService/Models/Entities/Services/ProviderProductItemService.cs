using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IProviderProductItemMappingService
    {
        #region Provider
        IQueryable<ProviderProductItemMapping> GetProviderProductItems();
        Task<ProviderProductItemMapping> GetProviderProductItemById(int ProviderId, int ItemId);
        IQueryable<ProviderProductItemMapping> GetProviderProductItemByItemId(int ItemId);
        #endregion
    }
    public partial class ProviderProductItemMappingService
    {
        #region Provider
        public IQueryable<ProviderProductItemMapping> GetProviderProductItems()
        {
            return this.Get(q => q.Active);
        }
        public async Task<ProviderProductItemMapping> GetProviderProductItemById(int ProviderId, int ItemId)
        {
            return await this.FirstOrDefaultAsync(q => q.ProviderID == ProviderId && q.ProductItemID == ItemId && q.Active);
        }
        public IQueryable<ProviderProductItemMapping> GetProviderProductItemByItemId(int ItemId)
        {
            return this.Get(q => q.ProductItemID == ItemId && q.Active == true);
        }
        #endregion
    }
}

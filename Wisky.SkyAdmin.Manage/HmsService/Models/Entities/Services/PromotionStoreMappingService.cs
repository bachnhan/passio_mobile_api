using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyWeb.DatVM.Data;

namespace HmsService.Models.Entities.Services
{

    public partial interface IPromotionStoreMappingService
    {
        IQueryable<PromotionStoreMapping> GetActivePromotionStoreMappingByPromotionID(int promotionId);
        IQueryable<PromotionStoreMapping> GetAllPromotionStoreMappingByPromotionID(int promotionId);
        IQueryable<int> GetAllActivePromotionIDByStoreID(int storeId);

    }
    public partial class PromotionStoreMappingService
    {
        public IQueryable<PromotionStoreMapping> GetActivePromotionStoreMappingByPromotionID(int promotionId)
        {
            return this.GetActive(q => q.PromotionId == promotionId);
        }
       public IQueryable<PromotionStoreMapping> GetAllPromotionStoreMappingByPromotionID(int promotionId)
        {
            return this.Get().Where(q => q.PromotionId == promotionId);
        }
        public IQueryable<int> GetAllActivePromotionIDByStoreID(int storeId)
        {
            return this.GetActive().Where(q => q.StoreId == storeId).Select(q => q.PromotionId);
        }

    }
}

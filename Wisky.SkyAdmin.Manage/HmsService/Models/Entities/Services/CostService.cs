using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICostService
    {
        #region CostManage
        IQueryable<Cost> GetCostByRangeTimeStoreId(DateTime startDate, DateTime endDate, int storeId);
        IQueryable<Cost> GetCostByRangeTimeStoreBrand(DateTime startDate, DateTime endDate, int storeId, int brandId);
        IQueryable<Cost> GetCostByRangeTimeAndCostType(DateTime startDate, DateTime endDate, int storeId, int status);
        IQueryable<Cost> GetCostByRangeTimeBrandIdAndCostType(DateTime startDate, DateTime endDate, int brandId, int status);
        IQueryable<Cost> GetCostbyCostCategoryandBrand(int brandId, int categoryId);
        #endregion

        IEnumerable<Cost> GetCosts();
    }
    public partial class CostService
    {
        #region CostManage
        public IQueryable<Cost> GetCostByRangeTimeStoreId(DateTime startDate, DateTime endDate, int storeId)
        {
            return this.Get(q => q.CostDate >= startDate && q.CostDate <= endDate && q.StoreId == storeId);
        }
        public IQueryable<Cost> GetCostByRangeTimeAndCostType(DateTime startDate, DateTime endDate, int storeId, int costType)
        {
            return this.Get(q => q.CostDate >= startDate && q.CostDate <= endDate && q.StoreId == storeId && q.CostType == costType);
        }
        public IQueryable<Cost> GetCostByRangeTimeBrandIdAndCostType(DateTime startDate, DateTime endDate, int brandId, int costType)
        {
            if (costType != 3) { 
            return this.Get(q => q.CostDate >= startDate && q.CostDate <= endDate && q.Store.BrandId == brandId && q.CostType == costType);
            }
            else
            {
                return this.Get(q => q.CostDate >= startDate && q.CostDate <= endDate && q.Store.BrandId == brandId);
            }
        }

        public IQueryable<Cost> GetCostbyCostCategoryandBrand(int brandId, int categoryId)
        {
                return this.Get(q => q.Store.BrandId == brandId && q.CostCategory.CatID == categoryId);
        }
        #endregion

        public IEnumerable<Cost> GetCosts()
        {
            var cost = this.GetActive();
            return cost;
        }


        public IQueryable<Cost> GetCostByRangeTimeStoreBrand(DateTime startDate, DateTime endDate, int storeId,int brandId)
        {
            if (storeId != 0)
            {
                return this.Get(q => q.CostDate >= startDate && q.CostDate <= endDate && q.StoreId == storeId);
            }
            else
            {
                return this.Get(q => q.CostDate >= startDate && q.CostDate <= endDate && q.Store.BrandId == brandId);
            }
           
        }
    }
}

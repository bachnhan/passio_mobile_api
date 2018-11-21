using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{
    public partial class CostApi
    {
        #region CostManage
        public IQueryable<Cost> GetCostByRangeTimeStoreId(DateTime startDate, DateTime endDate, int storeId)
        {
            var cost = this.BaseService.GetCostByRangeTimeStoreId(startDate, endDate, storeId);
            return cost;
        }
        public IQueryable<Cost> GetCostByRangeTimeAndCostType(DateTime startDate, DateTime endDate, int storeId, int costType)
        {
            var costCategories = this.BaseService.GetCostByRangeTimeAndCostType(startDate, endDate, storeId, costType);
            return costCategories;
        }

        public IQueryable<Cost> GetCostByRangeTimeBrandIdAndCostType(DateTime startDate, DateTime endDate, int brandId, int costType)
        {
            var costCategories = this.BaseService.GetCostByRangeTimeBrandIdAndCostType(startDate, endDate, brandId, costType);
            return costCategories;
        }

        public IQueryable<Cost> GetCostbyCostCategoryandBrand(int brandId, int categoryId)
        {
            return this.BaseService.GetCostbyCostCategoryandBrand(brandId, categoryId);
        }

        public IQueryable<Cost> getAllCost()
        {
            var listCost = this.BaseService.Get();
            return listCost;
        }

        #endregion

        public int CreateCost(CostViewModel cost)
        {
            var entity = cost.ToEntity();
            this.BaseService.Create(entity);
            this.BaseService.Save();
            cost.CopyFromEntity(entity);
            //this.BaseService.Create(cost.ToEntity());
            return cost.CostID;
        }

        public IQueryable<Cost> GetCostById(int id)
        {
            var listCost = this.BaseService.Get(q=>q.CostID == id);
            return listCost;
        }
        public IQueryable<Cost> GetCostByRangeTimeStoreBrand(DateTime startDate, DateTime endDate, int storeId, int brandId)
        {
            return this.BaseService.GetCostByRangeTimeStoreBrand(startDate, endDate, storeId, brandId);
        }
    }
}

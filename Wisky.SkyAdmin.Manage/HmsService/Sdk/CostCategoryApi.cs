using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;

namespace HmsService.Sdk
{
    public partial class CostCategoryApi
    {
        #region CostManage
        public IEnumerable<CostCategoryViewModel> GetCostCategories()
        {
            var costCategories = this.BaseService.GetCostCategories()
                .ProjectTo<CostCategoryViewModel>(this.AutoMapperConfig)
                .ToList();
            return costCategories;
        }
        public async Task<CostCategoryViewModel> GetCostCategoryById(int costCategoryId)
        {
            var costCategory = await this.BaseService.GetCostCategoryById(costCategoryId);
            if (costCategory == null)
            {
                return null;
            }
            else
            {
                return new CostCategoryViewModel(costCategory);
            }
        }

        public async System.Threading.Tasks.Task UpdateCostManageAsync(CostCategoryViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.CatID);
            entity.CatID = model.CatID;
            entity.CatName = model.CatName;
            entity.Type = model.Type;
            await this.BaseService.UpdateAsync(entity);
        }

        #endregion
        public IEnumerable<CostCategoryViewModel> GetActiveCostCategoriesByBrandId(int brandId, int type)
        {
            var costCategories = this.BaseService.Get().Where(q => q.Active == true && q.BrandId == brandId && q.Type == type)
                .ProjectTo<CostCategoryViewModel>(this.AutoMapperConfig)
                .ToList();
            return costCategories;
        }
    }
}

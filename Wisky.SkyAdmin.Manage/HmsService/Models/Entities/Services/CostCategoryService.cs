using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICostCategoryService
    {
        #region CostManage
        IQueryable<CostCategory> GetCostCategories();
        Task<CostCategory> GetCostCategoryById(int costCategoryId);
        #endregion
    }
    public partial class CostCategoryService
    {
        #region CostManage
        public IQueryable<CostCategory> GetCostCategories()
        {
            return this.Get();
        }
        public async Task<CostCategory> GetCostCategoryById(int costCategoryId)
        {
            return await this.GetAsync(costCategoryId);
        }
        #endregion
    }
}

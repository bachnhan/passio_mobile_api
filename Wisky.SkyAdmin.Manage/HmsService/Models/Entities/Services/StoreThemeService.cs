using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IStoreThemeService
    {
        IQueryable<StoreTheme> GetActiveStoreThemeByStoreId(int storeId);
        Task CreateStoreThemeAsync(StoreTheme entity);
        Task UpdateStoreThemeAsync(StoreTheme entity);
        Task DeactiveStoreThemeAsync(StoreTheme entity);
    }
    public partial class StoreThemeService
    {
        public IQueryable<StoreTheme> GetActiveStoreThemeByStoreId(int storeId)
        {
            try
            {
                var storeThemes = this.GetActive().Where(s => s.StoreId == storeId);
                return storeThemes;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task CreateStoreThemeAsync(StoreTheme entity)
        {
            await this.CreateAsync(entity);
        }

        public async Task UpdateStoreThemeAsync(StoreTheme entity)
        {
            await this.UpdateAsync(entity);
        }

        public async Task DeactiveStoreThemeAsync(StoreTheme entity)
        {
            entity.Active = false;
            await this.UpdateAsync(entity);
        }
    }
}

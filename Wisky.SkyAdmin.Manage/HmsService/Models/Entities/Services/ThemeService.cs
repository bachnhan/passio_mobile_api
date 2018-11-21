using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IThemeService
    {
        IQueryable<Theme> GetAllActiveThemes();
        Task CreateThemeAsync(Theme entity);
    }
    public partial class ThemeService
    {
        public IQueryable<Theme> GetAllActiveThemes()
        {
            return this.GetActive();
        }
        public async Task CreateThemeAsync(Theme entity)
        {
            await this.CreateAsync(entity);
        }
    }
}

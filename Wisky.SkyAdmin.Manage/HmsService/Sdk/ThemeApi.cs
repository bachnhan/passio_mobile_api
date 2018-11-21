using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class ThemeApi
    {
        public IQueryable<ThemeViewModel> GetActiveThemes()
        {
            return this.BaseService.GetActive().ProjectTo<ThemeViewModel>(this.AutoMapperConfig);
        }

        public async Task CreateThemeAsync(ThemeViewModel model)
        {
            await this.BaseService.CreateThemeAsync(model.ToEntity());
        }
    }
}

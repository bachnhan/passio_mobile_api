//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HmsService.Models.Entities.Services
{
    using System;
    using System.Collections.Generic;
    
    
    public partial interface IThemeService : SkyWeb.DatVM.Data.IBaseService<Theme>
    {
    }
    
    public partial class ThemeService : SkyWeb.DatVM.Data.BaseService<Theme>, IThemeService
    {
        public ThemeService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IThemeRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

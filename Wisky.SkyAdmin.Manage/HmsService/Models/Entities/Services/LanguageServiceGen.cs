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
    
    
    public partial interface ILanguageService : SkyWeb.DatVM.Data.IBaseService<Language>
    {
    }
    
    public partial class LanguageService : SkyWeb.DatVM.Data.BaseService<Language>, ILanguageService
    {
        public LanguageService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.ILanguageRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

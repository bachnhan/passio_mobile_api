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
    
    
    public partial interface IProviderService : SkyWeb.DatVM.Data.IBaseService<Provider>
    {
    }
    
    public partial class ProviderService : SkyWeb.DatVM.Data.BaseService<Provider>, IProviderService
    {
        public ProviderService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IProviderRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

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
    
    
    public partial interface IApplicationService : SkyWeb.DatVM.Data.IBaseService<Application>
    {
    }
    
    public partial class ApplicationService : SkyWeb.DatVM.Data.BaseService<Application>, IApplicationService
    {
        public ApplicationService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IApplicationRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}
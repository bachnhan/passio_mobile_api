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
    
    
    public partial interface IPriceAdditionService : SkyWeb.DatVM.Data.IBaseService<PriceAddition>
    {
    }
    
    public partial class PriceAdditionService : SkyWeb.DatVM.Data.BaseService<PriceAddition>, IPriceAdditionService
    {
        public PriceAdditionService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IPriceAdditionRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

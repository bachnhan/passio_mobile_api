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
    
    
    public partial interface ICostInventoryMappingService : SkyWeb.DatVM.Data.IBaseService<CostInventoryMapping>
    {
    }
    
    public partial class CostInventoryMappingService : SkyWeb.DatVM.Data.BaseService<CostInventoryMapping>, ICostInventoryMappingService
    {
        public CostInventoryMappingService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.ICostInventoryMappingRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

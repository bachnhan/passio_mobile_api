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
    
    
    public partial interface IInventoryCheckingService : SkyWeb.DatVM.Data.IBaseService<InventoryChecking>
    {
    }
    
    public partial class InventoryCheckingService : SkyWeb.DatVM.Data.BaseService<InventoryChecking>, IInventoryCheckingService
    {
        public InventoryCheckingService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IInventoryCheckingRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

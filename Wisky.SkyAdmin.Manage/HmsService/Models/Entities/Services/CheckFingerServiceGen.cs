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
    
    
    public partial interface ICheckFingerService : SkyWeb.DatVM.Data.IBaseService<CheckFinger>
    {
    }
    
    public partial class CheckFingerService : SkyWeb.DatVM.Data.BaseService<CheckFinger>, ICheckFingerService
    {
        public CheckFingerService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.ICheckFingerRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

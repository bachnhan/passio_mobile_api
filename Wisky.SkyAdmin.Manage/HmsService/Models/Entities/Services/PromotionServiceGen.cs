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
    
    
    public partial interface IPromotionService : SkyWeb.DatVM.Data.IBaseService<Promotion>
    {
    }
    
    public partial class PromotionService : SkyWeb.DatVM.Data.BaseService<Promotion>, IPromotionService
    {
        public PromotionService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IPromotionRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

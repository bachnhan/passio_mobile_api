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
    
    
    public partial interface ICostCategoryService : SkyWeb.DatVM.Data.IBaseService<CostCategory>
    {
    }
    
    public partial class CostCategoryService : SkyWeb.DatVM.Data.BaseService<CostCategory>, ICostCategoryService
    {
        public CostCategoryService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.ICostCategoryRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

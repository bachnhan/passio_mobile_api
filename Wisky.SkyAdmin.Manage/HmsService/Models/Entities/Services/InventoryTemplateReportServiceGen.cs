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
    
    
    public partial interface IInventoryTemplateReportService : SkyWeb.DatVM.Data.IBaseService<InventoryTemplateReport>
    {
    }
    
    public partial class InventoryTemplateReportService : SkyWeb.DatVM.Data.BaseService<InventoryTemplateReport>, IInventoryTemplateReportService
    {
        public InventoryTemplateReportService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IInventoryTemplateReportRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}
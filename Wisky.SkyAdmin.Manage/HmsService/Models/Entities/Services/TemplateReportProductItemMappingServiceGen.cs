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
    
    
    public partial interface ITemplateReportProductItemMappingService : SkyWeb.DatVM.Data.IBaseService<TemplateReportProductItemMapping>
    {
    }
    
    public partial class TemplateReportProductItemMappingService : SkyWeb.DatVM.Data.BaseService<TemplateReportProductItemMapping>, ITemplateReportProductItemMappingService
    {
        public TemplateReportProductItemMappingService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.ITemplateReportProductItemMappingRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

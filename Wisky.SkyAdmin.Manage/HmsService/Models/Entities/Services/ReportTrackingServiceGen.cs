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
    
    
    public partial interface IReportTrackingService : SkyWeb.DatVM.Data.IBaseService<ReportTracking>
    {
    }
    
    public partial class ReportTrackingService : SkyWeb.DatVM.Data.BaseService<ReportTracking>, IReportTrackingService
    {
        public ReportTrackingService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IReportTrackingRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}
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
    
    
    public partial interface IDateHotelReportService : SkyWeb.DatVM.Data.IBaseService<DateHotelReport>
    {
    }
    
    public partial class DateHotelReportService : SkyWeb.DatVM.Data.BaseService<DateHotelReport>, IDateHotelReportService
    {
        public DateHotelReportService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IDateHotelReportRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

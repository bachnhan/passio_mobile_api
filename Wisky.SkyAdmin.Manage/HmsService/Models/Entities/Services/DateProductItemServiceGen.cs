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
    
    
    public partial interface IDateProductItemService : SkyWeb.DatVM.Data.IBaseService<DateProductItem>
    {
    }
    
    public partial class DateProductItemService : SkyWeb.DatVM.Data.BaseService<DateProductItem>, IDateProductItemService
    {
        public DateProductItemService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IDateProductItemRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

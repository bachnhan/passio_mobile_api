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
    
    
    public partial interface ICustomerTypeService : SkyWeb.DatVM.Data.IBaseService<CustomerType>
    {
    }
    
    public partial class CustomerTypeService : SkyWeb.DatVM.Data.BaseService<CustomerType>, ICustomerTypeService
    {
        public CustomerTypeService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.ICustomerTypeRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

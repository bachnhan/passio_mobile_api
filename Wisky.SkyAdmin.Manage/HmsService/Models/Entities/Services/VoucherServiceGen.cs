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
    
    
    public partial interface IVoucherService : SkyWeb.DatVM.Data.IBaseService<Voucher>
    {
    }
    
    public partial class VoucherService : SkyWeb.DatVM.Data.BaseService<Voucher>, IVoucherService
    {
        public VoucherService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IVoucherRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

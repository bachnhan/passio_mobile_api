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
    
    
    public partial interface IProductCollectionService : SkyWeb.DatVM.Data.IBaseService<ProductCollection>
    {
    }
    
    public partial class ProductCollectionService : SkyWeb.DatVM.Data.BaseService<ProductCollection>, IProductCollectionService
    {
        public ProductCollectionService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IProductCollectionRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

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
    
    
    public partial interface IProductImageCollectionItemMappingService : SkyWeb.DatVM.Data.IBaseService<ProductImageCollectionItemMapping>
    {
    }
    
    public partial class ProductImageCollectionItemMappingService : SkyWeb.DatVM.Data.BaseService<ProductImageCollectionItemMapping>, IProductImageCollectionItemMappingService
    {
        public ProductImageCollectionItemMappingService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IProductImageCollectionItemMappingRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HmsService.Models.Entities.Repositories
{
    using System;
    using System.Collections.Generic;
    
    
    public partial interface IProductItemRepository : SkyWeb.DatVM.Data.IBaseRepository<ProductItem>
    {
    }
    
    public partial class ProductItemRepository : SkyWeb.DatVM.Data.BaseRepository<ProductItem>, IProductItemRepository
    {
    	public ProductItemRepository(System.Data.Entity.DbContext dbContext) : base(dbContext)
    	{
    	}
    }
}

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
    
    
    public partial interface IProviderProductItemMappingRepository : SkyWeb.DatVM.Data.IBaseRepository<ProviderProductItemMapping>
    {
    }
    
    public partial class ProviderProductItemMappingRepository : SkyWeb.DatVM.Data.BaseRepository<ProviderProductItemMapping>, IProviderProductItemMappingRepository
    {
    	public ProviderProductItemMappingRepository(System.Data.Entity.DbContext dbContext) : base(dbContext)
    	{
    	}
    }
}

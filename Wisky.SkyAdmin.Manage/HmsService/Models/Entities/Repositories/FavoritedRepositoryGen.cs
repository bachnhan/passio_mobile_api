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
    
    
    public partial interface IFavoritedRepository : SkyWeb.DatVM.Data.IBaseRepository<Favorited>
    {
    }
    
    public partial class FavoritedRepository : SkyWeb.DatVM.Data.BaseRepository<Favorited>, IFavoritedRepository
    {
    	public FavoritedRepository(System.Data.Entity.DbContext dbContext) : base(dbContext)
    	{
    	}
    }
}

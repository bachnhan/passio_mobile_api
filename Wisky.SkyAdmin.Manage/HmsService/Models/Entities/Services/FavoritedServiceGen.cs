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
    
    
    public partial interface IFavoritedService : SkyWeb.DatVM.Data.IBaseService<Favorited>
    {
    }
    
    public partial class FavoritedService : SkyWeb.DatVM.Data.BaseService<Favorited>, IFavoritedService
    {
        public FavoritedService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IFavoritedRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

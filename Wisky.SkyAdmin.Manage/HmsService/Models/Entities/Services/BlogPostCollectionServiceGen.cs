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
    
    
    public partial interface IBlogPostCollectionService : SkyWeb.DatVM.Data.IBaseService<BlogPostCollection>
    {
    }
    
    public partial class BlogPostCollectionService : SkyWeb.DatVM.Data.BaseService<BlogPostCollection>, IBlogPostCollectionService
    {
        public BlogPostCollectionService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IBlogPostCollectionRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

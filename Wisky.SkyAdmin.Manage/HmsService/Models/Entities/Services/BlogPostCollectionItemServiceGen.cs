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
    
    
    public partial interface IBlogPostCollectionItemService : SkyWeb.DatVM.Data.IBaseService<BlogPostCollectionItem>
    {
    }
    
    public partial class BlogPostCollectionItemService : SkyWeb.DatVM.Data.BaseService<BlogPostCollectionItem>, IBlogPostCollectionItemService
    {
        public BlogPostCollectionItemService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IBlogPostCollectionItemRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

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
    
    
    public partial interface IBlogPostImageService : SkyWeb.DatVM.Data.IBaseService<BlogPostImage>
    {
    }
    
    public partial class BlogPostImageService : SkyWeb.DatVM.Data.BaseService<BlogPostImage>, IBlogPostImageService
    {
        public BlogPostImageService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IBlogPostImageRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

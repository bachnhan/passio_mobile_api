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
    
    
    public partial interface IBlogCategoryService : SkyWeb.DatVM.Data.IBaseService<BlogCategory>
    {
    }
    
    public partial class BlogCategoryService : SkyWeb.DatVM.Data.BaseService<BlogCategory>, IBlogCategoryService
    {
        public BlogCategoryService(SkyWeb.DatVM.Data.IUnitOfWork unitOfWork, Repositories.IBlogCategoryRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}

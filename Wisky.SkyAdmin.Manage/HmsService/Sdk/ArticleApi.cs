using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class ArticleApi
    {


        public IQueryable<ArticleViewModel> GetArticles()
        {
            return this.BaseService.Get().ProjectTo<ArticleViewModel>(this.AutoMapperConfig);
        }

    }   
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IArticleService
    {


        IQueryable<Article> GetArticleName(string name);

    }

    public partial class ArticleService
    {
        public IQueryable<Article> GetArticleName(string name)
        {
            return this.Get(q => !q.IsAvailable).Where(q => q.Name == name);
        }

        
        


    }
}

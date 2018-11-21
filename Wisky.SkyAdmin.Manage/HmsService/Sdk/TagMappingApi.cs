using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class TagMappingApi
    {
        public IQueryable<TagMapping> GetAllMappingByBlogId(int blogId)
        {
            return this.BaseService.Get(q => q.TagBlogId == blogId);
        }
        public IQueryable<TagMapping> GetAllMappingByBlogCateId(int blogCateId)
        {
            return this.BaseService.Get(q => q.CategoryId == blogCateId);
        }
    }
}

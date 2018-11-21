using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CategoryExtraMappingApi
    {
        public List<ProductCategoryViewModel> GetProductCategoryExtra(int primaryCategoryId)
        {
            var categoryExtra = this.BaseService.GetProductCategoryExtra(primaryCategoryId)
                .AsQueryable()
                .ProjectTo<ProductCategoryViewModel>(this.AutoMapperConfig)
                .ToList();
            return categoryExtra;
        }

        // this Api must be run after 
        /// <summary>
        /// get category extra mapping by primary categoryId 
        /// </summary>
        /// <param name="primaryCategoryId">primary categoryId </param>
        /// <returns></returns>
        public IQueryable<CategoryExtraMapping> GetByPrimaryCategoryId(int primaryCategoryId)
        {
            var categoryExtra = this.BaseService.Get().Where(q => q.PrimaryCategoryId == primaryCategoryId);
            return categoryExtra;
        }

        public void CreateCategoryExtra(int primaryCategoryId, int[] categoryExtraIds)
        {
            this.BaseService.CreateCategoryExtra(primaryCategoryId, categoryExtraIds);
        }

        public void EditCategoryExtra(int primaryCategoryId, int[] categoryExtraIds)
        {
            this.BaseService.EditCategoryExtra(primaryCategoryId, categoryExtraIds);
        }
    }
}

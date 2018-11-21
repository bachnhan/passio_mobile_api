using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICategoryExtraMappingService
    {
        List<ProductCategory> GetProductCategoryExtra(int primaryCategoryId);
        void CreateCategoryExtra(int primaryCategoryId, int[] categoryExtraIds);
        void EditCategoryExtra(int primaryCategoryId, int[] categoryExtraIds);
    }
    public partial class CategoryExtraMappingService
    {
        public List<ProductCategory> GetProductCategoryExtra(int primaryCategoryId)
        {
            var listProductCategoryExtra = new List<ProductCategory>();
            var productCategoryExtra = this.Get(a => a.PrimaryCategoryId == primaryCategoryId && a.IsEnable == true);
            foreach (var item in productCategoryExtra)
            {
                listProductCategoryExtra.Add(item.ProductCategory1);
            }
            return listProductCategoryExtra;
        }

        public void CreateCategoryExtra(int primaryCategoryId, int[] categoryExtraIds)
        {
            foreach (var categoryExtraId in categoryExtraIds)
            {
                var categoryExtra = new CategoryExtraMapping()
                {
                    PrimaryCategoryId = primaryCategoryId,
                    ExtraCategoryId = categoryExtraId,
                    IsEnable = true
                };

                this.Create(categoryExtra);
            }

            this.Save();
        }

        public void EditCategoryExtra(int primaryCategoryId, int[] categoryExtraIds)
        {
            try
            {
                var categoryExtras = this.Get(ce => ce.PrimaryCategoryId == primaryCategoryId && ce.IsEnable == true).ToList();

                if(categoryExtraIds == null && categoryExtras != null)
                {
                    foreach (var categoryExtra in categoryExtras)
                    {
                        categoryExtra.IsEnable = false;
                    }
                }
                else
                {
                    foreach (var categoryExtraId in categoryExtraIds)
                    {
                        var categoryExtra = categoryExtras.FirstOrDefault(ce => ce.ExtraCategoryId == categoryExtraId);

                        if (categoryExtra == null)
                        {
                            categoryExtra = new CategoryExtraMapping()
                            {
                                PrimaryCategoryId = primaryCategoryId,
                                ExtraCategoryId = categoryExtraId,
                                IsEnable = true
                            };

                            this.Create(categoryExtra);
                        }
                    }

                    foreach (var categoryExtra in categoryExtras)
                    {
                        if (!categoryExtraIds.Contains(categoryExtra.ExtraCategoryId))
                        {
                            categoryExtra.IsEnable = false;
                        }
                    }
                }               
                this.Save();
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(ex.ToErrorsString(), ex);
            }
        }
    }
}

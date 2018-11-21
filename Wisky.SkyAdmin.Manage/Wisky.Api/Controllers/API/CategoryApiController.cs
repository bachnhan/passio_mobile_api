using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
//using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.Api.Controllers.API
{
    public class CategoryApiController : ApiController
    {
        [HttpGet]
        [Route("api/category/GetCategoryList/{token}/{terminalId}")]
        //ex: categoryApi/GetCategoryList?token= &terminalId=
        public HttpResponseMessage GetCategoryList(string token, int terminalId)
        {
            Utils.CheckToken(token);
            var productCategoryApi = new ProductCategoryApi();
            var cateExtraApi = new CategoryExtraMappingApi();
            var storeApi = new StoreApi();

            var storeService = this.Service<IStoreService>();
            var cateList = new List<ProductCategoryApiViewModel>();
            var cateExtraList = new List<CategoryExtraMappingApiViewModel>();
            

            try
            {
                var store = storeApi.GetStoreByIdSync(terminalId);
                var categories = productCategoryApi.GetProductCategoriesByBrandId((int)store.BrandId).ToList();
                var cateExtras = cateExtraApi.Get().Where(q => q.IsEnable == true);
                foreach (var item in categories)
                {
                    cateExtraList.AddRange(item.CategoryExtraMappings.Select(a => new CategoryExtraMappingApiViewModel()
                    {
                        Id = a.Id,
                        PrimaryCategoryId = a.PrimaryCategoryId,
                        ExtraCategoryId = a.ExtraCategoryId,
                        IsEnable = a.IsEnable
                    }));

                    var category = new ProductCategoryApiViewModel()
                    {
                        Code = item.CateID,
                        Name = item.CateName,
                        Type = item.Type,
                        DisplayOrder = item.DisplayOrder,
                        IsExtra = item.IsExtra ? 1 : 0,
                        IsDisplayed = item.IsDisplayed,
                        IsUsed = true,
                        AdjustmentNote = item.AdjustmentNote,
                        ImageFontAwsomeCss = item.ImageFontAwsomeCss,
                        ParentCateId = item.ParentCateId
                    };
                    cateList.Add(category);
                }
                //cateList.AddRange(categories.Select(item => new ProductCategoryApiViewModel()
                //{
                //    Code = item.CateID,
                //    Name = item.CateName,
                //    Type = item.Type,
                //    DisplayOrder = item.DisplayOrder,
                //    IsExtra = item.IsExtra ? 1 : 0,
                //    IsDisplayed = item.IsDisplayed,
                //    IsUsed = true,
                //    AdjustmentNote = item.AdjustmentNote,
                //    ImageFontAwsomeCss = item.ImageFontAwsomeCss,
                //    ParentCateId = item.ParentCateId
                //}));

                //foreach (var item in cateExtras)
                //{
                //    cateExtraList.Add(item);
                //}
               
                var model = new ProductCategoryExtraMappingViewModel()
                {
                    ProductCategory = cateList,
                    CategoryExtra = cateExtraList,
                };
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(model)
                };

            }
            catch (Exception e)
            {
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        ErrorMessage = e.Message,
                    })
                };
            }
        }
    }
}
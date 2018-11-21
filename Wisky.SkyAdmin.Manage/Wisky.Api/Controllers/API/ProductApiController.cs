using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
//using System.Web.Mvc;

namespace Wisky.Api.Controllers.API
{
    public class ProductApiController : ApiController
    {
        // GET: ProductApi
        [HttpGet]
        [Route("api/product/GetProductList/{token}/{terminalId}")]
        public HttpResponseMessage GetProductList(string token, int terminalId)
        //public List<ProductApiViewModel> GetProductList(string token, int terminalId)
        {
            Utils.CheckToken(token);

            var productList = new List<ProductApiViewModel>();
            var productApi = new ProductApi();
            var storeApi = new StoreApi();
            var productDetailMappingApi = new ProductDetailMappingApi();
            var store = storeApi.GetStoreByIdSync(terminalId);
            var productDetailApi = new ProductDetailMappingApi();
            //var listProduct = productApi.GetProductByBrandId((int)store.BrandId).ToList();
            //var listP = new List<ProductDetailMapping>();
            var listP = productDetailMappingApi.GetProductByStoreID(terminalId,store.BrandId.Value);
            //var listP = productDetailMappingApi.GetProductByStore(terminalId);

            try
            {
                //productList.Version = "1";
                foreach (var item in listP)
                {
                    //var p = new ProductApiViewModel()
                    //{
                    //    ProductId = item.ProductID,
                    //    ProductName = item.ProductName,
                    //    ShortName = null,
                    //    Code = item.Code,
                    //    PicURL = item.PicURL,
                    //    Price = item.Price,
                    //    DiscountPercent = item.DiscountPercent,
                    //    DiscountPrice = item.DiscountPrice,
                    //    CatID = item.CatID,
                    //    ProductType = item.ProductType,
                    //    DisplayOrder = item.DisplayOrder,
                    //    IsMenuDisplay = true,
                    //    IsAvailable = item.IsAvailable,
                    //    PosX = (int)item.PosX.GetValueOrDefault(),
                    //    PosY = (int)item.PosY.GetValueOrDefault(),
                    //    ColorGroup = item.ColorGroup,
                    //    Group = (int)item.Group.GetValueOrDefault(),
                    //    GeneralProductId = item.GeneralProductId,
                    //    Att1 = item.Att1,
                    //    Att2 = item.Att2,
                    //    Att3 = item.Att3,
                    //    MaxExtra = 0,
                    //    IsUsed = true,
                    //    HasExtra = false,
                    //    IsFixedPrice = item.IsFixedPrice,
                    //    IsDefaultChildProduct = item.IsDefaultChildProduct == (int)SaleTypeEnum.DefaultNothing ? false : true,
                    //    IsMostOrder = item.IsMostOrdered,
                    //    Category = new ProductCategoryApiViewModel()
                    //    {
                    //        Code = item.CatID,
                    //        Name = item.ProductCategory.CateName,
                    //        Type = item.ProductCategory.Type,
                    //        DisplayOrder = item.ProductCategory.DisplayOrder,
                    //        IsExtra = item.ProductCategory.IsExtra ? 1 : 0,
                    //        IsDisplayed = item.ProductCategory.IsDisplayed,
                    //        IsUsed = true,
                    //        AdjustmentNote = item.ProductCategory.AdjustmentNote,
                    //        ImageFontAwsomeCss = item.ProductCategory.ImageFontAwsomeCss,
                    //        ParentCateId = item.ProductCategory.ParentCateId
                    //    }
                    //};
                    if (item.Product != null)
                    {
                        var p = new ProductApiViewModel()
                        {
                            ProductId = item.Product.ProductID,
                            ProductName = item.Product.ProductName,
                            ShortName = null,
                            Code = item.Product.Code,
                            PicURL = item.Product.PicURL,
                            Price = item.Product.Price,
                            DiscountPercent = item.Product.DiscountPercent,
                            DiscountPrice = item.Product.DiscountPrice,
                            CatID = item.Product.CatID,
                            ProductType = item.Product.ProductType,
                            DisplayOrder = item.Product.DisplayOrder,
                            IsMenuDisplay = true,
                            IsAvailable = item.Product.IsAvailable,
                            PosX = (int)item.Product.PosX.GetValueOrDefault(),
                            PosY = (int)item.Product.PosY.GetValueOrDefault(),
                            ColorGroup = item.Product.ColorGroup,
                            Group = (int)item.Product.Group.GetValueOrDefault(),
                            GeneralProductId = item.Product.GeneralProductId,
                            Att1 = item.Product.Att1,
                            Att2 = item.Product.Att2,
                            Att3 = item.Product.Att3,
                            MaxExtra = 0,
                            IsUsed = true,
                            HasExtra = false,
                            IsFixedPrice = item.Product.IsFixedPrice,
                            IsDefaultChildProduct = item.Product.IsDefaultChildProduct == (int)SaleTypeEnum.DefaultNothing ? false : true,
                            IsMostOrder = item.Product.IsMostOrdered,
                            Category = new ProductCategoryApiViewModel()
                            {
                                Code = item.Product.CatID,
                                Name = item.Product.ProductCategory.CateName,
                                Type = item.Product.ProductCategory.Type,
                                DisplayOrder = item.Product.ProductCategory.DisplayOrder,
                                IsExtra = item.Product.ProductCategory.IsExtra ? 1 : 0,
                                IsDisplayed = item.Product.ProductCategory.IsDisplayed,
                                IsUsed = true,
                                AdjustmentNote = item.Product.ProductCategory.AdjustmentNote,
                                ImageFontAwsomeCss = item.Product.ProductCategory.ImageFontAwsomeCss,
                                ParentCateId = item.Product.ProductCategory.ParentCateId
                            }
                        };

                        //if (item.ProductID == 199 || item.ProductID == 211 || item.ProductID == 212 || item.ProductID == 213 || item.ProductID == 215 || item.ProductID == 221 || item.ProductID == 216
                        //    || item.ProductID == 224 || item.ProductID == 233 || item.ProductID == 241)
                        //{
                        //    p.IsMostOrder = true;
                        //}
                        if (p.IsFixedPrice == false)
                        {
                            var priceProduct = productDetailApi.GetPriceByStore(terminalId, item.Product.ProductID);
                            if (priceProduct != 0)
                            {
                                p.Price = priceProduct;
                            }
                            else
                            {
                                p.Price = item.Product.Price;
                                //p.IsAvailable = false;
                            }

                            var discountProduct = productDetailApi.GetDiscountByStore(terminalId, item.Product.ProductID);
                            if (priceProduct != 0)
                            {
                                p.DiscountPercent = discountProduct;
                            }
                            else
                            {
                                p.DiscountPercent = item.Product.DiscountPercent;
                            }

                        }
                        productList.Add(p);
                    }
                }
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
            //return productList;
            //return Json(productList,JsonRequestBehavior.AllowGet);

            return new HttpResponseMessage()
            {
                Content = new JsonContent(productList)
            };
        }
    }
}
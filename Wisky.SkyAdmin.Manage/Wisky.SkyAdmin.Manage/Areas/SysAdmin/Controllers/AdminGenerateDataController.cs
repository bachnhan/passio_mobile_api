using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Sdk;
using HmsService.Models;
using HmsService.ViewModels;
using HmsService.Models.Entities;
using System.Threading.Tasks;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class AdminGenerateDataController : DomainBasedController
    {
        // GET: Admin/AdminGenerateData
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> GenerateProductDetailMapping()
        {
            BrandApi brandApi = new BrandApi();
            StoreApi storeApi = new StoreApi();
            ProductApi productApi = new ProductApi();
            ProductCategoryApi productcategoryApi = new ProductCategoryApi();
            ProductDetailMappingApi productdetailApi = new ProductDetailMappingApi();
            var listBrand = brandApi.GetAllBrand();
            //var listBrand = new List<Brand>();
            //listBrand.Add(brandApi.Get(3).ToEntity());
            foreach (var brandItem in listBrand)
            {
                var liststore = storeApi.GetAllStore(brandItem.Id).ToList();
                var listproductCategory = productcategoryApi.GetByBrandId(brandItem.Id).ToList();
                List<Product> listProduct = new List<Product>();
                foreach (var productCategory in listproductCategory)
                {
                    var lisProducttmp = await productApi.GetByProductCategoryIdAsync(productCategory.CateID);
                    foreach (var item in lisProducttmp)
                    {
                        if (item.ProductType != (int) ProductTypeEnum.General)
                        {
                            listProduct.Add(item);
                        }
                    }
                }

                foreach (var store in liststore)
                {
                    foreach (var product in listProduct)
                    {
                        ProductDetailMappingViewModel productdetail = new ProductDetailMappingViewModel();
                        if (store.isAvailable.Value && product.Active)
                        {
                            productdetail.Active = true;
                        }
                        else
                        {
                            productdetail.Active = false;
                        }

                        productdetail.ProductID = product.ProductID;
                        productdetail.Price = product.Price;
                        productdetail.DiscountPercent = product.DiscountPercent;
                        productdetail.DiscountPrice = productdetail.DiscountPrice;
                        productdetail.StoreID = store.ID;
                        var tmpdata = productdetailApi.GetProductDetailByStore(productdetail.StoreID.Value, productdetail.ProductID.Value);
                        if (tmpdata == null)
                        {
                            try
                            {
                                await productdetailApi.CreateProductDetail(productdetail);
                            }
                            catch (Exception ex)
                            {
                            }
                          
                        }
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
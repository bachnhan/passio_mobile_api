using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Data;
using HmsService.Models;
using HmsService.Sdk;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.ViewModels;
using System.Threading.Tasks;
using HmsService.Models.Entities;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = "BrandManager, Manager")]
    public class ProductSettingController : DomainBasedController
    {
        // GET: Admin/ProductSetting
        [Authorize(Roles = "BrandManager")]
        public ActionResult Index(int productId)
        {
            ViewBag.productId = productId;
            return View();
        }



        [Authorize(Roles = "BrandManager, Manager")]
        public async Task<ActionResult> UpdateProductStoreMapping(string detailId, string priceAtStore, string discountAtStore, string discountPercent, bool status)
        {
            ProductDetailMappingApi productDetailApi = new ProductDetailMappingApi();
            ProductDetailMappingViewModel productmapping = productDetailApi.Get(int.Parse(detailId));

            productmapping.Price = int.Parse(priceAtStore);
            productmapping.DiscountPercent = double.Parse(discountPercent);
            productmapping.DiscountPrice = int.Parse(discountAtStore);
            productmapping.Active = status;
            try
            {
                await productDetailApi.EditAsync(int.Parse(detailId), productmapping);
                var storeApi = new StoreApi();
                int storeId = int.Parse(RouteData.Values["storeId"].ToString());
                var listStores = storeApi.GetStoreArrayByID(storeId);
                await Utils.PostNotiMessageToStores(listStores, (int)NotifyMessageType.ProductChange);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize(Roles = "BrandManager")]
        public ActionResult GetDataSetting(HmsService.Models.JQueryDataTableParamModel param, int productId)
        {
            ProductApi productapi = new ProductApi();
            var producttmp = productapi.Get(productId);
            if (producttmp == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (producttmp.ProductType == (int)ProductTypeEnum.General)
                {
                    return HttpNotFound();
                }
                StoreApi storeApi = new StoreApi();
                ProductDetailMappingApi productDetailMappingApi = new ProductDetailMappingApi();
                var listSetting = productDetailMappingApi.Get().Where(pm => pm.ProductID == productId);
                List<ProductSettingViewModel> listProductSetting = new List<ProductSettingViewModel>();

                foreach (var item in listSetting)
                {
                    //Mapping to list 
                    ProductSettingViewModel tmpItem = new ProductSettingViewModel
                    {
                        DiscountPrice = item.DiscountPrice,
                        DiscountPercent = item.DiscountPercent,
                        Price = item.Price,
                        Active = item.Active,
                        ProductDetailID = item.ProductDetailID,
                        StoreID = item.StoreID
                    };
                    var storeTmp = storeApi.Get().Where(s => s.ID == item.StoreID).FirstOrDefault();
                    if (storeTmp != null)
                    {
                        tmpItem.storeName = storeTmp.Name;
                        listProductSetting.Add(tmpItem);
                    }
                }
                ProductApi productApi = new ProductApi();
                DefaultSettingInBrand productDefaultSetting = new DefaultSettingInBrand();
                var product = productApi.Get(productId);

                #region Set data for productDefaultSetting
                productDefaultSetting.PriceDefault = product.Price;
                productDefaultSetting.DiscountPercentDefault = (int)product.DiscountPercent;
                productDefaultSetting.PriceDiscountDefault = product.DiscountPrice;
                productDefaultSetting.ProductName = product.ProductName;
                productDefaultSetting.ProductCode = product.Code;
                #endregion
                var count = 0;
                var rs = listProductSetting
                 .Select(a => new IConvertible[]
               {
                ++count,
                a.storeName,
                productDefaultSetting.PriceDefault,
                productDefaultSetting.DiscountPercentDefault,
                a.Price,
                a.DiscountPercent,
                a.Active,
                a.StoreID,
                a.ProductDetailID
             }).ToList();
                var totalRecords = rs.Count();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "BrandManager")]
        public async Task<ActionResult> UpdateStatus(string productDetailMappingId)
        {
            ProductDetailMappingApi productMappingApi = new ProductDetailMappingApi();
            var productMapping = productMappingApi.Get(int.Parse(productDetailMappingId));
            if (productMapping.Active == true)
            {
                productMapping.Active = false;
            }
            else
            {
                productMapping.Active = true;
            }
            try
            {
                await productMappingApi.EditAsync(int.Parse(productDetailMappingId), productMapping);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        [Authorize(Roles = "BrandManager, Manager")]
        public ActionResult GetDefaultSetting(int productId)
        {
            ProductApi productApi = new ProductApi();
            DefaultSettingInBrand productDefaultSetting = new DefaultSettingInBrand();
            var product = productApi.Get(productId);

            #region Set data for productDefaultSetting
            productDefaultSetting.PriceDefault = product.Price;
            productDefaultSetting.DiscountPercentDefault = (int)product.DiscountPercent;
            productDefaultSetting.PriceDiscountDefault = product.DiscountPrice;
            productDefaultSetting.ProductName = product.ProductName;
            productDefaultSetting.ProductCode = product.Code;
            #endregion

            return Json(new { defaultSetting = productDefaultSetting }, JsonRequestBehavior.AllowGet);
        }


        #region Use for Manager
        [Authorize(Roles = "BrandManager, Manager")]
        public ActionResult getListProduct(HmsService.Models.JQueryDataTableParamModel param, int storeId, int brandId, int productTypeId, int productStatus)
        {
            var productApi = new ProductApi();
            var productMapping = new ProductDetailMappingApi();
            var storeApi = new StoreApi();
            StoreViewModel store = storeApi.Get(storeId);
            #region join product with productmaping
            var table1 = productApi.GetAllProductsByBrand(brandId);
            var table2 = productMapping.Get().Where(a=> a.StoreID==storeId);
            var join = from t1 in table1
                           join t2 in table2
                           on t1.ProductID equals t2.ProductID
                           select new
                           {
                               t1.PicURL,
                               t1.ProductName,
                               t1.ProductCategory,
                               t2.Price,
                               t2.DiscountPercent,
                               t2.Active,
                               t2.ProductDetailID,
                               t1.ProductID
                           };
            #endregion
            var storeName = store.Name;
            var totalRecords = join.Count();

            join = join
                .Where(q => String.IsNullOrEmpty(param.sSearch) || q.ProductName.ToLower().Contains(param.sSearch.ToLower())).ToList();

            #region filter nhóm sản phẩm
            if (productTypeId != 16595)
            {
                join = join.Where(p => p.ProductCategory.CateID == productTypeId).ToList();
            }
            #endregion
            #region filter tình trạng
            if (productStatus == 1) join = join.Where(p => p.Active== true).ToList();
            if (productStatus == 0) join = join.Where(p => p.Active == false).ToList();
            #endregion
            #region filter search
            join = join.Where(q => String.IsNullOrEmpty(param.sSearch) 
                    || q.ProductName.ToLower().Contains(param.sSearch.ToLower())).ToList();
            #endregion
            var count = param.iDisplayStart;
            var displayRecord = join.Count();
            var rs = join
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .Select(a => new IConvertible[]
            {
                ++count,
                a.PicURL,
                a.ProductName,
                a.ProductCategory.CateName,
                a.Price,
                a.DiscountPercent,
                a.Active,
                a.ProductDetailID,
                storeName,
                a.ProductID
            }).ToList();
                      
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = displayRecord,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "BrandManager, Manager")]
        public ActionResult SettingAtStore()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            return View();
        }
        #endregion
        public ActionResult GetProductType(int brandId)
        {
            var productApi = new ProductApi();
            var productMapping = new ProductDetailMappingApi();
            List<Product> result = new List<Product>();
            result = productApi.GetProductByBrand(brandId).ToList();
            var finalResult = result.GroupBy(p => p.ProductCategory).Select(a => new
            {
                productTypeId = a.Key.CateID,
                productTypeName= a.Key.CateName
            });
            return Json(new
            {
                productType = finalResult
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
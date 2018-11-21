using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Products.Controllers
{
    public class ProductsController : DomainBasedController
    {
        // GET: Products/Products
        public ActionResult Index(int storeId)
        {
            ViewBag.storeId = storeId.ToString();

            return View();
        }
        public JsonResult IndexList(HmsService.Models.JQueryDataTableParamModel param, int brandId)
        {
            var result = new ProductApi().GetProductByBrand(brandId, param.sSearch).ToList();

            //            var model = new BootgridResponseViewModel<Product>(result);
            //            return this.Json(model, JsonRequestBehavior.AllowGet);
            var totalRecords = result.Count();

            var count = 0;
            var rs = result.Select(a => new IConvertible[]
            {
                ++count,
                a.Code,
                a.ProductName,
                a.ProductID
            }).ToList();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductDetail(int id)
        {
            ViewBag.ProductId = id;

            return View();
        }

        public ActionResult ListItemByPro(int productID)
        {
            //var api = new ProductApi();
            //var product = api.GetProductByIdEntity(productID);
            //var productItems =
            //    product.ProductItemCompositionMappings.Select(
            //        a => new ItemViewModel(a.ProducID, a.ItemID, a.ProductItem.ItemName, a.ProductItem.Unit, a.Quantity)).ToList();

            var api = new ProductItemCompositionMappingApi();
            var productItems = api.GetProductItemByProductID(productID)
                .OrderBy(p => p.ProductItem.ItemName).Select(p => new ItemViewModel()
                {
                    itemID = p.ItemID,
                    productID = p.ProducID,
                    itemName = p.ProductItem.ItemName,
                    itemUnit = p.ProductItem.Unit,
                    quantity = p.Quantity,
                    price = p.ProductItem.Price??0,
                }).ToList();

            ViewBag.Total = productItems.Select(q => q.price * q.quantity).Sum();
            return PartialView("_SelectItem", productItems);
        }

        public ActionResult ListProductByCategory(int cateId, string searchName, int productId, int brandId)
        {
            var api = new ProductItemApi();

            List<ProductItemViewModel> productItems;
            if (cateId == 0 && searchName.IsNullOrWhiteSpace())
            {
                productItems = api.GetAvailableProductItemsModelByBrand(brandId).OrderBy(p => p.ItemName).ToList();
            }
            else if (cateId == 0 && !searchName.IsNullOrWhiteSpace())
            {
                productItems = api.GetAvailableProductItemsModelByBrand(brandId).Where(a => a.ItemName.ToLower().Contains(searchName.ToLower()))
                    .OrderBy(p => p.ItemName).ToList();
            }
            else
            {
                productItems = api.GetProductItemsByCategoryId(cateId).Where(a => a.ItemName.ToLower()
                .Contains(searchName.ToLower())).OrderBy(p => p.ItemName).ToList();
            }

            var picmApi = new ProductItemCompositionMappingApi();
            var items = picmApi.GetProductItemByProductID(productId);
            foreach (var item in productItems)
            {
                if (items.Select(q => q.ItemID).Contains(item.ItemID))
                {
                    item.Quantity = items.FirstOrDefault(q => q.ItemID == item.ItemID).Quantity;
                }
                else
                {
                    item.Quantity = 0;
                }
            }

            ViewBag.ProductId = productId;
            return PartialView("_CompositionList", productItems);
        }

        public ActionResult LoadProductCategory(int productId, int brandId)
        {
            var api = new ProductItemCategoryApi();
            IEnumerable<ProductItemCategoryViewModel> model = api.GetItemCategoryByBrand(brandId);
            ViewBag.ProductId = productId;
            return PartialView("_LoadCategories", model);
        }
        public async Task<JsonResult> DeleteComposition(int productId, int itemId)
        {
            try
            {
                var api = new ProductItemCompositionMappingApi();
                await api.DeleteProductItemCompositionMapping(productId, itemId);
                return Json(new { success = true, });
            }
            catch (Exception)
            {

                return Json(new { success = false, });
            }

        }

        public ActionResult ViewProductDetail(int productId, int itemId)
        {
            var productItemCompositionApi = new ProductItemCompositionMappingApi();
            //var productApi = new ProductApi();
            //productApi.GetProductById(productId);
            ProductItemCompositionMappingViewModel model;
            try
            {
                model = productItemCompositionApi.GetItem(productId, itemId);
                if (model == null)
                {
                    var productItemApi = new ProductItemApi();
                    var productItem = new ProductItemViewModel(productItemApi
                        .GetProductItemsEntity().SingleOrDefault(p => p.ItemID == itemId));
                    model = new ProductItemCompositionMappingViewModel();
                    model.ProducID = productId;
                    model.ItemID = itemId;
                    model.ProductItem = productItem;

                    model.Quantity = 0;
                }
            }
            catch (Exception e)
            {
                throw (e);
            }

            return PartialView("_EditProductDetail", model);
        }

        public async Task<JsonResult> CreateComposition(int productId, int itemId, double quantity)
        {
            var api = new ProductItemCompositionMappingApi();
            try
            {
                var composition = api.GetItem(productId, itemId);
                if (composition == null)
                {
                    ProductItemCompositionMappingViewModel model = new ProductItemCompositionMappingViewModel();
                    model.ProducID = productId;
                    model.ItemID = itemId;
                    model.Quantity = quantity;
                    await api.CreateProductItemCompositionMapping(model);
                }
                else
                {
                    //composition.Quantity = composition.Quantity + quantity;
                    composition.Quantity = quantity;
                    await api.UpdateProductItemCompositionMapping(composition);
                }

                return Json(new { success = true });
                //return RedirectToAction("ProductDetail", "Products", new JsonResult { id = productId });
            }
            catch (Exception e)
            {
                throw (e);
                //return Json(new { success = false, });
            }
        }
    }
}
using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    //[Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class ProductCategoryController : DomainBasedController
    {

        public ActionResult Index(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            ViewBag.brandId = brandId.ToString();
            return this.View();
        }

        //public ActionResult IndexList(BootgridRequestViewModel request, int storeId)
        //{
        //    var result = new ProductCategoryApi().GetAdminWithFilterAsync(
        //        storeId, request.searchPhrase,
        //        request.current, request.rowCount, request.FirstSortTerm);

        //    var model = new BootgridResponseViewModel<ProductCategoryViewModel>(result);
        //    return this.Json(model, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult IndexList(HmsService.Models.JQueryDataTableParamModel param, int brandId)
        {
            var result = new ProductCategoryApi().GetActiveProductCategoriesByBrandId(brandId).ToList();

            var totalRecords = result.Count();

            var count = 0;
            var rs = result
                .OrderBy(q => q.DisplayOrder)
                //.Where(a => string.IsNullOrEmpty(param.sSearch) || a.CateName.ToLower().Contains(param.sSearch.ToLower()))
                //.Skip(param.iDisplayStart)
                //.Take(param.iDisplayLength)
                .Select(a => new IConvertible[]
            {
                ++count,
                a.CateName,
                Utils.GetEnumDescription((ProductCategoryType)a.Type),
                a.DisplayOrder,
                a.Active,
                a.IsExtra,
                a.CateID
            }).ToList();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Create(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            ViewBag.brandId = brandId.ToString();
            var model = new ProductCategoryEditViewModel();
            await this.PrepareCreate(model, brandId);
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductCategoryEditViewModel model,int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            var storeApi = new StoreApi();
            if (!this.ModelState.IsValid)
            {
                await this.PrepareCreate(model, brandId);
                return this.View(model);
            }

            //model.StoreId = this.CurrentStore.ID;
            model.BrandId = brandId;
            model.ImageFontAwsomeCss = model.IconEnum.ToString();
            model.Type = (int)model.CategoryTypes;
            model.IsDisplayed = true;
            if (model.Type == (int)ProductCategoryType.CardPayment)
            {
                model.IsDisplayed = false;
            }
            if(model.Position == null)
            {
                model.Position = 1000;
            }
            var productCategoryApi = new ProductCategoryApi();
            await productCategoryApi.CreateAsync(model);


            var listSelectedCategoryExtras = model.SelectedProductCategoryExtras;
            if (listSelectedCategoryExtras != null)
            {
                var categoryExtraApi = new CategoryExtraMappingApi();
                categoryExtraApi.CreateCategoryExtra(model.CateID, listSelectedCategoryExtras);
            }

            var stores = storeApi.GetActiveStoreByBrandId(brandId).ToList();
            await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.CategoryChange);            

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        private async Task PrepareCreate(ProductCategoryEditViewModel model, int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            model.AvailableCategories = (await productCategoryApi
                .GetByBrandIdAsync(brandId))
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.ParentCateId == q.CateID, true, "--Không có--");

            model.SelectedProductCategoryExtras = new int[] { };
            model.AvailableCategoryExtras = productCategoryApi.GetProductCategorieExtra()
                .ToSelectList(c => c.CateName, c => c.CateID.ToString(),
                    c => model.SelectedProductCategoryExtras.Contains(c.CateID));

            model.CategoryTypes = ProductCategoryType.Default;
            model.IconEnum = IconCategoryEnum.glass;
            model.Active = true;
        }

        public async Task<ActionResult> Edit(int? id, int storeId, int brandId)
        {
            var info = await new ProductCategoryApi()
                .GetByIdAndBrand(id.GetValueOrDefault(), brandId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            var model = new ProductCategoryEditViewModel(info, this.Mapper);

            await this.PrepareEdit(model, brandId);
            ViewBag.storeId = storeId.ToString();
            ViewBag.brandId = brandId.ToString();
            return this.View(model);
        }

        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Edit(ProductCategoryEditViewModel model,int storeId, int brandId)
        {
            var api = new ProductCategoryApi();
            var storeApi = new StoreApi();
            // Validate
            var info = await api
                .GetByIdAndBrand(model.CateID, brandId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                await this.PrepareEdit(model, brandId);
                return this.View(model);
            }

            model.ImageFontAwsomeCss = model.IconEnum.ToString();
            model.Type = (int)model.CategoryTypes;
            model.StoreId = storeId;
            model.Active = true;
            if(model.Position == null)
            {
                model.Position = 1000;
            }
            if (model.Type == (int)ProductCategoryType.CardPayment)
            {
                model.IsDisplayed = false;
                model.IsDisplayedWebsite = false;
            }
            //await api.EditAsync(model.CateID, model);
            api.EditProductCategory(model);
            var categoryExtraApi = new CategoryExtraMappingApi();
            categoryExtraApi.EditCategoryExtra(model.CateID, model.SelectedProductCategoryExtras);
            var stores = storeApi.GetActiveStoreByBrandId(brandId).ToList();
            await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.CategoryChange);
            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        public async Task PrepareEdit(ProductCategoryEditViewModel model, int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            model.AvailableCategories = (await productCategoryApi
                .GetByBrandIdEditAsync(brandId, model.CateID))
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.ParentCateId == q.CateID, true, "--Không có--");

            model.SelectedProductCategoryExtras =
                new CategoryExtraMappingApi().GetProductCategoryExtra(model.CateID).Select(ce => ce.CateID).ToArray();
            if (model.SelectedProductCategoryExtras == null)
            {
                model.SelectedProductCategoryExtras = new int[] { };
            }

            model.AvailableCategoryExtras = productCategoryApi.GetProductCategorieExtra()
                .ToSelectList(c => c.CateName, c => c.CateID.ToString(),
                    c => model.SelectedProductCategoryExtras.Contains(c.CateID));

            model.CategoryTypes = (ProductCategoryType)model.Type;

            var imageFontIcon = model.ImageFontAwsomeCss.Replace(".", "");
            if (!string.IsNullOrWhiteSpace(imageFontIcon))
            {
                model.IconEnum = (IconCategoryEnum)Enum.Parse(typeof(IconCategoryEnum), imageFontIcon);
            }
            else
            {
                model.IconEnum = IconCategoryEnum.glass;
            }
        }

        public async Task<JsonResult> Delete(int? id, int brandId)
        {
            var productCategoryapi = new ProductCategoryApi();
            var productApi = new ProductApi();
            var storeApi = new StoreApi();
            var product = productApi.GetActiveByBrandIdAndProductCategoryId(brandId, (int)id);
            var item = product.Count();
            var info = await productCategoryapi.GetByIdAndBrand(id.Value, brandId);

            if (info != null && item == 0)
            {
                await productCategoryapi.DeactivateAsync(id.Value);
                var stores = storeApi.GetActiveStoreByBrandId(brandId).ToList();
                await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.CategoryChange);
                return Json(new { success = true, message = "Xóa danh mục sản phẩm thành công" });
            }
            else
            {
                return Json(new { success = false, message = "Không thể xóa. Tồn tại sản phẩm chứa danh mục sản phẩm này!" });
            }
           
        }

        public JsonResult ValidateSeoName(string seoName, int id)
        {
            var api = new ProductCategoryApi();
            var productCategory = api.GetProductCategoryBySeo(seoName);
            if (id == 0)
            {
                if (productCategory != null)
                {
                    return Json(new { success = false });
                }
                else
                {
                    return Json(new { success = true });
                }
            }
            else
            {
                if (productCategory == null)
                {
                    return Json(new { success = true });
                }
                else if(productCategory.CateID == id)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
        }
        public async Task<JsonResult> LoadParentCategoryAttribute(int Id, int brandId)
        {
            try
            {
                ProductCategoryApi productCateApi = new ProductCategoryApi();
                var result = await productCateApi.GetByIdAndBrand(Id, brandId);
                var attr = new
                [] {
                    result.Att1,
                    result.Att2,
                    result.Att3,
                    result.Att4,
                    result.Att5,
                    result.Att6,
                    result.Att7,
                    result.Att8,
                    result.Att9,
                    result.Att10,
                };
                return Json(new { success = true, result = attr }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}
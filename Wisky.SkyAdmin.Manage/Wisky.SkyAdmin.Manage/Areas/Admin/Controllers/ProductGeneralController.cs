using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class ProductGeneralController : DomainBasedController
    {
        // GET: Admin/ProductGeneral
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult IndexList(HmsService.Models.JQueryDataTableParamModel param, int brandId)
        {

            var productApi = new ProductApi();
            var productMapping = new ProductDetailMappingApi();
            List<Product> result = productApi.GetAllProductsByBrand(brandId).Where(q=>q.ProductType==6).ToList();

            var totalRecords = result.Count();

            var count = 0;
            var rs = result
                .Select(a => new IConvertible[]
            {
                ++count,
                a.PicURL,
                a.ProductName,
                a.ProductCategory.CateName,
                a.IsAvailable,
                a.ProductID,
            }).ToList();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateProduct(int? generalProductId, int brandId)
        {
            ProductApi productApi = new ProductApi();
            var model = new Models.ViewModels.ProductEditViewModel();
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            model.GeneralProduct = productApi.GetProductById(generalProductId.GetValueOrDefault());
            model.Product.GeneralProductId = generalProductId.GetValueOrDefault();
            this.PrepareCreate(model, brandId);
            return this.View(model);
        }

        public JsonResult CheckProductCode(string code, string seoName)
        {
            var productApi = new ProductApi();
            var list = productApi.GetProductByCode(code);
            var seo = productApi.GetProductBySeoName(seoName);

            if (list == null && seo == null)
            {
                return Json(new { success = true });
            }
            else if (list != null)
            {
                return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
            }
            else if (seo != null)
            {
                return Json(new { success = false, message = "Đường dẫn Seo đã tồn tại!" });
            }
            else
            {
                return Json(new { success = true });
            }
        }

        public JsonResult CheckCodeEditProduct(string code, string seoName, int id)
        {
            var productApi = new ProductApi();
            var product = productApi.GetProductByIdEntity(id);
            var list = productApi.GetProductByCode(code);
            var seo = productApi.GetProductBySeoName(seoName);

            if (product != null && product.Code == code && (product.SeoName == null || product.SeoName == seoName))
            {
                return Json(new { success = true });
            }
            else if (list == null && seo == null)
            {
                return Json(new { success = true });
            }
            else if (list != null && list.ProductID != id)
            {
                return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
            }
            else if (seo != null && seo.ProductID != id)
            {
                return Json(new { success = false, message = "Đường dẫn Seo đã tồn tại!" });
            }
            else if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm này đã bị xóa. Không thể cập nhật!" });
            }
            else
            {
                return Json(new { success = true });
            }
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult CreateProduct(Models.ViewModels.ProductEditViewModel model, int brandId, String[] Atts)
        {
            // Validate category
            var categoryApi = new ProductCategoryApi();
            var productApi = new ProductApi();
            var storeApi = new StoreApi();
            if (!categoryApi.ValidateBrandCategory(model.Product.CatID, brandId))
            {
                //                this.ModelState.AddModelError(nameof(model.CatID), "Invalid Category ID");
                this.ModelState.AddModelError("", "Danh mục này không thuộc cửa hàng của bạn!!!");
            }

            if (!this.ModelState.IsValid)
            {
                this.PrepareCreate(model, brandId);
                return this.View(model);
            }
            if (model.IsExtra)
            {
                model.Product.ProductType = (int)ProductTypeEnum.Extra;
            }
            else
            {
                if (model.Product.GeneralProductId != null)
                {
                    model.Product.ProductType = (int)ProductTypeEnum.Detail;
                    List<int> tempAtt = new List<int>();
                    var tmpatt = 0;
                    var currentGeneralProduct = productApi.GetProductById(model.Product.GeneralProductId.Value);
                    if (!string.IsNullOrEmpty(currentGeneralProduct.Att1))
                    {
                        tmpatt = 1;
                    }
                    tempAtt.Add(tmpatt);
                    tmpatt = 0;

                    if (!string.IsNullOrEmpty(currentGeneralProduct.Att2))
                    {
                        tmpatt = 2;
                    }
                    tempAtt.Add(tmpatt);
                    tmpatt = 0;

                    if (!string.IsNullOrEmpty(currentGeneralProduct.Att3))
                    {
                        tmpatt = 3;
                    }

                    tempAtt.Add(tmpatt);
                    var index = 0;

                    for (int i = 1; i <= tempAtt.Count; i++)
                    {
                        if (i == 1)
                        {
                            if (i == tempAtt[i - 1])
                            {
                                model.Product.Att1 = Atts[index];
                                index++;
                            }
                        }
                        else if (i == 2)
                        {
                            if (i == tempAtt[i - 1])
                            {
                                model.Product.Att2 = Atts[index];
                                index++;
                            }
                        }
                        else
                        {
                            if (i == tempAtt[i - 1])
                            {
                                model.Product.Att3 = Atts[index];
                                index++;
                            }
                        }
                    }
                }
                else
                {
                    model.Product.ProductType = (int)ProductTypeEnum.Single;
                }
            }
            model.Product.CreateTime = Utils.GetCurrentDateTime();
            model.Product.ColorGroup = (int)model.ProductColorGroup;
            model.Product.Group = (int)model.ProductGroup;
            model.Active = true;
            //model.IsAvailable = true;
            if (String.IsNullOrEmpty(model.Description))
            {
                model.Description = "Thông tin đang được cập nhật!";
            }
            if (model.Product.Position == null)
            {
                model.Product.Position = 1000;
            }
            if (model.Product.PosX == null)
            {
                model.Product.PosX = 0;
            }
            if (model.Product.PosY == null)
            {
                model.Product.PosY = 0;
            }
            //Set cứng 1 số field
            model.Product.IsMostOrdered = true;
            model.Product.IsDefaultChildProduct = 1;
            model.Product.Active = true;
            model.Product.IsAvailable = true;
            var listStoreId = storeApi.GetActiveStoreByBrandId(brandId).Select(q => q.ID).AsEnumerable();

            var productId = productApi.CreateSync(model.Product,
                model.SelectedImages?.ToArray() ?? new string[0],
                model.SelectedProductCollections?.ToArray() ?? new int[0],
                model?.SpecValues ?? new KeyValuePair<string, string>[0],
                listStoreId);

            //            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
            return this.RedirectToAction("ProductGeneralDetail/" + model.Product.GeneralProductId, "ProductGeneral");
        }

        public ActionResult EditProduct(int? id, int brandId)
        {
            var product = new ProductApi()
                .GetByBrandId(id.GetValueOrDefault(), brandId);

            if (product == null)
            {
                return this.IdNotFound();
            }

            var model = new Models.ViewModels.ProductEditViewModel(product, this.Mapper);
            model.ProductColorGroup = (ProductColorGroup)model.Product.ColorGroup;
            model.ProductGroup = (ProductGroup)model.Product.Group;
            if (model.Product.GeneralProductId != null)
            {
                model.GeneralProduct = (new ProductApi()).GetByBrandId(model.Product.GeneralProductId.Value, brandId).Product;
            }
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            ViewBag.Mode = "EditProduct";
            this.PrepareEditProduct(model, brandId);
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult EditProduct(Models.ViewModels.ProductEditViewModel model, int brandId, String[] Atts)
        {
            var productApi = new ProductApi();
            var productCategoryApi = new ProductCategoryApi();
            // Validate
            var info = productApi.GetByBrandId(model.Product.ProductID, brandId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                this.PrepareEdit(model, brandId);
                return this.View(model);
            }

            if (model.Product.GeneralProductId != null)
            {
                List<int> tempAtt = new List<int>();
                var tmpatt = 0;
                var currentGeneralProduct = productApi.GetProductById(model.Product.GeneralProductId.Value);
                if (!string.IsNullOrEmpty(currentGeneralProduct.Att1))
                {
                    tmpatt = 1;
                }
                tempAtt.Add(tmpatt);
                tmpatt = 0;

                if (!string.IsNullOrEmpty(currentGeneralProduct.Att2))
                {
                    tmpatt = 2;
                }
                tempAtt.Add(tmpatt);
                tmpatt = 0;

                if (!string.IsNullOrEmpty(currentGeneralProduct.Att3))
                {
                    tmpatt = 3;
                }

                tempAtt.Add(tmpatt);
                var index = 0;

                for (int i = 1; i <= tempAtt.Count; i++)
                {
                    if (i == 1)
                    {
                        if (i == tempAtt[i - 1])
                        {
                            model.Product.Att1 = Atts[index];
                            index++;
                        }
                    }
                    else if (i == 2)
                    {
                        if (i == tempAtt[i - 1])
                        {
                            model.Product.Att2 = Atts[index];
                            index++;
                        }
                    }
                    else
                    {
                        if (i == tempAtt[i - 1])
                        {
                            model.Product.Att3 = Atts[index];
                            index++;
                        }
                    }
                }
            }
            model.Active = info.Product.Active;
            //model.IsAvailable = info.Product.IsAvailable;
            model.Product.CreateTime = Utils.GetCurrentDateTime();
            model.Product.ColorGroup = (int)model.ProductColorGroup;
            model.Product.Group = (int)model.ProductGroup;
            if (String.IsNullOrEmpty(model.Description))
            {
                model.Description = "Thông tin đang được cập nhật!";
            }
            if (model.Product.Position == null)
            {
                model.Product.Position = 1000;
            }

            //Set cứng 1 số field
            model.Product.IsDefaultChildProduct = 1;
            model.Product.IsMostOrdered = true;
            model.Product.ProductType = info.Product.ProductType;
            productApi.EditAsync(model.Product,
                model?.SelectedImages?.ToArray() ?? new string[0],
                model?.SelectedProductCollections?.ToArray() ?? new int[0],
                model?.SpecValues ?? new KeyValuePair<string, string>[0]);

            return this.RedirectToAction("ProductGeneralDetail/" + model.Product.GeneralProductId, "ProductGeneral");
        }

        public void PrepareEditProduct(Models.ViewModels.ProductEditViewModel model, int brandId)
        {
            model.AvailableCategories =
                (new ProductCategoryApi().GetProductCategoriesByBrandId(brandId))
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.CatID == q.CateID);
            model.AvailableCollections = (new ProductCollectionApi()
                .GetActiveByBrandId(brandId))
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => model.SelectedProductCollections.Contains(q.Id));
            model.ProductSpecifications = (new ProductSpecificationApi()).Get().Where(q => q.Active && q.ProductId == model.ProductID);
        }

        #region Create
        public ActionResult Create(int brandId)
        {
            var model = new Models.ViewModels.ProductEditViewModel()
            {
                IsAvailable = true,

            };
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            this.PrepareCreate(model, brandId);
            return this.View(model);
            
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Create(Models.ViewModels.ProductEditViewModel model, int brandId)
        {
            // Validate category
            var categoryApi = new ProductCategoryApi();

            if (!categoryApi.ValidateBrandCategory(model.CatID, brandId))
            {
               
                this.ModelState.AddModelError("", "Danh mục này không thuộc cửa hàng của bạn!!!");
            }

            if (!this.ModelState.IsValid)
            {
                this.PrepareCreate(model, brandId);
                return this.View(model);
            }

            var productApi = new ProductApi();
            model.Product.CreateTime = Utils.GetCurrentDateTime();
            model.Product.ColorGroup = (int)model.ProductColorGroup;
            model.Product.Group = (int)model.ProductGroup;
            model.Active = true;
          
            if (String.IsNullOrEmpty(model.Description))
            {
                model.Description = "Thông tin đang được cập nhật!";
            }
            if (model.Product.Position == null)
            {
                model.Product.Position = 1000;
            }
            if (model.Product.PosX == null)
            {
                model.Product.PosX = 0;
            }
            if (model.Product.PosY == null)
            {
                model.Product.PosY = 0;
            }
            //Set cứng 1 số field
            model.Product.IsMostOrdered = true;
            model.Product.IsDefaultChildProduct = 1;
            model.Product.ProductType = 6;
            var storeApi = new StoreApi();
            var listStoreId = storeApi.GetActiveStoreByBrandId(brandId).Select(q => q.ID).AsEnumerable();

            var productId = productApi.CreateSync(model.Product,
                model.SelectedImages?.ToArray() ?? new string[0],
                model.SelectedProductCollections?.ToArray() ?? new int[0],
                model?.SpecValues ?? new KeyValuePair<string, string>[0], listStoreId);

           
            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory});
        }
        public JsonResult CheckCode(string code, string seoName)
        {
            var productApi = new ProductApi();
            var list = productApi.GetProductByCode(code);
            var seo = productApi.GetProductBySeoName(seoName);

            if (list == null && seo == null)
            {
                return Json(new { success = true });
            }
            else if (list != null)
            {
                return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
            }
            else if (seo != null)
            {
                return Json(new { success = false, message = "Đường dẫn Seo đã tồn tại!" });
            }
            else
            {
                return Json(new { success = true });
            }
        }
        public JsonResult CheckCodeEdit(string code, string seoName, int id)
        {
            var productApi = new ProductApi();
            var product = productApi.GetProductByIdEntity(id);
            var list = productApi.GetProductByCode(code);
            var seo = productApi.GetProductBySeoName(seoName);

            if (product != null && product.Code == code && (product.SeoName == null || product.SeoName == seoName))
            {
                return Json(new { success = true });
            }
            else if (list == null && seo == null)
            {
                return Json(new { success = true });
            }
            else if (list != null && list.ProductID != id)
            {
                return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
            }
            else if (seo != null && seo.ProductID != id)
            {
                return Json(new { success = false, message = "Đường dẫn Seo đã tồn tại!" });
            }
            else if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm này đã bị xóa. Không thể cập nhật!" });
            }
            else
            {
                return Json(new { success = true });
            }
        }
        private void PrepareCreate(Models.ViewModels.ProductEditViewModel model, int brandId)
        {
            model.AvailableCategories = (new ProductCategoryApi().GetProductCategoriesByBrandId(brandId))
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.CatID == q.CateID);
            model.AvailableCollections =
                (new ProductCollectionApi()
                .GetActiveByBrandId(brandId))
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => false);
            model.ProductColorGroup = (ProductColorGroup)ProductColorGroup.Blue;
            model.ProductGroup = (ProductGroup)ProductGroup.Cold;
            model.Product.Position = 1000;
            model.Product.PosX = 0;
            model.Product.PosY = 0;

        }


        #endregion

        #region Edit
        public ActionResult Edit(int? id, int brandId)
        {
            var info = new ProductApi()
                .GetByBrandId(id.GetValueOrDefault(), brandId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            var model = new Models.ViewModels.ProductEditViewModel(info, this.Mapper);
            model.ProductColorGroup = (ProductColorGroup)model.Product.ColorGroup;
            model.ProductGroup = (ProductGroup)model.Product.Group;
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            ViewBag.Mode = "Edit";
            this.PrepareEdit(model, brandId);
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Edit(Models.ViewModels.ProductEditViewModel model, int brandId)
        {
            var api = new ProductApi();
            var productCategoryApi = new ProductCategoryApi();
            // Validate
            var info = api.GetByBrandId(model.ProductID, brandId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                this.PrepareEdit(model, brandId);
                return this.View(model);
            }

            model.Active = info.Product.Active;
            model.Product.CreateTime = Utils.GetCurrentDateTime();
            model.Product.ColorGroup = (int)model.ProductColorGroup;
            model.Product.Group = (int)model.ProductGroup;
            if (String.IsNullOrEmpty(model.Description))
            {
                model.Description = "Thông tin đang được cập nhật!";
            }
            if (model.Product.Position == null)
            {
                model.Product.Position = 1000;
            }

            //Set cứng 1 số field
            model.Product.IsDefaultChildProduct = 1;
            model.Product.IsMostOrdered = true;
            // Type =6 General Product
            model.Product.ProductType = 6;

            api.EditAsync(model.Product,
                model?.SelectedImages?.ToArray() ?? new string[0],
                model?.SelectedProductCollections?.ToArray() ?? new int[0],
                model?.SpecValues ?? new KeyValuePair<string, string>[0]);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }
        public void PrepareEdit(Models.ViewModels.ProductEditViewModel model, int brandId)
        {
            model.AvailableCategories =
                (new ProductCategoryApi().GetProductCategoriesByBrandId(brandId))
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.CatID == q.CateID);
            model.AvailableCollections = (new ProductCollectionApi()
                .GetActiveByBrandId(brandId))
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => model.SelectedProductCollections.Contains(q.Id));
        }
        #endregion

        #region Detail

        public ActionResult ProductGeneralDetail(int id)
        {
            var productApi = new ProductApi();
            ViewBag.productId = id;
            var product = productApi.GetProductById(id);
            
            if (product != null && product.GeneralProductId == null)
            {
                ViewBag.productName = product.ProductName;
                ViewBag.storeId = RouteData.Values["storeId"].ToString();
                return View("Detail",product);
            }
            else
            {
                ViewBag.productName = "";
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult GetProductDetail(int id)
        {
            var productApi = new ProductApi();
            var product = productApi.GetProductById(id);

            return Json(new
            {
                code = product.Code,
                category = product.CateName,
                price = product.Price,
                discount = product.DiscountPercent,
                description = product.Description
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListDetail(JQueryDataTableParamModel param, int Id)
        {
            var productApi = new ProductApi();
            var productGeneral = productApi.GetChildProductGeneral(Id).Where(q => (bool)q.IsAvailable).ToList();
            ViewBag.ProductGeneral = productApi.GetProductById(Id);
           
            var totalRecords = productGeneral.Count();

            var count = 0;
            var rs = productGeneral
                .Select(a => new IConvertible[]
            {
                ++count,
                a.PicURL,
                a.ProductName,
                a.Code,
                a.Price,
                a.DiscountPrice,
                a.CateName,
                a.ProductID,
            }).ToList();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GeneralProductList(int brandId)
        {
            ProductApi productApi = new ProductApi();
            var products = productApi.GetAllActiveByProductCategoryAndPattern(0, brandId, "")
                .Where(p => p.ProductType == (int)ProductTypeEnum.Single).ToList()
                .Select(
                p => new IConvertible[]
                {
                    p.ProductName,
                    p.ProductID
                })
                .ToList();
            return Json(new
            {
               products
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddProductToGeneral(int generalProdId, int prodId)
        {
            var success = false;

            var productApi = new ProductApi();
            var product = productApi.GetProductById(prodId);

            if (product != null)
            {
                product.ProductType = (int) ProductTypeEnum.Detail;
                product.GeneralProductId = generalProdId;

                productApi.Edit(prodId, product);
                success = true;
            }

            return Json(new
            {
                success
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductDetail(int? id, int brandId)
        {
            var info = new ProductApi()
                .GetByBrandId(id.GetValueOrDefault(), brandId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            var model = new ProductEditViewModel(info, this.Mapper);            
            return PartialView("_ProductDetail",model);
        }
        #endregion

        #region Delete
        public async Task<ActionResult> Delete(int? id)
        {
            var productApi = new ProductApi();
            //var info = await productApi
            //    .GetByStoreIdAsync(id.GetValueOrDefault(), this.CurrentStore.ID);

            //if (info == null)
            //{
            //    return this.IdNotFound();
            //}

            await productApi.DeactivateAsync(id.Value);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }
        #endregion

    }
}
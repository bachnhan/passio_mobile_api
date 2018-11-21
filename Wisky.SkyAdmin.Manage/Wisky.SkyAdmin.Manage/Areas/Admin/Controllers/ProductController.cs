using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    //[Authorize(Roles = ("BrandManager, Manager, ProductManager"))]
    public class ProductController : DomainBasedController
    {
        // GET: Admin/Product
        public ActionResult Index(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            ViewBag.brandId = brandId.ToString();
            return View();
        }

        /// <summary>
        /// Lấy danh sách sản phẩm
        /// </summary>
        /// <param name="param"></param>
        /// <param name="brandId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        //public ActionResult IndexList(BootgridRequestViewModel request, int storeId)
        //{
        //    var result = new ProductApi().GetAdminWithFilterAsync(
        //        storeId, request.searchPhrase,
        //        request.current, request.rowCount, request.FirstSortTerm);

        //    var model = new BootgridResponseViewModel<ProductViewModel>(result);
        //    return this.Json(model, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult IndexList(HmsService.Models.JQueryDataTableParamModel param, int brandId, int? productGroup, bool? status, int type)
        {
            //,int productGroup, int status

            var productApi = new ProductApi();
            var productMapping = new ProductDetailMappingApi();
            string searchPattern = string.IsNullOrWhiteSpace(param.sSearch) ? null : param.sSearch;
            var result = productApi.GetAllActiveByProductCategoryAndPattern(productGroup.Value, brandId, searchPattern);

            if(status != null)
            {
                result = result.Where(q => q.IsAvailable == status.Value);
            }
            if(type != -1)
            {
                result = result.Where(q => q.ProductType == type);
            }

            //if (productGroup == 0)
            //{
            //    if (status != null)
            //        result = productApi.GetAllProductsByBrand(brandId).Where(q => q.IsAvailable == status).ToList();
            //    else
            //        result = productApi.GetAllProductsByBrand(brandId).ToList();
            //}
            //else
            //{
            //    if (status != null)
            //        result = productApi.GetAllProductsByBrand(brandId).Where(q => q.IsAvailable == status && q.CatID == productGroup).ToList();
            //    else
            //        result = productApi.GetAllProductsByBrand(brandId).Where(q => q.CatID == productGroup).ToList();

            //}
            //result = result.Where(q => q.ProductType != (int)ProductTypeEnum.General).ToList();
            //var totalRecords = result.Count();
            //result = result
            //    .Where(q => String.IsNullOrEmpty(param.sSearch) || q.ProductName.ToLower().Contains(param.sSearch.ToLower())).ToList();
            var displayRecord = result.Count();
            var totalRecords = result.Count();
            var count = param.iDisplayStart;
            var rs = result.OrderBy(q=>q.DisplayOrder)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .Select(a => new IConvertible[]
            {
                ++count,
                a.PicURL,
                a.ProductName,
                a.ProductCategory.CateName,
                ((ProductTypeEnum) a.ProductType).DisplayName(),
                a.Price,
                a.Active,
                a.ProductID,
            }).ToList();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = displayRecord,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        //lấy danh sách nhóm sản phẩm
        public JsonResult LoadProductGroupList(int brandId)
        {
            var productGroupapi = new ProductCategoryApi();
            var listProductGroup = productGroupapi.GetProductCategoriesByBrandId(brandId).Select(q => new
            {
                q.CateID,
                q.CateName
            }).ToArray();
            return Json(new
            {
                success = true,
                productGroup = listProductGroup,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(int storeId,int brandId)
        {
            var model = new Models.ViewModels.ProductEditViewModel();
            ViewBag.storeId = storeId.ToString();
            ViewBag.brandId = brandId.ToString();
            ViewBag.Mode = "Create";
            this.PrepareCreate(model, brandId);
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Create(Models.ViewModels.ProductEditViewModel model, int brandId, string[] Atts, int? ddlProduct, bool selectedGeneralProduct = false)
        {
            // Validate category
            var categoryApi = new ProductCategoryApi();
            var productApi = new ProductApi();
            var storeApi = new StoreApi();
            if (model.SelectedType == (int)ProductTypeEnum.CardPayment)
            {
                model.Product.CatID = model.Product.MaxExtra.Value;
            }
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
            if (model.SelectedType == (int)ProductTypeEnum.Extra)
            {
                model.Product.ProductType = (int)ProductTypeEnum.Extra;
            }
            else if (model.SelectedType == (int)ProductTypeEnum.Combo)
            {
                model.Product.ProductType = (int)ProductTypeEnum.Combo;
                model.Product.ProductComboDetails = model.ProductComboDetails;
            }
            else if (selectedGeneralProduct)
            {
                model.Product.ProductType = (int)ProductTypeEnum.Detail;
                List<int> tempAtt = new List<int>();
                var tmpatt = 0;
                model.Product.GeneralProductId = ddlProduct.Value;
                var currentGeneralProduct = productApi.GetProductById(ddlProduct.Value);
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
            if (model.SelectedType == (int)ProductTypeEnum.CardPayment)
            {
                model.Product.ProductType = (int)ProductTypeEnum.CardPayment;
            }
            else
            {
                model.Product.ProductType = (int)ProductTypeEnum.Single;
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
            //model.Product.IsMostOrdered = true;
            model.Product.IsDefaultChildProduct = 1;
            model.Product.Active = true;
            //if (model.Product.ProductType == (int)ProductTypeEnum.CardPayment)
            //{
            //    model.Product.IsAvailable = false;
            //}
            var listStores = storeApi.GetActiveStoreByBrandId(brandId).ToList();

            var listStoreId = listStores.Select(q => q.ID);

            var productId = productApi.CreateSync(model.Product,
                model.SelectedImages?.ToArray() ?? new string[0],
                model.SelectedProductCollections?.ToArray() ?? new int[0],
                model?.SpecValues ?? new KeyValuePair<string, string>[0],
                listStoreId);
            await Utils.PostNotiMessageToStores(listStores, (int)NotifyMessageType.ProductChange);

            //            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory/*, id = productId*/ });
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
            model.AvailableCategoriesMemberCard = (new ProductCategoryApi().GetProductCategoriesByBrandId(brandId).Where(a => a.Type == (int)ProductCategoryType.CardPayment))
               .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.CatID == q.CateID);
            model.AvailableCollections =
                (new ProductCollectionApi()
                .GetActiveByBrandId(brandId))
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => false);
            model.AvailableComboProducts = (new ProductApi().GetProductForCombo(brandId))
                .ToSelectList(q => q.ProductName, q => q.ProductID.ToString() , q => false);
            model.ProductColorGroup = (ProductColorGroup)ProductColorGroup.Blue;
            model.ProductGroup = (ProductGroup)ProductGroup.Cold;
            model.Product.Position = 1000;
            model.Product.PosX = 0;
            model.Product.PosY = 0;

        }

        public ActionResult Edit(int? id,int storeId, int brandId)
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
            if (model.Product.GeneralProductId != null)
            {
                model.GeneralProduct = (new ProductApi()).GetByBrandId(model.Product.GeneralProductId.Value, brandId).Product;
            }
            ViewBag.storeId = storeId.ToString();
            ViewBag.brandId = brandId.ToString();
            ViewBag.Mode = "Edit";
            this.PrepareEdit(model, brandId);

            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Edit(Models.ViewModels.ProductEditViewModel model, int brandId, String[] Atts)
        {
            var productApi = new ProductApi();
            var storeApi = new StoreApi();
            var productCategoryApi = new ProductCategoryApi();
            if (model.SelectedType == (int)ProductTypeEnum.CardPayment)
            {
                model.Product.CatID = model.Product.MaxExtra.Value;
            }
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
            //if (model.Product.ProductType == (int)ProductTypeEnum.CardPayment)
            //{
            //    model.Product.IsAvailable = false;
            //}
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
            model.Product.IsDefaultChildProduct = 0;
            //model.Product.IsMostOrdered = true;
            model.Product.ProductType = model.SelectedType;
            //productApi.EditAsync(model.Product,
            //    model?.SelectedImages?.ToArray() ?? new string[0],
            //    model?.SelectedProductCollections?.ToArray() ?? new int[0],
            //    model?.SpecValues ?? new KeyValuePair<string, string>[0]);

            var listStores = storeApi.GetActiveStoreByBrandId(brandId).ToList();

            var listStoreId = listStores.Select(q => q.ID);

            if (model.SelectedType == (int)ProductTypeEnum.Combo)
            {
                var combos = model.ProductComboDetails == null ? new int[0] : model.ProductComboDetails.Select(q => q.Quantity).ToArray();
                productApi.EditAsync(model.Product,
                model.SelectedImages?.ToArray() ?? new string[0],
                model.SelectedProductCollections?.ToArray() ?? new int[0],
                model?.SpecValues ?? new KeyValuePair<string, string>[0],
                combos);
            }
            else
            {
                productApi.EditAsync(model.Product,
                    model.SelectedImages?.ToArray() ?? new string[0],
                    model.SelectedProductCollections?.ToArray() ?? new int[0],
                    model?.SpecValues ?? new KeyValuePair<string, string>[0]);
            }
            var productstoreMapping = new ProductDetailMappingApi();
            foreach (var item in listStoreId)
            {
                var productDetailStore = productstoreMapping.BaseService.Get(q => q.ProductID == model.ProductID && q.StoreID == item).FirstOrDefault();
                if (productDetailStore != null)
                {
                    productDetailStore.Price = model.Product.Price;
                    productDetailStore.DiscountPrice = model.Product.DiscountPrice;
                    productDetailStore.DiscountPercent = model.Product.DiscountPercent;
                    productstoreMapping.BaseService.Update(productDetailStore);
                }
                
            }

            await Utils.PostNotiMessageToStores(listStores, (int)NotifyMessageType.ProductChange);


            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        public void PrepareEdit(Models.ViewModels.ProductEditViewModel model, int brandId)
        {
            model.AvailableCategories =
                (new ProductCategoryApi().GetProductCategoriesByBrandId(brandId))
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.CatID == q.CateID);
            model.AvailableCategoriesMemberCard =
                (new ProductCategoryApi().GetProductCategoriesByBrandId(brandId).Where(a => a.Type == (int)ProductCategoryType.CardPayment))
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => model.CatID == q.CateID);
            model.AvailableCollections = (new ProductCollectionApi()
                .GetActiveByBrandId(brandId))
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => model.SelectedProductCollections.Contains(q.Id));
            var availableCombos = new ProductApi().GetProductForCombo(brandId);
            var comboDetails = model.Product.ProductComboDetails ?? new List<ProductComboDetailViewModel>();
            model.ProductComboDetails = comboDetails.Where(q => q.Active);
            model.AvailableComboProducts = availableCombos
                .ToSelectList(q => q.ProductName, q => q.ProductID.ToString(), q => comboDetails.Select(a => a.ProductID).Contains(q.ProductID));
            //model.ProductComboDetails = (new ProductComboDetailApi().Get().Where(q => q.ComboID == model.ProductID));
            model.ProductSpecifications = (new ProductSpecificationApi()).Get().Where(q => q.Active && q.ProductId == model.ProductID);
        }
        #region Edit Price In Store
        public ActionResult EditInStore(int id, int storeId)
        {
            var productDetailMappingApi = new ProductDetailMappingApi();
            var model = productDetailMappingApi.GetProductDetailByStore(storeId, id);
            if (model == null)
            {
                var productApi = new ProductApi();
                var product = productApi.GetProductById(id);

                model = new ProductDetailMappingViewModel();
                model.ProductID = id;
                model.StoreID = storeId;
                model.Price = product.Price;
                model.DiscountPercent = product.DiscountPercent;
            }

            return PartialView("_EditAtStore", model);
        }
        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> EditInStore(ProductDetailMappingViewModel model, int storeId)
        {
            var productDetailMappingApi = new ProductDetailMappingApi();

            // Kiểm tra model đã được tạo hay chưa, model đã tạo có id > 0
            if (model.ProductDetailID > 0)
            {
                await productDetailMappingApi.UpdateProductDetail(model);
            }
            else
            {
                await productDetailMappingApi.CreateProductDetail(model);
            }
            return RedirectToAction("Index", "Product");

        }
        #endregion
        public async Task<JsonResult> Delete(int? id, int brandId)
        {
            var productApi = new ProductApi();
            var info = productApi
                .GetByBrandId(id.GetValueOrDefault(), brandId);

            if (info == null)
            {
                return Json(new { success = false, message = "Không tồn tại sản phẩm này" });
            }
            try
            {
                info.Product.Active = false;
                info.Product.IsAvailable = false;
                await productApi.EditAsync(id, info.Product);
                //await productApi.DeactivateAsync(id.Value);
                var listStores = new StoreApi().GetActiveStoreByBrandId(brandId).ToList();
                await Utils.PostNotiMessageToStores(listStores, (int)NotifyMessageType.ProductChange);
                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        public ActionResult ListGeneral(int productId)
        {
            var productApi = new ProductApi();
            var productGeneral = productApi.GetChildProductGeneral(productId).Where(q => (bool)q.IsAvailable);
            ViewBag.ProductGeneral = productApi.GetProductById(productId);
            return PartialView("_ListGeneral", productGeneral);
        }

        //        [Authorize(Roles = "StoreManager, Administrator")]
        public ActionResult ProductGeneralDetail(int productId)
        {
            var productApi = new ProductApi();
            ViewBag.productId = productId;
            var product = productApi.GetProductById(productId);
            if (product != null && product.GeneralProductId == null)
            {
                ViewBag.productName = product.ProductName;
                ViewBag.storeId = RouteData.Values["storeId"].ToString();
                //ViewBag.storeName = RouteData.Values["storeName"].ToString();
                return View("ProductGeneralDetail");
            }
            else
            {
                ViewBag.productName = "";
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult CreateChildGeneral(int productId, int brandId)
        {
            var productApi = new ProductApi();
            var productCategoryApi = new ProductCategoryApi();
            ViewBag.ProductGeneral = productApi.GetProductById(productId);
            IEnumerable<SelectListItem> items = from c in productCategoryApi.GetProductCategories(brandId)
                                                where c.IsDisplayed == true
                                                select new SelectListItem
                                                {
                                                    Text = c.CateName,
                                                    Value = c.CateID.ToString()

                                                };
            ViewData["CatID"] = items;
            var dictionary = new Dictionary<int, string>
            {
                {(int) SaleTypeEnum.DefaultAtStore, "Tại quán"},
                {(int) SaleTypeEnum.DefaultTakeAway, "Mang đi"}
            };

            ViewBag.SaleTypeList = new SelectList(dictionary, "Key", "Value");
            return PartialView("_CreateChildGeneral");
        }

        [HttpPost]
        public JsonResult CreateChildGeneral(ProductViewModel product)
        {
            product.IsAvailable = true;
            product.Active = true;
            product.IsMenuDisplay = true;
            product.HasExtra = false;
            product.PosX = 0;
            product.PosY = 0;
            product.ColorGroup = 0;
            product.Group = 1;
            product.IsFixedPrice = true;
            product.ProductType = 0;
            product.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultNothing;
            var productApi = new ProductApi();
            var checkedFirstChild = (productApi.GetChildProductGeneral(product.GeneralProductId ?? 0).Count() == 0);
            if (checkedFirstChild && product.SaleType == (int)OrderTypeEnum.AtStore)
            {
                product.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultAtStore;
            }
            else if (checkedFirstChild && product.SaleType == (int)SaleTypeEnum.DefaultTakeAway)
            {
                product.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultTakeAway;
            }
            productApi.Create(product);
            return Json(new
            {
                success = true,
                data = product.GeneralProductId
            });
        }

        [HttpPost]
        public ActionResult UpdateProductGeneral(string productId, string Name, string Code, string[] Attributes, string Price, string Discount, string SaleType)
        {
            try
            {
                var productApi = new ProductApi();
                var id = int.Parse(productId);
                List<int> tempAtt = new List<int>();
                var tmpatt = 0;

                var currentProduct = productApi.GetProductById(id);
                var currentGeneralProduct = productApi.GetProductById(currentProduct.GeneralProductId.Value);
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
                            currentProduct.Att1 = Attributes[index];
                            index++;
                        }
                    }
                    else if (i == 2)
                    {
                        if (i == tempAtt[i - 1])
                        {
                            currentProduct.Att2 = Attributes[index];
                            index++;
                        }
                    }
                    else
                    {
                        if (i == tempAtt[i - 1])
                        {
                            currentProduct.Att3 = Attributes[index];
                            index++;
                        }
                    }
                }
                if (currentProduct.SaleType != int.Parse(SaleType))
                {
                    currentProduct.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultNothing;
                }
                currentProduct.SaleType = int.Parse(SaleType);
                currentProduct.ProductName = Name;
                currentProduct.Code = Code;
                currentProduct.Price = double.Parse(Price);
                currentProduct.DiscountPercent = double.Parse(Discount);
                productApi.Edit(currentProduct.ProductID, currentProduct);

                //                var dataAction = "changeDefaultProductChildTakeAway";
                //                if (currentGeneralProduct.SaleType == (int) SaleTypeEnum.DefaultAtStore)
                //                {
                //                    dataAction = "changeDefaultChildProduct";
                //                } else if (currentGeneralProduct.SaleType == (int) SaleTypeEnum.DefaultNothing)
                //                {
                //                    dataAction = "";
                //                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        public ActionResult DeleteChildGeneral(int productId)
        {
            var productApi = new ProductApi();
            var product = productApi.GetProductById(productId);

            product.IsAvailable = false;
            product.Active = false;

            productApi.Edit(product.ProductID, product);
            return Json(new
            {
                success = true,
                parentId = product.GeneralProductId
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ChangeDefaultProductChild(int generalProductId, int childProductId)
        {
            var productApi = new ProductApi();
            var childProducts =
                productApi.GetChildProductGeneral(generalProductId)
                    .Where(q => q.SaleType == (int)SaleTypeEnum.DefaultAtStore)
                    .ToList();
            if (childProducts != null && childProducts.Count() > 0)
            {
                var childProduct = childProducts.FirstOrDefault(q => q.ProductID == childProductId);
                if (childProduct != null)
                {
                    foreach (var item in childProducts)
                    {
                        item.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultNothing;
                        productApi.Edit(item.ProductID, item);
                    }
                    childProduct.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultAtStore;
                    productApi.Edit(childProduct.ProductID, childProduct);
                    return Json(new { success = true, message = "Đổi thành công!" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = "Lỗi server!!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ChangeDefaultProductChildTakeAway(int generalProductId, int childProductId)
        {
            var productApi = new ProductApi();
            var childProducts =
                productApi.GetChildProductGeneral(generalProductId)
                    .Where(q => q.SaleType == (int)SaleTypeEnum.DefaultTakeAway).ToList();
            if (childProducts != null && childProducts.Count() > 0)
            {
                var childProduct = childProducts.FirstOrDefault(q => q.ProductID == childProductId);
                if (childProduct != null)
                {
                    foreach (var item in childProducts)
                    {
                        item.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultNothing;
                        productApi.Edit(item.ProductID, item);
                    }
                    childProduct.IsDefaultChildProduct = (int)SaleTypeEnum.DefaultTakeAway;
                    productApi.Edit(childProduct.ProductID, childProduct);
                    return Json(new { success = true, message = "Đổi thành công!" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = "Lỗi server!!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]


        public ActionResult LoadProductGeneralCategory(int productCategoryId)
        {
            return PartialView("_LoadProductGeneralCategory", productCategoryId);
        }

        public ActionResult LoadProductComboByCategory(int productCategoryId)
        {
            return PartialView("_LoadProductComboByCategory", productCategoryId);
        }

        public ActionResult LoadProductByCategory(int productCategoryId)
        {
            return PartialView("_LoadProductByCategory", productCategoryId);
        }

        public JsonResult GetStoreList(HmsService.Models.JQueryDataTableParamModel param, int productID, int brandId)
        {
            // if product is invalid, return empty
            if (productID <= 0)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                }, JsonRequestBehavior.AllowGet);
            }

            int count = 0;
            var storeApi = new StoreApi();
            var productDetailApi = new ProductDetailMappingApi();

            var listStore = storeApi.GetStoreEntitiesByBrand(brandId).ToList();
            List<IConvertible[]> listProductByStore = new List<IConvertible[]>();
            foreach (var store in listStore)
            {
                var productDetail = productDetailApi.GetProductDetailByStore(store.ID, productID);
                double? price = 0;
                double? discountPercent = 0;
                var isProductInStore = false;

                if (productDetail != null)
                {
                    price = productDetail.Price;
                    discountPercent = productDetail.DiscountPercent;
                    isProductInStore = true;
                }

                var productInfo = new IConvertible[]
                {
                    count++,
                    store.Name,
                    price,
                    discountPercent,
                    isProductInStore,
                    store.ID
                };

                listProductByStore.Add(productInfo);
            }


            //                .Select(s => new IConvertible[]
            //                {
            //                    ++count,
            //                    s.Name,
            //                    productDetailApi.GetPriceByStore(s.ID, productID),
            //                    productDetailApi.GetDiscountByStore(s.ID, productID),
            //                    _productDetailService.CheckProductInStore(s.ID, productID),
            //                    s.ID
            //                }).ToList();

            var rs = listProductByStore.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToArray();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = listProductByStore.Count(),
                iTotalDisplayRecords = listProductByStore.Count(),
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateAssignToStore(int productId, List<ProductDetailAssignViewModel> model)
        {
            try
            {
                var productApi = new ProductApi();
                var product = productApi.GetProductEntityById(productId);
                foreach (var log in model)
                {
                    var pd = product.ProductDetailMappings.FirstOrDefault(a => a.StoreID == log.StoreId);
                    if (log.IsChecked)
                    {
                        if (pd == null)
                        {
                            product.ProductDetailMappings.Add(new ProductDetailMapping()
                            {
                                ProductID = productId,
                                Price = log.Price,
                                DiscountPercent = log.DiscountPrice,
                                StoreID = log.StoreId
                            });
                        }
                        else
                        {
                            pd.Price = log.Price;
                            pd.DiscountPercent = log.DiscountPrice;
                        }
                        //PhuongLHK: Do not update queue directly
                        //StoreProductQueue.Instance.UpdateStoreProduct(log.StoreId, product);
                    }
                    else
                    {
                        if (pd == null) continue;
                        product.ProductDetailMappings.Remove(pd);
                        //PhuongLHK: Do not update queue directly
                        //StoreProductQueue.Instance.UpdateStoreProduct(log.StoreId, product);
                    }
                }
                productApi.EditProductEntity(product);
                new ProductDetailMappingApi().CleanUnuseProductDetail();
            }
            catch (Exception ex)
            {

                return Json("0");
            }

            return Json("1");
        }

        public JsonResult GetListGeneralProduct(int brandId)
        {
            var productApi = new ProductApi();
            var listGeneralProduct = productApi.GetGeneralProductByBrandId(brandId);
            return Json(new
            {
                success = true,
                data = listGeneralProduct.Select(q => new
                {
                    id = q.ProductID,
                    text = q.ProductName,
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCategoryByProduct(int id)
        {
            var productApi = new ProductApi();
            var product = productApi.GetProductById(id);
            if (product == null || !product.IsAvailable)
            {
                return Json(new
                {
                    success = false,
                });
            }
            return Json(new
            {
                success = true,
                data = product.CatID,
            });
        }

        public JsonResult GetGeneralProduct(int id)
        {
            var productApi = new ProductApi();
            var product = productApi.GetProductById(id);
            if (product == null || !product.IsAvailable)
            {
                return Json(new
                {
                    success = false,
                });
            }
            var Atts = new List<string>();
            if (!String.IsNullOrEmpty(product.Att1))
            {
                Atts.Add(product.Att1);
            }
            if (!String.IsNullOrEmpty(product.Att2))
            {
                Atts.Add(product.Att2);
            }
            if (!String.IsNullOrEmpty(product.Att3))
            {
                Atts.Add(product.Att3);
            }
            return Json(new
            {
                success = true,
                Atts = Atts,
                categoryId = product.CatID,
            });
        }

        //public ActionResult EditPriceInStore(JQueryDataTableParamModel param,int storeId, int productId)
        //{
        //    var productDetailApi = new ProductDetailMappingApi();
        //    var productDetail = productDetailApi.GetProductDetailByStore(storeId, productId);
        //    int count = 0;
        //    double? price = 0;
        //    double? discountPrice = 0;
        //    var isProductInStore = false;

        //    if (productDetail != null)
        //    {
        //        price = productDetail.Price;
        //        discountPrice = productDetail.DiscountPrice;
        //        isProductInStore = true;
        //    }
        //    var productInfo = new IConvertible[]
        //        {
        //            price,
        //            discountPrice,
        //            isProductInStore,
        //        };
        //}

    }
}
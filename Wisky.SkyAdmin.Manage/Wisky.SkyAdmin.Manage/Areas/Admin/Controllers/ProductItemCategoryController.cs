using HmsService.Models;
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
    [Authorize(Roles = ("BrandManager, Manager, ProductManager"))]
    public class ProductItemCategoryController : DomainBasedController
    {
        // GET: Admin/ProductItemCategory
        public ActionResult Index(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            var model = new ProductItemCategoryViewModel();
            return View(model);
        }
        public JsonResult GetAllItemCategory(JQueryDataTableParamModel param, int brandId)
        {
            var productItemCategoryApi = new ProductItemCategoryApi();
            var allItemCategory = productItemCategoryApi.GetItemCategories().Where(i => i.Active && i.BrandId == brandId);
            //var itemCategory =
            //    allItemCategory
            //        .Where(a => string.IsNullOrEmpty(param.sSearch) || a.CateName.ToLower().Contains(param.sSearch.ToLower())
            //        || a.Type.ToString().Contains(param.sSearch))
            //        .ToArray();
            //var rs = allItemCategory.Skip(param.iDisplayStart).Take(param.iDisplayLength);

            //if (param.iSortCol_0 == 2)
            //{
            //    if (param.sSortDir_0 == "asc")
            //    {
            //        rs = rs.OrderBy(i => i.CateName);
            //    }
            //    else
            //    {
            //        rs = rs.OrderByDescending(i => i.CateName);
            //    }
            //}
            //else if (param.iSortCol_0 == 3)
            //{
            //    if (param.sSortDir_0 == "asc")
            //    {
            //        rs = rs.OrderBy(i => i.Type);
            //    }
            //    else
            //    {
            //        rs = rs.OrderByDescending(i => i.Type);
            //    }
            //}
            int count = 0;
            count = param.iDisplayStart + 1;

            //vẫn chưa biết Phân loại dùng để làm gì nên gán đỡ CateID vào
            var rp =
                allItemCategory.Select(a => new IConvertible[]
                    {
                       count++,
                        a.CateName,
                        a.Type,
                        a.CateID,
                    }).ToArray();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allItemCategory.Count(),
                iTotalDisplayRecords = allItemCategory.Count(),
                aaData = rp
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCategoryJSon(int id)
        {
            var productItemCategoryApi = new ProductItemCategoryApi();

            var category = productItemCategoryApi.Get(id);
            if (category == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var json = new
                {
                    category.CateID,
                    category.CateName,
                    category.Type,
                    category.Active
                };
                return Json(new { success = true, info = json }, JsonRequestBehavior.AllowGet);
            }
        }
        public bool CreateCategory(FormCollection formCollection, int brandId)
        {
            try
            {
                var cateName = formCollection["CateName"];
                var type = formCollection["Type"];

                var itemCategory = new ProductItemCategoryViewModel()
                {
                    CateName = cateName,
                    Active = true,
                    BrandId = brandId,
                    Type = int.Parse(type),
                };

                var productItemCategoryApi = new ProductItemCategoryApi();

                productItemCategoryApi.Create(itemCategory);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
        public async Task<bool> EditCategory(FormCollection formCollection)
        {
            try
            {
                var cateID = formCollection["CateID"];
                var cateName = formCollection["CateName"];
                var type = formCollection["Type"];
                var isDisplay = formCollection["Active"];

                int id = int.Parse(cateID);


                var productItemCategoryApi = new ProductItemCategoryApi();

                var itemCategory = productItemCategoryApi.Get(id);

                if (itemCategory == null)
                {
                    return false;
                }

                itemCategory.CateName = cateName;
                itemCategory.Type = int.Parse(type);
                itemCategory.Active = bool.Parse(isDisplay);

                await productItemCategoryApi.EditAsync(id, itemCategory);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        public async Task<ActionResult> CreateEditCategory(FormCollection formCollection, int brandId)
        {
            var formMode = formCollection["FormMode"];
            var cateName = formCollection["CateName"];
            var type = formCollection["Type"];
            var isDisplay = formCollection["Active"];

            // Processing.
            if (formMode == "Create")
            {
                if (CreateCategory(formCollection, brandId))
                {
                    return Json(new { success = true, type = "create", msg = "Tạo danh mục thành công." }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, msg = "Tạo danh mục không thành công do lỗi server." }, JsonRequestBehavior.AllowGet);
            }

            if (formMode == "Edit")
            {
                if (await EditCategory(formCollection))
                {
                    return Json(new { success = true, type = "edit", msg = "Cập nhật danh mục thành công." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, type = "edit", msg = "Cập nhật danh mục không thành công." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, msg = "Lệnh thực thi không phù hợp." }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                var productItemCategoryApi = new ProductItemCategoryApi();
                var productItemApi = new ProductItemApi();

                var productItem = productItemApi.GetProductItemsByCategoryId(id).Where(a => a.IsAvailable == true);
                var productitem = productItem.Count();
                ProductItemCategoryViewModel item = productItemCategoryApi.Get(id);
                if (item != null && productitem == 0)
                {
                    item.Active = false;
                    await productItemCategoryApi.EditAsync(id, item);
                    return Content("1");
                }
                else
                {
                    return Content("0");
                }

            }
            catch (Exception e)
            {
                return Content("0");
            }

        }
    }
}
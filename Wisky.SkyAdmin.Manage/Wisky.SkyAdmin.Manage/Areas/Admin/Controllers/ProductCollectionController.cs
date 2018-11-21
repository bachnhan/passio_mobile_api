using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    //[Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class ProductCollectionController : DomainBasedController
    {
        //
        // GET: /Admin/ProductCollection/
        public ActionResult Index()
        {
            return this.View();
        }

        #region Collection

        public ActionResult GetCollections(JQueryDataTableParamModel param, int storeId, int brandId)
        {
            // Vi ham GetByStoreIdAsync, mac dinh get collection nao active, nen ko xai dc
            // var collectionList = await (new ProductCollectionApi().GetByStoreIdAsync(storeId));
            var collectionList = new ProductCollectionApi().Get().Where(c => c.BrandId == brandId);

            IEnumerable<ProductCollectionViewModel> filterCollectionList;
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filterCollectionList = collectionList.Where(
                    c => c.Name != null && c.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filterCollectionList = collectionList;
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 2:
                    filterCollectionList = sortDirection == "asc"
                        ? filterCollectionList.OrderBy(c => c.Name)
                        : filterCollectionList.OrderByDescending(c => c.Name);
                    break;
            }


            var displayedList = filterCollectionList.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = displayedList.Select(c => new IConvertible[]{
                c.Id,
                c.Link,
                c.Name,
                c.SEO,
                c.Active,

            });
            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filterCollectionList.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult CollectionDetail(string id)
        //{
        //    ViewBag.Id = id;
        //    return View();
        //}

        [HttpGet]
        public JsonResult LoadCollectionDetail(string collectionId)
        {
            try
            {
                int id = Int32.Parse(collectionId);

                var result = new ProductCollectionApi().Get().Where(c => c.Id == id).FirstOrDefault();

                return Json(new
                {
                    collection = result,
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                });
            }


        }

        //public ActionResult CreateCollection()
        //{
        //    ViewBag.Title = "Thêm mới nhóm sản phẩm";
        //    ViewBag.Id = 0;
        //    ViewBag.IsNew = true;
        //    return View("CollectionDetail", null);
        //}

        //[HttpPost]
        ////using FormData from view
        //public JsonResult UpdateCollection()
        //{
        //    int id = int.Parse(Request.Params["id"]);
        //    var name = Request.Params["name"];
        //    var des = Request.Params["des"];
        //    var active = Request.Params["active"];
        //    var SEO = Request.Params["SEO"];
        //    var SEODes = Request.Params["SEODes"];
        //    var link = Request.Params["link"];
        //    //TODO: lay store ID
        //    int storeId = this.CurrentStore.ID;
        //    try
        //    {
        //        //co collection id, goi ham update
        //        if (id != 0)
        //        {
        //            ProductCollectionViewModel model = new ProductCollectionApi().Get().Where(c => c.Id == id).FirstOrDefault();

        //            model.Id = id;
        //            model.Name = name;
        //            model.Description = des;
        //            model.Active = bool.Parse(active);
        //            model.SEO = SEO;
        //            model.SEODescription = SEODes;
        //            model.Link = link;
        //            model.StoreId = storeId;

        //            new ProductCollectionApi().Edit(id, model);
        //        }
        //        else
        //        {
        //            ProductCollectionViewModel model = new ProductCollectionViewModel()
        //            {
        //                Name = name,
        //                Active = bool.Parse(active),
        //                Description = des,
        //                SEO = SEO,
        //                SEODescription = SEODes,
        //                Link = link,
        //                StoreId = storeId,
        //            };
        //            new ProductCollectionApi().Create(model);
        //        }
        //        return Json(new
        //        {
        //            success = true
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new
        //        {
        //            success = false
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        ////}


        //[HttpPost]
        //public JsonResult ChangeCollectionStatus(string collectionId)
        //{
        //    try
        //    {
        //        int id = Int32.Parse(collectionId);
        //        var api = new ProductCollectionApi();
        //        var collection = api.Get(id);
        //        bool? status = collection.Active;
        //        if (status.HasValue)
        //        {
        //            if (status.Value == true)
        //            {
        //                collection.Active = false;
        //                api.Edit(collection.Id, collection);
        //            }
        //            else
        //            {
        //                collection.Active = true;
        //                api.Edit(collection.Id, collection);
        //            }
        //        }
        //        else
        //        {
        //            collection.Active = true;
        //            api.Edit(collection.Id, collection);
        //        }
        //        return Json(new
        //        {
        //            success = true
        //        });
        //    }
        //    catch
        //    {
        //        return Json(new
        //        {
        //            success = false
        //        });
        //    }
        //}


        public ActionResult Delete(int collectionId)
        {
            try
            {
                var productCollectionApi = new ProductCollectionApi();
                var productCollection = productCollectionApi.Get(collectionId);
                if (productCollection != null && productCollection.Active == true)
                {
                    productCollectionApi.Deactivate(collectionId);
                }
                return Json(new { success = true, message = "Xóa thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi sảy ra" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetCollectionbyId(int collectionId)
        {
            try
            {
                var productCollectionApi = new ProductCollectionApi();
                var productCollection = productCollectionApi.Get(collectionId);
                if (productCollection != null && productCollection.Active == true)
                {
                    return Json(new { success = true, collection = productCollection }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Bộ sưu tập không tồn tại hoặc đã bị khóa" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi sảy ra" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        public ActionResult Create(int brandId, int storeId, string collectionName, string collectionSEO, string picUrl, string bannerUrl, string IdUpdate)
        {
            try
            {
                var productCollectionApi = new ProductCollectionApi();
                if (IdUpdate == "" || IdUpdate == null)
                {
                    var productCollection = new ProductCollection();
                    productCollection.Active = true;
                    productCollection.BrandId = brandId;
                    productCollection.StoreId = storeId;
                    productCollection.SEO = collectionSEO;
                    productCollection.Link = picUrl;
                    productCollection.BannerUrl = bannerUrl;
                    productCollection.Name = collectionName;
                    productCollectionApi.BaseService.Create(productCollection);
                    return Json(new { success = true, message = "tạo bộ sưu tập thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var collection = productCollectionApi.Get(int.Parse(IdUpdate));
                    collection.Name = collectionName;
                    collection.SEO = collectionSEO;
                    collection.Link = picUrl;
                    collection.BannerUrl = bannerUrl;
                    productCollectionApi.Edit(collection.Id, collection);
                    return Json(new { success = true, message = "Update thành công" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Tạo bộ sưu tập thất bại, xin vui lòng thử lại" }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}
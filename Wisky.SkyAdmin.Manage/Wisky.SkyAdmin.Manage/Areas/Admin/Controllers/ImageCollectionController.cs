using System.Threading.Tasks;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.ViewModels;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Models;
using System.Linq;
using System;
using SkyWeb.DatVM.Mvc;
using HmsService.Models.Entities;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class ImageCollectionController : DomainBasedController
    {
        // GET: Admin/ImageCollection
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult IndexList(JQueryDataTableParamModel param, int storeId, int brandId)
        {
            var imageCollectionApi = new ImageCollectionApi();
            var dataResult = imageCollectionApi.GetAllByStoreId(storeId, brandId);
            int displayRecord = dataResult.Count();
            var totalRecords = dataResult.Count();
            var count = param.iDisplayStart;
            var result = dataResult.ToList().Select(q => new IConvertible[] {
                ++count,
                q.Name,
                q.ImageCollectionItems.Count(),
                q.Id
            });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = displayRecord,
                aaData = result,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IndexListImageCollection(BootgridRequestViewModel request)
        {
            var result = new ImageCollectionApi().GetAdminWithFilter(
                this.CurrentStore.ID, request.searchPhrase,
                request.current, request.rowCount, request.FirstSortTerm);

            var model = new BootgridResponseViewModel<ImageCollectionDetailsViewModel>(result);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            var info = await new ImageCollectionApi().GetAsync(id.Value);

            if (info == null)
            {
                return this.IdNotFound();
            }

            // var model = new ImageCollectionDetailsViewModel(info, this.Mapper);

            return this.View("IndexDetail", info);
        }

        public ActionResult UpdateImageItem(int imageId, string linkImage, string title, string titleEng, string description, string descriptionEng, int position)
        {
            try
            {
                var imageItemApi = new ImageCollectionItemApi();
                var imageItem = imageItemApi.Get(imageId);
                imageItem.ImageUrl = linkImage;
                imageItem.Position = position;
                imageItem.Title = title;
                //imageItem.TitleEng = titleEng;
                imageItem.Description = description;
                //imageItem.DescriptionEng = descriptionEng;

                imageItemApi.Edit(imageItem.Id, imageItem);
                return Json(new { message = "Update Thành công", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { message = "Có lỗi sảy ra", success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult IndexListDetail(JQueryDataTableParamModel param, int imageCollectionId)
        {
            var imageCollectionItemApi = new ImageCollectionItemApi();
            var listResult = imageCollectionItemApi.BaseService.Get(q => q.ImageCollectionId == imageCollectionId && q.Active == true);

            int displayRecord = listResult.Count();
            var totalRecords = listResult.Count();
            var count = 0;
            var result = listResult.OrderByDescending(q => q.Position).ToList().Select(q => new IConvertible[] {
                ++count,
                q.ImageUrl,
                q.Title,
                q.Description,
                q.Position,
                q.Id,
                //q.TitleEng,
                //q.DescriptionEng
            });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = displayRecord,
                aaData = result,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int imageId)
        {
            var imageApi = new ImageCollectionItemApi();
            var imageItem = imageApi.BaseService.Get(imageId);
            if (imageItem != null)
            {
                imageItem.Active = false;
                imageApi.BaseService.Update(imageItem);
                return Json(new { success = true, message = "Đã xóa thành công" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Hình ảnh không tồn tại" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(ImageCollectionDetailsViewModel model)
        {
            var api = new ImageCollectionApi();
            // Validate
            var info = await api
                .GetByStoreIdAsync(model.ImageCollection.Id, this.CurrentStore.ID);

            if (info == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            model.ImageCollection.StoreId = this.CurrentStore.ID;
            model.ImageCollection.Active = true;
            await api.EditAsync(model.ImageCollection, model.Items);
            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });

        }

        public ActionResult CreateImage(string linkImage, string title, string titleEng, string description, string descriptionEng, int position, int collectionId)
        {
            try
            {
                var imageItemApi = new ImageCollectionItemApi();
                var imageItem = new ImageCollectionItem
                {
                    ImageCollectionId = collectionId,
                    ImageUrl = linkImage,
                    Position = position,
                    Title = title,
                    //TitleEng = titleEng,
                    Description = description,
                    //DescriptionEng = descriptionEng,
                };

                imageItemApi.BaseService.Create(imageItem);
                return Json(new { message = "Create Thành công", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { message = "Có lỗi sảy ra", success = false }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}
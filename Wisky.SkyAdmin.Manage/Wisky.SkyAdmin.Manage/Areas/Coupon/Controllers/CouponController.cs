using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using SkyWeb.DatVM.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Collections.Generic;

namespace Wisky.SkyAdmin.Manage.Areas.Coupon.Controllers
{
    public class CouponController : DomainBasedController
    {

        private static CloudBlobContainer _imagesBlobContainer;

        public CouponController()
        {
            InitializeStorage();
        }


        // GET: CRM/Coupon
        public ActionResult CouponCampaign(int brandId)
        {
            var CouponCampaignApi = new CouponCampaignApi();
            var CouponCampaigns = CouponCampaignApi.GetAllCouponCampaigns(brandId);
            var model = new CouponCampaignEditViewModel(CouponCampaigns, this.Mapper);
            return View(model);
        }

        public async Task<ActionResult> CouponProvider()
        {
            var CouponProviderApi = new CouponProviderApi();
            var CouponProviders = CouponProviderApi.GetAllCouponProviders();
            var model = new CouponProviderEditViewModel(CouponProviders, this.Mapper);
            await this.PrepareCreateCouponProvider(model);
            return View(model);
        }

        public ActionResult Coupon(int id)
        {
            var CouponCampaignApi = new CouponCampaignApi();
            var CouponCampaign = CouponCampaignApi.Get(id);
            if (CouponCampaign == null || !CouponCampaign.IsActive)
            {
                return HttpNotFound();
            }
            return View(CouponCampaign);
        }

        public JsonResult LoadCoupon(JQueryDataTableParamModel param, int Id)
        {
            var CouponApi = new CouponApi();
            var Coupons = CouponApi.GetAllCoupons(Id);
            try
            {
                var count = param.iDisplayStart + 1;
                var searchList = Coupons.Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.SerialNumber.ToLower().Contains(param.sSearch.ToLower()));

                var rs = searchList
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                            count++,
                            a.SerialNumber,
                            a.Status,
                            a.Store?.ShortName ?? "---",
                            a.DateUse?.ToString("dd/MM/yyyy") ?? "---",
                            a.Id,
                        });

                var totalRecords = Coupons.Count();
                var totalDisplayRecords = searchList.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalDisplayRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }
        [HttpGet]
        public ActionResult CreateCoupon(int Id, int brandID)
        {
            var CouponApi = new CouponApi();
            var model = new CouponEditViewModel();
            model.CouponViewModel = new CouponViewModel();
            model.CouponViewModel.CampaginId = Id;
            model.CouponCampaign = (new CouponCampaignApi()).GetCouponCampaignById(Id);
            PrepareCreateCoupon(model, brandID);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCoupon(CouponEditViewModel model, int brandID)
        {
            if (!ModelState.IsValid)
            {
                PrepareCreateCoupon(model, brandID);
                return RedirectToAction("Coupon", new { Id = model.CouponViewModel.CampaginId });
            }
            var CouponApi = new CouponApi();

            var uploadImage = model.imageUpload;

            if (uploadImage != null && uploadImage.ContentLength != 0)
            {
                CloudBlockBlob imageBlob = await UploadAndSaveBlobAsync(uploadImage);
                model.CouponViewModel.ImageUrl = imageBlob.Uri.ToString();
            }


            await CouponApi.CreateCouponAsync(model);
            return RedirectToAction("Coupon", new { Id = model.CouponViewModel.CampaginId });
        }
        [HttpGet]
        public async Task<ActionResult> EditCoupon(int Id, int brandID)
        {
            var CouponApi = new CouponApi();
            var model = await CouponApi.GetCouponAsync(Id);
            if (model == null || !model.CouponViewModel.IsActive)
            {
                return HttpNotFound();
            }
            PrepareEditCoupon(model, brandID);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditCoupon(CouponEditViewModel model, int brandID)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PrepareEditCoupon(model, brandID);
                    return View(model);
                }

                var uploadImage = model.imageUpload;
                if (uploadImage != null && uploadImage.ContentLength != 0)
                {
                    CloudBlockBlob imageBlob = await UploadAndSaveBlobAsync(uploadImage);
                    model.CouponViewModel.ImageUrl = imageBlob.Uri.ToString();
                }

                var CouponApi = new CouponApi();
                await CouponApi.EditCouponAsync(model);
                //await CouponApi.EditAsync(model.CouponViewModel.Id, model.CouponViewModel);
                return RedirectToAction("Coupon", new { Id = model.CouponViewModel.CampaginId });
            }
            catch (Exception e)
            {
                return RedirectToAction("Coupon", new { Id = model.CouponViewModel.CampaginId });
            }
        }

        public async Task<JsonResult> DeleteCoupon(int? Id)
        {
            try
            {
                var CouponApi = new CouponApi();
                var model = await CouponApi.GetAsync(Id);
                if (model == null || !model.IsActive)
                {
                    return Json(new { success = false });
                }
                model.IsActive = false;
                await CouponApi.EditAsync(model.Id, model);
                return Json(new { success = true });
            }
            catch (System.Exception e)
            {
                return Json(new { success = false });
            }
        }

        public JsonResult LoadCouponCampaign(JQueryDataTableParamModel param, int brandId)
        {
            var CouponCampaignApi = new CouponCampaignApi();
            var CouponCampaigns = CouponCampaignApi.GetAllCouponCampaigns(brandId).Where(q => q.IsActive == true);
            //set brain id = 1 to view page
            try
            {
                var rs = CouponCampaigns
                    //.Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch.ToLower()))
                    //.Skip(param.iDisplayStart)
                    //.Take(param.iDisplayLength)
                    //.ToList()
                    .Select(a => new IConvertible[]
                        {
                        a.Id,
                        string.IsNullOrEmpty(a.Name) ? "Không xác định" : a.Name,
                        String.Format("{0:#.##}", a.Price),
                        a.StartDate?.ToString("dd/MM/yyyy") ?? "---",
                        a.EndDate?.ToString("dd/MM/yyyy") ?? "---",
                        a.Status,
                        }).ToList();
                var totalRecords = CouponCampaigns.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }

        public ActionResult CouponCampaignDetail(int id)
        {
            var api = new CouponCampaignApi();
            var CouponCampaign = api.GetCouponCampaignById(id);
            var model = new CouponEditViewModel();
            model.CouponCampaign = CouponCampaign;
            return this.View(model);
        }

        public async Task<ActionResult> CouponCampaignCreate()
        {
            var model = new CouponCampaignEditViewModel();
            await PrepareCreateCouponCampaign(model);

            return this.View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CouponCampaignCreate(CouponCampaignEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await this.PrepareCreateCouponCampaign(model);
                return this.View(model);
            }
            var CouponCampaignApi = new CouponCampaignApi();
            model.IsActive = true;
            model.Status = 0;
            await CouponCampaignApi.CreateAsync(model);
            return this.RedirectToAction("CouponCampaign", "Coupon");
        }

        public async Task<ActionResult> CouponCampaignEdit(int id)
        {
            var api = new CouponCampaignApi();
            var CouponDetail = api.GetCouponCampaignById(id);
            var model = new CouponCampaignEditViewModel(CouponDetail, this.Mapper);
            await PrepareEditCouponCampaign(model);
            return this.View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CouponCampaignEdit(CouponCampaignEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareEditCouponCampaign(model);
                return this.View(model);
            }
            var api = new CouponCampaignApi();
            await api.EditCouponCampaign(model);

            return this.RedirectToAction("CouponCampaign", "Coupon");
        }



        public async Task<ActionResult> EditCouponProvider(int id)
        {   
            var api = new CouponProviderApi();
            var CouponProvider = api.GetCouponProviderById(id);
            var model = new CouponProviderEditViewModel(CouponProvider, this.Mapper);
            
            await PrepareEditCouponProvider(model);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditCouponProvider(CouponProviderEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                
                await PrepareEditCouponProvider(model);
                return this.View(model);
            }

                var api = new CouponProviderApi();
                model.IsActive = true;
                await api.EditAsync(model.Id, model);

            return this.RedirectToAction("CouponProvider", "Coupon");
        }

        #region Change Status
       
        public async Task<ActionResult> ChangeStatus(int id, int status)
        {
            var api = new CouponCampaignApi();
            var model = await api.GetAsync(id);
            switch (status)
            {
                case 0:
                    status = 1;
                    break;
                case 1:
                    status = 2;
                    break;                
                default:
                    status = 1;
                    break;
            }
            model.Status = status;
            await api.EditAsync(model.Id, model);
            return this.RedirectToAction("CouponCampaign", "Coupon");
           
        }


        #endregion

        [HttpPost]
        public async Task<ActionResult> CouponCampaignDelete(int? id)
        {
            var CouponCamApi = new CouponCampaignApi();
            var CouponCampaign = await CouponCamApi.GetAsync(id);
            if (CouponCampaign == null)
            {
                return this.HttpNotFound();
            }
            CouponCampaign.IsActive = false;
            await CouponCamApi.EditAsync(id, CouponCampaign);
            return this.RedirectToAction("CouponCampaign", "Coupon");
        }

        public JsonResult LoadCouponProvider(JQueryDataTableParamModel param)
        {
            var CouponProviderApi = new CouponProviderApi();
            var CouponProviders = CouponProviderApi.GetAllCouponProviders().Where(a => a.IsActive == true);
            try
            {
                var rs = CouponProviders
                    //.Where(a => string.IsNullOrEmpty(param.sSearch) || a.ProviderName.ToLower().Contains(param.sSearch.ToLower()))
                    //.Skip(param.iDisplayStart)
                    //.Take(param.iDisplayLength)
                    //.ToList()
                    .Select(a => new IConvertible[]
                        {
                        a.Id,
                        string.IsNullOrEmpty(a.ProviderName) ? "Không xác định" : a.ProviderName,
                        }).ToList();
                var totalRecords = CouponProviders.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateCouponProvider(CouponProviderEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareCreateCouponProvider(model);
                return this.View(model);
            }
            var api = new CouponProviderApi();
            model.IsActive = true;
            await api.CreateAsync(model);

            return this.RedirectToAction("CouponProvider");
        }

        [HttpPost]
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    var CouponProviderApi = new CouponProviderApi();
        //    var CouponCampaignApi = new CouponCampaignApi();

        //    var CouponCampaign = CouponCampaignApi.GetActive().Where(a => a.ProviderId == id);
        //    var item = CouponCampaign.Count();

        //    var CouponProvider = await CouponProviderApi.GetAsync(id);

        //    if (CouponProvider !=null && item ==0)
        //    {
        //        CouponProvider.IsActive = false;
        //        await CouponProviderApi.EditAsync(id, CouponProvider);
        //    }
        //    //if (CouponProvider == null)
        //    //{
        //    //    return this.HttpNotFound();
        //    //}
            

        //    return this.RedirectToAction("CouponProvider", "Coupon");
        //}

        public async Task<JsonResult> Delete(int? id, int brandId)
        {
            var CouponProviderApi = new CouponProviderApi();
            var CouponCampaignApi = new CouponCampaignApi();

            var CouponCampaign = CouponCampaignApi.GetActive().Where(a => a.ProviderId == id);
            var item = CouponCampaign.Count();
            var CouponProvider = await CouponProviderApi.GetAsync(id);


            if (CouponProvider != null && item == 0)
            {
                CouponProvider.IsActive = false;
                await CouponProviderApi.EditAsync(id, CouponProvider);
                return Json(new { success = true, message = "Xóa nhà cung cấp thành công" });
            }
            else
            {
                return Json(new { success = false, message = "Không thể xóa. Tồn tại đợt khuyến mãi chứa nhà cung cấp này!" });
            }

        }

        [HttpPost]
        public async Task<string> ValidateProvider(string name)
        {
            var api = new CouponProviderApi();
            var provider = await api.GetCouponProviderByNameActive(name);
            string result = "";
            if (provider != null)
            {
                result = "duplicated";
            }
            return result;
        }

        private void PrepareCreateCoupon(CouponEditViewModel model, int brandID)
        {
            //set brain id = 1 to view page
            var storeApi = new StoreApi();
            model.AvailableStore = storeApi.GetAllStore(brandID).Where(q => q.isAvailable.Value)
                .ToSelectList(q => q.Name, q => q.ID.ToString(), q => false);
        }

        private void PrepareEditCoupon(CouponEditViewModel model, int brandID)
        {
            var storeApi = new StoreApi();
            model.AvailableStore = storeApi.GetAllStore(brandID).Where(q => q.isAvailable.Value)
            //set brain id = 1 to view page
                .ToSelectList(q => q.Name, q => q.ID.ToString(), q => model.CouponViewModel.StoreId == q.ID);
        }

        private async Task PrepareCreateCouponCampaign(CouponCampaignEditViewModel model)
        {
            model.AvailableCouponProvider = (new CouponProviderApi()
                .GetAllCouponProviders().Where(q => q.IsActive == true))
                .ToSelectList(q => q.ProviderName, q => q.Id.ToString(), q => false);
        }
        private async Task PrepareEditCouponCampaign(CouponCampaignEditViewModel model)
        {
            model.AvailableCouponProvider = (new CouponProviderApi()
                .GetAllCouponProviders().Where(q => q.IsActive == true))
                .ToSelectList(q => q.ProviderName, q => q.Id.ToString(), q => model.ProviderId == q.Id);
        }

        private async Task PrepareCreateCouponProvider(CouponProviderEditViewModel model)
        {
            model.AvailableProvider = (new ProviderApi()
                .Get().Where(q => q.IsAvailable == true)
                .ToSelectList(q => q.ProviderName, q => q.ProviderName, q => false));
        }

        private async Task PrepareEditCouponProvider(CouponProviderEditViewModel model)
        {
            model.AvailableCouponProvider = (new CouponProviderApi()
               .GetAllCouponProviders().Where(q => q.IsActive == true))
               .ToSelectList(q => q.ProviderName, q => q.Id.ToString(), q => model.Id == q.Id);
            
        }

        private void InitializeStorage()
        {
            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            _imagesBlobContainer = blobClient.GetContainerReference("images");

            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            //
            //            // Get a reference to the queue.
            //            imagesQueue = queueClient.GetQueueReference("images");
        }


        private async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase imageFile)
        {
            //            Trace.TraceInformation("Uploading image file {0}", Guid.NewGuid().ToString());

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            // Retrieve reference to a blob. 
            CloudBlockBlob imageBlob = _imagesBlobContainer.GetBlockBlobReference(blobName);
            // Create the blob by uploading a local file.
            using (var fileStream = imageFile.InputStream)
            {
                await imageBlob.UploadFromStreamAsync(fileStream);
            }

            //            Trace.TraceInformation("Uploaded image file to {0}", imageBlob.Uri.ToString());

            return imageBlob;
        }


    }
}
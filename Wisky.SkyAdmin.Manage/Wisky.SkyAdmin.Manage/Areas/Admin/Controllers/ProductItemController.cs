using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Areas.Admin.Models.ViewModels;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = ("BrandManager, Manager, ProductManager"))]
    public class ProductItemController : DomainBasedController
    {



        private static CloudBlobContainer _imagesBlobContainer;

        public ProductItemController()
        {
            InitializeStorage();
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


        #region Index
        public ActionResult Index(int brandId, int storeId)
        {
            var productItemCategoryApi = new ProductItemCategoryApi();
            var categories = productItemCategoryApi.GetItemCategories().Where(a => a.Active && a.BrandId == brandId).ToList();
            ViewBag.storeId = storeId.ToString();

            return this.View(categories);
        }
        public JsonResult LoadProductItemByCategory(JQueryDataTableParamModel param, int productCategoryId, int brandId)
        {
            IEnumerable<ProductItemViewModel> listProductItem;
            var productItemApi = new ProductItemApi();

            if (productCategoryId == -1)
            {
                listProductItem = productItemApi.Get().Where(item => item.IsAvailable == true && item.ItemCategory.BrandId == brandId);
            }
            else
            {
                listProductItem = productItemApi.GetProductItemsByCategoryId(productCategoryId).Where(item => item.IsAvailable == true && item.ItemCategory.BrandId == brandId);
            }


            var query =
              listProductItem
              .Where(
                  i => string.IsNullOrEmpty(param.sSearch) || i.ItemName.ToLower().Contains(param.sSearch.ToLower())
                 )
                  .OrderByDescending(q => q.CreateDate)
                  .OrderBy(q => q.IndexPriority);
            var paging = query.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            // Col 2 : name
            // Col 3: unit
            // Col 4: price
            if (param.iSortCol_0 == 2)
            {
                if (param.sSortDir_0 == "asc")
                {
                    paging = paging.OrderBy(i => i.ItemName);
                }
                else
                {
                    paging = paging.OrderByDescending(i => i.ItemName);
                }
            }
            else if (param.iSortCol_0 == 3)
            {
                if (param.sSortDir_0 == "asc")
                {
                    paging = paging.OrderBy(i => i.Unit);
                }
                else
                {
                    paging = paging.OrderByDescending(i => i.Unit);
                }
            }
            else if (param.iSortCol_0 == 4)
            {
                if (param.sSortDir_0 == "asc")
                {
                    paging = paging.OrderBy(i => i.Price);
                }
                else
                {
                    paging = paging.OrderByDescending(i => i.Price);
                }
            }
            var index = param.iDisplayStart;
            var result = paging.Select(i => new IConvertible[]
                {
                         ++index,
                         i.ImageUrl,
                         i.ItemName,
                         i.Unit,
                         i.Price,
                         i.ItemID,
                });
            return Json(
                new
                {
                    aaData = result.ToList(),
                    sEcho = param.sEcho,
                    iTotalRecords = listProductItem.Count(),
                    iTotalDisplayRecords = query.Count(),
                }
                , JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProvidersByBrand(int brandId)
        {
            var count = 0;
            List<dynamic> listDt = new List<dynamic>();
            var providerApi = new ProviderApi();

            var providers = providerApi.GetProvidersByBrand(brandId)
                .Select(q => new
                {
                    No = ++count,
                    Name = q.ProviderName,
                    Address = q.Address,
                    Phone = q.Phone,
                    Id = q.Id,
                });
            return Json(new { data = providers.ToList() }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProviders(int brandId)
        {
            var count = 0;
            List<dynamic> listDt = new List<dynamic>();
            var providerApi = new ProviderApi();

            var providers = providerApi.GetProviders()
                .Where(q => q.IsAvailable == true && q.BrandId == brandId)
                .Select(q => new
                {
                    No = ++count,
                    Name = q.ProviderName,
                    Address = q.Address,
                    Phone = q.Phone,
                    Id = q.Id,
                });
            return Json(new { data = providers.ToList() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create
        public ActionResult Create(int brandId, int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            var productItemCategoryApi = new ProductItemCategoryApi();

            var model = new ProductItemEditViewModel();
            model.ProductItemType = ProductItemType.Materials;
            ViewBag.storeId = storeId.ToString();
            PrepareEdit(model, brandId);
            return this.View(model);
        }
        [HttpPost]
        public async Task<JsonResult> Create(ProductItemEditViewModel productItemModel)
        {
            //test
            CloudBlockBlob imageBlob = null;
            var uploadImage = productItemModel.uploadImage;

            var productItemApi = new ProductItemApi();
            var providerMappingApi = new ProviderProductItemMappingApi();

            if (uploadImage != null && uploadImage.ContentLength != 0)
            {
                imageBlob = await UploadAndSaveBlobAsync(uploadImage);
                productItemModel.ImageUrl = imageBlob.Uri.ToString();
                productItemModel.SelectedImage = imageBlob.Uri.ToString();
            }

            productItemModel.CreateDate = DateTime.Now;
            productItemModel.IsAvailable = true;
            productItemModel.ItemType = (int)productItemModel.ProductItemType;
            //productItemModel.ImageUrl = productItemModel.SelectedImage;
            try
            {
                await productItemApi.CreateProductItem(productItemModel);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Tạo nguyên liệu thất bại." });
            }

            return Json(new { success = true, message = "Tạo nguyên liệu thành công." });
        }
        public JsonResult checkItemCode(string code)
        {
            var productItemApi = new ProductItemApi();
            var result = productItemApi.Get()
                .Where(q => q.IsAvailable == true && q.ItemCode == code)
                .ToList();

            if (result.Count == 0)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        #endregion

        #region Edit
        public async Task<ActionResult> Edit(int Id, int brandId, int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            var productItemApi = new ProductItemApi();
            var model = new ProductItemEditViewModel((await productItemApi.GetProductItemById(Id)), this.Mapper);
            PrepareEdit(model, brandId);
            ViewBag.storeId = storeId.ToString();
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Edit(ProductItemEditViewModel model, int brandId)
        {
            //Set cứng, chưa làm
            model.ImageUrl = "";
            model.ItemType = (int)model.ProductItemType;
            model.ImageUrl = model.SelectedImage;
            var productItemApi = new ProductItemApi();
            await productItemApi.EditProductItemAsync(model);
            PrepareEdit(model, brandId);
            return Json(new { success = "true" });
        }
        public JsonResult checkItemCodeEdit(string originCode, string insertCode)
        {
            var productItemApi = new ProductItemApi();

            if (originCode != insertCode)
            {
                var result = productItemApi.Get()
                .Where(q => q.IsAvailable == true && q.ItemCode == insertCode)
                .ToList();
                if (result.Count == 0)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }

            return Json(new { success = true });
        }
        #endregion

        private void PrepareEdit(ProductItemEditViewModel model, int brandId)
        {
            var productItemCategoryApi = new ProductItemCategoryApi();
            var providerApi = new ProviderApi();
            model.ProductItemType = (ProductItemType)(model.ItemType.HasValue ? model.ItemType.Value : 1);
            model.AvailableCate = productItemCategoryApi.GetItemCategoryByBrand(brandId).Where(p => p.Active).Select(q => new SelectListItem()
            {
                Text = q.CateName,
                Value = q.CateID.ToString(),
                Selected = q.CateID == model.CatID ? true : false,
            });
            model.AvailableProvider = providerApi.GetProvidersByBrand(brandId).Select(q => new SelectListItem()
            {
                Text = q.ProviderName,
                Value = q.Id.ToString(),
                Selected = false,
            });
        }

        #region Delele
        public async Task<JsonResult> Delete(int id)
        {
            var api = new ProductItemCompositionMappingApi();
            var productItemApi = new ProductItemApi();

            var productItem = api.GetActive().Where(q => q.ItemID == id);
            var item = productItem.Count();
            //try
            //{
                var model = productItemApi.Get(id);
                if (model != null && item ==0)
                {
                    model.IsAvailable = false;
                    await productItemApi.EditAsync(id, model);
                    return Json(new { success = true, message = "Xóa nguyên liệu thành công" });
                }
                return Json(new { success = false, message = "Không thể xóa. Tồn tại sản phẩm chứa nguyên liệu này!" });
            //}
            //catch (Exception)
            //{
            //    return Content("0");
            //}

        }
        #endregion


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
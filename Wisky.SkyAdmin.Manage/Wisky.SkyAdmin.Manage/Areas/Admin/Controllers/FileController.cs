using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using ElFinder;
using HmsService.ViewModels;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    //[Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class FileController : DomainBasedController
    {
        private static CloudBlobContainer _imagesBlobContainer;

        public FileController()
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
            _imagesBlobContainer = blobClient.GetContainerReference("wiskycontent/WiskyCloudImage");

            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            //
            //            // Get a reference to the queue.
            //            imagesQueue = queueClient.GetQueueReference("images");
        }

        [HttpPost]
        public async Task<ActionResult> UploadImage()
        {
            CloudBlockBlob imageBlob = null;
            var imageFile = Request.Files["file"];
            ProductImageViewModel productImage = new ProductImageViewModel()
            {
                Position = 0,
                Active = true
            };

            // A production app would implement more robust input validation.
            // For example, validate that the image file size is not too large.
            //            if (ModelState.IsValid)
            //            {
            if (imageFile != null && imageFile.ContentLength != 0)
            {
                imageBlob = await UploadAndSaveBlobAsync(imageFile);
                productImage.ImageUrl = imageBlob.Uri.ToString();
            }
            //                ad.PostedDate = Utils.GetCurrentDateTime();
            //                Trace.TraceInformation("Created AdId {0} in database", ad.AdId);

            //                if (imageBlob != null)
            //                {
            //                    var queueMessage = new CloudQueueMessage(ad.AdId.ToString());
            //                    await imagesQueue.AddMessageAsync(queueMessage);
            ////                    Trace.TraceInformation("Created queue message for AdId {0}", ad.AdId);
            //                }
            return Json(new { success = true, imageUrl = productImage.ImageUrl }, JsonRequestBehavior.AllowGet);
            //            }

        }

        [HttpPost]
        public async Task<ActionResult> UploadImages()
        {
            CloudBlockBlob imageBlob = null;
            List<string> productImgs = new List<string>();

            if (Request.Files.Count != 0)
            {
                foreach (string item in Request.Files)
                {
                    var file = Request.Files[item];
                    imageBlob = await UploadAndSaveBlobAsync(file);
                    productImgs.Add(imageBlob.Uri.ToString());
                }
            }

            return Json(new { success = true, imagesUrl = productImgs }, JsonRequestBehavior.AllowGet);
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

        private Connector _connector;
        // GET: Admin/File
        public ActionResult Index()
        {
            return View();
        }
        public Connector Connector
        {
            get
            {
                if (_connector == null)
                {
                    var storeId = this.CurrentStore.ID;
                    var storeName = this.CurrentStore.Name;
                    FileSystemDriver driver = new FileSystemDriver();
                    var directory = new DirectoryInfo(Server.MapPath("~/Images/uploaded/" + storeId));
                    if (!directory.Exists)
                    {
                        directory.Create();
                        directory.Refresh();
                    }

                    //var directory02 = new DirectoryInfo(Server.MapPath("../"));
                    DirectoryInfo thumbsStorage = directory;
                    driver.AddRoot(new Root(directory, "/Images/uploaded/" + storeId + "/")
                    {
                        Alias = "/" + storeName,
                        StartPath = directory,
                        ThumbnailsStorage = thumbsStorage,
                        MaxUploadSizeInMb = 2.2,
                        ThumbnailsUrl = "/",
                        Directory = directory,
                    });
                    _connector = new Connector(driver);
                }
                return _connector;
            }
        }

        public ActionResult LoadFile()
        {
            return Connector.Process(this.HttpContext.Request);
        }

        [HttpPost]
        public ActionResult SelectFile(List<String> values)
        {
            //var directory = new DirectoryInfo(Path.Combine(Directory.GetParent(Server.MapPath("")).Parent.FullName, "images/uploaded"));
            string rootUrl = "\\Images\\uploaded\\";
            //new Uri("/Images/uploaded").ToString();
            var json = new List<String>();
            foreach (var file in values)
            {
                //@"\Images\" 
                var sliceString = Connector.GetFileByHash(file).FullName.Split(new string[] { rootUrl }, StringSplitOptions.None);
                //string[] sliceString = Regex.Split(Connector.GetFileByHash(file).FullName, @"\VC\");
                var url = rootUrl + sliceString[1];
                url = url.Replace(@"\", "/").Replace(@"\\", "/");
                json.Add(url);
            }
            return Json(json);
        }

        public ActionResult Thumbs(string tmb)
        {
            return Connector.GetThumbnail(Request, Response, tmb);
        }

        public ActionResult GetImageFromElfinder(string elementId)
        {
            return View("_ElfinderTextBox", model: elementId);
        }
        public ActionResult BrowseFile()
        {
            return View("_Elfinder");
        }
        // GET: Admin/File
        public JsonResult Upload()
        {
            //write your handler implementation here.
            this.HttpContext.Response.ContentType = "text/plain";

            var file = Request.Files["file"];
            var newFileName = "";
            if (file != null && file.ContentLength > 0)
            {
                var fileExtension = Path.GetExtension(file.FileName);
                newFileName = Guid.NewGuid().ToString() + fileExtension;
                var path = string.Format("{0}\\{1}", Server.MapPath("~/ImageUpload"), newFileName);
                file.SaveAs(path);
            }
            this.HttpContext.Response.Write(newFileName);
            return Json(new
            {
                success = true
            });
        }

        public FileResult UploadCanvas(string folder, string imageData, string fileName, int storeId)
        {
            byte[] data;
            var fileExtension = ".png";
            var newFileName = Guid.NewGuid().ToString() + fileExtension;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                newFileName = fileName + fileExtension;
            }
            var path = string.Format("{0}\\{1}\\{2}", Server.MapPath("~/ImageUpload"), storeId, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filePath = string.Format("{0}\\{1}", path, newFileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    data = Convert.FromBase64String(imageData);
                    bw.Write(data);
                    bw.Close();
                }
                fs.Close();
            }
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + folder + ".png\"");
            return File(data, "image/png");
        }
    }
}
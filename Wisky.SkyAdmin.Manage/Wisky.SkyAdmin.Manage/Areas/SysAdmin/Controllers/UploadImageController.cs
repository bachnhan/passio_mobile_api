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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class UploadImageController : DomainBasedController
    {
        private static CloudBlobContainer _imagesBlobContainer;


        // GET: SysAdmin/UploadImage
        public UploadImageController()
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

        [HttpPost]
        public async Task<ActionResult> UploadImage()
        {
            CloudBlockBlob imageBlob = null;
            var imageFile = Request.Files["file"];
            string imageUrl = "";

            // A production app would implement more robust input validation.
            // For example, validate that the image file size is not too large.
            //            if (ModelState.IsValid)
            //            {
            if (imageFile != null && imageFile.ContentLength != 0)
            {
                imageBlob = await UploadAndSaveBlobAsync(imageFile);
                imageUrl = imageBlob.Uri.ToString();
            }
            //                ad.PostedDate = Utils.GetCurrentDateTime();
            //                Trace.TraceInformation("Created AdId {0} in database", ad.AdId);

            //                if (imageBlob != null)
            //                {
            //                    var queueMessage = new CloudQueueMessage(ad.AdId.ToString());
            //                    await imagesQueue.AddMessageAsync(queueMessage);
            ////                    Trace.TraceInformation("Created queue message for AdId {0}", ad.AdId);
            //                }
            return Json(new { success = true, imageUrl = imageUrl }, JsonRequestBehavior.AllowGet);
            //            }

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
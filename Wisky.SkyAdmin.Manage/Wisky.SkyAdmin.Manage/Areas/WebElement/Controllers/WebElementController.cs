using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.ViewModels;
using AutoMapper;
using HmsService.Sdk;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Wisky.SkyAdmin.Manage.Areas.WebElement.Controllers
{
    
    public class WebElementController : Controller
    {       
        // GET: WebElement/WebElement
        public ActionResult Index(int webElementTypeId)
        {
            var api = new WebElementTypeApi();
            var model = api.Get(webElementTypeId);
            return View(model);
        }

        public JsonResult GetWebElement(int webElementTypeId)
        {
            var api = new WebElementApi();

            var count = 0;
            var items = api.GetActive().Where(q => q.ParentId == webElementTypeId);
            var results = items
                    .Select(a => new IConvertible[]
                    {
                       ++count,                       
                       a.ImageUrl ?? "",
                       a.Name,
                       a.Description ?? "",
                       a.Link ?? "",
                       a.Detail ?? "",
                       a.Id
                    }).ToArray();

            return Json(results);
        }

        public ActionResult CreateWebElement(int brandId, int parentId, String name, String description, String detail, String link, String imageUrl)
        {
            bool success = false;
            try
            {
                var api = new WebElementApi();
                var model = new WebElementViewModel { ParentId = parentId, Name = name, Description = description, Detail = detail, Active = true, BrandId = brandId, Link = link, ImageUrl = imageUrl };
                api.Create(model);
                success = true;
            }
            catch
            {
                success = false;
            }

            return Json(success);
        }

        public ActionResult prepareEdit(int WebElementId)
        {
            var api = new WebElementApi();
            var model = api.Get(WebElementId);
            return Json(model);
        }

        public ActionResult EditWebElement(int brandId, int id, String name, String description, String detail, String link, int parentId, String imageUrl)
        {
            bool result = false;
            var api = new WebElementApi();
            var model = new WebElementViewModel
            {
                ParentId = parentId,
                Id = id,
                Name = name,
                Detail = detail,
                Description = description,
                Link = link,
                ImageUrl = imageUrl,
                BrandId = brandId,
                Active = true 
                  
            };
            try
            {
                api.Edit(id, model);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(result);
        }
        public ActionResult DeleteElement(int ElementChilId)
        {
            var result = true;
            var api = new WebElementApi();
            var entity = api.Get(ElementChilId);

            if (entity == null)
            {
                result = false;

            }
            try
            {
                entity.Active = false;
                api.Edit(ElementChilId, entity);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return Json(result);
        }       
        
    }
}
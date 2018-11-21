using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.Models;
using System.Threading.Tasks;
using HmsService.ViewModels;

namespace Wisky.SkyAdmin.Manage.Areas.WebElement.Controllers
{
    public class WebElementTypeController : Controller
    {
        // GET: WebElement/WebElementType
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetWebElementType()
        {            
            int count = 1;
            var api = new WebElementTypeApi();
            var datatable = api.GetActive().ToList().Select(q => new IConvertible[] {
                            count++,
                            q.ImageUrl,
                            q.Name,
                            Utils.GetEnumDescription((ElementTypeTemplate) q.Template),
                            q.Link,                            
                            q.Description,
                            q.Id
            });
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);            
        }

        public ActionResult CreateElementType(int brandId,String name, int? template, String description, bool? showOnPage, String link, String imageUrl)
        {
            bool result = false;
            var api = new WebElementTypeApi();
            var model = new WebElementTypeViewModel
            {
                Name = name,
                Template = template,
                Description = description,
                ShowOnContentPage = showOnPage,
                Link = link,
                BrandId = brandId,
                Active = true,
                ImageUrl = imageUrl
            };
            try
            {
                api.Create(model);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(result);
        }

        public ActionResult prepareEdit(int WebElementTypeId)
        {
            var api = new WebElementTypeApi();
            var model = api.Get(WebElementTypeId);
            return Json(model);
        }

        public ActionResult EditWebElementType(int brandId, int Id, String name, int? template, String description, bool? showOnPage, String link, String imageUrl)
        {
            bool result = false;
            var api = new WebElementTypeApi();
            var model = new WebElementTypeViewModel
            {
                Id = Id,
                Name = name,
                Template = template,
                Description = description,
                ShowOnContentPage = showOnPage,
                Link = link,
                BrandId = brandId,
                ImageUrl = imageUrl,
                Active = true
            };
            try
            {
                api.Edit(Id, model);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(result);
        }
        
        public ActionResult DeleteElementType (int ElementTypeId)
        {
            var result = true;
            var api = new WebElementTypeApi();
            var entity = api.Get(ElementTypeId);

            if (entity == null)
            {
                result = false;
                                
            }
            try
            {
                entity.Active = false;
                api.Edit(ElementTypeId, entity);
            }catch (Exception ex)
            {
                result = false;
            }
            return Json(result);
        }
    }
}
using HmsService.Models;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class SettingPositionController : DomainBasedController
    {
        // GET: Admin/SettingPosition
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetCollection(int storeId)
        {
            var collectionApi = new BlogPostCollectionApi();
            var data = collectionApi.GetActiveByStoreId(storeId).Select(a => new { key = a.Id, value = a.Name });
            return Json(new { result = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdatePosition(string idchange, string position)
        {
            try
            {
                var collectionitem = new BlogPostCollectionItemApi();
                var data = collectionitem.BaseService.Get(int.Parse(idchange));
                data.Position = int.Parse(position);
                collectionitem.BaseService.Update(data);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult indexlist(JQueryDataTableParamModel param, int collectionId, int storeId)
        {
            var blogservice = new BlogPostApi();
            var collectionService = new BlogPostCollectionItemApi();

            var listBlog = blogservice.BaseService.Get(q => q.Active == true && storeId == q.StoreId).AsEnumerable();
            IEnumerable<HmsService.Models.Entities.BlogPostCollectionItem> listCollection;
            if (collectionId == 0)
            {
                listCollection = collectionService.BaseService.Get(q => q.Active == true);
            }
            else
            {
                listCollection = collectionService.BaseService.Get(q => q.Active == true && q.BlogPostCollectionId == collectionId);
            }
            var totalRecords = listCollection.Count();
            var count = param.iDisplayStart;
            var jointable = from t1 in listBlog
                            join t2 in listCollection
                            on t1.Id equals t2.BlogPostId
                            select new
                            {
                                t1.Title,
                                t1.Image,
                                t2.Position,
                                t2.BlogPostCollection.Name,
                                t2.BlogPostCollectionId,
                                t1.CreatedTime,
                                t1.UpdatedTime,
                                t2.Id
                            };
            var listdata = jointable.OrderBy(q => q.Position);
            var data = listdata.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList().Select(a => new IConvertible[] {
                ++count,
                a.Title,
                a.Image,
                a.Name,
                a.Position,
                a.CreatedTime.ToString("MM/dd/yyyy"),
                a.UpdatedTime.ToString("MM/dd/yyyy"),
                a.BlogPostCollectionId,
                a.Id
            }).ToList();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = data
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
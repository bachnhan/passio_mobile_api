using HmsService.Models;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    [Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class DefaultController : DomainBasedController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Collections()
        {
            return View();
        }

        public ActionResult Products()
        {
            return View();
        }

        public ActionResult Blogs()
        {
            return View();
        }

        public ActionResult Page()
        {
            return View();
        }

        public ActionResult CreateProduct()
        {
            return View();
        }

        public ActionResult CreateCollection()
        {
            return View();
        }

        public ActionResult CreateBlogPost()
        {
            return View();
        }

        public ActionResult CreatePage()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Map()
        {
            return View();
        }

        public ActionResult CreateGallery()
        {
            return View();
        }

        public ActionResult OrderList()
        {
            return View();
        }

    }
}
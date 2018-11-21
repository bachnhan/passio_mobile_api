using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class DefaultController : Controller
    {
        // GET: SysAdmin/Default
        public ActionResult Index()
        {
            return this.View();
        }
    }
}
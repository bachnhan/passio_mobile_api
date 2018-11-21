using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class MenuController : BaseController
    {
        // GET: SysAdmin/Menu
        public ActionResult Index()
        {
            var model = new MenuEditViewModel();
            model.DropDownForAreas = GetAllContextAreas().Where(q => q != "SysAdmin")
                .Select(q => new SelectListItem() { Text = q, Value = q});
            return View(model);
        }

        public ActionResult GetAllControllerInArea(string area)
        {
            var controllers = GetControllers(area).Select(q => q.Name.Replace("Controller", ""));
            return Json(controllers, JsonRequestBehavior.AllowGet);
        }

        IEnumerable<string> GetAllContextAreas()
        {
            return RouteTable.Routes.OfType<Route>()
            .Where(d => d.DataTokens != null && d.DataTokens.ContainsKey("area"))
            .Select(r => r.DataTokens["area"].ToString());
        }

        IEnumerable<Type> GetControllers(string area)
        {

            IEnumerable<Type> controllerNames = Assembly.GetCallingAssembly().GetTypes().Where(
            type => type.IsSubclassOf(typeof(Controller)) && type.Namespace == "Wisky.SkyAdmin.Manage.Areas." + area + ".Controllers");
            return controllerNames;
        }

        public ActionResult GetAllActionsInController(string ctrl, string area)
        {
            var controllerType = GetControllers(area).FirstOrDefault(q => q.Name == ctrl + "Controller");
            var actions = new List<string>();
            if (controllerType != null)
            {
                actions = GetAllActionsViaControllerType(controllerType).ToList();
            }
            return Json(actions, JsonRequestBehavior.AllowGet);
        }
        IEnumerable<string> GetAllActionsViaControllerType(Type controller)
        {
            return new ReflectedControllerDescriptor(controller)
        .GetCanonicalActions().Select(x => x.ActionName)
        .ToList();
        }

    }
}
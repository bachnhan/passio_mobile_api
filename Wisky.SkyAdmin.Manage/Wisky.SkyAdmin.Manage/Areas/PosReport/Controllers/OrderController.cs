using HmsService.Models;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.PosReport.Controllers
{
    [Authorize(Roles = "BrandManager, Manager, Reception, StoreReportViewer")]
    public class OrderController : Controller
    {
        // GET: PosReport/Order
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult LoadOrder(JQueryDataTableParamModel param, string _date,int brandId)
        {
            var orderApi = new OrderApi();
            var rpDate = _date.ToDateTime();

            var startTime = rpDate.GetStartOfDate();
            var endTime = rpDate.GetEndOfDate();

            var listOrder = orderApi.GetAllOrderByDate(startTime, endTime, brandId);
            int count = 0;

            try
            {
                count = param.iDisplayStart + 1;
                var rs = listOrder
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                        a.OrderDetailsTotalQuantity,
                        a.TotalAmount,
                        (a.Discount + a.DiscountOrderDetail),
                        a.FinalAmount,
                        a.CheckInDate.Value.ToShortDateString() + " " + a.CheckInDate.Value.ToShortTimeString(),
                        a.OrderType,
                        a.CheckInPerson,
                       //a.Store.Name
                        });
                //.ToArray()
                var totalRecords = listOrder.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }




        }
    }
}
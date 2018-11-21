using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Areas.DashBoard.Models;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.DashBoard.Controllers
{
    public class DashBoardController : DomainBasedController
    {
        // GET: DashBoard/DashBoard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDashboardPartial(int? storeId, int brandId)
        {
            //var storeApi = new StoreApi();

            //ViewBag.storeId = storeId;
            //ViewBag.CurrentStore = storeApi.Get(storeId);
            //ViewBag.StoreName = storeApi.Get(storeId).Name;
            //DashboardInfo info = new DashboardInfo();
            //var orderApi = new OrderApi();
            //var orderDetailApi = new OrderDetailApi();
            //IEnumerable<Order> report;
            //IEnumerable<Order> reportCancel;
            //IEnumerable<Order> reportPreCancel;
            //IEnumerable<OrderDetail> reportOrderCancel;
            //#region List all order
            //if ((int)storeId > 0)
            //{
            //    report = orderApi.GetRentsByTimeRange2((int)storeId, DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate())
            //        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
            //}
            //else
            //{
            //    report = orderApi.GetAllOrderByDate2(DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate(), brandId)
            //        .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish).ToList();
            //}
            //#endregion

            //#region List Cancel order
            //if ((int)storeId > 0)
            //{
            //    reportCancel = orderApi.GetRentsByTimeRange2((int)storeId, DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate())
            //        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel || a.OrderStatus == (int)OrderStatusEnum.PosCancel).ToList();
            //}
            //else
            //{
            //    reportCancel = orderApi.GetAllOrderByDate2(DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate(), brandId)
            //        .Where(a => a.OrderStatus == (int)OrderStatusEnum.Cancel || a.OrderStatus == (int)OrderStatusEnum.PosCancel).ToList();
            //}
            //#endregion

            //#region List Pre Cancel order

            //if ((int)storeId > 0)
            //{
            //    reportPreCancel = orderApi.GetRentsByTimeRange2((int)storeId, DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate())
            //        .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel || a.OrderStatus == (int)OrderStatusEnum.PosPreCancel).ToList();
            //}
            //else
            //{
            //    reportPreCancel = orderApi.GetAllOrderByDate2(DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate(), brandId)
            //        .Where(a => a.OrderStatus == (int)OrderStatusEnum.PreCancel || a.OrderStatus == (int)OrderStatusEnum.PosPreCancel).ToList();
            //}
            //#endregion

            //#region List Cancel OrderDetail
            //if ((int)storeId > 0)
            //{               
            //    reportOrderCancel = orderDetailApi.GetOrderDetailsByTimeRange(DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate(), (int)storeId)
            //        .Where(a=>a.Status ==(int)OrderStatusEnum.PosPreCancel || a.Status==(int)OrderStatusEnum.PosCancel).ToList();
            //}
            //else
            //{
            //    reportOrderCancel = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, DateTime.Now.GetStartOfDate(), DateTime.Now.GetEndOfDate())
            //         .Where(a =>a.Status ==(int)OrderStatusEnum.PosPreCancel || a.Status == (int)OrderStatusEnum.PosCancel).ToList();
            //}
            //#endregion

            //if (report!=null)
            //{
            //    info.FinalAmount = report.Sum(item => item.FinalAmount);
            //    info.TotalAmount = report.Sum(item => item.TotalAmount);
            //    info.TotalDiscount = report.Sum(a => a.Discount) + report.Sum(a => a.DiscountOrderDetail);
            //    info.TotalCancel = reportCancel.Sum(a => a.TotalAmount);
            //    info.TotalPreCancel = reportPreCancel.Sum(a => a.TotalAmount);
            //    info.TotalOrderCancel = reportOrderCancel.Sum(a => a.TotalAmount);


            //}
            //else
            //{
            //    info.FinalAmount = 0;
            //    info.TotalAmount = 0;
            //    info.TotalDiscount = 0;
            //    info.TotalCancel = 0;
            //    info.TotalPreCancel = 0;
            //    info.TotalOrderCancel = 0;
            //}
            ////return PartialView("_DashboardPartial", info);
            return PartialView("_DashBoardNew");

        }
    }
}
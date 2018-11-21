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
    public class CheckDataOrderController : DomainBasedController
    {
        // GET: Admin/CheckDataOrder
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CheckOrderCard(int storeId, int brandId)
        {
            var orderApi = new OrderApi();
            var paymentApi = new PaymentApi();
            var startDate = "01/11/2017".ToDateTime().GetStartOfDate();
            var endDate = startDate.GetEndOfMonth().GetEndOfDate();
            var orders = orderApi.BaseService.Get(q => q.OrderStatus == (int)OrderStatusEnum.Finish && q.CheckInDate >= startDate && q.CheckInDate <= endDate);
            if (storeId != 0)
            {
                var totalAmount = orders.Where(q =>q.StoreID == storeId).Sum(q => q.FinalAmount);
                var payment = paymentApi.BaseService.Get(q => q.Order.OrderStatus == (int)OrderStatusEnum.Finish && q.Order.StoreID == storeId && q.PayTime >= startDate && q.PayTime <= endDate).Sum(q => q.Amount);
                var result = orders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard && q.StoreID == storeId && (q.Payments.Count()>0 ? q.FinalAmount != q.Payments.ToList().Sum(a => a.Amount) : q.FinalAmount > 0));

                var jsonResult = result.ToList().Select(q => new
                {
                    Invoice = q.InvoiceID,
                    FinalAmount = q.FinalAmount,
                    Discount = q.Discount,
                    DiscountOrderDetail = q.DiscountOrderDetail
                });
                return Json(new { jsonResult = jsonResult, FinalAmount =totalAmount , Payment = payment },JsonRequestBehavior.AllowGet);

            }
            else
            {
                var totalAmount = orders.Where(q =>q.Store.BrandId == brandId).Sum(q => q.FinalAmount);
                var payment = paymentApi.BaseService.Get(q => q.Order.OrderStatus == (int)OrderStatusEnum.Finish && q.Order.Store.BrandId == brandId && q.PayTime >= startDate && q.PayTime <= endDate ).Sum(q => q.Amount);

                var result = orders.Where(q =>q.Store.BrandId == brandId && (q.Payments.Count()>0 ? q.FinalAmount != q.Payments.ToList().Sum(a => a.Amount) : q.FinalAmount > 0));
                var jsonResult = result.ToList().Select(q => new
                {
                    Invoice = q.InvoiceID,
                    FinalAmount = q.FinalAmount,
                    Discount = q.Discount,
                    DiscountOrderDetail = q.DiscountOrderDetail
                });
                return Json(new { jsonResult = jsonResult, FinalAmount = totalAmount, Payment = payment }, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult CheckDateReport(int storeId, int brandId)
        {
            var orderApi = new OrderApi();
            var paymentApi = new PaymentApi();
            var startDate = "01/11/2017".ToDateTime().GetStartOfDate();
            var endDate = startDate.GetEndOfMonth().GetEndOfDate();
            var orders = orderApi.BaseService.Get(q => q.OrderStatus == (int)OrderStatusEnum.Finish && q.CheckInDate >= startDate && q.CheckInDate <= endDate);
            if (storeId != 0)
            {
                var totalAmount = orders.Where(q => q.StoreID == storeId).Sum(q => q.FinalAmount);
                var payment = paymentApi.BaseService.Get(q => q.Order.OrderStatus == (int)OrderStatusEnum.Finish && q.Order.StoreID == storeId && q.PayTime >= startDate && q.PayTime <= endDate).Sum(q => q.Amount);
                var result = orders.Where(q => q.OrderType == (int)OrderTypeEnum.OrderCard && q.StoreID == storeId && (q.Payments.Count() > 0 ? q.FinalAmount != q.Payments.ToList().Sum(a => a.Amount) : q.FinalAmount > 0));

                var jsonResult = result.ToList().Select(q => new
                {
                    Invoice = q.InvoiceID,
                    FinalAmount = q.FinalAmount,
                    Discount = q.Discount,
                    DiscountOrderDetail = q.DiscountOrderDetail
                });
                return Json(new { jsonResult = jsonResult, FinalAmount = totalAmount, Payment = payment }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var totalAmount = orders.Where(q => q.Store.BrandId == brandId).Sum(q => q.FinalAmount);
                var payment = paymentApi.BaseService.Get(q => q.Order.OrderStatus == (int)OrderStatusEnum.Finish && q.Order.Store.BrandId == brandId && q.PayTime >= startDate && q.PayTime <= endDate).Sum(q => q.Amount);

                var result = orders.Where(q => q.Store.BrandId == brandId && (q.Payments.Count() > 0 ? q.FinalAmount != q.Payments.ToList().Sum(a => a.Amount) : q.FinalAmount > 0));
                var jsonResult = result.ToList().Select(q => new
                {
                    Invoice = q.InvoiceID,
                    FinalAmount = q.FinalAmount,
                    Discount = q.Discount,
                    DiscountOrderDetail = q.DiscountOrderDetail
                });
                return Json(new { jsonResult = jsonResult, FinalAmount = totalAmount, Payment = payment }, JsonRequestBehavior.AllowGet);

            }

        }
    }
}
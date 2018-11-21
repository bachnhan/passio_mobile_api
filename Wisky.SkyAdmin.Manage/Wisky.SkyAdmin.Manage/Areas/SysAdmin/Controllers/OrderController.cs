using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.Models;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HmsService.ViewModels;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class OrderController : Controller
    {
        // GET: SysAdmin/WebRouteSetting
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult GetListStoreTheme()
        {
            StoreApi storeApi = new StoreApi();
            BrandApi brandApi = new BrandApi();
            var listStoreWeb = storeApi.GetActive().Where(s => s.Type == (int)StoreTypeEnum.Website);
            var listBrand = brandApi.GetActive();
            return Json(new { listStore = listStoreWeb, listBrand = listBrand }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllProducts()
        {
            try
            {
                var productApi = new ProductApi();
                var productList = productApi.GetAllProducts().Select(p => new ProductEditViewModel()
                {
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Code = p.Code,
                    ProductID = p.ProductID
                });
                if (productList != null)
                {
                    return Json(new
                    {
                        success = true,
                        list = productList
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Danh sách loại thẻ thành viên không tồn tại"
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra",
                    exception = e
                });
            }
        }

        public ActionResult GetProduct(int productId)
        {
            try
            {
                var productApi = new ProductApi();
                var product = productApi.GetProductById(productId);
                if (product != null)
                {
                    return Json(new
                    {
                        success = true,
                        product = product.Price
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Danh sách loại thẻ thành viên không tồn tại"
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra",
                    exception = e
                });
            }
        }

        public ActionResult GetOrder(JQueryDataTableParamModel param, string invoiceId)
        {
            OrderApi orderApi = new OrderApi();
            BrandApi brandApi = new BrandApi();
            var totalRecords = 0;
            var totalDisplayRecords = 0;
            OrderEditViewModel o = null;
            List<OrderEditViewModel> list = new List<OrderEditViewModel>();
            var OrderWeb = orderApi.GetOrderByInvoiceId(invoiceId);
            if (OrderWeb != null)
            {
                totalRecords = 1;
                totalDisplayRecords = 1;
                o = new OrderEditViewModel()
                {
                    DiscountOrderDetail = OrderWeb.DiscountOrderDetail,
                    InvoiceID = invoiceId,
                    TotalAmount = OrderWeb.TotalAmount,
                    Discount = OrderWeb.Discount,
                    FinalAmount = OrderWeb.FinalAmount,
                    CheckInDate = OrderWeb.CheckInDate.HasValue ? OrderWeb.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                };
                list.Add(o);
            }


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetOrderDetails(JQueryDataTableParamModel param, string invoiceId)
        {
            OrderApi orderApi = new OrderApi();
            BrandApi brandApi = new BrandApi();
            ProductApi productApi = new ProductApi();
            var totalRecords = 0;
            var totalDisplayRecords = 0;
            OrderDetailEditViewModel od = null;
            List<OrderDetailEditViewModel> list = new List<OrderDetailEditViewModel>();
            var OrderWeb = orderApi.GetOrderByInvoiceId(invoiceId);
            if (OrderWeb != null && OrderWeb.OrderDetails != null)
            {
                totalRecords = OrderWeb.OrderDetails.Count;
                totalDisplayRecords = OrderWeb.OrderDetails.Count;
                foreach (var orderDetail in OrderWeb.OrderDetails)
                {
                    var product = productApi.GetProductById(orderDetail.ProductID);
                    od = new OrderDetailEditViewModel()
                    {
                        OrderDetailID = orderDetail.OrderDetailID,
                        ProductID = orderDetail.ProductID,
                        RentID = orderDetail.RentID,
                        ProductName = product.ProductName,
                        UnitPrice = orderDetail.UnitPrice,
                        Quantity = orderDetail.Quantity,
                        Discount = orderDetail.Discount,
                        FinalAmount = orderDetail.FinalAmount,
                        Status = ((OrderDetailStatusEnum)orderDetail.Status).ToString(),
                    };
                    list.Add(od);
                }
            }
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateOrderDetail()
        {
            try
            {
                //nguyen lieu
                var productId = int.Parse(Request["productId"]);
                var quantity = int.Parse(Request["quantity"]);
                var invoiceId = Request["invoiceId"];
                var odId = int.Parse(Request["orderDetailID"]);
                var rentId = int.Parse(Request["rentId"]);
                var productApi = new ProductApi();
                var orderdetailApi = new OrderDetailApi();
                var product = productApi.GetProductById(productId);
                var orderdetail = orderdetailApi.GetOrderDetailsByRentId(rentId).Where(o => o.OrderDetailID == odId).FirstOrDefault();
                var orderApi = new OrderApi();
                var storeApi = new StoreApi();
                var order = orderApi.GetOrderByInvoiceId(invoiceId);
                //var membershipCard = membershipCardApi.GetMembershipCardById(membershipCardId);
                //update orderdetail

                if (orderdetail != null)
                {
                    orderdetail.ProductID = product.ProductID;
                    orderdetail.ProductType = product.ProductType;
                    orderdetail.TotalAmount = product.Price * quantity;
                    orderdetail.UnitPrice = product.Price;
                    orderdetail.Quantity = quantity;
                    orderdetail.OrderDate = DateTime.Now;
                    orderdetail.FinalAmount = product.Price * quantity;
                    orderdetail.Discount = 0;
                    orderdetail.OrderDetailPromotionMappingId = null;
                    orderdetail.OrderPromotionMappingId = null;
                    await orderdetailApi.BaseService.UpdateAsync(orderdetail);

                    //update order (final amount, total amount )
                    order.TotalAmount = 0;

                    foreach (var orderDetail in order.OrderDetails)
                    {
                        if (orderDetail.Status != (int)OrderDetailStatusEnum.Cancel)
                        {
                            order.TotalAmount += orderDetail.TotalAmount;
                        }
                    }
                    UpdateDiscountOrder(order);
                    await orderApi.BaseService.UpdateAsync(order);
                    var msg = new NotifyOrder()
                    {
                        StoreId = (int)order.StoreID,
                        //StoreName = store.Name,
                        NotifyType = (int)NotifyMessageType.OrderChange,
                        Content = "Có đơn hàng mới",
                        OrderId = order.RentID
                    };
                    await Utils.RequestOrderWebApi(msg);
                }
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        public async Task<JsonResult> AddOrderDetail()
        {
            try
            {
                //nguyen lieu
                var productId = int.Parse(Request["productId"]);
                var quantity = int.Parse(Request["quantity"]);
                var invoiceId = Request["invoiceId"];
                var odId = int.Parse(Request["orderDetailID"]);
                var rentId = int.Parse(Request["rentId"]);
                var productApi = new ProductApi();
                var orderdetailApi = new OrderDetailApi();
                var product = productApi.GetProductById(productId);
                //order detail entity
                var orderApi = new OrderApi();
                var storeApi = new StoreApi();
                var order = orderApi.GetOrderByInvoiceId(invoiceId);
                //var membershipCard = membershipCardApi.GetMembershipCardById(membershipCardId);
                //update orderdetail
                int length = order.OrderDetails.Count;
                OrderDetailApi orderDetailApi = new OrderDetailApi();
                OrderDetailViewModel orderDetail = new OrderDetailViewModel()
                {
                    Discount = 0,
                    FinalAmount = product.Price * quantity,
                    Quantity = quantity,
                    StoreId = order.StoreID,
                    TotalAmount = product.Price * quantity,
                    UnitPrice = product.Price,
                    Status = (int)OrderStatusEnum.New,
                    ProductName = product.ProductName,
                    ProductImage = null,
                    ProductID = product.ProductID,
                    ProductCode = product.Code,
                    OrderDate = Utils.GetCurrentDateTime(),
                    ItemQuantity = 0,
                    IsAddition = false,
                    DetailDescription = "",
                    ProductType = product.ProductType,
                    OrderDetailPromotionMappingId = null,
                    OrderPromotionMappingId = null,
                    RentID = rentId
                };
                orderDetailApi.Create(orderDetail);
                //update order
                order.TotalAmount = 0;

                foreach (var od in order.OrderDetails)
                {
                    if (od.Status != (int)OrderDetailStatusEnum.Cancel)
                    {
                        order.TotalAmount += od.TotalAmount;
                    }
                }
                UpdateDiscountOrder(order);
                await orderApi.BaseService.UpdateAsync(order);
                var msg = new NotifyOrder()
                {
                    StoreId = (int)order.StoreID,
                    //StoreName = store.Name,
                    NotifyType = (int)NotifyMessageType.OrderChange,
                    Content = "Có đơn hàng mới",
                    OrderId = order.RentID
                };
                await Utils.RequestOrderWebApi(msg);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        public async Task<JsonResult> DeleteOrderDetail()
        {
            try
            {
                //nguyen lieu
                var invoiceId = Request["invoiceId"];
                var odId = int.Parse(Request["orderDetailID"]);
                var rentId = int.Parse(Request["rentId"]);
                var productApi = new ProductApi();
                var orderdetailApi = new OrderDetailApi();
                var orderdetail = orderdetailApi.GetOrderDetailsByRentId(rentId).Where(o => o.OrderDetailID == odId).FirstOrDefault();
                var orderApi = new OrderApi();
                var storeApi = new StoreApi();
                var order = orderApi.GetOrderByInvoiceId(invoiceId);
                //var membershipCard = membershipCardApi.GetMembershipCardById(membershipCardId);
                //update orderdetail

                if (orderdetail != null)
                {
                    orderdetail.Status = (int)OrderDetailStatusEnum.Cancel;
                    await orderdetailApi.BaseService.UpdateAsync(orderdetail);
                    //update order (final amount, total amount )
                    order.TotalAmount = 0;

                    foreach (var orderDetail in order.OrderDetails)
                    {
                        if (orderDetail.Status != (int)OrderDetailStatusEnum.Cancel)
                        {
                            order.TotalAmount += orderDetail.TotalAmount;
                        }
                    }
                    UpdateDiscountOrder(order);
                    await orderApi.BaseService.UpdateAsync(order);

                    var msg = new NotifyOrder()
                    {
                        StoreId = (int)order.StoreID,
                        //StoreName = store.Name,
                        NotifyType = (int)NotifyMessageType.OrderChange,
                        Content = "Có đơn hàng mới",
                        OrderId = order.RentID
                    };
                    await Utils.RequestOrderWebApi(msg);
                }
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        private void UpdateDiscountOrder(HmsService.Models.Entities.Order order)
        {
            //Reset
            order.Discount = 0;
            order.DiscountOrderDetail = 0;
            //Tính giảm giá ở orderdetail
            foreach (var orderDetail in order.OrderDetails)
            {
                if (orderDetail.Status != (int)OrderDetailStatusEnum.Cancel)
                {
                    order.DiscountOrderDetail += (int)orderDetail.Discount;
                }
            }
            //Sau giảm giá sản phẩm
            order.FinalAmount = order.TotalAmount
                - order.DiscountOrderDetail
                - order.Discount;
        }
        public class ProductEditViewModel
        {
            public int ProductID { get; set; }
            public string ProductName { get; set; }
            public double Price { get; set; }
            public string Code { get; set; }
        }
    }
    public class OrderEditViewModel
    {
        public string InvoiceID { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double DiscountOrderDetail { get; set; }

        public double FinalAmount { get; set; }
        public string CheckInDate { get; set; }
    }

    public class OrderDetailEditViewModel
    {
        public string Status { get; set; }
        public int OrderDetailID { get; set; }
        public int RentID { get; set; }
        public string ProductName { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }
        public double FinalAmount { get; set; }
        public double UnitPrice { get; set; }
    }


}







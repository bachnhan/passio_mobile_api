//DeliveryController.cs

using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Ajax.Utilities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Delivery.Controllers
{
    [Authorize]
    public class Delivery2Controller : DomainBasedController
    {
        [Authorize(Roles = "BrandManager, Manager, Reception")]
        // GET: Delivery/Delivery
        public ActionResult Index(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }

        //[HttpGet]
        //public ActionResult PromotionApply(int brandId)
        //{
        //    PromotionApi promotionApi = new PromotionApi();
        //    promotionApi.GetPromotionByBrandId(brandId);
        //    return View("PromotionApply");
        //}

        [Authorize(Roles = "BrandManager, Manager, Reception")]
        public ActionResult Create(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }

        [Authorize(Roles = "BrandManager, Manager, Reception")]
        public ActionResult Create2(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            var groupApi = new GroupApi();
            var promotionApi = new PromotionApi();
            var promotion = promotionApi.GetPromotionByIdTime(brandId);
            //promotion = promotion.Where(q => q.)

            //List<PromotionEditViewModel> list = promotion.ToList();

            PromotionEditViewModel model = new PromotionEditViewModel();
            //model.AvailablePromotion = promotion.ToSelectList(q => q.PromotionName, q => q.PromotionID.ToString(), q => false);
            model.AvailablePromotion = promotion.OrderBy(q => q.PromotionType);

            var groupList = groupApi.GetActive();
            var list = (from p in promotion.AsEnumerable()
                        join g in groupList.AsEnumerable()
                        on p.PromotionType equals g.GroupId
                        select new { GroupId = p.PromotionType, GroupName = g.Name }).Distinct().OrderBy(q => q.GroupId);
            model.AvailableGroup = list.ToSelectList(q => q.GroupName, q => q.GroupId.ToString(), q => q.GroupName == null);
            return View(model);
        }

        public JsonResult LoadAllCategory(int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            var productCategories = productCategoryApi.GetProductCategoriesByBrandId(brandId)
                .Select(q => new
                {
                    CategoryId = q.CateID,
                    Name = q.CateName,
                });
            return Json(productCategories);
        }


        public ActionResult LoadItemByCategory(int echo, int cateId, string pattern, int brandId)
        {
            pattern = (pattern ?? "").ToLower();
            var productApi = new ProductApi();
            //products = _productService.GetProducts().Where(p => p.CatID == cateId);
            //List<ProductViewModel> products = productApi.GetProductByBrand(brandId)
            //    .Where(
            //        p =>
            //            (pattern.IsNullOrWhiteSpace() || p.ProductName.ToLower().Contains(pattern.ToLower())) &&
            //            (cateId <= 0 || p.CatID == cateId)).ToList();
            var listProductCate =
                productApi.GetProductByBrand(brandId)
                    .Where(
                        //p => p.IsFixedPrice &&
                        p =>
                             (cateId <= 0 || p.CatID == cateId) &&
                             (pattern.IsNullOrWhiteSpace() || p.ProductName.ToLower().Contains(pattern.ToLower())));
            //products.AddRange(listProductCate);
            return Json(new
            {
                echo = echo,
                products = listProductCate.Select(a => new
                {
                    image = a.PicURL == null ? "Default_product_img.jpg" : "product/" + a.PicURL,
                    name = a.ProductName,
                    id = a.ProductID,
                    discount = a.DiscountPercent,
                    price = a.Price,
                    type = a.ProductType
                })
            });
        }

        public JsonResult LoadAllCustomer(int brandId)
        {
            var customerApi = new CustomerApi();
            var customers = customerApi.GetCustomersByBrand(brandId);
            return Json(new
            {
                success = true,
                data = customers.Select(a => new
                {
                    id = a.CustomerID,
                    text = a.Name,
                    phone = a.Phone
                })
            });
        }

        [HttpPost]
        public ActionResult GetStoreCoordinateList(int brandId)
        {
            var storeApi = new StoreApi();
            var result = storeApi.GetStores().Where(a => a.Type == (int)StoreTypeEnum.Store && a.BrandId == brandId)
                .Select(q => new
                {
                    ID = q.ID,
                    Name = q.Name,
                    Address = q.Address,
                    Longitude = q.Lon,
                    Latitude = q.Lat,
                })
                .ToList();

            return Json(result);
        }

        public JsonResult GetCustomerDetail(int id)
        {
            var customerApi = new CustomerApi();
            var customer = customerApi.GetCustomerById(id);
            return Json(new
            {
                success = true,
                data = new
                {
                    name = customer.Name,
                    phone = customer.Phone,
                    address = customer.Address
                }
            });
        }

        [HttpPost]
        [Authorize(Roles = "BrandManager, Manager, Reception")]
        public async System.Threading.Tasks.Task<JsonResult> Create(OrderViewModel order, int storeId, int brandId)
        {
            DateTime time = Utils.GetCurrentDateTime();
            if (order.CustomerID == 0 && order.Customer == null)
            {
                return Json(new
                {
                    success = false,
                    msg = "Không tìm thấy khách hàng"
                });
            }
            if (order.DeliveryAddress == null)
            {
                return Json(new
                {
                    success = false,
                    msg = "Không tìm thấy địa chỉ"
                });
            }
            if (order.StoreID == null)
            {
                return Json(new
                {
                    success = false,
                    msg = "Không tìm thấy cửa hàng"
                });
            }
            double tempTotalAmount = 0;
            double tempFinalAmount = 0;
            double discountOrderDetail = 0;
            foreach (var item in order.OrderDetails)
            {
                tempFinalAmount += item.FinalAmount;
                tempTotalAmount += item.TotalAmount;
                discountOrderDetail += item.Discount;
                item.OrderDate = time;
            }
            order.Payments = new List<PaymentViewModel>();
            order.Payments.Add(new PaymentViewModel
            {
                Amount = tempTotalAmount,
                CurrencyCode = "VND",
                Status = (int)PaymentStatusEnum.New,
                Type = (int)PaymentTypeEnum.Cash,
                FCAmount = (decimal)tempFinalAmount,
                PayTime = time,
                //Username = User.Identity.Name

            });


            order.CheckInDate = time;
            order.CheckInPerson = User.Identity.Name;
            order.TotalAmount = tempTotalAmount;
            //rent.FinalAmount = tempFinalAmount;       Final Amount = ToTal Amount - VATAmount - Discount
            //Calculator VAT amount
            //var vatAmount = (tempFinalAmount * 10 / 100); //VAT 10%
            var vatAmount = 0; //VAT 10%
            order.FinalAmount = tempFinalAmount - vatAmount;
            order.DeliveryStatus = (int)DeliveryStatus.Assigned;
            order.OrderType = (int)OrderTypeEnum.Delivery;
            order.OrderStatus = (int)OrderStatusEnum.New;
            order.InvoiceID = Utils.GetCurrentDateTime().Ticks.ToString() + "-43";
            order.SourceType = (int)SourceTypeEnum.CallCenter; // Tam thoi de bang 0
            order.SourceID = storeId;
            order.GroupPaymentStatus = 0; //Tạm thời chưa xài đến
            var orderApi = new OrderApi();
            var storeApi = new StoreApi();
            var rs = await orderApi.CreateOrderAsync(order);
            //NotifyMessage sent Queue, and Pos
            //var msg = new NotifyOrder()
            //{
            //    StoreId = (int)order.StoreID,
            //    //StoreName = store.Name,
            //    NotifyType = (int)NotifyMessageType.OrderChange,
            //    Content = "Có đơn hàng mới",
            //    RentId = rent.RentID

            //};
            //ApiHttpClient.RequestOrderWebApi(msg);

            if (!rs)
            {
                return Json(new
                {
                    success = false,
                    msg = "Tạo đơn hàng không thành công"
                });
            }
            //var deliveryOrder = new DeliveryOrder
            //{
            //    CallCenterId = order.SourceID ?? 0,
            //    DeliveryStatus = DeliveryStatus.Assigned,
            //    Id = order.RentID,
            //    StoreId = order.StoreID.Value
            //};

            //DeliveryQueue.Instance.Push(deliveryOrder);
            //RealtimeQueue.Instance.Push(deliveryOrder);
            return Json(new
            {
                success = true,
                msg = "Tạo đơn hàng thành công"
            });
        }

        [Authorize(Roles = "BrandManager, Manager, Reception")]
        public JsonResult LoadDeliveryOrders(JQueryDataTableParamModel param, int brandId, int check)
        {
            var orderApi = new OrderApi();
            var orders = orderApi.GetOrderByBrand(brandId);
            var totalRecords = orders.Count();
            var totalDisplayRecords = 0;
            var pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));
            //|| a.DeliveryAddress.ToLower().Contains(param.sSearch.ToLower()))

            if (check == 2)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    //|| a.InvoiceID.ToLower().Contains(param.sSearch.ToLower())
                    || a.DeliveryAddress.ToLower().Contains(param.sSearch.ToLower()));
            }
            if (check == 0)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.Customer.Name.ToLower().Contains(param.sSearch.ToLower()));
                // || a.DeliveryAddress.ToLower().Contains(param.sSearch.ToLower()))
            }

            var count = param.iDisplayStart;
            totalDisplayRecords = pagingResult.Count();
            var rs = pagingResult
                .OrderByDescending(a => a.RentID).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList()
                .Select(a => new IConvertible[]
            {
                ++count,
                a.InvoiceID,
                a.CustomerID != null ? a.Customer.Name: "Chưa có khách hàng",
                a.DeliveryAddress ?? "Chưa có địa chỉ",
                a.CustomerID != null ? a.Customer.Phone: "Chưa có khách hàng",
                a.CheckInDate.HasValue? a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                a.StoreID != null ? a.Store.Name :"Chưa có cửa hàng",
                a.DeliveryStatus,
                a.RentID,
                a.StoreID,
                a.DeliveryStatus,
                a.Notes
            });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "BrandManager, Manager, Reception")]
        public async System.Threading.Tasks.Task<ActionResult> OrderDetail(int Id, int brandId)
        {
            var orderApi = new OrderApi();
            var order = await orderApi.GetOrderByIdAsync(Id);
            PrepareDetail(order, brandId);
            return View(order);
        }

        private void PrepareDetail(OrderViewModel model, int brandId)
        {
            var storeApi = new StoreApi();
            model.AvailableStore = storeApi.GetStoreByBrandId(brandId)
                .Select(q => new SelectListItem
                {
                    Text = q.ShortName,
                    Value = q.ID.ToString(),
                    Selected = q.ID == model.StoreID,
                });
        }

        [Authorize(Roles = "BrandManager, Manager, Reception")]
        public async System.Threading.Tasks.Task<JsonResult> LoadOrderDetail(JQueryDataTableParamModel param, int id)
        {
            var orderApi = new OrderApi();
            var order = await orderApi.GetOrderByIdAsync(id);
            if (order == null)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var total = (order.OrderDetails.Count());
                var orderDetails = order.OrderDetails
                    .Where(b =>
                        string.IsNullOrEmpty(param.sSearch) ||
                        b.Product.ProductName.ToLower().Contains(param.sSearch.ToLower()))
                        .OrderByDescending(q => q.ProductID)
                        .Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength);
                var totalQuery = order.OrderDetails
                    .Count(b =>
                        string.IsNullOrEmpty(param.sSearch) ||
                        b.Product.ProductName.ToLower().Contains(param.sSearch.ToLower()));
                var list = orderDetails.Select(a => new IConvertible[]
                        {
                            a.OrderDetailID,
                            a.Product.ProductName,
                            a.UnitPrice,
                            a.Quantity,
                            a.Discount
                        });
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalQuery,
                    aaData = list
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [Authorize(Roles = "BrandManager, Manager, Reception")]
        public async System.Threading.Tasks.Task<JsonResult> ChangeDeliveryStore(int id, int newStoreId)
        {
            var orderApi = new OrderApi();
            var order = await orderApi.GetOrderByIdAsync(id);
            if (order == null || order.OrderType != (int)OrderTypeEnum.Delivery)
            {
                return Json(new { success = false });
            }
            order.DeliveryStatus = (int)DeliveryStatus.Assigned;
            order.OrderStatus = (int)OrderStatusEnum.New;
            order.OrderType = (int)OrderTypeEnum.Delivery;
            order.StoreID = newStoreId;

            var rs = await orderApi.EditOrderAsync(order);

            return Json(new { success = rs });
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult> CheckPromotion(int brandId, string strTotal, string orderDetailsIDs, JQueryDataTableParamModel param)
        {
            var orderApi = new OrderApi();
            var groupApi = new GroupApi();
            var promotionApi = new PromotionApi();
            var promotion = promotionApi.GetPromotionByIdTime(brandId);
            //promotion = promotion.Where(q => q.)

            //List<PromotionEditViewModel> list = promotion.ToList();

            PromotionEditViewModel model = new PromotionEditViewModel();
            //model.AvailablePromotion = promotion.ToSelectList(q => q.PromotionName, q => q.PromotionID.ToString(), q => false);
            model.AvailablePromotion = promotion.OrderBy(q => q.PromotionType);

            var groupList = groupApi.GetActive();
            var list = (from p in promotion.AsEnumerable()
                        join g in groupList.AsEnumerable()
on p.PromotionType equals g.GroupId
                        select new { GroupId = p.PromotionType, GroupName = g.Name }).Distinct().OrderBy(q => q.GroupId);
            model.AvailableGroup = list.ToSelectList(q => q.GroupName, q => q.GroupId.ToString(), q => q.GroupName == null);

            //promotion.

            //foreach trong promotion
            //promotionDetail
            //ForeachDetail.
            //If itemDetail = sản phẩm => order.product == itemDetail.product && order.quantity

            return View("PromotionApply", model);
        }

        /// <summary>
        /// <param name="brandId"></param>
        /// <param name="strTotal">Chuỗi chứa giá trị tổng tiền</param>
        /// <param name="orderDetailsIDs">IDs của các item được order</param>
        /// <param name="promotionId">ID của promotion vừa chọn</param>
        /// <param name="discountBillBefore">0 -> 100</param>
        /// <param name="calculate"> = 1: Thêm, = -1 : xóa</param>
        /// <returns></returns>
        /// </summary>
        private bool? CheckPromotionDetail(int brandId, double strTotal,
            int[] orderDetailsIDs, int promotionId, int[] quantityDetails,
            List<int> otherPromotionGroup,
            PromotionBillTracking promoBillTrack, PromotionProductTracking promoProductTrack)
        {

            var manager = new HmsEntities();

            //Lấy promotionDetails có id phù hợp với promotionId
            var promotionDetails = (from PDVM in manager.PromotionDetails
                                    join PVM in manager.Promotions on PDVM.PromotionCode equals PVM.PromotionCode
                                    //join PA in manager.Products on PDVM.GiftProductCode equals PA.ProductID.ToString()
                                    where PVM.PromotionID == promotionId && PVM.IsVoucher == false
                                    select new
                                    {
                                        PromotionDetailId = PDVM.PromotionDetailID,
                                        PromotionID = PVM.PromotionID,
                                        GiftType = PVM.GiftType,
                                        ApplyLevel = PVM.ApplyLevel,
                                        Group = PVM.PromotionType,
                                        BuyProductCode = PDVM.BuyProductCode,
                                        DiscountRate = PDVM.DiscountRate,
                                        GiftProductId = PDVM.GiftProductCode,
                                        GiftQuantity = PDVM.GiftQuantity,
                                        MinOrderAmount = PDVM.MinOrderAmount,
                                        MaxOrderAmount = PDVM.MaxOrderAmount,
                                        MinBuyQuantity = PDVM.MinBuyQuantity,
                                        MaxBuyQuantity = PDVM.MaxBuyQuantity,
                                        DiscountAmount = PDVM.DiscountAmount,
                                        //ProductName = PA.ProductName

                                    });

            //lấy đống id của ordered items
            //int[] ids = new int[orderDetailsIDs.Length];
            //for (int i = 0; i < orderDetailsIDs.Length; i++)
            //{
            //    ids[i] = int.Parse(orderDetailsIDs[i]);
            //}

            int[] ids = orderDetailsIDs;
            //lấy tổng giá tiền
            double total = strTotal;

            //Kiểm tra từng promotionDetail với mức độ áp dụng và hình thức khuyến mãi
            var productApi = new ProductApi();
            bool isSuccess = false;

            if (promotionDetails.Count() > 0)
            {
                if (otherPromotionGroup == null || !otherPromotionGroup.Contains(promotionDetails.ToList()[0].Group))
                {
                    #region vòng for kiểm tra từng promotionDetails
                    foreach (var item in promotionDetails)
                    {
                        if (item.ApplyLevel == 1) //sản phẩm
                        {
                            int check = isHave(ids, int.Parse(item.BuyProductCode), item.MinBuyQuantity.Value,
                                item.MaxBuyQuantity, quantityDetails);
                            if (check > -1)
                            {
                                if (item.GiftType == 0) //giảm giá
                                {
                                    promoProductTrack.push(item.PromotionID, orderDetailsIDs[check], 0, 0, null, 0, item.DiscountRate.Value);
                                    isSuccess = true;
                                }
                                else if (item.GiftType == 1) //quà tặng
                                {
                                    var gift = productApi.GetProductById(int.Parse(item.GiftProductId));
                                    int itemGiftQuantity = item.GiftQuantity.GetValueOrDefault();
                                    promoProductTrack.push(item.PromotionID, orderDetailsIDs[check],
                                        itemGiftQuantity, int.Parse(item.GiftProductId), gift.ProductName, 0, 0);
                                    isSuccess = true;
                                }
                                else if (item.GiftType == 2)
                                {
                                    promoProductTrack.push(item.PromotionID, orderDetailsIDs[check], 0, 0, null, item.DiscountAmount.Value, 0);
                                    isSuccess = true;
                                }

                            }
                        }
                        else if (item.ApplyLevel == 0) //hóa đơn
                        {
                            if (item.GiftType == 0) //giảm giá
                            {
                                if (item.MinOrderAmount <= total && (!item.MaxOrderAmount.HasValue || total <= item.MaxOrderAmount))
                                {
                                    //discountBillBefore = discountBillBefore + (1 - discountBillBefore * 0.01) * item.DiscountRate.Value;

                                    //promotionDetailIdDiscountforBill = item.PromotionDetailId;
                                    promoBillTrack.push(item.PromotionID, item.DiscountRate.Value, 0, 0, null, 0);
                                    isSuccess = true;
                                }
                            }
                            else if (item.GiftType == 1) //quà tặng
                            {
                                if (item.MinOrderAmount <= total && (!item.MaxOrderAmount.HasValue || total <= item.MaxOrderAmount))
                                {
                                    var gift = productApi.GetProductById(int.Parse(item.GiftProductId));
                                    promoBillTrack.push(item.PromotionID, 0, int.Parse(item.GiftProductId),
                                        item.GiftQuantity.GetValueOrDefault(), gift.ProductName, 0);
                                    isSuccess = true;
                                }
                            }
                            else if (item.GiftType == 2)
                            {
                                if (item.MinOrderAmount <= total && (!item.MaxOrderAmount.HasValue || total <= item.MaxOrderAmount))
                                {
                                    promoBillTrack.push(item.PromotionID, 0, 0, 0, null, item.DiscountAmount.Value);
                                    isSuccess = true;
                                }
                            }
                        }

                    }
                    #endregion  
                    if (isSuccess) otherPromotionGroup.Add(promotionDetails.ToList()[0].Group);
                }
                else
                {
                    #region vòng for kiểm tra từng promotionDetails
                    foreach (var item in promotionDetails)
                    {
                        if (item.ApplyLevel == 1) //sản phẩm
                        {
                            int check = isHave(ids, int.Parse(item.BuyProductCode), item.MinBuyQuantity.Value, item.MaxBuyQuantity, quantityDetails);
                            if (check > -1)
                            {
                                return null;

                            }
                        }
                        else if (item.ApplyLevel == 0) //hóa đơn
                        {
                            if (item.GiftType == 0) //giảm giá
                            {
                                if (item.MinOrderAmount <= total && total <= item.MaxOrderAmount)
                                {
                                    return null;
                                }
                            }
                            else if (item.GiftType == 1) //quà tặng
                            {
                                if (item.MinOrderAmount <= total && total <= item.MaxOrderAmount)
                                {
                                    return null;
                                }
                            }
                            else if (item.GiftType == 2)
                            {
                                return null;
                            }
                        }

                    }
                    #endregion  
                    return false;
                }

            }

            return isSuccess;
            #region
            ////testing
            //foreach (var item in promotion)
            //{
            //    var detailApi = new PromotionDetailApi();
            //    var detail = detailApi.GetDetailListById(item.PromotionID);
            //    int count = 0;
            //    foreach (var item1 in detail)
            //    {
            //        if (item1.MinOrderAmount <= order.TotalAmount && item1.MaxOrderAmount >= order.TotalAmount)
            //        {
            //            list.Add(item1);
            //            var rs = list.Skip(param.iDisplayStart).Take(param.iDisplayLength)
            //                .Select(a => new IConvertible[]
            //                {
            //                ++count,
            //                a.RegExCode,
            //                    string.Format(CultureInfo.InvariantCulture,
            //                        "{0:0,0}", a.MinOrderAmount),
            //                     string.Format(CultureInfo.InvariantCulture,
            //                        "{0:0,0}", a.MaxOrderAmount),
            //                a.DiscountRate,
            //                a.PromotionDetailID
            //                }
            //                );
            //        }
            //    }
            //}
            #endregion
        }



        public JsonResult CreateOnlineOrder(DeliveryOrder orderList, PromotionBillTracking promoBillTrack, PromotionProductTracking promoProductTrack)
        {

            bool isNewUser = false;
            CustomerApi cusApi = new CustomerApi();
            CustomerViewModel cusModel = new CustomerViewModel();

            if (!string.IsNullOrEmpty(orderList.customerName))
            {
                cusModel.Name = orderList.customerName;
                cusModel.Address = orderList.customerAddress;
                cusModel.Phone = orderList.customerPhone;
                cusModel.Email = orderList.customerEmail;
                cusModel.CustomerTypeId = 2;
                cusApi.Create(cusModel);
                isNewUser = true;
            }

            OrderViewModel order = new OrderViewModel();
            order.InvoiceID = Utils.GetCurrentDateTime().Ticks.ToString() + "-43";
            order.CheckInDate = Utils.GetCurrentDateTime();
            order.FinalAmount = orderList.finalAmount;
            order.TotalAmount = orderList.totalAmount;
            order.CheckInPerson = User.Identity.Name;
            order.OrderType = (int)OrderTypeEnum.Delivery;
            order.OrderStatus = (int)OrderStatusEnum.New;
            order.DeliveryStatus = (int)DeliveryStatus.Assigned;
            order.SourceType = (int)SourceTypeEnum.CallCenter; // Tam thoi de bang 0
            order.StoreID = orderList.storeID;
            order.GroupPaymentStatus = 0; //Tạm thời chưa xài đến
            order.RentStatus = orderList.rentStatus;
            order.RentType = orderList.rentType;
            order.DeliveryAddress = orderList.deliveryAddress;
            order.CustomerID = isNewUser ? cusModel.CustomerID : orderList.customerID;
            int vat = 10; //VAT 10%
            var vatAmount = (order.FinalAmount * vat / 100); //VAT 10%


            //Lưu order
            OrderApi api = new OrderApi();
            api.CreateOrder(order.ToEntity());

            //Lưu orderDetail
            int length = orderList.orderDetails.Count;
            ProductApi proApi = new ProductApi();
            OrderDetailApi orderDetailApi = new OrderDetailApi();
            CustomerProductMappingApi cusProMapApi = new CustomerProductMappingApi();


            for (int i = 0; i < length; i++)
            {
                var p = proApi.GetProductById(orderList.orderDetails[i].productID);
                OrderDetailViewModel orderDetail = new OrderDetailViewModel();
                orderDetail.Discount = promoProductTrack.getTotalDiscountRate(orderList.orderDetails[i].productID);
                orderDetail.FinalAmount = orderList.orderDetails[i].finalAmount;
                orderDetail.Quantity = orderList.orderDetails[i].quantity;
                orderDetail.StoreId = orderList.storeID;
                orderDetail.TotalAmount = orderList.orderDetails[i].totalAmount;
                orderDetail.UnitPrice = p.Price;
                orderDetail.Status = (int)OrderDetailStatus.Ordered;
                orderDetail.ProductName = p.ProductName;
                orderDetail.ProductImage = p.PicURL;
                orderDetail.ProductID = p.ProductID;
                orderDetail.ProductCode = p.Code;
                orderDetail.OrderDate = Utils.GetCurrentDateTime();
                orderDetail.ItemQuantity = orderList.orderDetails[i].quantity;
                orderDetail.IsAddition = false;
                orderDetail.DetailDescription = "";
                if (orderList.orderDetails[i].parentID > 0)
                {
                    orderDetail.ParentId = orderList.orderDetails[i].parentID;
                }
                orderDetailApi.Create(orderDetail);
                var t = cusProMapApi.GetByCustomerIdProductId(isNewUser ? cusModel.CustomerID : orderList.customerID,
                    orderDetail.ProductID);
                if (t != null)
                {
                    t.TotalQuantity += orderDetail.Quantity;
                    cusProMapApi.Edit(t.ID, t);
                }
                else
                {
                    CustomerProductMappingViewModel cusProMap = new CustomerProductMappingViewModel();
                    cusProMap.CustomerID = isNewUser ? cusModel.CustomerID : orderList.customerID;
                    cusProMap.ProductID = orderDetail.ProductID;
                    cusProMap.TotalQuantity = orderDetail.Quantity;
                    cusProMap.UpdateDate = Utils.GetCurrentDateTime();
                    cusProMapApi.Create(cusProMap);
                }
            }

            //Lưu extra


            //Lưu giảm giá hóa đơn
            OrderPromotionMappingApi orderPromoMapApi = new OrderPromotionMappingApi();
            length = promoBillTrack.promotionID.Count;
            for (int i = 0; i < length; i++)
            {
                if (promoBillTrack.discountAmount[i] != 0 || promoBillTrack.discountRate[i] != 0)
                {
                    OrderPromotionMappingViewModel orderPromoMapViewModel = new OrderPromotionMappingViewModel();
                    orderPromoMapViewModel.Active = true;
                    orderPromoMapViewModel.DiscountAmount = promoBillTrack.getTotalDiscountAmount();
                    orderPromoMapViewModel.DiscountRate = promoBillTrack.getTotalDiscountRate();
                    orderPromoMapViewModel.OrderId = api.GetOrderByInvoiceId(order.InvoiceID).RentID;
                    orderPromoMapViewModel.PromotionId = promoBillTrack.promotionID[i];
                    orderPromoMapApi.Create(orderPromoMapViewModel);
                }
            }

            //Lưu quà tặng hóa đơn
            for (int i = 0; i < length; i++)
            {
                if (promoBillTrack.giftID[i] != 0)
                {
                    OrderPromotionMappingViewModel orderPromoMapViewModel = new OrderPromotionMappingViewModel();
                    orderPromoMapViewModel.Active = true;
                    orderPromoMapViewModel.DiscountAmount = 0;
                    orderPromoMapViewModel.DiscountRate = 0;
                    orderPromoMapViewModel.OrderId = api.GetOrderByInvoiceId(order.InvoiceID).RentID;
                    orderPromoMapViewModel.PromotionId = promoBillTrack.promotionID[i];
                    orderPromoMapApi.Create(orderPromoMapViewModel);

                    var p = proApi.GetProductById(promoBillTrack.giftID[i]);
                    OrderDetailViewModel orderDetail = new OrderDetailViewModel();
                    orderDetail.Discount = 0;
                    orderDetail.FinalAmount = 0;
                    orderDetail.Quantity = promoBillTrack.giftAmount[i];
                    orderDetail.StoreId = orderList.storeID;
                    orderDetail.TotalAmount = 0;
                    orderDetail.UnitPrice = 0;
                    orderDetail.Status = (int)OrderDetailStatus.Ordered;
                    orderDetail.ProductName = promoBillTrack.giftName[i];
                    orderDetail.ProductImage = p.PicURL;
                    orderDetail.ProductID = p.ProductID;
                    orderDetail.ProductCode = p.Code;
                    orderDetail.OrderDate = Utils.GetCurrentDateTime();
                    orderDetail.ItemQuantity = promoBillTrack.giftAmount[i];
                    orderDetail.IsAddition = false;
                    orderDetail.DetailDescription = "";
                    orderDetail.OrderPromotionMappingId = orderPromoMapApi.GetByOrderInvoiceId(order.InvoiceID);
                    orderDetailApi.Create(orderDetail);
                }

            }

            //Lưu giảm giá sản phẩm
            length = promoProductTrack.promotionID.Count;
            OrderDetailPromotionMappingApi orderDetailPromoMapApi = new OrderDetailPromotionMappingApi();
            for (int i = 0; i < length; i++)
            {
                if (promoProductTrack.discountAmount[i] != 0 || promoProductTrack.discountRate[i] != 0)
                {
                    OrderDetailPromotionMappingViewModel m = new OrderDetailPromotionMappingViewModel();
                    m.Active = true;
                    m.DiscountAmount = promoProductTrack.getTotalDiscountAmount(promoProductTrack.buyProductID[i]);
                    m.DiscountRate = promoProductTrack.getTotalDiscountRate(promoProductTrack.buyProductID[i]);
                    m.OrderDetailId = promoProductTrack.buyProductID[i];
                    m.PromotionId = promoProductTrack.promotionID[i];
                    orderDetailPromoMapApi.Create(m);
                }
            }

            //Lưu quà tặng sản phẩm
            for (int i = 0; i < length; i++)
            {
                if (promoProductTrack.giftID[i] != 0)
                {
                    OrderDetailPromotionMappingViewModel m = new OrderDetailPromotionMappingViewModel();
                    m.Active = true;
                    m.DiscountAmount = 0;
                    m.DiscountRate = 0;
                    m.OrderDetailId = promoProductTrack.buyProductID[i];
                    m.PromotionId = promoProductTrack.promotionID[i];
                    orderDetailPromoMapApi.Create(m);

                    var p = proApi.GetProductById(promoProductTrack.giftID[i]);
                    OrderDetailViewModel orderDetail = new OrderDetailViewModel();
                    orderDetail.Discount = 0;
                    orderDetail.FinalAmount = 0;
                    orderDetail.Quantity = promoProductTrack.giftAmount[i];
                    orderDetail.StoreId = orderList.storeID;
                    orderDetail.TotalAmount = 0;
                    orderDetail.UnitPrice = 0;
                    orderDetail.Status = (int)OrderDetailStatus.Ordered;
                    orderDetail.ProductName = promoProductTrack.giftName[i];
                    orderDetail.ProductImage = p.PicURL;
                    orderDetail.ProductID = p.ProductID;
                    orderDetail.ProductCode = p.Code;
                    orderDetail.OrderDate = Utils.GetCurrentDateTime();
                    orderDetail.ItemQuantity = promoProductTrack.giftAmount[i];
                    orderDetail.IsAddition = false;
                    orderDetail.DetailDescription = "";
                    orderDetail.OrderDetailPromotionMappingId = m.Id;
                    orderDetailApi.Create(orderDetail);
                }

            }


            //rent.FinalAmount = tempFinalAmount;       Final Amount = ToTal Amount - VATAmount - Discount
            //Calculator VAT amount
            //order.DiscountOrderDetail = discountOrderDetail;


            return Json(new
            {
                success = true,
                msg = "Đặt hàng thành công"
            });
        }

        



        /// <summary>
        /// Kiểm tra sản phẩm cần 
        /// </summary>
        /// <param name="ids">IDs của đống ordered item</param>
        /// <param name="code">Code của product muốn kiểm tra </param>
        /// <param name="min">Min buy product </param>
        /// <param name="max">Max buy product</param>
        /// <param name="quantity">Số lượng muốn mua</param>
        /// <returns>Trat về vị trí của order item</returns>
        private int isHave(int[] ids, int code, int min, int? max, int[] quantity)
        {
            ProductViewModel pvm = new ProductApi().GetProductById(code);
            for (int i = 0; i < ids.Length; ++i)
            {
                if (ids[i] == pvm.ProductID && min <= quantity[i] && (!max.HasValue || quantity[i] <= max.Value))
                {
                    return i;
                }
            }
            return -1;
        }

        [HttpPost]
        public JsonResult Test()
        {
            var list = new object[] { 1, "wee", new object[] { new object[] { 2, "child1" }, new object[] { 3, "child2" } } };

            return Json(new
            {
                list = list
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSuitablePromotions(int brandId, int[] orderDetailsIDs,
            double strTotal, int[] quantityDetails, int[] selectedPromotionIDs, int[] allPromotionIDs,
            bool isChange)
        {
            if (orderDetailsIDs == null || orderDetailsIDs.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    msg = "Đơn hàng không được rỗng",
                });
            }
            var length = orderDetailsIDs.Length;
            List<int> promotionGroup = new List<int>();
            PromotionBillTracking promoBillTrack = new PromotionBillTracking();
            PromotionProductTracking promoProductTrack = new PromotionProductTracking();
            bool isSuccess = false;

            //Get all active promotions
            GroupApi groupApi = new GroupApi();
            var promotions = new PromotionApi().GetPromotionByBrandId(brandId).ToList();
            var temp = new HmsService.Models.Entities.Promotion[promotions.Count];
            promotions.CopyTo(temp);
            var promotionsOrigin = temp.ToList();
            if (isChange)
            {
                if (selectedPromotionIDs != null)
                {
                    int len = promotions.Count;
                    for (int i = 0; i < len; i++)
                    {
                        if (!selectedPromotionIDs.Contains(promotions[i].PromotionID))
                        {
                            promotions.RemoveAt(i);
                            --len;
                            --i;
                        }
                    }

                }
                else
                {
                    promotions = new List<HmsService.Models.Entities.Promotion>();
                    isSuccess = true;
                }
                int count = promotionsOrigin.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!allPromotionIDs.Contains(promotionsOrigin[i].PromotionID))
                    {
                        promotionsOrigin.RemoveAt(i);
                        --count; --i;
                    }
                }
            }

            var groups = groupApi.GetActive();
            int promotionsLength = (isChange ? promotions.Count : promotionsOrigin.Count);
            for (int i = 0; i < promotionsLength; i++)
            {
                bool? r = CheckPromotionDetail(brandId, strTotal, orderDetailsIDs, (isChange ? promotions[i].PromotionID : promotionsOrigin[i].PromotionID), quantityDetails,
                     promotionGroup, promoBillTrack, promoProductTrack);
                ;
                if (r.HasValue && r.Value == false)
                {
                    promotions.RemoveAll(c => c.PromotionID == promotionsOrigin[i].PromotionID);
                    promotionsOrigin.RemoveAt(i);
                    --i;
                    --promotionsLength;
                }
                else if (!r.HasValue)
                {
                    if (isChange)
                    {
                        promotions.RemoveAt(i);
                        --i; --length;
                    }
                    else
                    {
                        promotions.RemoveAll(c => c.PromotionID == promotionsOrigin[i].PromotionID);

                    }
                }
                isSuccess = true;
            }
            promotionsOrigin.Sort((x, y) => x.PromotionType.CompareTo(y.PromotionType));

            List<string> promotionGroupNames = new List<string>();
            for (int i = 0; i < promotionsOrigin.Count; i++)
            {
                switch (promotionsOrigin[i].PromotionType)
                {
                    case 0: promotionGroupNames.Add(PromotionTypeEnum.Internal.ToString()); break;
                    case 1: promotionGroupNames.Add(PromotionTypeEnum.Separate.ToString()); break;
                    default: promotionGroupNames.Add(PromotionTypeEnum.Common.ToString()); break;
                }

            }

            return Json(new
            {
                success = isSuccess,
                promotionSelectedIDs = promotions.Select(c => new { c.PromotionID }).ToList(),
                promotionNames = promotionsOrigin.Select(c => new { c.PromotionName }).ToList(),
                promotionGroups = promotionsOrigin.Select(c => new { c.PromotionType }).ToList(),
                promotionGroupNames = promotionGroupNames,
                promotionOriginIDs = promotionsOrigin.Select(c => new { c.PromotionID }).ToList(),
                promotionDescriptions = promotionsOrigin.Select(c => new { c.Description }).ToList(),

                promoBillTrack = promoBillTrack,
                promoProductTrack = promoProductTrack,


            });
        }

        [HttpPost]
        public ActionResult ExtraMenu(int brandId, int productId)
        {

            ProductApi pApi = new ProductApi();
            var product = pApi.GetProductById(productId);
            ProductCategoryApi pcApi = new ProductCategoryApi();



            //Load topping và extra trong này
            //var extra =  pApi.GetActive().Where(q => q.ProductCategory.Active == true 
            //                          && q.ProductCategory.BrandId == brandId
            //                          && q.ProductCategory.IsExtra == true);

            var extra = pApi.GetProductExtraByBrandId(brandId);
            var extraGroup = pcApi.GetActive().Where(q => q.IsExtra == true && q.BrandId == brandId);
            product.ExtraProduct = extra;
            product.ExtraGroup = extraGroup;


            return View("ExtraMenu", product);
        }

    }
    public class PromotionBillTracking
    {
        public List<int> promotionID { get; set; }
        public List<double> discountRate { get; set; }
        public List<int> giftID { get; set; }
        public List<int> giftAmount { get; set; }
        public List<string> giftName { get; set; }
        public List<decimal> discountAmount { get; set; }

        public PromotionBillTracking()
        {
            this.promotionID = new List<int>();
            this.discountRate = new List<double>();
            this.giftID = new List<int>();
            this.giftAmount = new List<int>();
            this.giftName = new List<string>();
            this.discountAmount = new List<decimal>();
        }

        public void push(int promotionID, double discountRate, int giftID, int giftAmount, string giftName, decimal discountAmount)
        {
            if (this.promotionID.Contains(promotionID))
            {
                replace(promotionID, discountRate, discountAmount);
            }
            else
            {
                this.promotionID.Add(promotionID);
                this.giftAmount.Add(giftAmount);
                this.giftID.Add(giftID);
                this.giftName.Add(giftName);
                this.discountAmount.Add(discountAmount);
                this.discountRate.Add(discountRate);
            }
        }

        public void replace(int promotionID, double discountRate, decimal discountAmount)
        {
            for (int i = 0; i < this.promotionID.Count; i++)
            {
                if (this.promotionID[i] == promotionID)
                {
                    this.discountRate[i] = discountRate;
                    this.discountAmount[i] = discountAmount;
                }
            }
        }

        public decimal getTotalDiscountAmount()
        {
            decimal a = 0;
            int length = this.discountAmount.Count;
            for (int i = 0; i < length; i++)
            {
                a += this.discountAmount[i];
            }
            return a;
        }

        public double getTotalDiscountRate()
        {
            double a = 0;
            int length = this.discountRate.Count;
            for (int i = 0; i < length; i++)
            {
                a += (100 - a) * this.discountRate[i] / 100;
            }
            return a;
        }
    }

    public class PromotionProductTracking
    {
        public List<int> promotionID { get; set; }
        public List<int> buyProductID { get; set; }
        public List<int> giftID { get; set; }
        public List<string> giftName { get; set; }
        public List<int> giftAmount { get; set; }
        public List<double> discountRate { get; set; }
        public List<decimal> discountAmount { get; set; }

        public PromotionProductTracking()
        {
            this.promotionID = new List<int>();
            this.buyProductID = new List<int>();
            this.giftAmount = new List<int>();
            this.giftID = new List<int>();
            this.giftName = new List<string>();
            this.discountAmount = new List<decimal>();
            this.discountRate = new List<double>();
        }

        public void push(int promotionID, int buyProductID, int giftAmount, int giftID, string giftName,
            decimal discountAmount, double discountRate)
        {
            int pos;
            if ((pos = this.promotionID.IndexOf(promotionID)) >= 0)
            {
                if (this.buyProductID[pos] == buyProductID)
                {
                    replace(promotionID, discountRate, discountAmount);
                    return;
                }
            }
            this.promotionID.Add(promotionID);
            this.buyProductID.Add(buyProductID);
            this.giftAmount.Add(giftAmount);
            this.giftID.Add(giftID);
            this.giftName.Add(giftName);
            this.discountAmount.Add(discountAmount);
            this.discountRate.Add(discountRate);

        }

        public void replace(int promotionID, double discountRate, decimal discountAmount)
        {
            for (int i = 0; i < this.promotionID.Count; i++)
            {
                if (this.promotionID[i] == promotionID)
                {
                    this.discountRate[i] = discountRate;
                    this.discountAmount[i] = discountAmount;
                }
            }
        }

        public double getTotalDiscountRate(int productID)
        {
            double discount = 0;
            int length = this.promotionID.Count;
            for (int i = 0; i < length; i++)
            {
                if (this.buyProductID[i] == productID)
                {
                    discount += (100 - discount) * this.discountRate[i] / 100;
                }
            }
            return discount;
        }

        public decimal getTotalDiscountAmount(int productID)
        {
            decimal discount = 0;
            int length = this.promotionID.Count;
            for (int i = 0; i < length; i++)
            {
                if (this.buyProductID[i] == productID)
                {
                    discount += this.discountAmount[i];
                }
            }
            return discount;
        }

        public int getPromotionID(int productID)
        {
            int length = this.promotionID.Count;
            for (int i = 0; i < length; i++)
            {
                if (this.buyProductID[i] == productID)
                {
                    return this.promotionID[i];
                }
            }
            return -1;
        }
    }
    public class DeliveryOrderDetail
    {
        public int productID { get; set; }
        public int quantity { get; set; }
        public int status { get; set; }
        public double totalAmount { get; set; }
        public double finalAmount { get; set; }
        public int parentID { get; set; }
    }

    public class DeliveryOrder
    {
        public double totalAmount { get; set; }
        public double finalAmount { get; set; }
        public int customerID { get; set; }
        public string deliveryAddress { get; set; }
        public int rentStatus { get; set; }
        public int rentType { get; set; }
        public int storeID { get; set; }
        public int brandId { get; set; }
        public List<DeliveryOrderDetail> orderDetails { get; set; }
        public string customerName { get; set; }
        public string customerPhone { get; set; }
        public string customerAddress { get; set; }
        public string customerEmail { get; set; }
        public string note { get; set; }
    }
}
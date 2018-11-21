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

namespace Wisky.SkyAdmin.Manage.Areas.Delivery.Controllers
{
    [Authorize]
    public class DeliveryController : DomainBasedController
    {
        [Authorize(Roles = "BrandManager, Manager, Reception, Booking")]
        // GET: Delivery/Delivery
        public ActionResult Index(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }
        public ActionResult OrderFromWeb(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }
        public ActionResult LoadStoreDelivery(string lonatt, string latatt, string ward, string district, string province)
        {
            var lat = double.Parse(latatt);
            var lon = double.Parse(lonatt);
            var storeApi = new StoreApi();
            var brandId = int.Parse(RouteData.Values["brandId"].ToString());
            var listStores = storeApi.GetStoreByBrandId(brandId).AsEnumerable();
            var listSelectted = new List<StoreViewModel>();
            var listDistance = new List<StoreModel>();
            StringCompare sc = new StringCompare();

            var provinceApi = new ProvinceApi();
            var listProvinces = provinceApi.Get().ToList();
            var similarProvince = sc.SelectMostSimilarProvince(province, listProvinces);
           
            var districtApi = new DistrictApi();
            var listDistricts = districtApi.Get().Where(q => q.ProvinceCode == similarProvince.ProvinceCode).ToList();
            var similarDistrict = sc.SelectMostSimilarDistrict(district, listDistricts);

            var wardApi = new WardApi();
            var listWards = wardApi.Get().Where(q => q.DistrictCode == similarDistrict.DistrictCode).ToList();
            var similarWard = sc.SelectMostSimilarWard(ward, listWards);

            foreach (var item in listStores)
            {
                if (!String.IsNullOrEmpty(item.Lat) && !String.IsNullOrEmpty(item.Lon))
                {
                    var storeLat = double.Parse(item.Lat);
                    var storeLon = double.Parse(item.Lon);
                    var distanceTmp = Utils.distance(lat, lon, storeLat, storeLon, 'K');
                    listDistance.Add(new StoreModel
                    {
                        StoreId = item.ID,
                        Distance = distanceTmp
                    });
                }
            }
            listDistance = listDistance.OrderBy(a => a.Distance).Take(10).Skip(0).ToList();

            var result = new List<Object>();
            foreach (var item in listDistance)
            {
                var store = storeApi.Get(item.StoreId);
                result.Add(new
                {
                    ID = store.ID,
                    Name = store.Name,
                    Address = store.Address,
                    Longitude = store.Lon,
                    Latitude = store.Lat,
                });
            }
            return Json(new
            {
                success = true,
                listdata = result,
                mostsimilarWard = similarWard,
                mostsimilarDistrict = similarDistrict,
                mostsimilarProvince = similarProvince
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadLocation()
        {
            var provinceApi = new ProvinceApi();
            var listProvinces = provinceApi.Get().OrderBy(q => q.ProvinceName).ToList();
            var districtApi = new DistrictApi();
            var listDistrict = districtApi.Get().OrderBy(q => q.DistrictName).ToList();
            var wardApi = new WardApi();
            var listWard = wardApi.Get().OrderBy(q => q.WardName).ToList();

            return Json(new
            {
                success = true,
                provinces = listProvinces,
                districts = listDistrict,
                wards = listWard
            }, JsonRequestBehavior.AllowGet);
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
            var listProductCate = productApi.GetProductByBrand(brandId).Where(q=>q.ProductType != (int)ProductTypeEnum.General);
            if (cateId > 0)
            {
                listProductCate = listProductCate.Where(q => q.CatID == cateId);
            }
            if (!String.IsNullOrEmpty(pattern))
            {
                listProductCate = listProductCate.Where(q => q.ProductName.ToLower().Contains(pattern.ToLower()));
            }

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
                }).Take(100)
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
            var result = storeApi.GetListStoreByBrandId(brandId).Where(a => a.Type == (int)StoreTypeEnum.Store)
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
        [Authorize(Roles = "BrandManager, Manager, Reception, CallCenter")]
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
            order.CheckInDate = time;
            order.CheckInPerson = User.Identity.Name;
            order.TotalAmount = tempTotalAmount;
            //rent.FinalAmount = tempFinalAmount;       Final Amount = ToTal Amount - VATAmount - Discount
            //Calculator VAT amount
            //var vatAmount = (tempFinalAmount * 10 / 100); //VAT 10%
            var vatAmount = 0; //VAT 10%
            order.FinalAmount = tempFinalAmount - vatAmount;
            order.DiscountOrderDetail = discountOrderDetail;
            order.DeliveryStatus = (int)DeliveryStatus.Assigned;
            order.OrderType = (int)OrderTypeEnum.Delivery;
            order.OrderStatus = (int)OrderStatusEnum.New;
            order.InvoiceID = Utils.GetCurrentDateTime().Ticks.ToString() + "-" + storeId;
            order.SourceType = (int)SourceTypeEnum.CallCenter; // Tam thoi de bang 0
            order.SourceID = storeId;
            order.GroupPaymentStatus = 0; //Tạm thời chưa xài đến
            order.PaymentStatus = (int)OrderPaymentStatusEnum.Finish;

            if (order.Customer != null)
            {
                order.Customer.BrandId = brandId;
            }
            var orderApi = new OrderApi();
            var storeApi = new StoreApi();
            OrderCustomEntityViewModel orderEntity = new OrderCustomEntityViewModel()
            {
                Order = order,
                OrderDetails = order.OrderDetails,
                Customer = order.Customer,
            };
            var rs = 0;
            rs = orderApi.CreateOrderDelivery(orderEntity);
            //NotifyMessage sent Queue, and Pos
            var msg = new NotifyOrder()
            {
                StoreId = (int)order.StoreID,
                //StoreName = store.Name,
                NotifyType = (int)NotifyMessageType.OrderChange,
                Content = "Có đơn hàng mới",
                OrderId = rs,

            };
            await Utils.RequestOrderWebApi(msg);

            if (rs == 0)
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
        public JsonResult LoadDeliveryOrders(JQueryDataTableParamModel param, int brandId, int check, int checkStatus, string starttime, string endtime)
        {
            var orderApi = new OrderApi();
            var sTime = Utils.ToDateTime(starttime).GetStartOfDate();
            var eTime = Utils.ToDateTime(endtime).GetEndOfDate();
            var orders = orderApi.GetAllOrdersByDate(sTime, eTime, brandId).Where(q => q.OrderType == (int)OrderTypeEnum.Delivery || q.OrderType == (int)OrderTypeEnum.OnlineProduct);
            var totalRecords = orders.Count();
            var totalDisplayRecords = 0;
            var pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));

            if (check == 2)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.DeliveryAddress.ToLower().Contains(param.sSearch.ToLower()));
            }
            if (check == 0)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.Customer.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            if (check == 3)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.Store.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            switch (checkStatus)
            {
                case 5:
                    pagingResult = pagingResult
            .Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.PreCancel);
                    break;
                case 6:
                    pagingResult = pagingResult
            .Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Cancel);
                    break;
                case 7:
                    pagingResult = pagingResult
            .Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Assigned);
                    break;
                case 8:
                    pagingResult = pagingResult
            .Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Delivery);
                    break;
                case 4:
                    pagingResult = pagingResult
            .Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Finish);
                    break;
                case 9:
                    pagingResult = pagingResult
            .Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.New);
                    break;
                case 3:
                    pagingResult = pagingResult
            .Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Fail);
                    break;
            }
            var count = param.iDisplayStart;
            totalDisplayRecords = pagingResult.Count();
            var rs = pagingResult
                .OrderByDescending(a => a.RentID).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList()
                .Select(a => new IConvertible[]
            {
                ++count,
                a.InvoiceID,
                (a.CustomerID != null && a.CustomerID!=0) ? a.Customer.Name : a.DeliveryReceiver,
                a.DeliveryAddress ?? "N/A",
                (a.CustomerID != null && a.CustomerID!=0) ? a.Customer.Phone : a.DeliveryPhone,
                a.CheckInDate.HasValue? a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                a.StoreID != null ? a.Store.Name :"N/A",
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
        public JsonResult LoadDeliveryOrdersNotAssign(JQueryDataTableParamModel param, int brandId, int check, string starttime, string endtime)
        {
            var orderApi = new OrderApi();
            var sTime = Utils.ToDateTime(starttime).GetStartOfDate();
            var eTime = Utils.ToDateTime(endtime).GetEndOfDate();
            var orders = orderApi.GetAllOrdersByDate(sTime, eTime, brandId).Where(q => q.OrderType == (int)OrderTypeEnum.Delivery || q.OrderType == (int)OrderTypeEnum.OnlineProduct);
            var totalRecords = orders.Count();
            var totalDisplayRecords = 0;
            var pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));

            if (check == 2)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.DeliveryAddress.ToLower().Contains(param.sSearch.ToLower()));
            }
            if (check == 0)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.Customer.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            if (check == 3)
            {
                pagingResult = orders
                .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.Store.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            //switch (checkStatus)
            //{
            //    case 5:
            //        pagingResult = pagingResult
            //.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.PreCancel);
            //        break;
            //    case 6:
            //        pagingResult = pagingResult
            //.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Cancel);
            //        break;
            //    case 7:
            //        pagingResult = pagingResult
            //.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Assigned);
            //        break;
            //    case 8:
            //        pagingResult = pagingResult
            //.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Delivery);
            //        break;
            //    case 4:
            //        pagingResult = pagingResult
            //.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Finish);
            //        break;
            //    case 9:
            //        pagingResult = pagingResult
            //.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.New);
            //        break;
            //    case 3:
            //        pagingResult = pagingResult
            //.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.Fail);
            //        break;
            //}
            //Get All delivery order not assign
            pagingResult = pagingResult.Where(a => a.DeliveryStatus.Value == (int)DeliveryStatus.New);
            var count = param.iDisplayStart;
            totalDisplayRecords = pagingResult.Count();
            var rs = pagingResult
                .OrderByDescending(a => a.RentID).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList()
                .Select(a => new IConvertible[]
            {
                ++count,
                a.InvoiceID,
                (a.CustomerID != null && a.CustomerID!=0) ? a.Customer.Name : a.DeliveryReceiver,
                a.DeliveryAddress ?? "N/A",
                (a.CustomerID != null && a.CustomerID!=0) ? a.Customer.Phone : a.DeliveryPhone,
                a.CheckInDate.HasValue? a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                a.StoreID != null ? a.Store.Name :"N/A",
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
            var orderPromotionMappingApi = new OrderPromotionMappingApi();
            var order = await orderApi.GetOrderByIdAsync(Id);
            var storeApi = new StoreApi();
            var liststore = storeApi.GetListStoreByBrandAndNone(brandId).Select(q => new SelectListItem()
            {
                Text = q.Name,
                Value = q.ID.ToString()
            });
            ViewBag.listStore = liststore;
            //PrepareDetail(order, brandId);
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
                        .OrderBy(q => q.OrderDetailID)
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
                            Utils.ToMoney(a.UnitPrice),
                            a.Quantity,
                            Utils.ToMoney(a.Discount)
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
            var paymentApi = new PaymentApi();
            var order = await orderApi.GetOrderByIdAsync(id);
            if (order == null || order.OrderType != (int)OrderTypeEnum.Delivery)
            {
                return Json(new { success = false });
            }
            order.DeliveryStatus = (int)DeliveryStatus.Assigned;
            order.OrderStatus = (int)OrderStatusEnum.New;
            order.OrderType = (int)OrderTypeEnum.Delivery;
            order.StoreID = newStoreId;
            var paymentId = -1;
            try
            {
                var payment = paymentApi.GetPaymentByOrder(order.RentID);
                if (payment.Count() == 0)
                {
                    var payments = new PaymentViewModel()
                    {
                        ToRentID = order.RentID,
                        Amount = order.FinalAmount,
                        CurrencyCode = "VND",
                        Status = (int)PaymentStatusEnum.Approved,
                        Type = (int)PaymentTypeEnum.Cash,
                        FCAmount = (decimal)order.FinalAmount,
                        PayTime = Utils.GetCurrentDateTime(),
                        //Username = User.Identity.Name

                    };
                    paymentId = paymentApi.CreatePaymentReturnId(payments.ToEntity());
                }
                var rs = await orderApi.EditOrderAsync(order);
                var msg = new NotifyOrder()
                {
                    StoreId = (int)order.StoreID,
                    //StoreName = store.Name,
                    NotifyType = (int)NotifyMessageType.OrderChange,
                    Content = "Có đơn hàng mới",
                    OrderId = order.RentID,
                };
                await Utils.RequestOrderWebApi(msg);
                return Json(new { success = rs });
            }
            catch (Exception e)
            {
                if (paymentId != -1)
                {
                    paymentApi.Delete(paymentId);
                }
                return Json(new { success = false });
            }


        }

        [HttpGet]
        public ActionResult CheckPromotion(int brandId, string strTotal, string orderDetailsIDs, JQueryDataTableParamModel param)
        {
            var orderApi = new OrderApi();
            var promotionApi = new PromotionApi();
            var promotion = promotionApi.GetPromotionByIdTime(brandId);
            //promotion = promotion.Where(q => q.)

            //List<PromotionEditViewModel> list = promotion.ToList();

            PromotionEditViewModel model = new PromotionEditViewModel();
            //model.AvailablePromotion = promotion.ToSelectList(q => q.PromotionName, q => q.PromotionID.ToString(), q => false);
            model.AvailablePromotion = promotion.OrderBy(q => q.PromotionType);
            //promotion.

            //foreach trong promotion
            //promotionDetail
            //ForeachDetail.
            //If itemDetail = sản phẩm => order.product == itemDetail.product && order.quantity

            return View("PromotionApply", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="strTotal">Chuỗi chứa giá trị tổng tiền</param>
        /// <param name="orderDetailsIDs">IDs của các item được order</param>
        /// <param name="promotionId">ID của promotion vừa chọn</param>
        /// <param name="discountBillBefore">0 -> 1</param>
        /// 
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckPromotionDetail(int brandId, double strTotal,
            int[] orderDetailsIDs, int promotionId, int[] quantityDetails, double[] discount
            , double discountBillBefore)
        {
            var pvm = new PromotionApi().GetPromotionByIdTime(brandId);

            var pdvm = new PromotionDetailApi().GetActive();

            //Lấy promotionDetails có id phù hợp với promotionId
            var promotionDetails = (from PDVM in pdvm
                                    join PVM in pvm on PDVM.PromotionCode equals PVM.PromotionCode
                                    where PVM.PromotionID == promotionId
                                    select new
                                    {
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
                                        MaxBuyQuantity = PDVM.MaxBuyQuantity
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
            bool isSuccess = false;
            List<int> giftQuantity = new List<int>();
            List<string> giftItemId = new List<string>();
            List<double> discountRate = new List<double>();
            int giftItemBillId = 0;
            int giftBillQuantity = 0;
            double discountBill = 0;
            int j = 0;
            foreach (var item in promotionDetails)
            {
                if (item.ApplyLevel == 1) //sản phẩm
                {
                    int check = isHave(ids, int.Parse(item.BuyProductCode), item.MinBuyQuantity.Value, item.MaxBuyQuantity.Value, quantityDetails);
                    if (item.GiftType == 0 && check > -1) //giảm giá
                    {
                        discountRate.Insert(check, discount[check] + (1 - discount[check] * 0.01) * item.DiscountRate.Value);
                        isSuccess = true;
                    }
                    else if (item.GiftType == 1 && check > -1) //quà tặng
                    {
                        giftItemId.Insert(check, (item.GiftProductId)); // mã số của quà tặng kèm
                        giftQuantity.Insert(check, item.GiftQuantity.GetValueOrDefault());
                        isSuccess = true;

                    }
                }
                else if (item.ApplyLevel == 0) //hóa đơn
                {
                    if (item.GiftType == 0) //giảm giá
                    {
                        if (item.MinOrderAmount <= total && total <= item.MaxOrderAmount)
                        {
                            discountBill = discountBillBefore + (1 - discountBillBefore * 0.01) * item.DiscountRate.Value;
                            isSuccess = true;

                        }
                    }
                    else if (item.GiftType == 1) //quà tặng
                    {
                        if (item.MinOrderAmount <= total && total <= item.MaxOrderAmount)
                        {
                            giftItemBillId = int.Parse(item.GiftProductId);
                            giftBillQuantity = item.GiftQuantity.GetValueOrDefault();
                            isSuccess = true;

                        }
                    }
                }
                ++j;
            }
            return Json(new
            {
                success = isSuccess,
                discountRate = discountRate,
                giftItemId = giftItemId,
                giftQuantity = giftQuantity,
                promotionId = promotionId,
                promotionGroup = promotionDetails.ElementAt(0).Group,
                discountBill = discountBill,
                giftBillQuantity = giftBillQuantity,
                giftItemBillId = giftItemBillId,
            });


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

        /// <summary>
        /// Kiểm tra sản phẩm cần 
        /// </summary>
        /// <param name="ids">IDs của đống ordered item</param>
        /// <param name="code">Code của product muốn kiểm tra </param>
        /// <param name="min">Min buy product </param>
        /// <param name="max">Max buy product</param>
        /// <param name="quantity">Số lượng muốn mua</param>
        /// <returns></returns>
        private int isHave(int[] ids, int code, int min, int max, int[] quantity)
        {
            ProductViewModel pvm = new ProductApi().GetProductById(code);
            for (int i = 0; i < ids.Length; ++i)
            {
                if (ids[i] == pvm.ProductID && min <= quantity[i] && quantity[i] <= max)
                {
                    return i;
                }
            }
            return -1;
        }
        public JsonResult LoadCardPaymentCategory(int brandId)
        {
            var productCategoryApi = new ProductCategoryApi();
            var productCategories = productCategoryApi.BaseService.Get(q => q.Type == (int)ProductCategoryType.CardPayment && q.BrandId == brandId && q.Active == true)
                .Select(q => new
                {
                    CategoryId = q.CateID,
                    Name = q.CateName,
                });
            return Json(productCategories);
        }
    }

    public class StoreModel
    {
        public int StoreId { get; set; }
        public double Distance { get; set; }
    }
    class StringCompare
    {
        int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        public double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        public ProvinceViewModel SelectMostSimilarProvince(string unit, List<ProvinceViewModel> collection)
        {
            double curentSimilarity = 0;
            ProvinceViewModel mostSimilarLocation = null;
            foreach (var item in collection)
            {
                double similarity = CalculateSimilarity(unit, item.ProvinceType + " " + item.ProvinceName);
                if (similarity > curentSimilarity)
                {
                    mostSimilarLocation = item;
                    curentSimilarity = similarity;
                }
            }
            return mostSimilarLocation;
        }
        public DistrictViewModel SelectMostSimilarDistrict(string unit, List<DistrictViewModel> collection)
        {
            double curentSimilarity = 0;
            DistrictViewModel mostSimilarLocation = null;
            foreach (var item in collection)
            {
                double similarity = CalculateSimilarity(unit, item.DistrictType + " " + item.DistrictName);
                if (similarity > curentSimilarity)
                {
                    mostSimilarLocation = item;
                    curentSimilarity = similarity;
                }
            }
            return mostSimilarLocation;
        }
        public WardViewModel SelectMostSimilarWard(string unit, List<WardViewModel> collection)
        {
            double curentSimilarity = 0;
            WardViewModel mostSimilarLocation = null;
            foreach (var item in collection)
            {
                double similarity = CalculateSimilarity(unit, item.WardType + " " + item.WardName);
                if (similarity > curentSimilarity)
                {
                    mostSimilarLocation = item;
                    curentSimilarity = similarity;
                }
            }
            return mostSimilarLocation;
        }
    }
}
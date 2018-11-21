using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Web.Http;
using System.Threading.Tasks;
using StackExchange.Redis;
using RedisCache;

namespace Wisky.Api.Controllers.API
{
    public class OrderJsonApiController : ApiController
    {
        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];

        private readonly ICustomerService _customerService = DependencyUtils.Resolve<ICustomerService>();
        private readonly IProductService _productService = DependencyUtils.Resolve<IProductService>();
        private readonly IOrderService _orderService = DependencyUtils.Resolve<IOrderService>();

        IDatabase db = null;

        private IDatabase GetDatabase()
        {
            if (db == null)
            {
                //Store to Redis
                var redisFactory = RedisConnectionFactory.Instance;
                var RedisConnection = redisFactory.GetConnection();
                db = RedisConnection != null ? RedisConnection.GetDatabase() : null;
            }
            return db;
        }

        //Order to server from android devices
        [HttpPost]
        [Route("api/product/SendOrderToServerOnMobile/{token}/{terminalId}")]
        public async Task<IHttpActionResult> SendOrderToServerOnMobile(string token, string terminalId, OrderApiJson model)
        {
            //string jsonContent = json.json.Replace("'", "\"");
            // Chuyển thẳng model qua luôn, ko cần Deserialize nữa, dễ gây lỗi

            if (_enableToken.Equals("true"))
            {
                if (token != _accessToken)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Token incorrect",
                    });
                }
            }

            try
            {
                //var model = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderApiJson>(jsonContent);
                var orderApi = new OrderApi();
                var productApi = new ProductApi();
                var customerApi = new CustomerApi();
                var orderStatusModel = new OrderStatusApiViewModel();
                var brandId = (new StoreApi()).Get(terminalId).BrandId;

                var customer = customerApi.GetCustomerByNameAddressAndPhone(model.Name, model.Address, model.Phone);

                if (customer == null)
                {
                    customer = await customerApi.CreateCustomerByNameAddressAndPhone(new CustomerViewModel
                    {
                        Name = model.Name,
                        Address = model.Address,
                        Phone = model.Phone,
                        Type = 1,
                        BrandId = brandId,
                    });
                }

                //Thông tin order
                var rent = new HmsService.Models.Entities.Order()
                {
                    DeliveryAddress = model.Address,
                    CheckInDate = model.CheckInDate,
                    TotalAmount = model.TotalAmount,
                    FinalAmount = model.FinalAmount,
                    Discount = model.Discount,
                    DiscountOrderDetail = model.DiscountOrderDetail,
                    OrderStatus = (int)OrderStatusEnum.New,
                    OrderType = (int)OrderTypeEnum.Delivery,
                    StoreID = 7, //model.StoreId, //Hardcode storeID 7
                    Notes = model.Notes,
                    IsFixedPrice = model.IsFixedPrice,
                    InvoiceID = HmsService.Models.Utils.GetCurrentDateTime().Ticks.ToString() + "-" + terminalId,
                    DeliveryStatus = (int)DeliveryStatus.Assigned,
                    SourceType = 0,
                    CustomerID = customer.CustomerID
                    //GroupPaymentStatus = 0
                };

                //Lưu thông tin order
                foreach (var itemDetail in model.Items)
                {
                    if (itemDetail.ParentId == null)
                    {
                        var product = productApi.GetProductById(itemDetail.ProductId);
                        var orderDetail = new OrderDetail()
                        {
                            Discount = product.DiscountPrice,
                            TotalAmount = product.Price * itemDetail.Quantity,
                            FinalAmount = (product.Price - product.DiscountPrice) * itemDetail.Quantity,
                            ProductID = product.ProductID,
                            Quantity = itemDetail.Quantity,
                            ItemQuantity = 0,
                            Status = (int)OrderStatusEnum.New,
                            UnitPrice = product.Price,
                            OrderDate = model.CheckInDate,
                            StoreId = 7, //Hardcode storeID 7
                            //Product = productApi.GetProductById(product.ProductID).ToEntity(),
                            ProductType = product.ProductType,
                            TmpDetailId = itemDetail.TempOrderId,
                            ParentId = itemDetail.ParentId
                        };
                        rent.OrderDetails.Add(orderDetail);
                    }
                }

                var check = orderApi.CreateOrder(rent);

                if (check)
                {
                    foreach (var extraDetail in model.Items)
                    {
                        if (extraDetail.ParentId != null)
                        {
                            var product = productApi.GetProductById(extraDetail.ProductId);
                            var itemDetail = rent.OrderDetails.Where(q => q.TmpDetailId == extraDetail.ParentId).FirstOrDefault();
                            var orderDetail = new OrderDetail()
                            {
                                Discount = product.DiscountPrice,
                                TotalAmount = product.Price * extraDetail.Quantity,
                                FinalAmount = (product.Price - product.DiscountPrice) * extraDetail.Quantity,
                                ProductID = product.ProductID,
                                Quantity = extraDetail.Quantity,
                                ItemQuantity = 0,
                                Status = (int)OrderStatusEnum.New,
                                UnitPrice = product.Price,
                                OrderDate = model.CheckInDate,
                                StoreId = 7, //Hardcode storeID 7
                                //Product = productApi.GetProductById(product.ProductID).ToEntity(),
                                ProductType = product.ProductType,
                                TmpDetailId = extraDetail.TempOrderId,
                                ParentId = itemDetail.OrderDetailID
                            };
                            rent.OrderDetails.Add(orderDetail);
                        }
                    }

                    foreach (var itemDetail in rent.OrderDetails)
                    {
                        itemDetail.TmpDetailId = itemDetail.OrderDetailID;
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msg = "Tạo đơn hàng không thành công"
                    });
                }

                orderApi.EditOrder(rent);

                var msg = new MessageCOM.NotifyOrder()
                {
                    StoreId = (int)rent.StoreID,
                    //StoreName = store.Name,
                    NotifyType = (int)HmsService.Models.NotifyMessageType.OrderChange,
                    Content = "Có đơn hàng mới",
                    OrderId = rent.RentID,
                };
                NotifyOrderToPosJson(msg);

                return Json(new
                {
                    success = true,
                    msg = "Tạo đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    msg = ex.Message
                });
            }
        }

        private void NotifyOrderToPosJson(MessageCOM.NotifyOrder msg)
        {
            string code = "";
            code = _storeCode + "-" + msg.StoreId.ToString();
            //Queuer.SendMessageOrder(msg, code);
            db = GetDatabase();
            try
            {
                db.ListRightPush(code, msg.ToJson());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class OrderApiJson
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public List<Item> Items { get; set; }
            public System.DateTime CheckInDate { get; set; }
            public double TotalAmount { get; set; }
            public double Discount { get; set; }
            public double DiscountOrderDetail { get; set; }
            public double FinalAmount { get; set; }
            public int OrderType { get; set; }
            public string Notes { get; set; }
            public bool IsFixedPrice { get; set; }
        }

        public class Item
        {
            public int TempOrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public int? ParentId { get; set; }
        }
    }
}

using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using MessageCOM;
using RedisCache;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.WebPages;
using Wisky.SkyAdmin.Manage.Controllers;
using NotifyMessage = MessageCOM.NotifyMessage;
using NotifyMessageType = MessageCOM.NotifyMessageType;
using NotifyOrder = HmsService.Models.NotifyOrder;

namespace Wisky.Api.Controllers.API
{
    public class MessageApiController : ApiController
    {
        StackExchange.Redis.IDatabase db = null;

        private StackExchange.Redis.IDatabase GetDatabase()
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

        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        // GET: MessageQueuer
        //Kiểm tra trong queu có gì mới không
        /// <summary>
        /// api/message/GetMessage/494EC308-7344-41A9-9347-D05754002CFC/1
        /// </summary>
        /// <param name="token"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("api/message/GetMessage/{token}/{terminalId}")]
        public MessageSend GetMessage(string token, string terminalId)
        {
            Utils.CheckToken(token);
            db = GetDatabase();

            try
            {
                ////System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue(@".\private$\" + terminalId);
                //System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue(@".\private$\" + _storeCode + "-" + terminalId);
                //var count = queue.GetAllMessages().Count();
                var count = (int)db.ListLength(_storeCode + "-" + terminalId);
                bool flag = count > 1;

                if (count > 0)
                {
                    var m = NotifyMessage.FromJson(db.ListRange(_storeCode + "-" + terminalId, 0, 0).FirstOrDefault().ToString());
                    int checkType = m.NotifyType;
                    switch (checkType)
                    {
                        //Trường hợp có thay đổi account
                        case (int)NotifyMessageType.AccountChange:
                            var account = m;
                            var mes = new MessageSend()
                            {
                                NotifyType = account.NotifyType,
                                Content = account.Content,
                                CountQueue = count,
                                CheckFlag = flag
                            };
                            return mes;

                        //Trường hợp có thay đổi product
                        case (int)NotifyMessageType.ProductChange:
                            var product = m;
                            var pro = new MessageSend()
                            {
                                NotifyType = product.NotifyType,
                                Content = product.Content,
                                CountQueue = count,
                                CheckFlag = flag
                            };
                            return pro;
                        //Trường hợp có thay đổi category
                        case (int)NotifyMessageType.CategoryChange:
                            var category = m;
                            var cate = new MessageSend()
                            {
                                NotifyType = category.NotifyType,
                                Content = category.Content,
                                CountQueue = count,
                                CheckFlag = flag
                            };
                            return cate;
                        //Trường hợp có thay đổi order
                        case (int)NotifyMessageType.OrderChange:
                            var order = MessageCOM.NotifyOrder.FromJson(db.ListRange(_storeCode + "-" + terminalId, 0, 0).FirstOrDefault().ToString());
                            var o = new MessageSend()
                            {
                                OrderId = order.OrderId,
                                NotifyType = order.NotifyType,
                                Content = order.Content,
                                CountQueue = count,
                                CheckFlag = flag
                            };
                            return o;
                        case (int)NotifyMessageType.PromotionChange:
                            var promo = m;
                            var msg = new MessageSend()
                            {
                                NotifyType = promo.NotifyType,
                                Content = promo.Content,
                                CountQueue = count,
                                CheckFlag = flag
                            };
                            return msg;
                    }
                }
                var no = new MessageSend()
                {
                    //Error	7	'MessageQueuer.NotifyMessageType' does not contain a definition for 'NoThing' - 17/09/2015 - CuongHH
                    // Tạm thời chuyển sang NotifyType = 0
                    NotifyType = 0,
                    //NotifyType = (int)NotifyMessageType.NoThing,
                    Content = "Nothing",
                    CheckFlag = flag
                };
                return no;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MessageSend()
                {
                    NotifyType = 0,
                    Content = "Nothing",
                    CheckFlag = false
                };
            }
        }

        //Để máy POS lấy thông tin order từ trên server
        /// <summary>
        /// api/message/getOrder/{token}/{orderId}
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/message/GetOrder/{token}/{orderId}")]
        public async System.Threading.Tasks.Task<OrderApiViewModel> GetOrder(string token, int orderId)
        {
            Utils.CheckToken(token);
            var orderApi = new OrderApi();
            try
            {
                var rent = await orderApi.GetAsync(orderId);
                var orderPromotionMapping = new OrderPromotionMappingApi();
                var order = new OrderApiViewModel
                {
                    CheckInDate = rent.CheckInDate == null ? HmsService.Models.Utils.GetCurrentDateTime() : (DateTime)rent.CheckInDate,
                    TotalAmount = rent.TotalAmount,
                    Discount = rent.Discount,
                    DiscountOrderDetail = rent.DiscountOrderDetail,
                    FinalAmount = rent.FinalAmount,
                    OrderStatus = rent.OrderStatus,
                    OrderType = rent.OrderType,
                    CheckInPerson = rent.CheckInPerson,
                    IsFixedPrice = rent.IsFixedPrice,
                    SourceID = rent.SourceID ?? 43,
                    OrderCode = rent.InvoiceID,
                    DeliveryStatus = rent.DeliveryStatus != null ? (int)rent.DeliveryStatus.Value : 0,
                    DeliveryAddress = rent.DeliveryAddress == null ? "" : rent.DeliveryAddress,
                    DeliveryCustomer = rent.Customer.Name,
                    DeliveryPhone = rent.Customer.Phone,
                    Notes = rent.Notes == null ? "" : rent.Notes,
                    GroupPaymentStatus = 0, //Tạm thời để như vậy,
                    LastModifiedPayment = rent.LastModifiedPayment,
                    LastModifiedOrderDetail = rent.LastModifiedOrderDetail
                };
                //Thông tin orderDetail
                order.OrderDetailMs = rent.OrderDetails.Select(a => new OrderDetailApiViewModel
                {
                    ProductId = a.Product.ProductID,
                    ProductName = a.Product.ProductName,
                    UnitPrice = (int)a.UnitPrice,
                    Quantity = a.Quantity,
                    ItemQuantity = 0,
                    ProductCode = a.Product.Code,
                    TotalAmount = a.TotalAmount,
                    FinalAmount = a.FinalAmount,
                    Discount = a.Discount,
                    OrderDate = a.OrderDate,
                    Status = a.Status,
                    ProductType = a.ProductType == null ? -1 : (int)a.ProductType,
                    ParentId = a.ParentId,
                    TmpDetailId = a.TmpDetailId,
                    OrderDetailPromotionMappingId = a.OrderDetailPromotionMappingId,
                    OrderPromotionMappingId = a.OrderPromotionMappingId
                }).ToList();
                order.OrderPromotioMappingMs = orderPromotionMapping.GetMappingByOrderId(rent.RentID).Select(q => new OrderPromotionMappingModel()
                {
                    DiscountAmount = q.DiscountAmount,
                    DiscountRate = (int)q.DiscountRate.GetValueOrDefault(),
                    OrderId = q.Order.RentID,
                }).ToList();
                order.PaymentMs = rent.Payments.Select(a => new PaymentApiViewModel {
                    PaymentID = a.PaymentID,
                    OrderId = a.ToRentID.Value,
                    Amount = (int)a.Amount,
                    CurrencyCode = a.CurrencyCode ?? "",
                    FCAmount = a.FCAmount,
                    Notes = a.Notes ?? "",
                    PayTime = a.PayTime,
                    Status = a.Status,
                    Type = a.Type,
                    CardCode = a.CardCode ?? "",
                }).ToList();
                return order;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Xóa queue sau khi máy Pos xử lý xong việc get order
        [HttpGet]
        [Route("api/message/GetDone/{token}/{terminalId}")]
        public string GetDone(string token, string terminalId)
        {
            Utils.CheckToken(token);
            //System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue(@".\private$\" + _storeCode + "-" + terminalId);
            //queue.Receive();

            db = GetDatabase();
            try
            {

                db.ListLeftPop(_storeCode + "-" + terminalId);

                return "Solve Queue";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Fail because your connection is not existed";
            }
        }

        //Xóa queue sau khi máy Pos xử lý xong việc get account, cate, product
        [HttpGet]
        [Route("api/message/GetUpdated/{token}/{terminalId}/{type}")]
        public string GetUpdated(string token, string terminalId, int type)
        {
            Utils.CheckToken(token);
            //System.Messaging.MessageQueue queue = new System.Messaging.MessageQueue(@".\private$\" + _storeCode + "-" + terminalId);
            db = GetDatabase();

            try
            {
                var messages = db.ListRange(_storeCode + "-" + terminalId);
                var receiveMessages = new List<NotifyMessage>();
                foreach (var m in messages)
                {
                    var tmp = db.ListLeftPop(_storeCode + "-" + terminalId);
                    bool check = false;

                    //Format m before take type
                    string mStr = m.ToString();
                    int firstIndexOfComma = mStr.IndexOf(",");
                    StringBuilder sb = new StringBuilder(mStr);
                    sb = sb.Remove(firstIndexOfComma, mStr.Length - firstIndexOfComma);
                    sb = sb.Remove(0, sb.Length - 1);
                    string messageType = !sb.ToString().IsEmpty() ? sb.ToString() : "-999999";

                    //Convert after take TypeID of NotifyType
                    int checkType = Convert.ToInt32(messageType);

                    if (type == (int)NotifyMessageType.NoThing)
                    {
                        if (checkType == (int)NotifyMessageType.AccountChange
                            || checkType == (int)NotifyMessageType.CategoryChange
                            || checkType == (int)NotifyMessageType.ProductChange
                            || checkType == (int)NotifyMessageType.PromotionChange)
                        {
                            receiveMessages.Add(NotifyMessage.FromJson(m.ToString()));
                            check = true;
                        }
                    }
                    else if (checkType == type)
                    {
                        receiveMessages.Add(NotifyMessage.FromJson(m.ToString()));
                        check = true;
                    }
                    if (!check)
                    {
                        db.ListRightPush(_storeCode + "-" + terminalId, tmp);
                    }
                }

                return "Solve Queue";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Fail because your connection is not existed";
            }
        }
    }
}
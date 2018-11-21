using MessageCOM;
using RedisCache;
using SkyWeb.DatVM.Mvc;
﻿using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;
using System.Web.Mvc;

namespace Wisky.Api.Controllers.API
{
    public class PostMessageApiController : BaseController
    {
        StackExchange.Redis.IDatabase db = null;

        private StackExchange.Redis.IDatabase GetDatabase()
        {
           
           
            if (db == null)
            {
                //Store to Redis
                var redisFactory = RedisConnectionFactory.Instance;
                var redisConnection = redisFactory.GetConnection();
                //if (redisConnection == null)
                //{
                //    new RedisConnectionFactory();

                //    redisConnection = redisFactory.GetConnection();
                //}
                db = redisConnection != null ? redisConnection.GetDatabase() : null;
            }
            try
            {
                var time = db.Ping(StackExchange.Redis.CommandFlags.None);
            }
            catch (Exception)
            {
                return null;
            }
            
            return db;
        }

        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        //static readonly XmlMessageFormatter formatter = new XmlMessageFormatter(new[] { typeof(NotifyMessage) });
        // GET: PostMessageApi
        [HttpPost]
        public void PostNotiMessage(MessageCOM.NotifyMessage msg)
        {
            var storeId = msg.StoreId;
            //MessageQueue queue = new System.Messaging.MessageQueue(@".\private$\" + _storeCode + "-" + storeId);

            db = GetDatabase();

            try
            {
                var listMessage = db.ListRange(_storeCode + "-" + storeId, 0, -1);

                foreach (var m in listMessage)
                {
                    var messBody = NotifyMessage.FromJson(m.ToString());
                    if (messBody.NotifyType == msg.NotifyType)
                    {
                        return;
                    }
                }

                string code = "";
                code = _storeCode + "-" + msg.StoreId.ToString();
                //Queuer.SendMessage(msg, code);
                if (msg.NotifyType == (int)NotifyMessageType.OrderChange)
                {
                    db.ListLeftPush(code, msg.ToJson());
                } else
                {
                    db.ListRightPush(code, msg.ToJson());
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }
}
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebApplication1.Models
{
    public class Message
    {
        public string MessageId { get; set; }
        public string Content { get; set; }
        public string CreateDate { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int TotalVote { get; set; }
        public bool IsAnonymous { get; set; }

        public string  ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Message FromJson(string json)
        {
           return (Message)JsonConvert.DeserializeObject<Message>(json);
        }
    }
}
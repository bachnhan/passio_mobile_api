using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Teek.Redis
{
    public class VoteMessage
    {
        public int OptionId { get; set; }
        public string Content { get; set; }
        public double Percentage { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static VoteMessage FromJson(string json)
        {
            return JsonConvert.DeserializeObject<VoteMessage>(json);
        }
    }
}
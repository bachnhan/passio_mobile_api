using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models
{
    public class SkyPlusMessage
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }

    }

    public class NotifyMessage : SkyPlusMessage
    {
        public int NotifyType { get; set; }

        /// <summary>
        /// COntent = Null: call update all data change
        /// Order: Content = OrderId
        /// </summary>
        public string Content { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static NotifyMessage FromJson(string json)
        {
            return JsonConvert.DeserializeObject<NotifyMessage>(json);
        }
    }

    public class NotifyOrder : SkyPlusMessage
    {
        public int OrderId { get; set; }
        public string Content { get; set; }
        public int NotifyType { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static NotifyOrder FromJson(string json)
        {
            return JsonConvert.DeserializeObject<NotifyOrder>(json);
        }
    }


    public enum NotifyMessageType
    {
        NoThing = 0,
        AccountChange = 1,
        ProductChange = 2,
        CategoryChange = 3,
        OrderChange = 4,
        PromotionChange = 5,
    }
}

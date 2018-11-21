using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;

namespace MessageCOM
{
    public class Queuer
    {
        //static readonly XmlMessageFormatter formatter = new XmlMessageFormatter(new[] { typeof(NotifyMessage) });

        //public static List<MessageQueue> GetQueues(string startPattern)
        //{
        //    var queueList = MessageQueue.GetPrivateQueuesByMachine(".");
        //    return queueList.Where(q => q.QueueName.StartsWith(startPattern)).ToList();
        //}

        //public static void SendMessageToQueues(SkyPlusMessage msg, string pattern)
        //{
        //    var queues = GetQueues(pattern);

        //    foreach (var messageQueue in queues)
        //    {
        //        messageQueue.Formatter = formatter;
        //        messageQueue.Send(msg);
        //    }
        //}

        //public static void SendMessage(NotifyMessage msg, string queueName)
        //{
        //    CreatePrivateQueues(queueName);

        //    string path = (@".\private$\" + queueName);
        //    if (MessageQueue.Exists(path))
        //    {
        //        MessageQueue msgQueue = new MessageQueue(path);
        //        msgQueue.Formatter = formatter;
        //        var message = new Message();
        //        message.Label = queueName + "-" + msg.NotifyType;
        //        message.Body = msg;
        //        message.Priority = MessagePriority.Normal;  //Message thông báo có update
        //        msgQueue.Send(message);
        //    }
        //}

        //public static void SendMessageOrder(NotifyOrder msg, string queueName)
        //{
        //    CreatePrivateQueues(queueName);

        //    string path = (@".\private$\" + queueName);
        //    if (MessageQueue.Exists(path))
        //    {
        //        MessageQueue msgQueue = new MessageQueue(path);
        //        var message = new Message();
        //        message.Label = queueName + "-" + msg.NotifyType;
        //        message.Body = msg;
        //        message.Priority = MessagePriority.High;    //Message thông báo Order
        //        msgQueue.Formatter = formatter;
        //        msgQueue.Send(message);
        //    }
        //}

        ////public static void SendMessageOrder(NotifyOrder msg, string queueName)
        ////{
        ////    CreatePrivateQueues(queueName);

        ////    string path = (@".\private$\" + queueName);
        ////    if (MessageQueue.Exists(path))
        ////    {
        ////        MessageQueue msgQueue = new MessageQueue(path);
        ////        msg.Priority = MessagePriority.High;
        ////        msgQueue.Formatter = new BinaryMessageFormatter();
        ////        var message = new Message();
        ////        message.Label = msg.Content;
        ////        message.BodyType = 1;
        ////        message.Body = msg.RentId;
        ////        msgQueue.Send(message);
        ////    }
        ////}

        //public static void CreatePrivateQueues(string queueName)
        //{
        //    // Create and connect to a private Message Queuing queue. 
        //    if (!MessageQueue.Exists(".\\Private$\\" + queueName))
        //    {
        //        // Create the queue if it does not exist.
        //        MessageQueue myNewPrivateQueue =
        //            MessageQueue.Create(".\\Private$\\" + queueName);
                
        //        myNewPrivateQueue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl);
        //    }
        //}
    }
}

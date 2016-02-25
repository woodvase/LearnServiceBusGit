using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;

namespace ServiceBus
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionStr = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            NamespaceManager nsm = NamespaceManager.CreateFromConnectionString(connectionStr);
            string topicName = "yolitopic";
            TopicDescription topic = null;
            if (!nsm.TopicExists(topicName))
            {
                nsm.CreateTopic(new TopicDescription(topicName) { 
                    EnablePartitioning = false
                });
            }

            topic = nsm.GetTopic(topicName);

            if (topic.SubscriptionCount == 0)
            {
                SqlFilter filter1 = new SqlFilter("(index % 2) = 0");
                nsm.CreateSubscription(topic.Path, "YoliSubscription1", filter1);
                SqlFilter filter2 = new SqlFilter("(index % 2) > 0");
                nsm.CreateSubscription(topic.Path, "YoliSubscription2", filter2);
            }
            //
            //sadfsdf

            foreach(var s in nsm.GetSubscriptions(topicName))
            {
                Console.WriteLine(s.Name);
                foreach(var r in nsm.GetRules(topic.Path,s.Name))
                {
                    Console.WriteLine("{0}-{1}", r.Name, r.Filter.ToString());
                }
            }

            Console.WriteLine("Sending message to topic");

            TopicClient topicClient = TopicClient.CreateFromConnectionString(connectionStr, topicName);
            for (int i = 0; i < 5000; i++)
            {
                BrokeredMessage message = new BrokeredMessage();
                message.Properties["index"] = i;
                message.Properties["value"] = (i * 10 + 5) % 11;
                topicClient.Send(message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

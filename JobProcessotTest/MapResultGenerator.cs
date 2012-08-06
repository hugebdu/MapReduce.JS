using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace JobProcessotTest
{
    class MapResultGenerator
    {
        private const string ServiceBusAddress = "sb://MapReduceJS.servicebus.windows.net";
        private const string ServiceBusIssuerName = "owner";
        private const string ServiceBusIssuerSecret = "kpt3ZfiSyEiC1GKZ9ZkjwuFG2kNMEv9Bl+aHt4Z7tNw=";

        private string _watchingQueueName;
        QueueClient queueClient;
        public MapResultGenerator()
        {
            _watchingQueueName = JobProcessor.RoleSettings.ChunkResponseQueue;
            queueClient = PrepareWatchingQueue();
        }

        public void test()
        {
            var body = "{\"ChunkUid\":{\"JobId\":\"job1\",\"SplitId\":\"d54f9e43-a754-496f-b57f-20cbb6ced358\"},\"Data\":\"hello from mapper\",\"ProcessorNodeId\":\"node1\"}";
            using (var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(body)))
            {
                var msg = new BrokeredMessage(body);
                var s = msg.GetBody<string>();
                s = null;
            }
        }

        public void Pop()
        {
            var msg = queueClient.Receive();
            if (msg != null)
                msg.Complete();
        }


        public void Generate(string key, string value, string splitid)
        {
            if (string.IsNullOrEmpty(splitid))
                splitid = "d54f9e43-a754-496f-b57f-20cbb6ced358";
            if (string.IsNullOrEmpty(key))
                key = "akey";
            if (string.IsNullOrEmpty(value))
                value = "vvvalue";

            var body = string.Format("{{\"ChunkUid\":{{\"JobId\":\"job1\",\"SplitId\":\"{2}\"}},\"Data\":\"{0},{1}\",\"ProcessorNodeId\":\"node1\"}}", key, value, splitid);
            var msg = new BrokeredMessage(body);
            queueClient.Send(msg);
        }

        private QueueClient PrepareWatchingQueue()
        {
            var namespaceManagerSetting = new NamespaceManagerSettings()
            {
                TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(ServiceBusIssuerName, ServiceBusIssuerSecret),
            };

            var namespaceManager = new NamespaceManager(ServiceBusAddress, namespaceManagerSetting);
            if (!namespaceManager.QueueExists(_watchingQueueName))
            {
                namespaceManager.CreateQueue(_watchingQueueName);
            }

            var factory = MessagingFactory.Create(ServiceBusAddress, SharedSecretTokenProvider.CreateSharedSecretTokenProvider(ServiceBusIssuerName, ServiceBusIssuerSecret));
            var queueClient = factory.CreateQueueClient(_watchingQueueName);
            return queueClient;
        }
    }
}

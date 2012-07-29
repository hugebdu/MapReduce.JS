using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using JobProcessor.Interfaces;
using JobProcessor.Model;

namespace JobProcessor.Implementation
{
    class SBJobChunkResultWatcher : IJobChunkResultWatcher
    {
        private const string ServiceBusAddress = "sb://MapReduceJS.servicebus.windows.net";
        private const string ServiceBusIssuerName = "Endpoint=sb://MapReduceJS.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=kpt3ZfiSyEiC1GKZ9ZkjwuFG2kNMEv9Bl+aHt4Z7tNw=";
        private const string ServiceBusIssuerSecret = "kpt3ZfiSyEiC1GKZ9ZkjwuFG2kNMEv9Bl+aHt4Z7tNw=";
        
        private const int WatcherRateMs = 10000;

        public event Action<object, MapResultMessage> ChunkMapResultArrive;
        
        private Task _watcher;
        private CancellationTokenSource _cancellationTokenSource;
        private string _watchingQueueName;

        public SBJobChunkResultWatcher()
        {
            // Normally we will read it from the configuration file - different for every worker role
            _watchingQueueName = "MapResultsQueue";
        }

        public void StartWatch()
        {
            Logger.Log.Instance.Info("ChunkResultWatcher. Start watch request");
            
            if (_watcher != null)
            {
                if (_watcher.Status == TaskStatus.Running)
                {
                    Logger.Log.Instance.Info("ChunkResultWatcher. Watcher is running already. Do nothing");
                    return;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _watcher = Task.Factory.StartNew(Watch, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, null);
        }

        public void StopWatch()
        {
            Logger.Log.Instance.Info("ChunkResultWatcher. Stop watcher request");
            
            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();
        }

        private void Watch()
        {
            Logger.Log.Instance.Info("ChunkResultWatcher. Watcher started");

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

            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Logger.Log.Instance.Info("ChunkResultWatcher. Watcher stopped by cancel request");
                    return;
                }

                Logger.Log.Instance.Debug("ChunkResultWatcher. Watcher -> check for new messages");
                var msg = queueClient.Receive();
                if (msg != null)
                    ProcessMessage(msg);
                else
                    Thread.Sleep(WatcherRateMs);
            }
        }

        private void ProcessMessage(BrokeredMessage msg)
        {
            try
            {
                var body = msg.GetBody<string>();
                if (string.IsNullOrEmpty(body))
                {
                    Logger.Log.Instance.Warning(string.Format("ChunkResultWatcher. Cannot process map result message - body is null"));
                    return;
                }

                var mapResult = Newtonsoft.Json.JsonConvert.DeserializeObject(body, typeof(MapResultMessage)) as MapResultMessage;
                if (mapResult == null)
                {
                    Logger.Log.Instance.Warning(string.Format("ChunkResultWatcher. Process map result message - cannot get MapResultMsg (null) from body '{0}'", body));
                    return;
                }
                var chunkMapResultArrive = ChunkMapResultArrive;
                if (chunkMapResultArrive != null)
                {
                    chunkMapResultArrive(this, mapResult);
                }

                msg.Complete();
            }
            catch (Exception ex)
            {
                msg.Abandon();
                Logger.Log.Instance.Error(string.Format("ChunkResultWatcher. Failed to process map result message: {0}", ex.Message));
            }
        }
    }
}

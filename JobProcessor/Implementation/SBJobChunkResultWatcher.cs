using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using JobProcessor.Interfaces;
using JobProcessor.Model;
using JobProcessor;

namespace JobProcessor.Implementation
{
    class SBJobChunkResultWatcher : IJobChunkResultWatcher
    {
        #region Constants
        private const int WatcherRateMs = 10000;
        #endregion Constants

        #region Events
        public event Action<object, MapResultMessage> ChunkMapResultArrive;
        #endregion Events

        #region Data members
        private Task _watcher;
        private CancellationTokenSource _cancellationTokenSource;
        private string _watchingQueueName;
        #endregion Data members

        #region Ctor
        public SBJobChunkResultWatcher()
        {
            // Normally we will read it from the configuration file - different for every worker role
            _watchingQueueName = JobProcessor.RoleSettings.ChunkResponseQueue;
        }
        #endregion Ctor

        #region Public Methods
        internal void SyncWatch()
        {
            var queueClient = PrepareWatchingQueue();
            Watch(queueClient);
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

            var queueClient = PrepareWatchingQueue();
            _cancellationTokenSource = new CancellationTokenSource();
            _watcher = Task.Factory.StartNew(new Action<object>(Watch), queueClient,_cancellationTokenSource.Token);
        }

        public void StopWatch()
        {
            Logger.Log.Instance.Info("ChunkResultWatcher. Stop watcher request");
            
            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();
        }
        #endregion Public Methods

        #region Private Methods
        private void Watch(object objQueueClient)
        {
            Logger.Log.Instance.Info("ChunkResultWatcher. Watcher started");

            var queueClient = (QueueClient)objQueueClient;
            while (true)
            {
                if (_cancellationTokenSource!=null && _cancellationTokenSource.IsCancellationRequested)
                {
                    Logger.Log.Instance.Info("ChunkResultWatcher. Watcher stopped by cancel request");
                    return;
                }

                try
                {
                    Logger.Log.Instance.Debug("ChunkResultWatcher. Watcher -> check for new messages");
                    var msg = queueClient.Receive();
                    if (msg != null)
                        ProcessMessage(msg);
                    else
                        Thread.Sleep(WatcherRateMs);
                }
                catch (Exception ex)
                {
                    Logger.Log.Instance.Exception(ex, string.Format("ChunkResultWatcher. Watcher cycle failed. Wait {0} ms and retry", WatcherRateMs * 5));
                    Thread.Sleep(WatcherRateMs * 5);
                }
            }
        }

        private QueueClient PrepareWatchingQueue()
        {
            var namespaceManagerSetting = new NamespaceManagerSettings()
            {
                TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(RoleSettings.ServiceBusIssuerName, RoleSettings.ServiceBusIssuerSecret),
            };

            var namespaceManager = new NamespaceManager(RoleSettings.ServiceBusAddress, namespaceManagerSetting);
            if (!namespaceManager.QueueExists(_watchingQueueName))
            {
                namespaceManager.CreateQueue(_watchingQueueName);
            }

            var factory = MessagingFactory.Create(RoleSettings.ServiceBusAddress, SharedSecretTokenProvider.CreateSharedSecretTokenProvider(RoleSettings.ServiceBusIssuerName, RoleSettings.ServiceBusIssuerSecret));
            var queueClient = factory.CreateQueueClient(_watchingQueueName);
            return queueClient;
        }

        private void ProcessMessage(BrokeredMessage msg)
        {
            try
            {
                Logger.Log.Instance.Info(string.Format("ChunkResultWatcher. Got a map result message #{0}. Process.", msg.MessageId));
                
                string body = null;
                using (var streamReader = new System.IO.StreamReader(msg.GetBody<Stream>()))
                { 
                    body = streamReader.ReadToEnd();
                }
                
                if (string.IsNullOrEmpty(body))
                {
                    Logger.Log.Instance.Warning(string.Format("ChunkResultWatcher. Cannot process map result message - body is null"));
                    return;
                }

                Logger.Log.Instance.Info(string.Format("ChunkResultWatcher. Process map result message: {0}", body));

                var mapResult = Newtonsoft.Json.JsonConvert.DeserializeObject(body, typeof(MapResultMessage)) as MapResultMessage;
                if (mapResult == null)
                {
                    Logger.Log.Instance.Warning(string.Format("ChunkResultWatcher. Process map result message - cannot get MapResultMsg (null) from body '{0}'", body));
                    return;
                }
                Logger.Log.Instance.Info(string.Format("ChunkResultWatcher. Map result message is for JobId '{0}', ChunkId '{1}'",
                    mapResult.ChunkUid.JobId,
                    mapResult.ChunkUid.ChunkId));

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
                Logger.Log.Instance.Error(string.Format("ChunkResultWatcher. Failed to process map result message #{1}. Error: {0}", ex.Message, msg.MessageId));
            }
        }
        #endregion Private Methods
    }
}

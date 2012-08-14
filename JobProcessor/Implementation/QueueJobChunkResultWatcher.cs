using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;
using JobProcessor.Interfaces;
using JobProcessor.Model;
using JobProcessor;

namespace JobProcessor.Implementation
{
    class QueueJobChunkResultWatcher : IJobChunkResultWatcher
    {
        #region Constants
        private const int WatcherRateMs = 10000;
        #endregion Constants

        #region Events
        public event Action<object, ChunkResultMessage> ChunkResultArrive;
        #endregion Events

        #region Data members
        private Task _watcher;
        private CancellationTokenSource _cancellationTokenSource;
        private string _watchingQueueName;
        #endregion Data members

        #region Ctor
        public QueueJobChunkResultWatcher()
        {
            // Normally we will read it from the configuration file - different for every worker role
            _watchingQueueName = JobProcessor.RoleSettings.ChunkResponseQueue;
        }
        #endregion Ctor

        #region Public Methods
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

            var queue = (CloudQueue)objQueueClient;
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
                    var msg = queue.GetMessage();
                    if (msg != null)
                    {
                        if (ProcessMessage(msg))
                            queue.DeleteMessage(msg);
                    }
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

        private CloudQueue PrepareWatchingQueue()
        {

            var queue = AzureClient.Instance.QueueClient.GetQueueReference(_watchingQueueName);
            queue.CreateIfNotExist();
            return queue;
        }

        private bool ProcessMessage(CloudQueueMessage msg)
        {
            try
            {
                Logger.Log.Instance.Info(string.Format("ChunkResultWatcher. Got a chunk result message #{0}. Process.", msg.Id));
                
                string body = msg.AsString;
                
                if (string.IsNullOrEmpty(body))
                {
                    Logger.Log.Instance.Warning(string.Format("ChunkResultWatcher. Cannot process chunk result message - body is null"));
                    return true;
                }

                Logger.Log.Instance.Info(string.Format("ChunkResultWatcher. Process chunk result message: {0}", body));

                var chunkResult = Newtonsoft.Json.JsonConvert.DeserializeObject(body, typeof(ChunkResultMessage)) as ChunkResultMessage;
                if (chunkResult == null)
                {
                    Logger.Log.Instance.Warning(string.Format("ChunkResultWatcher. Process chunk result message - cannot get ChunkResultMessage (null) from body '{0}'", body));
                    return true;
                }

                Logger.Log.Instance.Info(string.Format("ChunkResultWatcher. {2} chunk result message is for JobId '{0}', ChunkId '{1}'",
                    chunkResult.ChunkUid.JobId,
                    chunkResult.ChunkUid.ChunkId,
                    chunkResult.Mode));

                var chunkResultArrive = ChunkResultArrive;
                if (chunkResultArrive != null)
                {
                    chunkResultArrive(this, chunkResult);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Error(string.Format("ChunkResultWatcher. Failed to process chunk result message #{1}. Error: {0}", ex.Message, msg.Id));
                return false;
            }
        }
        #endregion Private Methods
    }
}

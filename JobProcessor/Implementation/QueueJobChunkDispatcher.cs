using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using JobProcessor;
using JobProcessor.Model;
using JobProcessor.Interfaces;

namespace JobProcessor.Implementation
{
    class QueueJobChunkDispatcher : IJobChunkDispatcher
    {
        #region Data Members
        private CloudQueue _queue;
        private IJobChunkRegistrator _jobChunkRegistrator;
        #endregion Data Members

        #region Ctor
        public QueueJobChunkDispatcher(IJobChunkRegistrator jobChunkRegistrator)
        {
            _jobChunkRegistrator = jobChunkRegistrator;
            _queue = AzureClient.Instance.QueueClient.GetQueueReference(RoleSettings.JobsChunksQueueName);
            _queue.CreateIfNotExist();
        }
        #endregion Ctor

        #region Public Methods
        public void Dispatch(JobChunk chunk)
        {
            Logger.Log.Instance.Info(string.Format("QueueJobChunkDispatcher. Dispatch chunk. JobId '{0}', ChunkId '{1}'",
                chunk.ChunkUid.JobId,
                chunk.ChunkUid.ChunkId));
            _queue.AddMessage(new CloudQueueMessage(chunk.ToJson()));
            _jobChunkRegistrator.UpdateChunkMapSent(chunk.ChunkUid);
        }
        #endregion Public Methods
    }
}

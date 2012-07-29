using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor;
using JobProcessor.Model;
using Microsoft.WindowsAzure.StorageClient;
using JobProcessor.Interfaces;

namespace JobProcessor.Implementation
{
    class QueueJobChunkDispatcher : IJobChunkDispatcher
    {
        private const string QueueName = "jobchunksqueue";
        private CloudQueue _queue;
        private IJobChunkRegistrator _jobChunkRegistrator;

        public QueueJobChunkDispatcher(IJobChunkRegistrator jobChunkRegistrator)
        {
            _jobChunkRegistrator = jobChunkRegistrator;
            _queue = AzureClient.Instance.QueueClient.GetQueueReference(QueueName);
            _queue.CreateIfNotExist();
        }

        public void Dispatch(JobChunk chunk)
        {
            _queue.AddMessage(new CloudQueueMessage(chunk.ToJson()));
            _jobChunkRegistrator.UpdateChunkMapSent(chunk.ChunkUid);
        }
    }
}

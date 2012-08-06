using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using JobProcessor.Exceptions;
using JobProcessor.Model;
using JobProcessor.Interfaces;

namespace JobProcessor.Implementation
{
    internal class DefaultJobChunksProvider : IJobChunksProvider
    {
        public IEnumerable<JobChunk> SplitJob(JobInfo info)
        {
            Logger.Log.Instance.Info(string.Format("DefaultJobChunksProvider. Split job. JobId '{0}'",
                info.JobId));

            var blobDirectory = AzureClient.Instance.BlobClient.GetBlobDirectoryReference(info.DataSource.ToString());
            if (blobDirectory == null)
                throw new JobProcessorException("Data source is not available");

            var blobs = blobDirectory.ListBlobs();

            foreach (var blob in blobs)
            {
                var chunk = new JobChunk()
                {
                    Data = blob.Uri,
                    Handler = info.Mapper,
                    Mode = ProcessingMode.Map,
                    ResponseQueueName = JobProcessor.RoleSettings.ChunkResponseQueue
                };

                chunk.ChunkUid.ChunkId = Guid.NewGuid().ToString();
                chunk.ChunkUid.JobId = info.JobId;
                chunk.ChunkUid.JobName = info.JobName;

                Logger.Log.Instance.Info(string.Format("DefaultJobChunksProvider. Create new map chunk for JobId '{0}'. ChunkId: '{1}'",
                    chunk.ChunkUid.JobId,
                    chunk.ChunkUid.ChunkId));

                yield return chunk;
            }
        }
    }
}

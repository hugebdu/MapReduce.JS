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
                };

                chunk.ChunkUid.SplitId = Guid.NewGuid().ToString();
                chunk.ChunkUid.JobId = info.JobId;

                yield return chunk;
            }
        }
    }
}

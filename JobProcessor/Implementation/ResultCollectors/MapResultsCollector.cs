using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;
using JobProcessor.Exceptions;
using JobProcessor.Interfaces;
using Microsoft.WindowsAzure.StorageClient;

namespace JobProcessor.Implementation
{
    class MapResultsCollector : ResultsCollector, IMapResultsCollector
    {
        #region Ctor
        public MapResultsCollector(JobInfo jobInfo)
            : base(jobInfo)
        {
        }
        #endregion Ctor

        #region Public methods
        
        public IEnumerable<JobChunk> SplittedMappedData()
        {
            Logger.Log.Instance.Info(string.Format("MapResultsCollector. SplittedMappedData called for job {0}", JobId));
            
            foreach (var item in _results)
            {
                var message = new KeyValueMessage()
                {
                    key = item.Key,
                    value = item.Value.ToArray()
                };

                var jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                var blob = UploadToBlob(item.Key, jsonMessage);
                
                var chunk = new JobChunk()
                {
                    Mode = ProcessingMode.Reduce,
                    Handler = _jobInfo.Handler,
                    Data = blob.Uri.ToString(),
                    IsBlob = true,
                    BlobContainer = blob.Container.Name,
                    BlobName = (blob as CloudBlockBlob).Name,
                    ResponseQueueName = JobProcessor.RoleSettings.ChunkResponseQueue
                };
                chunk.ChunkUid.JobId = JobId;
                chunk.ChunkUid.JobName = _jobInfo.JobName;
                chunk.ChunkUid.ChunkId = Guid.NewGuid().ToString();

                yield return chunk;
            }
        }
        #endregion Public methods

        #region Private methods
        private CloudBlob UploadToBlob(string filename, string value)
        {
            //TODO: add errors handling + record created blobs
            var directoryRef = AzureClient.Instance.BlobClient.GetBlobDirectoryReference(string.Format("mr{0}", JobId).ToLower());
            directoryRef.Container.CreateIfNotExist();
            //var containerRef = AzureClient.Instance.BlobClient.GetContainerReference(string.Format("mapreducejs_{0}", JobId));
            //containerRef.SetPermissions(new Microsoft.WindowsAzure.StorageClient.BlobContainerPermissions()
            //{
            //    PublicAccess = Microsoft.WindowsAzure.StorageClient.BlobContainerPublicAccessType.Container
            //});
            //containerRef.CreateIfNotExist();
            var blobRef = directoryRef.GetBlobReference(filename);
            blobRef.UploadText(value);
            return blobRef;
        }

        #endregion Private methods
    }
}

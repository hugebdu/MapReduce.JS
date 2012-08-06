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
    class MapResultsCollector : IMapResultsCollector
    {
        #region Properties
        public string JobId { get { return _jobInfo.JobId; } }
        #endregion Properties

        #region Data Members
        private SortedList<string, string> _results;
        private JobInfo _jobInfo;
        #endregion Data Members

        #region Ctor
        public MapResultsCollector(JobInfo jobInfo)
        {
            _jobInfo = jobInfo;
            _results = new SortedList<string, string>();
        }
        #endregion Ctor

        #region Public methods
        public void AddResult(MapResultMessage mapResultMessage)
        {
            Logger.Log.Instance.Info(string.Format("MapResultsCollector. Add partial map result for JobId '{0}', ChunkId '{0}'",
                mapResultMessage.ChunkUid.JobId,
                mapResultMessage.ChunkUid.ChunkId));

            if (mapResultMessage.ChunkUid.JobId != JobId)
            {
                throw new InvalidChunkException()
                {
                    ChunkUid = mapResultMessage.ChunkUid,
                    CorrectJobId = JobId
                };
            }


            // TODO: Check this logic!!!!!
            var key = mapResultMessage.Data.Substring(0, mapResultMessage.Data.IndexOf(','));
            var value = mapResultMessage.Data.Substring(mapResultMessage.Data.IndexOf(',') + 1);

            if (_results.ContainsKey(key))
            {
                var newValue = Merge(_results[key], value);
                _results[key] = value;
            }
            else
            {
                _results.Add(key, value);
            }
        }

        public IEnumerable<JobChunk> SplittedMappedData()
        {
            Logger.Log.Instance.Info(string.Format("MapResultsCollector. SplittedMappedData called for job {0}", JobId));
            
            foreach (var item in _results)
            {
                var blob = UploadToBlob(item.Key, item.Value);
                var chunk = new JobChunk()
                {
                    Mode = ProcessingMode.Reduce,
                    Handler = _jobInfo.Reducer,
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
        private CloudBlob UploadToBlob(string key, string value)
        {
            //TODO: add errors handling + record created blobs
            var directoryRef = AzureClient.Instance.BlobClient.GetBlobDirectoryReference(string.Format("mapreducejs_{0}", JobId));
            directoryRef.Container.CreateIfNotExist();
            var containerRef = AzureClient.Instance.BlobClient.GetContainerReference(string.Format("mapreducejs_{0}", JobId));
            //containerRef.SetPermissions(new Microsoft.WindowsAzure.StorageClient.BlobContainerPermissions()
            //{
            //    PublicAccess = Microsoft.WindowsAzure.StorageClient.BlobContainerPublicAccessType.Container
            //});
            containerRef.CreateIfNotExist();
            var blobRef = containerRef.GetBlobReference(key);
            blobRef.UploadText(value);
            return blobRef;
        }

        private string Merge(string value1, string value2)
        {
            var jArray1 = Newtonsoft.Json.Linq.JArray.Parse(value1);
            var jArray2 = Newtonsoft.Json.Linq.JArray.Parse(value2);

            foreach (var item in jArray2)
            {
                jArray1.Add(item);
            }

            return jArray1.ToString();
        }
        #endregion Private methods
    }
}

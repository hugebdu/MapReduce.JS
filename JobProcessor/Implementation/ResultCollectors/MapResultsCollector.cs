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
        #region Const
        private const double MaxReduceChunkLength = 500 * 1024;
        #endregion Const

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

            var messages = new List<KeyValueMessage>();

            for (var i = 0; i < _results.Keys.Count; i++)
            {
                var message = new KeyValueMessage()
                {
                    key = _results.Keys[i],
                    value = _results[_results.Keys[i]].ToArray()
                };

                messages.Add(message);

                if (i % 500 == 0 || i == _results.Keys.Count - 1)
                {
                    var jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messages);
                    if (jsonMessage.Length > MaxReduceChunkLength
                        || i == _results.Keys.Count - 1)
                    {
                        var blob = UploadToBlob(string.Format("rchunk{0}.json", i), jsonMessage);

                        var chunk = new JobChunk()
                        {
                            Mode = ProcessingMode.Reduce,
                            Handler = _jobInfo.Handler,
                            Data = blob.Uri.ToString(),
                            IsBlob = true,
                            BlobContainer = blob.Container.Name,
                            BlobName = blob.Name,
                            ResponseQueueName = JobProcessor.RoleSettings.ChunkResponseQueue
                        };
                        chunk.ChunkUid.JobId = JobId;
                        chunk.ChunkUid.JobName = _jobInfo.JobName;
                        chunk.ChunkUid.ChunkId = Guid.NewGuid().ToString();

                        yield return chunk;

                        messages.Clear();
                    }
                }
            }

            _results.Clear();
        }

        public void CleanUp()
        {
            var directoryRef = GetBlobDirectoryRef();
            directoryRef.Container.Delete();
        }

        #endregion Public methods

        #region Private methods
        private CloudBlob UploadToBlob(string filename, string value)
        {
            //TODO: add errors handling + record created blobs
            var directoryRef = GetBlobDirectoryRef();
            directoryRef.Container.CreateIfNotExist();
            var blobRef = directoryRef.GetBlobReference(filename);
            blobRef.UploadText(value);
            return blobRef;
        }

        private CloudBlobDirectory GetBlobDirectoryRef()
        {
            var jobIdForBlod = SanitizeJobIdToBlobName();
            Logger.Log.Instance.Info(string.Format("MapResultsCollector. Generate blob directory: {0}", jobIdForBlod));
            return AzureClient.Instance.BlobClient.GetBlobDirectoryReference(string.Format("mr{0}", jobIdForBlod));
        }

        #endregion Private methods
    }
}

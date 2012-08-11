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
    class ReduceResultsCollector : ResultsCollector, IReduceResultsCollector
    {
        #region Ctor
        public ReduceResultsCollector(JobInfo jobInfo)
            : base(jobInfo)
        {
        }
        #endregion Ctor

        #region Public methods
        public void SubmitResult()
        {
            Logger.Log.Instance.Info(string.Format("ReduceResultsCollector. SubmitResult called for job {0}", JobId));

            var resultArray = _results.Select((pair) =>
            {
                return new KeyValueMessage()
                    {
                        key = pair.Key,
                        value = pair.Value.Count > 1 ? pair.Value.ToArray() : pair.Value[0]
                    };
            }).ToArray();

            var jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(resultArray);
            var blob = UploadToBlob(jsonMessage);

        }
        #endregion Public methods

        #region Private methods
        private CloudBlob UploadToBlob(string value)
        {
            //TODO: add errors handling + record created blobs
            var jobIdForBlod = SanitizeJobIdToBlobName();
            var directoryName = string.Format("result{0}", jobIdForBlod).ToLower();
            Logger.Log.Instance.Info(string.Format("ReduceResultsCollector. Generate blob directory: {0}", directoryName));
            var directoryRef = AzureClient.Instance.BlobClient.GetBlobDirectoryReference(directoryName);
            directoryRef.Container.CreateIfNotExist();
            var blobRef = directoryRef.GetBlobReference(string.Format("{0}.json", jobIdForBlod));
            blobRef.UploadText(value);
            return blobRef;
        }

        #endregion Private methods
    }
}

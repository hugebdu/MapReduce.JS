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
                        value = pair.Value.ToArray()
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
            var directoryRef = AzureClient.Instance.BlobClient.GetBlobDirectoryReference(string.Format("result{0}", JobId).ToLower());
            directoryRef.Container.CreateIfNotExist();
            //var containerRef = AzureClient.Instance.BlobClient.GetContainerReference(string.Format("mapreducejs_{0}", JobId));
            //containerRef.SetPermissions(new Microsoft.WindowsAzure.StorageClient.BlobContainerPermissions()
            //{
            //    PublicAccess = Microsoft.WindowsAzure.StorageClient.BlobContainerPublicAccessType.Container
            //});
            //containerRef.CreateIfNotExist();
            var blobRef = directoryRef.GetBlobReference(string.Format("{0}.json",JobId));
            blobRef.UploadText(value);
            return blobRef;
        }

        #endregion Private methods
    }
}

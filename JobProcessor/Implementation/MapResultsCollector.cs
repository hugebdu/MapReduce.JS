using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;
using JobProcessor.Exceptions;
using JobProcessor.Interfaces;

namespace JobProcessor.Implementation
{
    class MapResultsCollector : IMapResultsCollector
    {
        public string JobId { get { return _jobInfo.JobId; } }
        private SortedList<string, string> _results;
        private JobInfo _jobInfo;

        public MapResultsCollector(JobInfo jobInfo)
        {
            _jobInfo = jobInfo;
            _results = new SortedList<string, string>();
        }

        public void AddResult(MapResultMessage mapResultMessage)
        {
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
            var value = mapResultMessage.Data.Substring(mapResultMessage.Data.IndexOf(','));

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
            foreach (var item in _results)
            {
                Uri dataUri = UploadToBlob(item.Key, item.Value);
                var chunk = new JobChunk()
                {
                    Mode = ProcessingMode.Reduce,
                    Handler = _jobInfo.Reducer,
                    Data = dataUri
                };
                chunk.ChunkUid.JobId = JobId;
                chunk.ChunkUid.SplitId = Guid.NewGuid().ToString();

                yield return chunk;
            }
        }

        private Uri UploadToBlob(string key, string value)
        {
            //TODO: add errors handling + record created blobs
            var containerRef = AzureClient.Instance.BlobClient.GetContainerReference(string.Format("MapReduceJS_{0}", JobId));
            containerRef.CreateIfNotExist();
            var blobRef = containerRef.GetBlobReference(key);
            blobRef.UploadText(value);
            return blobRef.Uri;
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
    }
}

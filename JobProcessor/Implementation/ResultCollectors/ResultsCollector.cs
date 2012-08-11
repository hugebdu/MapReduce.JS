using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Exceptions;
using JobProcessor.Interfaces;
using JobProcessor.Model;

namespace JobProcessor.Implementation
{
    class ResultsCollector : IResultsCollector
    {
        #region Properties
        public string JobId { get { return _jobInfo.JobId; } }
        #endregion Properties

        #region Data Members
        protected readonly SortedList<string, List<object>> _results;
        protected readonly JobInfo _jobInfo;
        #endregion Data Members

        #region Ctor
        public ResultsCollector(JobInfo jobInfo)
        {
            _jobInfo = jobInfo;
            _results = new SortedList<string, List<object>>();
        }
        #endregion Ctor

        #region Public methods
        public void AddResult(ChunkResultMessage mapResultMessage)
        {
            Logger.Log.Instance.Info(string.Format("Base ResultsCollector. Add partial map result for JobId '{0}', ChunkId '{0}'",
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

            foreach (var keyValuePair in mapResultMessage.Data)
            {
                if (_results.ContainsKey(keyValuePair.Key))
                {
                    _results[keyValuePair.Key].Add(keyValuePair.Value);
                }
                else
                {
                    _results.Add(keyValuePair.Key, new List<object> { keyValuePair.Value });
                }

            }
        }
        #endregion Public Methods
    }
}

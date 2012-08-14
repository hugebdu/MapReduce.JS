using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;
using JobProcessor.Interfaces;

namespace JobProcessor.Manage
{
    class JobProcessData
    {
        #region Properties
        public JobInfo JobInfo { get; set; }
        public IMapResultsCollector MapResultsCollector { get; private set; }
        public IReduceResultsCollector ReduceResultsCollector { get; private set; }
        public Action<JobInfo, JobProcessStatus> JobCompleteCallback;
        #endregion Properties

        #region Ctor
        public JobProcessData(IFactory factory, JobInfo jobInfo)
        {
            this.JobInfo = jobInfo;
            MapResultsCollector = factory.CreateMapResultsCollector(jobInfo);
            ReduceResultsCollector = factory.CreateReduceResultsCollector(jobInfo);
        }
        #endregion Ctor
    }
}
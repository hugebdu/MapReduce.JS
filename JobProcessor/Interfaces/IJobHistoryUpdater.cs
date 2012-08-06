using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;

namespace JobProcessor.Interfaces
{
    public interface IJobHistoryUpdater
    {
        void AddChunckInfo(JobChunk chunk);
        void UpdateJobStatus(JobInfo jobInfo, JobProcessStatus status);
    }
}

using System;
using JobProcessor.Model;

namespace JobProcessor.Interfaces
{
    public interface IJobChunksProvider
    {
        System.Collections.Generic.IEnumerable<JobChunk> SplitJob(JobInfo info);
    }
}

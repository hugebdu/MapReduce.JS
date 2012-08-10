using System;
using JobProcessor.Model;

namespace JobProcessor.Interfaces
{
    public interface IFactory
    {
        JobProcessor.Interfaces.IJobChunkDispatcher CreateChunkDispatcher(IJobChunkRegistrator jobChunkRegistrator);
        JobProcessor.Interfaces.IJobChunksProvider CreateChunksProvider();
        JobProcessor.Interfaces.IJobChunkRegistrator CreateJobChunkRegistrator();
        JobProcessor.Interfaces.IJobSupplier CreateJobSupplier();
        JobProcessor.Interfaces.IJobChunkResultWatcher CreateChunkResultWatcher();
        JobProcessor.Interfaces.IMapResultsCollector CreateMapResultsCollector(JobInfo jobInfo);
        JobProcessor.Interfaces.IReduceResultsCollector CreateReduceResultsCollector(JobInfo jobInfo);
        JobProcessor.Interfaces.IJobHistoryUpdater CreateJobHistoryUpdater();
    }
}

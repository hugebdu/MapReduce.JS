using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Interfaces;
using JobProcessor.Model;

namespace JobProcessor.Implementation
{
    internal class DefaultFactory : IFactory
    {
        #region Factory methods
        public IJobChunkDispatcher CreateChunkDispatcher(IJobChunkRegistrator jobChunkRegistrator)
        {
            return new QueueJobChunkDispatcher(jobChunkRegistrator);
        }

        public IJobChunksProvider CreateChunksProvider()
        {
            return new DefaultJobChunksProvider();
        }

        public IJobSupplier CreateJobSupplier()
        {
            return new DefaultJobSupplier();
        }

        public IJobChunkRegistrator CreateJobChunkRegistrator()
        {
            return new CacheJobChunkRegistrator();
        }

        public IJobChunkResultWatcher CreateChunkResultWatcher()
        {
            return new SBJobChunkResultWatcher();
        }

        public IMapResultsCollector CreateMapResultsCollector(JobInfo jobInfo)
        {
            return new MapResultsCollector(jobInfo);
        }
        #endregion Factory methods
    }
}

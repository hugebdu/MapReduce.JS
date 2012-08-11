using System;
using System.Collections.Generic;
using JobProcessor.Model;

namespace JobProcessor.Interfaces
{
    public interface IMapResultsCollector : IResultsCollector
    {
        IEnumerable<JobChunk> SplittedMappedData();
        void CleanUp();
    }
}

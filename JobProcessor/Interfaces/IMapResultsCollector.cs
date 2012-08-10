using System;
namespace JobProcessor.Interfaces
{
    public interface IMapResultsCollector : IResultsCollector
    {
        System.Collections.Generic.IEnumerable<JobProcessor.Model.JobChunk> SplittedMappedData();
    }
}

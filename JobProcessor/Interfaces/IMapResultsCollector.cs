using System;
namespace JobProcessor.Interfaces
{
    public interface IMapResultsCollector
    {
        string JobId { get; }
        void AddResult(JobProcessor.Model.MapResultMessage mapResultMessage);
        System.Collections.Generic.IEnumerable<JobProcessor.Model.JobChunk> SplittedMappedData();
    }
}

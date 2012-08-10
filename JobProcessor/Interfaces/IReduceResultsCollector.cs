using System;
namespace JobProcessor.Interfaces
{
    public interface IReduceResultsCollector : IResultsCollector
    {
        void SubmitResult();
    }
}

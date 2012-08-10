using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Interfaces
{
    public interface IResultsCollector
    {
        string JobId { get; }
        void AddResult(JobProcessor.Model.ChunkResultMessage mapResultMessage);
    }
}

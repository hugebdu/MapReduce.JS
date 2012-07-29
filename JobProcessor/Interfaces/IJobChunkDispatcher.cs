using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;

namespace JobProcessor.Interfaces
{
    public interface IJobChunkDispatcher
    {
        void Dispatch(JobChunk chunk);
    }
}

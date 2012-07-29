using System;

namespace JobProcessor.Interfaces
{
    public interface IJobChunkResultWatcher
    {
        event Action<object, JobProcessor.Model.MapResultMessage> ChunkMapResultArrive;
        void StartWatch();
        void StopWatch();
    }
}

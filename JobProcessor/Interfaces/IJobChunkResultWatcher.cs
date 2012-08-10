using System;

namespace JobProcessor.Interfaces
{
    public interface IJobChunkResultWatcher
    {
        event Action<object, JobProcessor.Model.ChunkResultMessage> ChunkResultArrive;
        void StartWatch();
        void StopWatch();
    }
}

namespace JobProcessor.Model
{
    public enum ProcessingMode
    {
        Map,
        Reduce
    }

    public enum ChunkStatus
    {
        NewMap,
        MapSent,
        MapCompleted,
        NewReduce,
        ReduceSent,
        ReduceCompleted
    }

    public enum JobProcessStatus
    {
        New,
        MapRunning,
        MapComplete,
        ReduceRunning,
        ReduceComplete,
        Completed,
        Failed
    }
}
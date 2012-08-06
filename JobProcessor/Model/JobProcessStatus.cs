namespace JobProcessor.Model
{
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
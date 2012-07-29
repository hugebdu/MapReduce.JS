using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Model
{
    public enum ChunkStatus 
    {
        NewMap,
        MapSent,
        MapCompleted,
        NewReduce,
        ReduceSent,
        ReduceCompleted
    }

    public class JobChunkStatus
    {
        public JobChunkUid ChunkUid { get; private set; }
        public ChunkStatus Status { get; set; }
        public DateTime LastUpdate { get; set; }

        public JobChunkStatus()
        {
            ChunkUid = new JobChunkUid();
        }
    }
}

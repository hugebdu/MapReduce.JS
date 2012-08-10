using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Model
{
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

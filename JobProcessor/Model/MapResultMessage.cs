using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Model
{
    public class MapResultMessage
    {
        public JobChunkUid ChunkUid { get; set; }
        public string Data { get; set; }
        public string ProcessorNodeId { get; set; }
    }
}

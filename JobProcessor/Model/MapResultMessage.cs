using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Model
{
    public class MapResultMessage
    {
        public JobChunkUid ChunkUid { get; set; }
        public KeyValue[] Data { get; set; }
        public string ProcessorNodeId { get; set; }
    }

    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

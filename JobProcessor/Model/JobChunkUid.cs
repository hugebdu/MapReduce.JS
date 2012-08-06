using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JobProcessor.Model
{
    public class JobChunkUid
    {
        public string JobId { get; set; }
        public string ChunkId { get; set; }
    }
}

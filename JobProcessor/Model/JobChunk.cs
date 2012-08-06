using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JobProcessor.Model
{
    public enum ProcessingMode 
    {
        Map,
        Reduce
    }

    public class JobChunk
    {
        public JobChunkUid ChunkUid { get; private set; }
        public Uri Data { get; set; }
        public Uri Handler { get; set; }
        public ProcessingMode Mode { get; set; }
        public string ResponseQueueName { get; set; }

        public JobChunk()
        {
            ChunkUid = new JobChunkUid();
        }

        public virtual string ToJson()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            var jsonSerializer = JsonSerializer.Create(settings);
            var stringWriter = new StringWriter();
            jsonSerializer.Serialize(stringWriter, this);
            return stringWriter.ToString();
        }
    }
}

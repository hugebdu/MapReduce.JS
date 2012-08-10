using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JobProcessor.Model
{
    public class JobChunk
    {
        public JobChunkUid ChunkUid { get; private set; }
        public string Data { get; set; }
        public bool IsBlob { get; set; }
        public string BlobContainer { get; set; }
        public string BlobName { get; set; }

        public string Handler { get; set; }
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

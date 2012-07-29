using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;

namespace JobProcessor.Exceptions
{
    public class UnknownChunkException : JobProcessorException
    {
        public JobChunkUid ChunkUid { get; set; }

        public UnknownChunkException()
        { }

        public UnknownChunkException(string message)
            : base(message)
        { }

        public UnknownChunkException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}

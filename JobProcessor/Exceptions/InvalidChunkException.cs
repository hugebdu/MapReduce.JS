using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;

namespace JobProcessor.Exceptions
{
    public class InvalidChunkException : JobProcessorException
    {
        public JobChunkUid ChunkUid { get; set; }
        public string  CorrectJobId { get; set; }

        public InvalidChunkException()
        { }

        public InvalidChunkException(string message)
            : base(message)
        { }

        public InvalidChunkException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}

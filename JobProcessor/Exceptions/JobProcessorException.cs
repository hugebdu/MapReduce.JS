using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Exceptions
{
    public class JobProcessorException : Exception
    {
        public JobProcessorException()
        { }

        public JobProcessorException(string message)
            : base(message)
        { }

        public JobProcessorException(string message, Exception innerException)
            : base(message, innerException) 
        { }
    }
}

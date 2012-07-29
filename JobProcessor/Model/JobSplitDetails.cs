using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Model
{
    public class JobSplitDetails
    {
        public string JobId { get; set; }
        public List<string> JobChunkIds { get; private set; }

        public JobSplitDetails()
        {
            this.JobChunkIds = new List<string>();
        }
    }
}

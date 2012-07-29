using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Model
{
    public class JobInfo
    {
        public Uri DataSource { get; set; }
        public Uri Mapper { get; set; }
        public Uri Reducer { get; set; }
        public string JobId { get; set; }
        public string JobMessageId { get; set; }
        public string PopReceipt { get; set; }
    }
}

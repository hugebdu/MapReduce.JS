using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Model
{
    public class JobInfo
    {
        public string DataSource { get; set; }
        public string Mapper { get; set; }
        public string Reducer { get; set; }
        public string JobName { get; set; }
        public string JobId { get; set; }
        public string JobMessageId { get; set; }
        public string PopReceipt { get; set; }
    }
}

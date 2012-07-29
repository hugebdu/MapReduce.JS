using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;

namespace JobProcessor.Interfaces
{
    public interface IJobSupplier
    {
        JobInfo GetNextJob();

        void RemoveJob(JobInfo jobInfo);
        void ReturnJob(JobInfo jobInfo);
    }
}

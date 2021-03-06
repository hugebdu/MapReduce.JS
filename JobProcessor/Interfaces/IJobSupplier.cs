﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;

namespace JobProcessor.Interfaces
{
    public interface IJobSupplier
    {
        JobInfo GetNextJob();

        bool RemoveJob(JobInfo jobInfo);
        bool ReturnJob(JobInfo jobInfo);
    }
}

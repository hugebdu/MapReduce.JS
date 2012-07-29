using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Interfaces;
using JobProcessor.Model;

namespace JobProcessor.Manage
{
    class JobsMonitor
    {
        private IJobSupplier _jobSupplier;
        private JobProcessManager _jobManager;

        public JobsMonitor(IFactory factory)
        {
            Logger.Log.Instance.Info("JobMonitor. Constructor");
            _jobSupplier = factory.CreateJobSupplier();
            _jobManager = new JobProcessManager(factory);
        }

        public void CheckForNewJobs()
        {
            Logger.Log.Instance.Info("JobMonitor. Start checking for new jobs");
            while (true)
            {
                var job = _jobSupplier.GetNextJob();
                if (job == null)
                {
                    Logger.Log.Instance.Info("JobMonitor. No more jobs. Return.");
                    break;
                }

                Logger.Log.Instance.Info(string.Format("JobMonitor. Got a job. MessageId: {0}, PopReceipt: {1}, DataSource: {2}. Process.",
                    job.JobMessageId,
                    job.PopReceipt,
                    job.DataSource));

                // TODO: Define callback function
                _jobManager.ProcessJob(job, OnResumeJobProcessing);
            }
        }

        
        private void OnResumeJobProcessing(JobInfo jobInfo, JobProcessStatus status)
        {
            Logger.Log.Instance.Info(string.Format("JobMonitor. OnResumeJobProcessing. MessageId: {0},  PopReceipt: {1}. Status: {2}",
                    jobInfo.JobMessageId,
                    jobInfo.PopReceipt,
                    status));

            switch (status)
            {
                case JobProcessStatus.Completed:
                    _jobSupplier.RemoveJob(jobInfo);
                    break;

                case JobProcessStatus.Failed:
                    _jobSupplier.ReturnJob(jobInfo);
                    break;

                default:
                    Logger.Log.Instance.Warning("JobMonitor. OnResumeJobProcessing - unsupported status");
                    break;
            }
        }
    }
}

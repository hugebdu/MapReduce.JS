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
        #region Data Members
        private IJobSupplier _jobSupplier;
        private JobProcessManager _jobManager;
        #endregion Data Members

        #region Ctor
        public JobsMonitor(IFactory factory)
        {
            Logger.Log.Instance.Info("JobMonitor. Constructor");
            _jobSupplier = factory.CreateJobSupplier();
            _jobManager = new JobProcessManager(factory);
        }
        #endregion Ctor

        #region Public Methods
        public void CheckForNewJobs()
        {
            Logger.Log.Instance.Info("JobMonitor. Start checking for new jobs");
            while (true)
            {
                var jobInfo = _jobSupplier.GetNextJob();
                if (jobInfo == null)
                {
                    Logger.Log.Instance.Info("JobMonitor. No more jobs. Return.");
                    break;
                }

                Logger.Log.Instance.Info(string.Format("JobMonitor. Got a job. MessageId: {0}, PopReceipt: {1}, DataSource: {2}. Process.",
                    jobInfo.JobMessageId,
                    jobInfo.PopReceipt,
                    jobInfo.DataSource));

                if (_jobManager.ProcessJob(jobInfo, OnResumeJobProcessing))
                {
                    Logger.Log.Instance.Info(string.Format("JobMonitor. Job started. MessageId: {0}, PopReceipt: {1}, DataSource: {2}. Process.",
                        jobInfo.JobMessageId,
                        jobInfo.PopReceipt,
                        jobInfo.DataSource));

                    _jobSupplier.RemoveJob(jobInfo);
                }
                else
                {
                    Logger.Log.Instance.Info(string.Format("JobMonitor. Job failed to start. Return it to the queue MessageId: {0}, PopReceipt: {1}, DataSource: {2}. Process.",
                        jobInfo.JobMessageId,
                        jobInfo.PopReceipt,
                        jobInfo.DataSource));

                    _jobSupplier.ReturnJob(jobInfo);
                }
            }
        }
        #endregion Public Method

        #region Private methods
        private void OnResumeJobProcessing(JobInfo jobInfo, JobProcessStatus status)
        {
            Logger.Log.Instance.Info(string.Format("JobMonitor. OnResumeJobProcessing. MessageId: {0},  PopReceipt: {1}. Status: {2}",
                    jobInfo.JobMessageId,
                    jobInfo.PopReceipt,
                    status));
            try
            {
                switch (status)
                {
                    case JobProcessStatus.Completed:
                        //_jobSupplier.RemoveJob(jobInfo);
                        break;

                    case JobProcessStatus.Failed:
                        _jobSupplier.ReturnJob(jobInfo);
                        break;

                    default:
                        Logger.Log.Instance.Warning("JobMonitor. OnResumeJobProcessing - unsupported status");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, string.Format("JobMonitor. OnResumeJobProcessing error. MessageId: {0},  Status: {1}",
                    jobInfo.JobMessageId,
                    status));
            }
        }
        #endregion Private methods
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using JobProcessor;
using JobProcessor.Model;
using JobProcessor.Interfaces;

namespace JobProcessor.Implementation
{
    class DefaultJobSupplier : IJobSupplier
    {
        #region Data Members
        private CloudQueue _queue;
        #endregion Data Members

        #region Ctor
        public DefaultJobSupplier()
        {
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Constructor. Create queue '{0}' client", RoleSettings.JobRequestsQueueName));
            _queue = AzureClient.Instance.QueueClient.GetQueueReference(RoleSettings.JobRequestsQueueName);
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Queue client created: {0}", 
                _queue == null ? "failed" : "successfully"));
            _queue.CreateIfNotExist();
        }
        #endregion Ctor

        #region Public methods
        public JobInfo GetNextJob()
        {
            try
            {
                Logger.Log.Instance.Info("DefaultJobSupplier. GetNextJob called");
                var message = _queue.GetMessage();
                if (message == null)
                {
                    Logger.Log.Instance.Info("DefaultJobSupplier. No messages in the queue.");
                    return null;
                }

                if (message.DequeueCount > RoleSettings.MaxDequeueCount)
                {
                    Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Message's dequeue count ({0}) exeeds the max allowed count ({1}). Ignore message. ", message.DequeueCount, RoleSettings.MaxDequeueCount));
                    return null;
                }

                _queue.UpdateMessage(message, new TimeSpan(0, 5, 0), MessageUpdateFields.Visibility);

                Logger.Log.Instance.Info("DefaultJobSupplier. Got message from the queue. Create JobInfo");
                return parseQueueMessage(message);
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, string.Format("DefaultJobSupplier. Failed to get next job from the queue {0}.", RoleSettings.JobRequestsQueueName));
                return null;
            }
        }

        public bool RemoveJob(JobInfo jobInfo)
        {
            try
            {
                if (jobInfo == null)
                    return false;
                Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Remove message. Id: {0}, PopReceipt: {1}",
                    jobInfo.JobMessageId,
                    jobInfo.PopReceipt));
                _queue.DeleteMessage(jobInfo.JobMessageId, jobInfo.PopReceipt);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, string.Format("DefaultJobSupplier. Failed to remove the job (JobId: {1}, MessageId: {2}, popReceipt: {3}) from the queue {0}.", RoleSettings.JobRequestsQueueName, jobInfo.JobId, jobInfo.JobMessageId, jobInfo.PopReceipt));
                return false;
            }
        }

        public bool ReturnJob(JobInfo jobInfo)
        {
            try
            {
                if (jobInfo == null)
                    return false;

                Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Return message to the queue. Id: {0}, PopReceipt: {1}",
                    jobInfo.JobMessageId,
                    jobInfo.PopReceipt));                
                
                // Implement retrun Job
                //var message = new CloudQueueMessage(string.Empty)
                //{
                //    //Id = jobInfo.JobMessageId,
                //    //PopReceipt = jobInfo.PopReceipt
                //};

                //_queue.UpdateMessage(message, new TimeSpan(0), MessageUpdateFields.Visibility);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, string.Format("DefaultJobSupplier. Failed to return the job (JobId: {1}, MessageId: {2}, popReceipt: {3}) from the queue {0}.", RoleSettings.JobRequestsQueueName, jobInfo.JobId, jobInfo.JobMessageId, jobInfo.PopReceipt));
                return false;
            }
        }
        #endregion Public methods

        #region Private methods
        private JobInfo parseQueueMessage(CloudQueueMessage message)
        {
            if (message == null)
            {
                Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Cannot parse queue null message"));            
            }
            
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Parse queue message into JobInfo. Id: {0}, PopReceipt: {1}",
                message.Id,
                message.PopReceipt));

            var jsonMessage = Newtonsoft.Json.Linq.JObject.Parse(message.AsString);
            var jobInfo = new JobInfo();
            jobInfo.JobId = jsonMessage.Property("JobId").Value.ToString() + "_" + Guid.NewGuid().ToString("N");
            jobInfo.JobName = jsonMessage.Property("Name").Value.ToString();
            jobInfo.DataSource = jsonMessage.Property("DataSource").Value.ToString();
            jobInfo.Handler = jsonMessage.Property("Handler").Value.ToString();
            jobInfo.JobMessageId = message.Id;
            jobInfo.PopReceipt = message.PopReceipt;

            return jobInfo;
        }
        #endregion Private methods
    }
}
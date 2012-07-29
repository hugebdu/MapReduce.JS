using System;
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
        private static string QueueName = "JobRequestsQueue";

        private CloudQueue queue;

        public DefaultJobSupplier()
        {
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Constructor. Create queue '{0}' client", QueueName));
            queue = AzureClient.Instance.QueueClient.GetQueueReference(QueueName);
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Queue client created: {0}", 
                queue == null ? "failed" : "successfully"));
            queue.CreateIfNotExist();
        }

        public JobInfo GetNextJob()
        {
            // TODO: Consider setting visibility timeout here - per message
            Logger.Log.Instance.Info("DefaultJobSupplier. GetNextJob called");
            var message = queue.GetMessage();
            if (message == null)
            {
                Logger.Log.Instance.Info("DefaultJobSupplier. No messages in the queue.");
                return null;
            }

            Logger.Log.Instance.Info("DefaultJobSupplier. Got message from the queue. Create JobInfo");
            return parseQueueMessage(message);
        }

        public void RemoveJob(JobInfo jobInfo)
        {
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Remove message. Id: {0}, PopReceipt: {1}",
                jobInfo.JobMessageId,
                jobInfo.PopReceipt));
            queue.DeleteMessage(jobInfo.JobMessageId, jobInfo.PopReceipt);
        }

        public void ReturnJob(JobInfo jobInfo)
        {
            //TODO: Implement retrun Job
            throw new NotImplementedException();
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Return message to the queue. Id: {0}, PopReceipt: {1}",
                jobInfo.JobMessageId,
                jobInfo.PopReceipt));

            var message = new CloudQueueMessage(string.Empty)
            {
                 //Id = jobInfo.JobMessageId,
                 //PopReceipt = jobInfo.PopReceipt
            };

            queue.UpdateMessage(message, new TimeSpan(0), MessageUpdateFields.Visibility);
        }

        private JobInfo parseQueueMessage(CloudQueueMessage message)
        {
            var jobInfo = new JobInfo();
            Logger.Log.Instance.Info(string.Format("DefaultJobSupplier. Parse queue message into JobInfo. Id: {0}, PopReceipt: {1}",
                message.Id,
                message.PopReceipt));

            // TODO: Parse JSON and handle errors
            var parts = message.AsString.Split(',');
            jobInfo.DataSource = new Uri(parts[0]);
            jobInfo.Mapper = new Uri(parts[1]);
            jobInfo.Reducer = new Uri(parts[2]);
            jobInfo.JobMessageId = message.Id;
            jobInfo.PopReceipt = message.PopReceipt;

            return jobInfo;
        }
    }
}
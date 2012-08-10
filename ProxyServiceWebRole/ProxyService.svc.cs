using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace ProxyServiceWebRole
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class ProxyService : IProxyService
    {
        #region Data Members
        private CloudQueue _queue;
        private CloudQueueClient _queueClient;
        #endregion Data Members

        public ProxyService()
        {
            Logger.Log.Instance.Active = false;
            CloudStorageAccount storageAccount = null;

            if (RoleSettings.UseDevelopAccount)
                storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            else
                storageAccount = new CloudStorageAccount(new StorageCredentialsAccountAndKey(RoleSettings.AccountName, RoleSettings.AccountKey), true);

            this._queueClient = storageAccount.CreateCloudQueueClient();

            Logger.Log.Instance.Info(string.Format("ProxyService. Constructor. Create queue '{0}' client", RoleSettings.JobRequestsQueueName));
            this._queue = _queueClient.GetQueueReference(RoleSettings.JobRequestsQueueName);
            Logger.Log.Instance.Info(string.Format("ProxyService. Queue client created: {0}",
                this._queue == null ? "failed" : "successfully"));
            this._queue.CreateIfNotExist();
        }


        public bool AddJobRequest(Stream data)
        {
            try
            {
                string request = null;
                using (StreamReader reader = new StreamReader(data))
                {
                    request = reader.ReadToEnd();
                }

                Logger.Log.Instance.Info(string.Format("ProxyService. Send message to queue '{0}': {1}", RoleSettings.JobRequestsQueueName, request));
                this._queue.AddMessage(new CloudQueueMessage(request));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, string.Format("ProxyService. Failed to ssend message to queue '{0}", RoleSettings.JobRequestsQueueName));
                return false;
            }
        }

    }
}

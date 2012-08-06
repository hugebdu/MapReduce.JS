using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.ServiceBus;
using JobProcessor.Interfaces;

namespace JobProcessor
{
    internal class AzureClient
    {
        public static AzureClient Instance {get; private set;}
        static AzureClient()
        {
            Instance = new AzureClient();
        }

        public CloudQueueClient QueueClient { get; private set; }
        public CloudBlobClient BlobClient { get; private set; }
        public ICache CacheClient { get; private set; }
        
        private AzureClient()
        {
            //CloudStorageAccount.SetConfigurationSettingPublisher(ConfigurationSettingPublisher);
            //var client = CloudStorageAccount.FromConfigurationSetting("idc");
            var storageAccount = new CloudStorageAccount(new StorageCredentialsAccountAndKey(RoleSettings.AccountName, RoleSettings.AccountKey), true);
            storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            this.QueueClient = storageAccount.CreateCloudQueueClient();
            this.BlobClient = storageAccount.CreateCloudBlobClient();
            this.CacheClient = new Implementation.DefaultCache();
        }

        private static void ConfigurationSettingPublisher(string s, Func<string, bool> func)
        {
            func(RoleEnvironment.GetConfigurationSettingValue(s));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.ApplicationServer.Caching;
using Microsoft.ServiceBus;

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
        public DataCache CacheClient { get; private set; }
        
        private AzureClient()
        {
            CloudStorageAccount.SetConfigurationSettingPublisher(ConfigurationSettingPublisher);
            var client = CloudStorageAccount.FromConfigurationSetting("idc");
            this.QueueClient = client.CreateCloudQueueClient();
            this.BlobClient = client.CreateCloudBlobClient();
            
            var factory = new DataCacheFactory();
            this.CacheClient = factory.GetDefaultCache();
        }

        private static void ConfigurationSettingPublisher(string s, Func<string, bool> func)
        {
            func(RoleEnvironment.GetConfigurationSettingValue(s));
        }
    }
}

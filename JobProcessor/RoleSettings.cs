using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor
{
    internal class RoleSettings
    {
        public const string JobsChunksQueueName = "jobchunks";
        public const string JobRequestsQueueName = "jobrequests";

        public const string AccountName = "vainshteinalexander";
        public const string AccountKey = "NL3ag1ZkU7794cAspW+HBHlKXcmg+j0XpyY6TOK5X89KIqB/Rog+Yn4NdpHQ20YEpJ/p/lewHXLWoMpESFRLnw==";

        public const string ServiceBusAddress = "sb://MapReduceJS.servicebus.windows.net";
        public const string ServiceBusIssuerName = "owner";
        public const string ServiceBusIssuerSecret = "kpt3ZfiSyEiC1GKZ9ZkjwuFG2kNMEv9Bl+aHt4Z7tNw=";

        public const string DbConnectionString = "Server=tcp:yloh7tw5n9.database.windows.net,1433;Database=mrjs;User ID=cloud@yloh7tw5n9;Password=MapReduce1;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

        public static string ChunkResponseQueue { get; set; }
        
        public static string RoleId { get; set; }

        public const int MaxDequeueCount = 10;

        static RoleSettings()
        {
            ChunkResponseQueue = "mapresults";
            RoleId = "Role1";
        }

    }
}

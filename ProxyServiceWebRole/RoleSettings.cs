using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyServiceWebRole
{
    internal class RoleSettings
    {
        public const string JobRequestsQueueName = "jobrequests";

        public const string AccountName = "vainshteinalexander";
        public const string AccountKey = "NL3ag1ZkU7794cAspW+HBHlKXcmg+j0XpyY6TOK5X89KIqB/Rog+Yn4NdpHQ20YEpJ/p/lewHXLWoMpESFRLnw==";

        public static string RoleId { get; set; }
        
        public static bool UseDevelopAccount;

        static RoleSettings()
        {
            UseDevelopAccount = Properties.Settings.Default.UserDevelopmentAccount;
            RoleId = "ProxyRole1";
        }
    }
}

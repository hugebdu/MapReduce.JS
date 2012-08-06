using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor;
using JobProcessor.Interfaces;
using Microsoft.ApplicationServer.Caching;

namespace JobProcessor.Implementation
{
    class AzureCache : ICache
    {
        private const string AuthenticationToken = "YWNzOmh0dHBzOi8vbWFwcmVkdWNlanNjYWNoZS1jYWNoZS5hY2Nlc3Njb250cm9sLndpbmRvd3MubmV0L1dSQVB2MC45LyZvd25lciZ5amVzTnM5YWVobFRXeVNsQTNhcThKR0JOL1B2aTUrQU5FMUZYNkl0YXU0PSZodHRwOi8vTWFwUmVkdWNlSlNDYWNoZS5jYWNoZS53aW5kb3dzLm5ldA==";
        private const string CacheHost = "MapReduceJSCache.cache.windows.net";
        private const int  CachePort = 22233;
        private DataCache _cacheClient;

        public AzureCache()
        {
            var token = new System.Security.SecureString();
            foreach (var c in AuthenticationToken.ToCharArray())
            {
                token.AppendChar(c);
            }

            var securityProperties = new DataCacheSecurity(token);

            var factory = new DataCacheFactory(new DataCacheFactoryConfiguration()
            {
                SecurityProperties = securityProperties,
                Servers = new List<DataCacheServerEndpoint> { new DataCacheServerEndpoint(CacheHost, CachePort) }
            });
            
            this._cacheClient = factory.GetDefaultCache();
        }



        public object this[string key]
        {
            get
            {
                return _cacheClient[key];
            }
            set
            {
                _cacheClient[key] = value;
            }
        }

        public object Get(string key)
        {
            return _cacheClient.Get(key);
        }

        public object Add(string key, object value)
        {
            return _cacheClient.Add(key, value);
        }
    }
}

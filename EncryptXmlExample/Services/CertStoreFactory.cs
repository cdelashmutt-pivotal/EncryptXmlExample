using NLog;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EncryptXmlExample.Services
{
    public class CertStoreFactory
    {
        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();
        private static ICertStore _instance;

        public static ICertStore Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = GetCertStore();
                }
                return _instance;
            }
        }

        private static ICertStore GetCertStore()
        {
            string redisConnectionString = GetRedisConnectionString();
            if (redisConnectionString != null)
            {
                return new RedisCertStore(redisConnectionString);
            }
            else
            {
                return new InMemoryCertStore();
            }
        }

        private static string GetRedisConnectionString()
        {
            CloudFoundryServicesOptions services = ServerConfig.CloudFoundryServices;
            foreach (Service s in services.Services)
            {
                Nlog.Trace("Found service {0}: {1} ", s.Name, s);
            }
            Nlog.Trace("Looking for certstore service");
            Service certstore = services.Services.Where(s => s.Name == "certstore").FirstOrDefault();
            if (certstore != null)
            {
                Nlog.Trace("Found certstore!  Creating connection string.");
                String connectionString = String.Format("{0}:{1}",
                    certstore.Credentials["host"].Value,
                    certstore.Credentials["port"].Value);
                String password = certstore.Credentials["password"].Value;
                if (password != null)
                {
                    connectionString = String.Format(connectionString + ",password={0}", password);
                }
                return connectionString;
            }

            return null;
        }
    }
}
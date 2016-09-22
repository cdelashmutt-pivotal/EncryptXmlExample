using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EncryptXmlExample.Services
{
    public class RedisCertStore : ICertStore
    {
        private StackExchangeRedisCacheClient _cache;

        public RedisCertStore(string ConnectionString)
        {
            var serializer = new NewtonsoftSerializer();
            _cache = new StackExchangeRedisCacheClient(serializer, ConnectionString);
        }

        public Tuple<string, byte[]> this[string index]
        {
            get
            {
                return _cache.Get<Tuple<string, byte[]>>(index);
            }

            set
            {
                _cache.Add<Tuple<string, byte[]>>(index,value);
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return _cache.SearchKeys("*");
            }
        }
    }
}
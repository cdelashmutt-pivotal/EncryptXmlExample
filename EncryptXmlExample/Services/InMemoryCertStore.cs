using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EncryptXmlExample.Services
{
    public class InMemoryCertStore : ICertStore
    {
        private Dictionary<String, Tuple<String,byte[]>> certStore = new Dictionary<string, Tuple<String,byte[]>>();

        public Tuple<String, byte[]> this[String index]
        {
            get
            {
                return certStore[index];
            }
            set
            {
                certStore[index] = value;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return certStore.Keys.AsEnumerable();
            }
        }
    }
}
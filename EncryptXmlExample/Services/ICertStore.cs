using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncryptXmlExample.Services
{
    interface ICertStore
    {
        Tuple<String, byte[]> this[String index] { get; set; }
        IEnumerable<string> Keys { get; }
    }
}

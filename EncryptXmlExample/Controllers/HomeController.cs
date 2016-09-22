using EncryptXmlExample.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace EncryptXmlExample.Controllers
{

    public class HomeController : Controller
    {
        private static ICertStore _certStore = GetCertStore();

        public ActionResult Index()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach(string key in _certStore.Keys)
            {
                items.Add( new SelectListItem { Text = key, Value = key } );
            }
            ViewBag.Certificates = items.AsEnumerable();
            return View();
        }

        [HttpPost]
        public ActionResult UploadCert(String CertificateName, String Password, HttpPostedFileBase CertificateFile)
        {
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(CertificateFile.InputStream))
            {
                fileData = binaryReader.ReadBytes(CertificateFile.ContentLength);
            }
            
            _certStore[CertificateName] = Tuple.Create(Password, fileData);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        // Never do this for real!  Just done for demo purposes
        [ValidateInput(false)]
        public ActionResult DoDecrypt(String EncryptedData)
        {
            //First lookup your cert/key.  Again, this is just an example.
            X509Certificate2 cert = new X509Certificate2(_certStore["TestEncrypt"].Item2, _certStore["TestEncrypt"].Item1);

            //Put the cert into the personal store of the user Cloud Foundry dynamically 
            //created to run this app.
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();

            //Now, decrypt as normal
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(EncryptedData);

            EncryptedXml eXml = new EncryptedXml(xmlDoc);

            eXml.DecryptDocument();

            return View((object)xmlDoc.OuterXml);
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
                return new CertStore();
            }
        }

        private static string GetRedisConnectionString()
        {
            VcapServices vcap = VcapServices.Instance();

            if (vcap.IsCF)
            {
                JToken service = vcap.GetService("certstore");

                if (service != null)
                {
                    JToken creds = service["credentials"];
                    return String.Format("{0}:{1},password={2}"
                        , creds["host"]
                        , creds["port"]
                        , creds["password"]
                    );
                }
            }

            return null;
        }
    }
}
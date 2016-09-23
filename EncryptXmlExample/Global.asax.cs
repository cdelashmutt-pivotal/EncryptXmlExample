using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EncryptXmlExample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            Nlog.Trace("App starting up!");
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ServerConfig.RegisterConfig("development");
        }
    }
}

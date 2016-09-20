using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EncryptXmlExample.Startup))]
namespace EncryptXmlExample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

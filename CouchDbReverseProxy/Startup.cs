using Microsoft.Owin;
using Owin;
using System.Web.Http;

[assembly: OwinStartup(typeof(CouchDbReverseProxy.Startup))]

namespace CouchDbReverseProxy
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            UnityConfig.Register(config);
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}

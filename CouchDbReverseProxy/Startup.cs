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
            var config = WebApiConfig.Register(new HttpConfiguration());
            app.UseWebApi(config);
        }
    }
}

using System.Web.Http;

namespace CouchDbReverseProxy
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Web API routes
            config.MapHttpAttributeRoutes();

#if USE_CORS
            config.EnableCors(
                new EnableCorsAttribute("https://localhost:44300, http://localhost:21575, http://localhost:37045, http://localhost:37046, https://localhost:44301",
                    "accept, authorization", "GET", "WWW-Authenticate"));
#endif
        }
    }
}
using System.Web.Http;
using Microsoft.Owin;
using Owin;
using IdentityServer3.AccessTokenValidation;
using System.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(CouchDbReverseProxy.Startup))]

namespace CouchDbReverseProxy
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

            var options = new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44336/core",
                RequiredScopes = new[] { "write" },

                // client credentials for the introspection endpoint
                ClientId = "write",
                ClientSecret = "secret"
            };
            app.UseIdentityServerBearerTokenAuthentication(options);

            var config = new HttpConfiguration();
            UnityConfig.Register(config);
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}

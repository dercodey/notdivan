using System;
using System.Net.Http;

namespace CouchDbReverseProxy
{
    public class CouchDbOptions
    {
        public CouchDbOptions(string baseAddress)
        {
            BaseCouchDbApiAddress = baseAddress;
            Client = new HttpClient()
            {
                BaseAddress = new Uri(BaseCouchDbApiAddress)
            };
        }

        public string BaseCouchDbApiAddress { get; private set; }
        public HttpClient Client { get; private set; }

    }
}
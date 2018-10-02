using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDbReverseProxy
{
    /// <summary>
    /// 
    /// </summary>
    public class CouchDbService
    {
        public CouchDbService(Uri couchDbBaseAddress)
        {
            Client = new HttpClient()
            {
                BaseAddress = couchDbBaseAddress
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// returns the URL for a document, given its ID and optionally revision
        /// </summary>
        /// <param name="dbname">name of the db that contains the document</param>
        /// <param name="docid">ID of the document</param>
        /// <param name="rev">optional document revision</param>
        /// <returns></returns>
        public Uri GetDocumentRequestUri(string dbname, string docid, string rev = null)
        {
            var relativeUri = 
                new Uri(rev != null 
                    ? $"{dbname}/{docid}?rev={rev}" 
                    : $"{dbname}/{docid}", 
                    UriKind.Relative);
            return new Uri(Client.BaseAddress, relativeUri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> 
            GetAsync(Uri requestUri)
        {
            return await Client.GetAsync(requestUri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="contentString"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> 
            PutStringAsync(Uri requestUri, string contentString)
        {
            var content = new StringContent(contentString);
            return await Client.PutAsync(requestUri, content);
        }

    }
}
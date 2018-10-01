using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace CouchDbReverseProxy.Controllers
{
    /// <summary>
    /// WebAPI controller that facades a subset of the CouchDB API, and forwards accordingly
    /// </summary>
    public class CouchDbApiController : ApiController
    {
        // holds the options and default client for calls
        CouchDbOptions options;
        HttpClient client;

        /// <summary>
        /// construct a new controller with the given options
        /// </summary>
        /// <param name="initOptions">options to be used</param>
        public CouchDbApiController(CouchDbOptions initOptions)
        {
            options = initOptions;
            client = options.Client;
        }

        /// <summary>
        /// creates the named db
        /// </summary>
        /// <param name="dbname">name of the db to create</param>
        /// <returns>OK if success</returns>
        [Route("{dbname}")]
        [HttpPut]
        public async Task<IHttpActionResult>
            CreateDb(string dbname)
        {
            var emptyContent = new StringContent(string.Empty);
            var response = await client.PutAsync(dbname, emptyContent);
            return ResponseMessage(response);
        }


        /// <summary>
        /// get info about the given db
        /// </summary>
        /// <param name="dbname">name of db for which info is requested</param>
        /// <returns>json with various infos about the db</returns>
        [Route("{dbname}")]
        [HttpGet]
        public async Task<IHttpActionResult>
            GetDbInfo(string dbname)
        {
            var response = await client.GetAsync(dbname);
            return ResponseMessage(response);
        }

        /// <summary>
        /// deletes the given db
        /// </summary>
        /// <param name="dbname">name of the db to be deleted</param>
        /// <returns></returns>
        [Route("{dbname}")]
        [HttpDelete]
        public async Task<IHttpActionResult>
            DeleteDb(string dbname)
        {
            var response = await client.DeleteAsync(dbname);
            return ResponseMessage(response);
        }

        /// <summary>
        /// put to create or update an existing document       
        /// can provide a document rev, provided as a query paramter:
        ///     PUT http://localhost/db/1234-12324-123124-1233/attach?rev=1-1234-12345-1234-12334
        /// </summary>
        /// <param name="dbname">name of the db to put the doc in</param>
        /// <param name="docid">guid for the new or existing document</param>
        /// <param name="rev">revision of document, from query parameter (null if none)</param>
        /// <returns>id and rev of the new or updated document</returns>
        [Route("{dbname}/{docid}")]
        [HttpPut]
        public async Task<IHttpActionResult>
            CreateOrUpdateDocument(string dbname, string docid, string rev = null)
        {
            var requestContent = 
                new StringContent(
                    await Request.Content.ReadAsStringAsync());
            string requestUrl = 
                rev != null ? $"{dbname}/{docid}?rev={rev}" : $"{dbname}/{docid}";
            var response = 
                await client.PutAsync(requestUrl,
                    requestContent);
            return ResponseMessage(response);
        }

        /// <summary>
        /// retrieves a document
        /// can provide a document rev, provided as a query paramter:
        ///     GET http://localhost/db/1234-12324-123124-1233/attach?rev=1-1234-12345-1234-12334
        /// </summary>
        /// <param name="dbname">name of the db to hit</param>
        /// <param name="docid">the document to be retrieved</param>        
        /// <param name="rev">revision of document, from query parameter (null if none)</param>
        /// <returns>json of the document object</returns>
        [Route("{dbname}/{docid}")]
        [HttpGet]
        public async Task<IHttpActionResult> 
            GetDocument(string dbname, string docid, string rev = null)
        {
            var response = await client.GetAsync($"{dbname}/{docid}");
            return ResponseMessage(response);
        }

        /// <summary>
        /// adds an attachment to the doc
        /// </summary>
        /// <param name="dbname">name of database that will contain the attachment document</param>
        /// <param name="docid">attachment document ID</param>
        /// <param name="attname">attachment name</param>
        /// <returns>a reference to the created attachment document</returns>
        [Route("{dbname}/{docid}/{attname}")]
        [HttpPut]
        public async Task<IHttpActionResult> 
            AddAttachment(string dbname, string docid, string attname)
        {
            var requestContent =
                CreateStreamContentWithMimeType(
                    await Request.Content.ReadAsStreamAsync());
            var response =
                await client.PutAsync($"{dbname}/{docid}/{attname}",
                    requestContent);
            return ResponseMessage(response);
        }


        // helper to provide stream content with default binary mime type
        private static StreamContent
            CreateStreamContentWithMimeType(System.IO.Stream stream)
        {
            var newContent = new StreamContent(stream);
            newContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet");
            newContent.Headers.ContentLength = stream.Length;
            return newContent;
        }

        /// <summary>
        /// retrieves an attachment for the given db and document
        /// note that this isn't supporting the rev parameter, as we expect attachments to not be revved once created
        /// </summary>
        /// <param name="dbname">the db from which to fetch the attachement</param>
        /// <param name="docid">the document id to be retrieved</param>
        /// <param name="attname">the attachment name</param>
        /// <returns>content is the attachment</returns>
        [Route("{dbname}/{docid}/{attname}")]
        [HttpGet]
        public async Task<IHttpActionResult>
            GetAttachment(string dbname, string docid, string attname)
        {
            var response = await client.GetAsync($"{dbname}/{docid}/{attname}");
            return ResponseMessage(response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace CouchDbReverseProxy.Controllers
{
    public class CouchDbApiController : ApiController
    {
        private static string baseCouchDbApiAddress = "http://localhost:5984";
        private static HttpClient client = 
            new HttpClient()
            {
                BaseAddress = new Uri(baseCouchDbApiAddress)
            };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbname"></param>
        /// <returns></returns>
        [Route("{dbname}")]
        [HttpPut]
        public async Task<IHttpActionResult> 
            CreateDb(string dbname) => 
            
            ResponseMessage(
                await client.PutAsync(dbname, 
                    new StringContent(string.Empty)));

        /// <summary>
        /// get info about the given db
        /// </summary>
        /// <param name="dbname"></param>
        /// <returns></returns>
        [Route("{dbname}")]
        [HttpGet]
        public async Task<IHttpActionResult> 
            GetDbInfo(string dbname) =>

            ResponseMessage(
                await client.GetAsync(dbname));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbname"></param>
        /// <returns></returns>
        [Route("{dbname}")]
        [HttpDelete]
        public async Task<IHttpActionResult> 
            DeleteDb(string dbname) =>

            ResponseMessage(
                await client.DeleteAsync(dbname));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="docid"></param>
        /// <returns></returns>
        [Route("{dbname}/{docid}")]
        [HttpPut]
        public async Task<IHttpActionResult> 
            CreateOrUpdateDocument(string dbname, string docid) =>

            ResponseMessage(
                await client.PutAsync($"{dbname}/{docid}", 
                    new StringContent(
                        await Request.Content.ReadAsStringAsync())));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="docid"></param>
        /// <returns></returns>
        [Route("{dbname}/{docid}")]
        [HttpGet]
        public async Task<IHttpActionResult> 
            GetDocument(string dbname, string docid) =>

            ResponseMessage(
                await client.GetAsync($"{dbname}/{docid}"));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="docid"></param>
        /// <param name="attname"></param>
        /// <returns></returns>
        [Route("{dbname}/{docid}/{attname}")]
        [HttpPut]
        public async Task<IHttpActionResult> 
            AddAttachment(string dbname, string docid, string rev, string attname) =>

            ResponseMessage(
                await client.PutAsync($"{dbname}/{docid}/{attname}?rev={rev}",
                    CreateStreamContentWithMimeType(
                        await Request.Content.ReadAsStreamAsync())));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static StreamContent
            CreateStreamContentWithMimeType(System.IO.Stream stream)
        {
            var newContent = new StreamContent(stream);
            newContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet");
            newContent.Headers.ContentLength = stream.Length;
            return newContent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="docid"></param>
        /// <param name="attname"></param>
        /// <returns></returns>
        [Route("{dbname}/{docid}/{attname}")]
        [HttpGet]
        public async Task<IHttpActionResult> 
            GetAttachment(string dbname, string docid, string attname) =>

            ResponseMessage(
                await client.GetAsync($"{dbname}/{docid}/{attname}"));
    }
}
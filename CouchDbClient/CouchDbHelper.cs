using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CouchDbClient
{
    /// <summary>
    /// static helper class for access to CouchDb (via proxy)
    /// </summary>
    public static class CouchDbHelper
    {
        // parameters for proxy access
        private static readonly string baseCouchDbApiAddress;
        private static readonly string masterDbName;
        private static readonly string attachmentDbName;

        /// <summary>
        /// static constructor retrieves parameters from appSettings
        /// </summary>
        static CouchDbHelper()
        {
            baseCouchDbApiAddress = ConfigurationManager.AppSettings["CouchDbProxyBaseAddress"];
            masterDbName = ConfigurationManager.AppSettings["MasterDbName"];
            attachmentDbName = ConfigurationManager.AppSettings["AttachmentDbName"];
        }

        /// <summary>
        /// common client to be used for calls to proxy
        /// </summary>
        public static async Task<HttpClient> GetClient()
        {
            if (helperClient == null)
            {
                helperClient = new HttpClient()
                {
                    BaseAddress = new Uri(baseCouchDbApiAddress)
                };

                var response = await RequestTokenAsync();
                helperClient.SetBearerToken(response.AccessToken);
            }
            return helperClient;
        }
        private static HttpClient helperClient = null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static async Task<TokenResponse> RequestTokenAsync()
        {
            var authority = ConfigurationManager.AppSettings["TokenAuthority"];
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ClientSecret"];

            var tokenClient = new TokenClient($"{authority}/connect/token",
                clientId,
                clientSecret);

            var tokenScope = ConfigurationManager.AppSettings["TokenScope"];
            return await tokenClient.RequestClientCredentialsAsync(tokenScope);
        }

        /// <summary>
        /// checks for master db, and prints info
        /// </summary>
        /// <returns>true if master exists; false otherwise</returns>
        public static async Task<bool> GetDbInformation()
        {
            var client = await GetClient();
            var result = await client.GetAsync($"{masterDbName}");
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine(result.ReasonPhrase);
                return false;
            }

            Console.WriteLine(await result.Content.ReadAsStringAsync());
            return true;
        }

        /// <summary>
        /// creates the two dbs
        /// assumes they don't exist
        /// </summary>
        /// <returns>true if the create was successful</returns>
        public static  async Task<bool> CreateDbs()
        {
            // put with empty content
            var masterContent = new StringContent(string.Empty);
            var client = await GetClient();
            var masterDbCreateResult = await client.PutAsync($"{masterDbName}", masterContent);
            masterDbCreateResult.EnsureSuccessStatusCode();

            var attachmentContent = new StringContent(string.Empty);
            var attachmentDbCreateResult = await client.PutAsync($"{attachmentDbName}", attachmentContent);
            attachmentDbCreateResult.EnsureSuccessStatusCode();

            return true;
        }

        /// <summary>
        /// insert a new document with the given metadata
        /// </summary>
        /// <returns>document reference with revision</returns>
        public static async Task<DbReference> InsertNewDocument(string metadata)
        {
            var doc = new MasterDocument()
            {
                DatabaseName = masterDbName,
                Metadata = metadata,
                Status = "ACTIVE",
                StorageSystemType = "CouchDB",
            };

            return await PutJsonAsync<MasterDocument,DbReference>(doc);
        }

        /// <summary>
        /// retrieves a document by its ID and optional revision
        /// </summary>
        /// <param name="id">the document ID to be retrieved</param>
        /// <param name="rev">the revision to be retrieved</param>
        /// <returns>MasterDocument structure</returns>
        public static async Task<MasterDocument> FindDocumentById(string id, string rev = null)
        {
            var requestUrl = 
                rev == null 
                    ? $"{masterDbName}/{id}" 
                    : $"{masterDbName}/{id}?rev={rev}";
            var client = await GetClient();
            var result = await client.GetAsync(requestUrl);
            result.EnsureSuccessStatusCode();

            var stringContent = await result.Content.ReadAsStringAsync();
            var doc = JsonConvert.DeserializeObject<MasterDocument>(stringContent);
            return doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rev"></param>
        /// <param name="attname"></param>
        /// <param name="blob"></param>
        /// <returns></returns>
        public static async Task<DbReference> AddAttachment(string id, string rev, string attname, byte[] blob)
        {
            var attachStreamContent = new StreamContent(new System.IO.MemoryStream(blob));
            var client = await GetClient();
            var attachResult = 
                await client.PutAsync($"{attachmentDbName}/{id}/{attname}?rev={rev}", 
                    attachStreamContent);
            attachResult.EnsureSuccessStatusCode();

            var attachmentReference =
                JsonConvert.DeserializeObject<DbReference>(
                    attachResult.Content.ReadAsStringAsync().Result);

            // add the attachment reference to the master doc
            var masterDoc = await FindDocumentById(id);
            if (masterDoc.Attachments == null)
            {
                masterDoc.Attachments = new List<AttachmentReference>();
            }

            masterDoc.Attachments.Add(
                new AttachmentReference()
                {
                    AttachmentId = attachmentReference.Id,
                    Name = attname,
                    SequenceNumber = 1,
                    Size = blob.Length
                });

            var updatedMasterContent = new StringContent(JsonConvert.SerializeObject(masterDoc));
            var updatedMasterResult = 
                await client.PutAsync($"{masterDbName}/{id}?rev={rev}", updatedMasterContent);
            updatedMasterResult.EnsureSuccessStatusCode();

            return attachmentReference;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="attname"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetAttachmentForDoc(string id, string attname)
        {
            var masterDoc = await FindDocumentById(id);
            var foundAtt = 
                masterDoc.Attachments
                    .Where(att => att.Name.CompareTo(attname) == 0)
                    .First();
            var client = await GetClient();
            var getAttachmentResult = 
                await client.GetAsync($"{attachmentDbName}/{foundAtt.AttachmentId}/{attname}");
            getAttachmentResult.EnsureSuccessStatusCode();


            var stream = 
                await getAttachmentResult.Content.ReadAsStreamAsync();
            var length = getAttachmentResult.Content.Headers.ContentLength.Value;
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }

        /// <summary>
        /// helper that will do a put with a json-serialized object, and returns
        /// from a json
        /// </summary>
        /// <typeparam name="TRequest">input type</typeparam>
        /// <typeparam name="TResponse">reponse type</typeparam>
        /// <param name="doc">the input object</param>
        /// <returns>the response object</returns>
        private static async Task<TResponse> PutJsonAsync<TRequest, TResponse>(TRequest doc)
        {
            var jsonString = JsonConvert.SerializeObject(doc);
            var content = new StringContent(jsonString, Encoding.UTF8, @"application/json");
            var docid = Guid.NewGuid().ToString("D");

            var client = await GetClient();
            var result = await client.PutAsync($"{masterDbName}/{docid}", content);
            result.EnsureSuccessStatusCode();

            var jsonStringContent = await result.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<TResponse>(jsonStringContent);
            return responseObject;
        }

    }
}

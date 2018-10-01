using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NotDivan
{
    /// <summary>
    /// 
    /// </summary>
    public static class CouchDbHelper
    {
        private static string baseCouchDbApiAddress = "http://localhost:55464"; // "http://localhost:5984";
        private static HttpClient client = new HttpClient() { BaseAddress = new Uri(baseCouchDbApiAddress) };

        private static string masterdbname = "blobstoragemaster";
        private static string attachmentdbname = "blobstorageattachnmennts";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> GetDbInformation()
        {
            var result = await client.GetAsync($"{masterdbname}");
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine(result.ReasonPhrase);
                return false;
            }

            Console.WriteLine(await result.Content.ReadAsStringAsync());
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static  async Task<bool> CreateDbs()
        {
            var masterContent = new StringContent(string.Empty);
            var masterDbCreateResult = await client.PutAsync($"{masterdbname}", masterContent);
            if (!masterDbCreateResult.IsSuccessStatusCode)
            {
                Console.WriteLine(masterDbCreateResult.ReasonPhrase);
                return false;
            }

            var attachmentContent = new StringContent(string.Empty);
            var attachmentDbCreateResult = await client.PutAsync($"{attachmentdbname}", attachmentContent);
            if (!attachmentDbCreateResult.IsSuccessStatusCode)
            {
                Console.WriteLine(attachmentDbCreateResult.ReasonPhrase);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<DbReference> InsertNewDocument(string metadata)
        {
            var doc = new MasterDocument()
            {
                DatabaseName = masterdbname,
                Metadata = metadata,
                Status = "ACTIVE",
                StorageSystemType = "CouchDB",
            };

            var jsonString = JsonConvert.SerializeObject(doc);
            var content = new StringContent(jsonString, Encoding.UTF8, @"application/json");
            var docid = Guid.NewGuid().ToString("D");

            var result = await client.PutAsync($"{masterdbname}/{docid}", content);
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine(result.ReasonPhrase);
                return null;
            }

            var jsonStringContent = await result.Content.ReadAsStringAsync();
            var referenceToNew = JsonConvert.DeserializeObject<DbReference>(jsonStringContent);
            return referenceToNew;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<MasterDocument> FindDocumentById(string id)
        {
            var result = await client.GetAsync($"{masterdbname}/{id}");
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine(result.ReasonPhrase);
                return null;
            }

            var stringContent = await result.Content.ReadAsStringAsync();
            var doc = JsonConvert.DeserializeObject<MasterDocument>(stringContent);
            Console.WriteLine($"{doc.DocumentId} {doc.DatabaseName} {doc.Status}");
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
            var attachResult = 
                await client.PutAsync($"{attachmentdbname}/{id}/{attname}?rev={rev}", 
                    attachStreamContent);
            if (!attachResult.IsSuccessStatusCode)
            {
                Console.WriteLine(attachResult.ReasonPhrase);
                return null;
            }

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
                await client.PutAsync($"{masterdbname}/{id}?rev={rev}", updatedMasterContent);
            if (!updatedMasterResult.IsSuccessStatusCode)
            {
                Console.WriteLine(updatedMasterResult.ReasonPhrase);
                return null;
            }

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
            var foundAtt = masterDoc.Attachments.Where(att => att.Name.CompareTo(attname) == 0).FirstOrDefault();

            var getAttachmentResult = 
                await client.GetAsync($"{attachmentdbname}/{foundAtt.AttachmentId}/{attname}");
            if (!getAttachmentResult.IsSuccessStatusCode)
            {
                Console.WriteLine(getAttachmentResult.ReasonPhrase);
                return null;
            }

            var stream = 
                await getAttachmentResult.Content.ReadAsStreamAsync();
            var length = getAttachmentResult.Content.Headers.ContentLength.Value;
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }
    }
}

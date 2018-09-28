using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NotDivan
{
    class Program
    {
        private static string BaseCouchDbApiAddress = "http://localhost:55464"; // "http://localhost:5984";
        private static HttpClient Client = new HttpClient() { BaseAddress = new Uri(BaseCouchDbApiAddress) };

        private static string masterdbname = "blobstoragemaster";
        private static string attachmentdbname = "blobstorageattachnmennts";

        static void Main(string[] args)
        {
            if (!GetTrivialDbInformation())
            {
                CreateDb();
            }

            var reference = InsertNewDocument();
            FindDocumentById(reference.Id);

            AddAttachment(reference.Id, reference.RevisionId, "image", new byte[100 * 100]);
            var attachment = GetAttachmentForDoc(reference.Id, "image");

            Console.ReadLine();
        }

        private static bool GetTrivialDbInformation()
        {
            var result = Client.GetAsync($"{masterdbname}").Result;
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine(result.ReasonPhrase);
                return false;
            }

            Console.WriteLine(result.Content.ReadAsStringAsync().Result);
            return true;
        }

        private static bool CreateDb()
        {
            var content = new StringContent(string.Empty);
            var masterDbCreateResult = Client.PutAsync($"{masterdbname}", content).Result;
            if (!masterDbCreateResult.IsSuccessStatusCode)
            {
                Console.WriteLine(masterDbCreateResult.ReasonPhrase);
                return false;
            }

            var attachmentDbCreateResult = Client.PutAsync($"{attachmentdbname}", content).Result;
            if (!attachmentDbCreateResult.IsSuccessStatusCode)
            {
                Console.WriteLine(attachmentDbCreateResult.ReasonPhrase);
                return false;
            }

            return true;
        }

        private static DbReference InsertNewDocument()
        {
            var doc = new MasterDocumentDb()
            {
                DatabaseName = masterdbname,
                Metadata = @"<xml></xml>",
                Status = "ACTIVE",
                StorageSystemType = "CouchDB",
            };

            var jsonString = JsonConvert.SerializeObject(doc);
            var content = new StringContent(jsonString, Encoding.UTF8, @"application/json");
            var docid = Guid.NewGuid().ToString("D");

            var result = Client.PutAsync($"{masterdbname}/{docid}", content).Result;
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine(result.ReasonPhrase);
                return null;
            }

            var jsonStringContent = result.Content.ReadAsStringAsync().Result;
            var referenceToNew = JsonConvert.DeserializeObject<DbReference>(jsonStringContent);
            return referenceToNew;
        }

        private static MasterDocumentDb FindDocumentById(string id)
        {
            var result = Client.GetAsync($"{masterdbname}/{id}").Result;
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine(result.ReasonPhrase);
                return null;
            }

            var stringContent = result.Content.ReadAsStringAsync().Result;
            var doc = JsonConvert.DeserializeObject<MasterDocumentDb>(stringContent);
            Console.WriteLine($"{doc.DocumentId} {doc.DatabaseName} {doc.Status}");
            return doc;
        }

        private static DbReference AddAttachment(string id, string rev, string attname, byte[] blob)
        {
            var attachStreamContent = new StreamContent(new System.IO.MemoryStream(blob));
            var attachResult = 
                Client.PutAsync($"{attachmentdbname}/{id}/{attname}?rev={rev}", attachStreamContent).Result;
            if (!attachResult.IsSuccessStatusCode)
            {
                Console.WriteLine(attachResult.ReasonPhrase);
                return null;
            }

            var attachmentReference = 
                JsonConvert.DeserializeObject<DbReference>(
                    attachResult.Content.ReadAsStringAsync().Result);

            // add the attachment reference to the master doc
            var masterDoc = FindDocumentById(id);
            masterDoc.Attachments.Add(
                new AttachmentReferenceDb()
                {
                    AttachmentId = attachmentReference.Id,
                    Name = attname,
                    SequenceNumber = 1,
                    Size = blob.Length
                });

            var updatedMasterContent = new StringContent(JsonConvert.SerializeObject(masterDoc));
            var updatedMasterResult = Client.PutAsync($"{masterdbname}/{id}", updatedMasterContent).Result;
            if (!updatedMasterResult.IsSuccessStatusCode)
            {
                Console.WriteLine(updatedMasterResult.ReasonPhrase);
                return null;
            }

            return attachmentReference;
        }

        private static byte[] GetAttachmentForDoc(string id, string attname)
        {
            var masterDoc = FindDocumentById(id);
            var foundAtt = masterDoc.Attachments.Where(att => att.Name.CompareTo(attname) == 0).FirstOrDefault();

            var getAttachmentResult = Client.GetAsync($"{attachmentdbname}/{foundAtt.AttachmentId}/{attname}").Result;
            if (!getAttachmentResult.IsSuccessStatusCode)
            {
                Console.WriteLine(getAttachmentResult.ReasonPhrase);
                return null;
            }

            var stream = getAttachmentResult.Content.ReadAsStreamAsync().Result;
            var length = getAttachmentResult.Content.Headers.ContentLength.Value;
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }
    }

    public class DbReference
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "rev")]
        public string RevisionId { get; set; }
    }

    public class MasterDocumentDb
    {
        [JsonProperty(PropertyName = "_id", NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentId { get; set; }
        [JsonProperty(PropertyName = "_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string RevisionId { get; set; }

        public string DatabaseName { get; set; }
        public string Status { get; set; }
        public string StorageSystemType { get; set; }
        public string Metadata { get; set; }

        public List<AttachmentReferenceDb> Attachments { get; set; }
    }

    public class AttachmentReferenceDb
    {
        public string AttachmentId { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public int SequenceNumber { get; set; }
    }

    public class AttachmentDocumentDb
    {
        [JsonProperty(PropertyName = "_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AttachmentId { get; set; }
        [JsonProperty(PropertyName = "_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string RevisionId { get; set; }

        public string MasterDocumentId { get; set; }
    }
}

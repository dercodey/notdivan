using Newtonsoft.Json;
using System.Collections.Generic;

namespace NotDivan
{
    public class MasterDocument
    {
        [JsonProperty(PropertyName = "_id", NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentId { get; set; }
        [JsonProperty(PropertyName = "_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string RevisionId { get; set; }

        public string DatabaseName { get; set; }
        public string Status { get; set; }
        public string StorageSystemType { get; set; }
        public string Metadata { get; set; }

        public List<AttachmentReference> Attachments { get; set; }
    }
}

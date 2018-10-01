using Newtonsoft.Json;
using System.Collections.Generic;

namespace NotDivan
{
    /// <summary>
    /// represents a master document in CouchDB
    /// </summary>
    public class MasterDocument
    {
        /// <summary>
        /// CouchDB ID for the document -- may be null if creating
        /// </summary>
        [JsonProperty(PropertyName = "_id", NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentId { get; set; }

        /// <summary>
        /// CouchDB revision for the document
        /// </summary>
        [JsonProperty(PropertyName = "_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string RevisionId { get; set; }

        /// <summary>
        /// name of the database where the document attachments are stored
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// status = ACTIVE, ARCHIVED, DELETED
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// storage system type is used to retrieve the plugin for attachments
        /// </summary>
        public string StorageSystemType { get; set; }

        /// <summary>
        /// metadata encoded as a string (in known format i.e. XML or JSON)
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// collection of attachment references for the document
        /// </summary>
        public List<AttachmentReference> Attachments { get; set; }
    }
}

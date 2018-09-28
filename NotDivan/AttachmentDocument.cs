using Newtonsoft.Json;

namespace NotDivan
{
    /// <summary>
    /// a document representing an attachment to a master document
    /// </summary>
    public class AttachmentDocument
    {
        /// <summary>
        /// couchdb id of the attachment document
        /// </summary>
        [JsonProperty(PropertyName = "_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AttachmentId { get; set; }

        /// <summary>
        /// revision of the attachment document
        /// </summary>
        [JsonProperty(PropertyName = "_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string RevisionId { get; set; }

        /// <summary>
        /// reference to the master document that this attachment belongs to
        /// </summary>
        public string MasterDocumentId { get; set; }
    }
}

using Newtonsoft.Json;

namespace NotDivan
{
    /// <summary>
    /// raw CouchDB reference with revision
    /// used to return from create or query
    /// </summary>
    public class DbReference
    {
        /// <summary>
        /// document ID is the primary key for accessing the document
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// revision for the document reference
        /// </summary>
        [JsonProperty(PropertyName = "rev")]
        public string RevisionId { get; set; }
    }

}

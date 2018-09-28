using Newtonsoft.Json;

namespace NotDivan
{
    public class DbReference
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "rev")]
        public string RevisionId { get; set; }
    }

}

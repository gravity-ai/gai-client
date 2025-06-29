using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GravityAI.Client.Models
{
    internal class GaiEnterpriseMetaDataModel
    {   

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("customData")]
        public string CustomData { get; set; }

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }

        [JsonPropertyName("mapping")]
        public List<GaiPathMappingModel> Mapping { get; set; }

        [JsonPropertyName("outputMapping")]
        public List<GaiPathMappingModel> OutputMapping { get; set; }
    }


}

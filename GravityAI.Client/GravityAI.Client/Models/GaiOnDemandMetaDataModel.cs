using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GravityAI.Client.Models
{
    internal class GaiOnDemandMetaDataModel
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("jobName")]
        public string JobName { get; set; }

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }

        [JsonPropertyName("mapping")]
        public List<GaiPathMappingModel> Mapping { get; set; }

        [JsonPropertyName("outputMapping")]
        public List<GaiPathMappingModel> OutputMapping { get; set; }
    }


}

using System.Text.Json;

namespace GravityAI.Client
{
    public class GaiInferenceOptions
    {
        public string ApiKey { get; set; }
        public string SourceFileName { get; set; }
        public string Name { get; set; }
        public GaiDataMap InputMap { get; set; } = null;
        public GaiDataMap OutputMap { get; set; } = null;

        public JsonSerializerOptions JsonSerializerOptions { get; set; }
    }

    internal interface GaiEnterpriseKey
    {
        string AccountId { get; set; }
        string ApiKey { get; set; }
    }

    public class GaiEnterpriseInferenceOptions : GaiInferenceOptions, GaiEnterpriseKey
    {
        public string AccountId { get; set; }

        public string CustomData { get; set; }
    }

    public class GaiOnDemandInferenceOptions : GaiInferenceOptions
    {
        public string ModelVersion { get; set; }
    }


}

using System;

namespace GravityAI.Client
{
    public enum GxOnDemandJobStatus
    {
        Incomplete = 0,
        Success = 1,
        Error = 2,
        Fail = 3,
    }

    internal class GaiOnDemandJobModel
    {
        public string Id { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public DateTime LastUpdatedUtc { get; set; }

        public string Name { get; set; }
        public string InputFileName { get; set; }
        public string InputMime { get; set; }
        public int BilledUsage { get; set; }
        public int RecordCount { get; set; }
        public int RecordGroupCount { get; set; }
        public int ProcessingTimeMS { get; set; }
        public GxOnDemandJobStatus Status { get; set; }
        public string StatusMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string VersionNumber { get; set; }


    }


    public enum GxEnterpriseJobStatus
    {
        Created = 0,
        InProgress = 1,
        Success = 2,
        ProcessingError = 3,
        Fail = 4,
    }


    public class GaiEnterpriseJobModel 
    {
        public string Id { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime ModifiedOnUtc { get; set; }

        public string Name { get; set; }

        public GxEnterpriseJobStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        public string CustomData { get; set; }

    }

}

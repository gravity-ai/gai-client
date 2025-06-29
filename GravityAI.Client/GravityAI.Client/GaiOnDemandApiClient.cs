using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Text;
using GravityAI.Client.Models;

namespace GravityAI.Client
{
    internal interface IGaiOnDemandApiClient
    {
        Task<byte[]> GetJobResult(string jobId, string apiKey = null, CancellationToken cancel = default);
        Task<string> GetJobResultLink(string jobId, string apiKey = null, CancellationToken cancel = default);
        Task<GaiOnDemandJobModel> GetJobStatus(string jobId, string apiKey = null, CancellationToken cancel = default);
        Task<GaiOnDemandJobModel> PostJobToGravity(HttpContent data, string fileName, GaiOnDemandMetaDataModel metaData, JsonSerializerOptions options, string apiKey = null, CancellationToken cancel = default);
    }

    internal class GaiOnDemandApiClient : GaiApiClient<GaiOnDemandConfiguration>, IGaiOnDemandApiClient
    {

        private const string GravityJobsEndpoint = "api/v1/jobs";
        private const string GravityRetrieveJobResultEndpoint = "api/v1/jobs/result-link";
        private const string GravityLatestJobsEndpoint = "api/v1/jobs/latest";

        public GaiOnDemandApiClient(IOptions<GaiOnDemandConfiguration> options, HttpClient http) : base(options.Value, http)
        {

        }



        public Task<GaiOnDemandJobModel> PostJobToGravity(HttpContent data, string fileName, GaiOnDemandMetaDataModel metaData, JsonSerializerOptions options, string apiKey = null, CancellationToken cancel = default)
        {
            var content = new MultipartFormDataContent();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                content.Headers.Add(GaiHttpConstants.XApiKey, apiKey);
            }

            var json = new StringContent(JsonSerializer.Serialize(metaData), Encoding.UTF8, "application/json");

            content.Add(json, "data");
            content.Add(data, "file", fileName);

            return PostForm<GaiOnDemandJobModel>(GravityJobsEndpoint, content, options, cancel);

        }

        public Task<GaiOnDemandJobModel> GetJobStatus(string jobId, string apiKey = null, CancellationToken cancel = default)
        {
            Dictionary<string, string> headers = null;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                headers = new Dictionary<string, string>
                {
                    { GaiHttpConstants.XApiKey, apiKey }
                };
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return Get<GaiOnDemandJobModel>($"{GravityJobsEndpoint}/{jobId}", headers, options, cancel);
        }

        public Task<string> GetJobResultLink(string jobId, string apiKey = null, CancellationToken cancel = default)
        {
            Dictionary<string, string> headers = null;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                headers = new Dictionary<string, string>
                {
                    { GaiHttpConstants.XApiKey, apiKey }
                };
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return Get<string>($"{GravityRetrieveJobResultEndpoint}/{jobId}", headers, options, cancel);
        }

        public async Task<byte[]> GetJobResult(string jobId, string apiKey = null, CancellationToken cancel = default)
        {
            var link = await GetJobResultLink(jobId, apiKey, cancel);

            var request = new HttpRequestMessage(HttpMethod.Get, link);
            var response = await ApiClient.SendAsync(request, cancel);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}

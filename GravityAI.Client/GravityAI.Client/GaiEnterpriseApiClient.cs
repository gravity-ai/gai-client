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
    internal interface IGaiEnterpriseApiClient
    {
        Task<byte[]> GetJobResult(string jobId, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default);
        Task<string> GetJobResultLink(string jobId, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default);
        Task<GaiEnterpriseJobModel> GetJobStatus(string jobId, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default);
        Task<GaiEnterpriseJobModel> PostJobToGravity(HttpContent data, string fileName, GaiEnterpriseMetaDataModel metaData, JsonSerializerOptions options, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default);
    }

    internal class GaiEnterpriseApiClient : GaiApiClient<GaiEnterpriseConfiguration>, IGaiEnterpriseApiClient
    {

        private const string GravityJobsEndpoint = "api/v1/inference";
        private const string GravityRetrieveJobResultEndpoint = "api/v1/jobs/result";
        private const string GravityActiveJobsEndpoint = "api/v1/inference/in-progress";

        public GaiEnterpriseApiClient(IOptions<GaiEnterpriseConfiguration> options, HttpClient http) : base(options.Value, http)
        {
            if (!string.IsNullOrWhiteSpace(Config.AccountId))
            {
                ApiClient.DefaultRequestHeaders.Add(GaiHttpConstants.XAccountId, Config.AccountId);
            }
        }

        private static Dictionary<string,string> ToHeaders(GaiEnterpriseKey apiKey)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(apiKey?.ApiKey))
            {
                headers.Add(GaiHttpConstants.XApiKey, apiKey.ApiKey);
            }
            if (!string.IsNullOrWhiteSpace(apiKey?.AccountId))
            {
                headers.Add(GaiHttpConstants.XAccountId, apiKey.AccountId);
            }
            return headers;
        }


        public Task<GaiEnterpriseJobModel> PostJobToGravity(HttpContent data, string fileName, GaiEnterpriseMetaDataModel metaData, JsonSerializerOptions options, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default)
        {
            var content = new MultipartFormDataContent();
            if (!string.IsNullOrWhiteSpace(apiKey.ApiKey))
            {
                content.Headers.Add(GaiHttpConstants.XApiKey, apiKey.ApiKey);
            }

            if (!string.IsNullOrWhiteSpace(apiKey.AccountId))
            {
                content.Headers.Add(GaiHttpConstants.XAccountId, apiKey.AccountId);
            }

            var json = new StringContent(JsonSerializer.Serialize(metaData), Encoding.UTF8, "application/json");

            content.Add(json, "metadata");
            content.Add(data, "file", fileName);

            return PostForm<GaiEnterpriseJobModel>(GravityJobsEndpoint, content, options, cancel);

        }

        public Task<GaiEnterpriseJobModel> GetJobStatus(string jobId, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return Get<GaiEnterpriseJobModel>($"{GravityJobsEndpoint}/{jobId}", ToHeaders(apiKey), options, cancel);
        }

        public Task<string> GetJobResultLink(string jobId, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return Get<string>($"{GravityRetrieveJobResultEndpoint}/{jobId}", ToHeaders(apiKey), options, cancel);
        }

        public async Task<byte[]> GetJobResult(string jobId, GaiEnterpriseKey apiKey = null, CancellationToken cancel = default)
        {
            var link = await GetJobResultLink(jobId, apiKey, cancel);

            var request = new HttpRequestMessage(HttpMethod.Get, link);
            var response = await ApiClient.SendAsync(request, cancel);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}

using GravityAI.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GravityAI.Client
{


    internal interface IGaiApiConfiguration
    {
        string ApiKey { get; set; }
        string BaseUrl { get; set; }
        TimeSpan Timeout { get; set; }
    }

    public class GaiOnDemandConfiguration : IGaiApiConfiguration
    {
        public string ApiKey { get; set; }
        public virtual string BaseUrl { get; set; } = "https://on-demand.gravity-ai.com/";
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(15);
    }

    public class GaiEnterpriseConfiguration : IGaiApiConfiguration
    {
        public string ApiKey { get; set; }
        public string AccountId { get; set; }
        public string BaseUrl { get; set; } = "https://enterprise.gravity-ai.com/";
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(15);
    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddGravityAiOnDemandClient(this IServiceCollection services, string apiKey = null)
        {
            return services.AddGravityAiOnDemandClient((o) =>
            {
                o.ApiKey = apiKey;
            });

        }

        public static IServiceCollection AddGravityAiEnterpriseClient(this IServiceCollection services, string accountId, string apiKey = null)
        {
            return services.AddGravityAiEnterpriseClient((o) =>
            {
                o.AccountId = accountId;
                o.ApiKey = apiKey;
            });
        }

        public static IServiceCollection AddGravityAiOnDemandClient(this IServiceCollection services, Action<GaiOnDemandConfiguration> configureOptions)
        {
            services.Configure(configureOptions);

            services.AddHttpClient<IGaiOnDemandApiClient, GaiOnDemandApiClient>();
            services.AddTransient<IGravityAiOnDemandClient, GravityAiOnDemandClient>();

            return services;
        }

        public static IServiceCollection AddGravityAiEnterpriseClient(this IServiceCollection services, Action<GaiEnterpriseConfiguration> configureOptions)
        {
            services.Configure(configureOptions);

            services.AddHttpClient<IGaiEnterpriseApiClient, GaiEnterpriseApiClient>();
            services.AddHttpClient<IGravityAiEnterpriseClient, GravityAiEnterpriseClient>();

            return services;
        }


    }




    

    //public enum GaiMediaType
    //{
    //    Text,
    //    Image,
    //    Audio,
    //    Video,
    //    Binary,
    //    Json,
    //    CsvWithHeaders,
    //    CsvWithoutHeaders,
    //    TsvWithHeaders,
    //    TsvWithoutHeaders,
    //}







    //internal class GaiOnDemandService : IGravityAiOnDemandClient
    //{
    //    private const string ApiClientName = "GravityApi";
    //    private const string GravityUrl = "https://on-demand.gravity-ai.com/";
    //    private const string GravityJobsEndpoint = "api/v1/jobs";
    //    private const string GravityRetrieveJobResultEndpoint = "api/v1/jobs/result-link";

    //    private readonly GaiOnDemandConfiguration _options;

    //    private HttpClient ApiClient { get; }
    //    public GaiOnDemandService(IOptions<GaiOnDemandConfiguration> options, HttpClient http)
    //    {
    //        _options = options.Value;

    //        ApiClient = http;
    //        ApiClient.Timeout = _options.Timeout;
    //        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
    //        {
    //            ApiClient.DefaultRequestHeaders.Add("X-Api-Key", _options.ApiKey);
    //        }
    //    }

    //    private async Task<T_Result> HandleWebRequest<T_Result>(Func<Task<HttpResponseMessage>> action, JsonSerializerOptions options, CancellationToken cancel)
    //        where T_Result : class
    //    {
    //        var response = await action();

    //        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
    //        {
    //            var badResult = await DecodeResult<T_Result>(response.Content, options, cancel);
    //            if (badResult.IsError)
    //            {
    //                throw new HttpRequestException("Bad Request: " + badResult.ErrorMessage);
    //            }
    //        }

    //        response.EnsureSuccessStatusCode();

    //        var result = await DecodeResult<T_Result>(response.Content, options, cancel);

    //        if (result == null || result.IsError)
    //        {
    //            throw new Exception(result?.ErrorMessage ?? "Failed to Retrieve Data");
    //        }

    //        return result.Data;
    //    }



    //    private async Task<T_Result> PostForm<T_Result>(string route, MultipartFormDataContent content, JsonSerializerOptions options, CancellationToken cancel = default)
    //        where T_Result : class
    //    {
    //        return await HandleWebRequest<T_Result>(() => ApiClient.PostAsync(route, content, cancel), options, cancel);
    //    }

    //    private async Task<GaiApiResult<T>> DecodeResult<T>(HttpContent data, JsonSerializerOptions options, CancellationToken cancel) where T : class
    //    {
    //        var serOpts = options == null ? new JsonSerializerOptions() : new JsonSerializerOptions(options);
    //        serOpts.PropertyNameCaseInsensitive = true;

    //        var stream = await data.ReadAsStreamAsync();

    //        var result = await JsonSerializer.DeserializeAsync<GaiApiResult<T>>(stream, options, cancel);

    //        return result;
    //    }


    //    private async Task<T_Result> Get<T_Result>(string apiKey, string route, JsonSerializerOptions options, CancellationToken cancel)
    //    where T_Result : class
    //    {
    //        var request = new HttpRequestMessage(HttpMethod.Get, route);
    //        request.Headers.Add("x-api-key", apiKey);

    //        return await HandleWebRequest<T_Result>(() => ApiClient.SendAsync(request, cancel), options, cancel);
    //    }


    //    public Task<GaiOnDemandJobModel> PostJobToGravity(string apiKey, Stream file, string fileName, string mimeType, string name = null, JsonSerializerOptions options = null, Version modelVersion = null, CancellationToken cancel = default)
    //    {
    //        var content = new MultipartFormDataContent();
    //        content.Headers.Add("x-api-key", apiKey);

    //        var data = new GaiOnDemandMetaDataModel()
    //        {
    //            Mapping = new List<GaiPathMappingModel>(),
    //            OutputMapping = new List<GaiPathMappingModel>(),
    //            Version = null,
    //            JobName = name ?? "",
    //            MimeType = mimeType
    //        };



    //        var fileContent = new StreamContent(file);

    //        if (mimeType.StartsWith("text/plain; charset="))
    //        {
    //            var charSet = mimeType.Replace("text/plain; charset=", "");
    //            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = charSet };
    //        }
    //        else
    //        {
    //            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType) { };
    //        }
    //        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

    //        content.Add(json, "data");
    //        content.Add(fileContent, "file", fileName);

    //        return PostForm<GaiOnDemandJobModel>(GravityJobsEndpoint, content, options, cancel);

    //    }

    //    public Task<GaiOnDemandMetaDataModel> GetJobStatus(string apiKey, Guid jobId, CancellationToken cancel = default)
    //    {
    //        return Get<GaiOnDemandMetaDataModel>(apiKey, $"{GravityUrl}{GravityJobsEndpoint}/{jobId:N}", null, cancel);
    //    }

    //    public Task<string> GetJobResultLink(string apiKey, Guid jobId, CancellationToken cancel = default)
    //    {
    //        return Get<string>(apiKey, $"{GravityUrl}{GravityRetrieveJobResultEndpoint}/{jobId:N}", null, cancel);
    //    }

    //    public async Task<T_Result> GetJobResultAsJson<T_Result>(string apiKey, Guid jobId, JsonSerializerOptions options = null, CancellationToken cancel = default)
    //        where T_Result : class
    //    {
    //        var link = await GetJobResultLink(apiKey, jobId, cancel);

    //        var serOpts = options == null ? new JsonSerializerOptions() : new JsonSerializerOptions(options);
    //        serOpts.PropertyNameCaseInsensitive = true;

    //        var request = new HttpRequestMessage(HttpMethod.Get, link);
    //        var response = await ApiClient.SendAsync(request, cancel);

    //        response.EnsureSuccessStatusCode();

    //        var text = await response.Content.ReadAsStringAsync();

    //        if (text?.TrimStart().StartsWith("[") ?? false)
    //        {
    //            var results = JsonSerializer.Deserialize<List<T_Result>>(text, serOpts);
    //            return results?.FirstOrDefault();
    //        }

    //        var result = JsonSerializer.Deserialize<T_Result>(text, serOpts);
    //        return result;
    //    }
    //}



}

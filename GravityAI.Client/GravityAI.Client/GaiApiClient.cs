using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace GravityAI.Client
{
    internal abstract class GaiApiClient<T_Config> where T_Config : IGaiApiConfiguration
    {

        protected T_Config Config { get; }

        protected HttpClient ApiClient { get; }
        public GaiApiClient(T_Config config, HttpClient http)
        {
            Config = config;

            ApiClient = http;
            ApiClient.Timeout = Config.Timeout;

            if (!string.IsNullOrWhiteSpace(Config.ApiKey))
            {
                ApiClient.DefaultRequestHeaders.Add(GaiHttpConstants.XApiKey, Config.ApiKey);
            }
        }

        private async Task<T_Result> HandleWebRequest<T_Result>(Func<Task<HttpResponseMessage>> action, JsonSerializerOptions options, CancellationToken cancel)
            where T_Result : class
        {
            var response = await action();

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var badResult = await DecodeResult<T_Result>(response.Content, options, cancel);
                if (badResult.IsError)
                {
                    throw new HttpRequestException("Bad Request: " + badResult.ErrorMessage);
                }
            }

            response.EnsureSuccessStatusCode();

            var result = await DecodeResult<T_Result>(response.Content, options, cancel);

            if (result == null || result.IsError)
            {
                throw new Exception(result?.ErrorMessage ?? "Failed to Retrieve Data");
            }

            return result.Data;
        }

        private async Task<GaiApiResult<T>> DecodeResult<T>(HttpContent data, JsonSerializerOptions options, CancellationToken cancel) where T : class
        {
            var serOpts = options == null ? new JsonSerializerOptions() : new JsonSerializerOptions(options);
            serOpts.PropertyNameCaseInsensitive = true;

            var text = await data.ReadAsStringAsync();            
            var result = JsonSerializer.Deserialize<GaiApiResult<T>>(text, serOpts);

            //var stream = await data.ReadAsStreamAsync();
            //var result = await JsonSerializer.DeserializeAsync<GaiApiResult<T>>(stream, serOpts, cancel);

            return result;
        }

        protected string GetRoute(string endpoint)
        {
            return Config.BaseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
        }


        protected async Task<T_Result> Get<T_Result>(string endpoint, Dictionary<string, string> headers, JsonSerializerOptions options, CancellationToken cancel)
        where T_Result : class
        {
            var route = GetRoute(endpoint);

            var request = new HttpRequestMessage(HttpMethod.Get, route);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return await HandleWebRequest<T_Result>(() => ApiClient.SendAsync(request, cancel), options, cancel);
        }

        protected async Task<T_Result> PostForm<T_Result>(string endpoint, MultipartFormDataContent content, JsonSerializerOptions options, CancellationToken cancel = default)
            where T_Result : class
        {
            var route = GetRoute(endpoint);
            return await HandleWebRequest<T_Result>(() => ApiClient.PostAsync(route, content, cancel), options, cancel);
        }

        protected async Task<T_Result> Put<T_Result>(string endpoint, HttpContent content, JsonSerializerOptions options, CancellationToken cancel = default)
            where T_Result : class
        {
            var route = GetRoute(endpoint);
            return await HandleWebRequest<T_Result>(() => ApiClient.PutAsync(route, content, cancel), options, cancel);
        }

    }


}

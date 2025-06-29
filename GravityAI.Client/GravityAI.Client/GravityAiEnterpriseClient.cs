using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using GravityAI.Client.Models;

namespace GravityAI.Client
{
    public interface IGravityAiEnterpriseClient
    {
        Task<List<T_Result>> RunInferenceAsync<T_Result, T_Data>(T_Data data, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default);

        Task<List<T_Result>> RunInferenceAsync<T_Result>(Stream data, MediaTypeHeaderValue mediaType, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default);

        Task<byte[]> RunInferenceAsync<T_Data>(T_Data data, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default);

        Task<byte[]> RunInferenceAsync(Stream data, MediaTypeHeaderValue mimeType, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default);


    }


    internal class GravityAiEnterpriseClient : IGravityAiEnterpriseClient
    {
        private readonly IGaiEnterpriseApiClient Api;

        public GravityAiEnterpriseClient(IGaiEnterpriseApiClient api)
        {
            Api = api;
        }

        public Task<List<T_Result>> RunInferenceAsync<T_Result, T_Data>(T_Data data, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default)
        {
            var json = JsonSerializer.Serialize(data, options?.JsonSerializerOptions);

            return RunInferenceAsync<T_Result>(new MemoryStream(Encoding.UTF8.GetBytes(json)), new MediaTypeHeaderValue("application/json"), options, cancel);
        }

        public async Task<List<T_Result>> RunInferenceAsync<T_Result>(Stream data, MediaTypeHeaderValue mediaType, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default)
        {
            var result = await RunInferenceAsync(data, mediaType, options, cancel);
            var json = Encoding.UTF8.GetString(result);

            if (json?.TrimStart().StartsWith("[") ?? false)
            {
                return JsonSerializer.Deserialize<List<T_Result>>(json, options?.JsonSerializerOptions);
            }

            return new List<T_Result>
            {
                JsonSerializer.Deserialize<T_Result>(json, options?.JsonSerializerOptions)
            };
        }

        public Task<byte[]> RunInferenceAsync<T_Data>(T_Data data, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default)
        {
            var json = JsonSerializer.Serialize(data, options?.JsonSerializerOptions);

            return RunInferenceAsync(new MemoryStream(Encoding.UTF8.GetBytes(json)), new MediaTypeHeaderValue("application/json"), options, cancel);
        }

        public async Task<byte[]> RunInferenceAsync(Stream data, MediaTypeHeaderValue mimeType, GaiEnterpriseInferenceOptions options = null, CancellationToken cancel = default)
        {
            var meta = new GaiEnterpriseMetaDataModel()
            {
                Mapping = options?.InputMap.GetMap(),
                OutputMapping = options?.OutputMap.GetMap(),
                CustomData = options?.CustomData,
                Name = options?.Name ?? "",
            };

            var scontent = new StreamContent(data);
            scontent.Headers.ContentType = mimeType;
            var fileName = string.IsNullOrWhiteSpace(options?.SourceFileName) ? "input.dat" : options.SourceFileName;

            var jobStatus = await Api.PostJobToGravity(scontent, fileName, meta, options?.JsonSerializerOptions, options, cancel);

            if (jobStatus == null)
            {
                throw new Exception("Failed to submit data: Api returned a null result.");
            }

            var count = 0;
            do
            {
                cancel.ThrowIfCancellationRequested();

                if (jobStatus == null)
                {
                    throw new Exception("Failed to retrieve data: Api returned a null result.");
                }

                if (jobStatus.Status == GxEnterpriseJobStatus.Fail)
                {
                    throw new Exception("Failed to Process Data: " + jobStatus?.ErrorMessage ?? "No Error Message Available");
                }

                if (jobStatus.Status == GxEnterpriseJobStatus.ProcessingError)
                {
                    throw new Exception("Error while Processing Data: " + jobStatus?.ErrorMessage ?? "No Error Message Available");
                }

                if (jobStatus.Status == GxEnterpriseJobStatus.Success)
                {
                    return await Api.GetJobResult(jobStatus.Id, options, cancel);
                }

                jobStatus = await Api.GetJobStatus(jobStatus.Id, options, cancel);
                count++;

                await Task.Delay(GaiApiHelper.GetPollDelay(count), cancel);
            }
            while (true);


            //if (mimeType.StartsWith("text/plain; charset="))
            //{
            //    var charSet = mimeType.Replace("text/plain; charset=", "");
            //    fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = charSet };
            //}
            //else
            //{
            //    fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType) { };
            //}


        }


    }

}

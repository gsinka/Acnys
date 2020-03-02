using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Scheduler.Jobs.Http
{
    public class HttpJobRunner : IRunJob<HttpJob>
    {
        private readonly HttpClientHandler _httpClientHandler;

        public HttpJobRunner(HttpClientHandler httpClientHandler)
        {
            _httpClientHandler = httpClientHandler;
        }

        public async Task<JobResult> Run(HttpJob job, CancellationToken cancellationToken)
        {
            using var client = new HttpClient(_httpClientHandler);

            // Add header properties

            foreach (var property in job.HeaderProperties)
            {
                client.DefaultRequestHeaders.Add(property.Key, property.Value);
            }

            // Send http request

            var response = job.Method switch
            {
                { } method when method == HttpMethod.Get => await client.GetAsync(new Uri(job.Uri), cancellationToken),
                { } method when method == HttpMethod.Post => await client.PostAsync(new Uri(job.Uri),  new StringContent(job.Payload, Encoding.UTF8, job.MediaType), cancellationToken),
                { } method when method == HttpMethod.Put => await client.PutAsync(new Uri(job.Uri), new StringContent(job.Payload, Encoding.UTF8, job.MediaType), cancellationToken),
                { } method when method == HttpMethod.Delete => await client.DeleteAsync(new Uri(job.Uri), cancellationToken),
                _ => throw new InvalidOperationException($"Invalid HTTP method: {job.Method.Method}. Only GET, POST, PUT and DELETE is supported.")
            };

            // Return result based on response

            return response != null && response.IsSuccessStatusCode
                ? JobResult.Success
                : JobResult.Failure(response.ReasonPhrase);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Scheduler.Jobs.Http
{
    public class HttpJob : Job
    {
        public HttpMethod Method { get; }
        public IDictionary<string, string> HeaderProperties { get; }
        public string Payload { get; }
        public string Uri { get; }
        public string MediaType { get; }

        public HttpJob(Guid jobId, string name, string description, string uri, HttpMethod method, IDictionary<string, string> headerProperties, string payload, string mediaType = "application/json") 
            : base(jobId, name, description)
        {
            Uri = uri;
            Method = method;
            HeaderProperties = headerProperties;
            Payload = payload;
            MediaType = mediaType;
        }
    }
}
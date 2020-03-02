using System;
using System.Collections.Generic;
using Acnys.Core.Scheduler.Jobs;

namespace Acnys.Core.Scheduler.Amqp
{
    public class AmqpJob : Job
    {
        public IDictionary<string, string> Headers { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public string Payload { get; }

        public AmqpJob(Guid jobId, string name, string description, IDictionary<string, string> headers, IDictionary<string, string> properties, string payload) 
            : base(jobId, name, description)
        {
            Headers = headers;
            Properties = properties;
            Payload = payload;
        }
    }
}
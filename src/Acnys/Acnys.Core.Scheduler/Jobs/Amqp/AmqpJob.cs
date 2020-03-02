using System;
using System.Collections.Generic;

namespace Acnys.Core.Scheduler.Jobs.Amqp
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
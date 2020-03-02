using System;
using Acnys.Core.Scheduler.Jobs;

namespace Acnys.Core.Scheduler.Mqtt
{
    public class MqttJob : Job
    {
        public string Topic { get; }
        public string Payload { get; set; }

        public MqttJob(Guid jobId, string name, string description, string topic, string payload) : base(jobId, name, description)
        {
            Topic = topic;
            Payload = payload;
        }
    }
}
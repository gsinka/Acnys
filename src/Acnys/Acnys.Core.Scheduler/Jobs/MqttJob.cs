﻿using System;

namespace Acnys.Core.Scheduler.Jobs
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
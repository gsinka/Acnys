using System;
using System.Net.Http;

namespace Acnys.Core.Scheduler.Jobs
{
    public abstract class Job : IJob
    {
        public Guid JobId { get; }
        public string Name { get; }
        public string Description { get; }

        protected Job(Guid jobId, string name, string description)
        {
            JobId = jobId;
            Name = name;
            Description = description;
        }
    }
}
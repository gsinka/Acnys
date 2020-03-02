using System;

namespace Acnys.Core.Scheduler
{
    public class ScheduledJob
    {
        public Guid Id { get; }
        public Guid JobId { get; }
        public DateTime NextRun { get; }

        public ScheduledJob(Guid id, Guid jobId, DateTime nextRun)
        {
            Id = id;
            JobId = jobId;
            NextRun = nextRun;
        }
    }
}
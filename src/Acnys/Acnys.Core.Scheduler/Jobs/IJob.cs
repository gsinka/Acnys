using System;

namespace Acnys.Core.Scheduler.Jobs
{
    public interface IJob
    {
        Guid JobId { get; }
        string Name { get; }
        string Description { get; }
    }

    public interface IJobSchedule
    {

    }

    
}
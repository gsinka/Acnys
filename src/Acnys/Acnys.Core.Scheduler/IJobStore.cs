using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Scheduler.Jobs;

namespace Acnys.Core.Scheduler
{
    public interface IJobStore
    {
        Task<ICollection<IJob>> GetJobs(CancellationToken cancellationToken);

        Task MarkCompleted(Guid jobId);
    }
}
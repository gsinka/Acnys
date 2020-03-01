using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Scheduler
{
    public interface IJobStore
    {
        Task<ICollection<IJob>> GetJobs(CancellationToken cancellationToken);

        Task MarkCompleted(Guid jobId);
    }
}
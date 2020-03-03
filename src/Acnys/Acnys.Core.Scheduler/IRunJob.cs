using System;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Scheduler.Jobs;

namespace Acnys.Core.Scheduler
{
    public interface IRunJob
    {
        Task Run(Guid job, CancellationToken cancellationToken);
    }
    
    public interface IRunJob<in T> where T : IJob
    {
        Task<JobResult> Run(T job, CancellationToken cancellationToken);
    }

    public interface IResolveJobRunner
    {
        IRunJob GetRunner(IJob job);
    }

    public interface IJobTrigger
    {

    }
    
}
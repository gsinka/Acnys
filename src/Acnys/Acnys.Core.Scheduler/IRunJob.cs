using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Scheduler.Jobs;
using Acnys.Core.Scheduler.Jobs.Http;

namespace Acnys.Core.Scheduler
{
    public interface IRunJob
    {
        Task<JobResult> Run(IJob job, CancellationToken cancellationToken);
    }

    public class JobRunnr : IRunJob
    {
        public Task<JobResult> Run(IJob job, CancellationToken cancellationToken)
        {
            return default;
        }

        public Task<JobResult> Run(HttpJob httpJob, CancellationToken cancellationToken)
        {
            return default;
        }

    }
    
    public interface IRunJob<in T> where T : IJob
    {
        Task<JobResult> Run(T job, CancellationToken cancellationToken);
    }

    public interface IResolveJobRunner
    {
        IRunJob<IJob> RunnerFor(IJob job);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Scheduler.Jobs
{
    public interface IRunJob<in T> where T : IJob
    {
        Task<JobResult> Run(T job, CancellationToken cancellationToken);
    }
}
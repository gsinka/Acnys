using System;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Scheduler.Jobs.Delegate
{
    public class DelegateJobRunner : IRunJob<DelegateJob>
    {
        public async Task<JobResult> Run(DelegateJob job, CancellationToken cancellationToken)
        {
            try
            {
                await job.Action(cancellationToken);
                return JobResult.Success;
            }
            catch (Exception exception)
            {
                return JobResult.Failure(exception.Message);
            }
        }
    }
}
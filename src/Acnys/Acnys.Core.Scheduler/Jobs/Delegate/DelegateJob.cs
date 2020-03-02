using System;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Scheduler.Jobs.Delegate
{
    public class DelegateJob : Job
    {
        public Func<CancellationToken, Task> Action { get; }

        public DelegateJob(Guid jobId, string name, string description, Func<CancellationToken, Task> action) 
            : base(jobId, name, description)
        {
            Action = action;
        }
    }
}
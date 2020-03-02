using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Scheduler.Jobs;
using Serilog;

namespace Acnys.Core.Scheduler
{
    public class SchedulerService
    {
        private readonly ILogger _log;
        private readonly IRunJob _jobRunner;
        private readonly IJobStore _jobStore;
        private readonly SchedulerOptions _options;
        private readonly ConcurrentDictionary<Guid, DateTime> _schedules = new ConcurrentDictionary<Guid, DateTime>();
        
        public SchedulerService(ILogger log, IRunJob jobRunner, IJobStore jobStore, SchedulerOptions options)
        {
            _log = log;
            _jobRunner = jobRunner;
            _jobStore = jobStore;
            _options = options;
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            try
            {
                var stoppingTokenSource = new CancellationTokenSource();
                cancellationToken.Register(() => stoppingTokenSource.Cancel());

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_options.PollInterval, stoppingTokenSource.Token);
                    _log.Debug("Checking tasks to run");

                    IJob job = null;

                    await _jobRunner.Run(job, cancellationToken);

                }

            }
            catch (Exception exception)
            {
                _log.Debug("Scheduler stopped processing");
            }
        }

        public Task Schedule(Action action, TimeSpan delay, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(delay, cancellationToken);
                action();

            }, cancellationToken);
        }
    }
}

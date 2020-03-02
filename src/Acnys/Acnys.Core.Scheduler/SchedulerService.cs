using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Acnys.Core.Scheduler
{
    public class SchedulerService
    {
        private readonly ILogger _log;
        private readonly IJobStore _jobStore;
        private readonly SchedulerOptions _options;
        private readonly ConcurrentDictionary<Guid, DateTime> _schedules = new ConcurrentDictionary<Guid, DateTime>();
        
        public SchedulerService(ILogger log, IJobStore jobStore, SchedulerOptions options)
        {
            _log = log;
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

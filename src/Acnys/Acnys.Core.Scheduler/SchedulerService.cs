using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Scheduler.Jobs;
using Serilog;

namespace Acnys.Core.Scheduler
{
    public class SchedulerService
    {
        private readonly ILogger _log;
        private readonly IClock _clock;
        private readonly IResolveJobRunner _jobRunnerResolver;
        private readonly IRunJob _jobRunner;
        private readonly IJobStore _jobStore;
        private readonly SchedulerOptions _options;
        private readonly ConcurrentDictionary<Guid, (IJob job, IJobTrigger trigger)> _schedules = new ConcurrentDictionary<Guid, (IJob job, IJobTrigger trigger)>();
        
        public SchedulerService(ILogger log, IClock clock, IResolveJobRunner jobRunnerResolver, IRunJob jobRunner, IJobStore jobStore, SchedulerOptions options)
        {
            _log = log;
            _clock = clock;
            _jobRunnerResolver = jobRunnerResolver;
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

                    foreach (var schedule in _schedules)
                    {
                        var (job, trigger) = schedule.Value;

                        trigger.NextRun(_clock.UtcNow, DateTime.UtcNow.AddMilliseconds(_options.PollInterval));



                    }
                }

            }
            catch (Exception exception)
            {
                _log.Debug("Scheduler stopped processing");
            }
        }

        public Guid Schedule(IJob job, IJobTrigger trigger, CancellationToken cancellationToken)
        {
            var scheduleId = Guid.NewGuid();

            return _schedules.TryAdd(scheduleId, (job, trigger))
                ? scheduleId
                : throw new InvalidOperationException("Adding schedule failed");
        }
    }
}

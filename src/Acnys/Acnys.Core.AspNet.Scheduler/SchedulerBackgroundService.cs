using System;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Scheduler;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.AspNet.Scheduler
{
    public class SchedulerBackgroundService : BackgroundService
    {
        private readonly ILogger _log;
        private readonly SchedulerService _schedulerService;

        public SchedulerBackgroundService(ILogger log, SchedulerService schedulerService)
        {
            _log = log;
            _schedulerService = schedulerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _schedulerService.Process(stoppingToken);
        }
    }
}

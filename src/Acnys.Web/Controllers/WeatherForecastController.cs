using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Application;
using Acnys.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Api.Requests;
using Acnys.Core.Testing;

namespace Acnys.Web.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IClock _clock;
        private readonly ISendCommand _commandSender;
        private readonly EventAwaiter _awaiter;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IClock clock, ISendCommand commandSender, EventAwaiter awaiter)
        {
            _logger = logger;
            _clock = clock;
            _commandSender = commandSender;
            _awaiter = awaiter;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
        {
            //var command = new TestCommand("test", Guid.Empty, Guid.NewGuid());
            var command = new TestCommand(_clock.UtcNow.Second % 2 == 0 ? "test" : "no test", Guid.Empty, Guid.NewGuid());

            _logger.LogInformation($"Sending command on thread {Thread.CurrentThread.ManagedThreadId}");
            var result = _awaiter.GetAwaiter<TestEvent>(evnt => evnt.CorrelationId == command.CorrelationId && evnt.Data == "test", TimeSpan.FromSeconds(5), cancellationToken);
            //var result = _awaiter.GetAwaiter<TestEvent>(evnt => evnt.CorrelationId == command.CorrelationId, TimeSpan.FromSeconds(10), cancellationToken);
            await _commandSender.Send(command, cancellationToken);
            
            result.Wait(cancellationToken);
            
            _logger.LogInformation($"Result on thread {Thread.CurrentThread.ManagedThreadId}");

            var rng = new Random();
            return result.Result == null ? new WeatherForecast[0] : Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = _clock.UtcNow.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })

            .ToArray();
        }
    }
}

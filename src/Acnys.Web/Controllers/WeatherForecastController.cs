using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Application;
using Acnys.Core.Services;
using Acnys.Core.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Api.Requests;

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
        private readonly EventAwaiter<TestEvent> _awaiter;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IClock clock, ISendCommand commandSender, EventAwaiter<TestEvent> awaiter)
        {
            _logger = logger;
            _clock = clock;
            _commandSender = commandSender;
            _awaiter = awaiter;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
        {
            var command = new TestCommand( _clock.UtcNow.Second % 2 == 0 ? "test" : "no test", Guid.Empty, Guid.NewGuid());

            var @event = _awaiter.DoAndWaitForEvent(
                //() => _commandSender.Send(command, cancellationToken),
                command,
                e => e.Data == "test", 
                TimeSpan.FromSeconds(1));
            


            //var result = await _awaiter.WaitFor(e => e.Data == "test1", TimeSpan.FromSeconds(1));

            //var returnEvent = await _testHelper.SendCommandAndWaitForEvent<TestEvent, TestCommand>(
            //    new TestCommand("test", Guid.Empty, Guid.NewGuid()), 
            //    evnt => true, 
            //    TimeSpan.FromSeconds(5), 
            //    cancellationToken);

            var rng = new Random();
            return @event == null 
                ? new WeatherForecast[0] 
                : Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = _clock.UtcNow.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })

            .ToArray();
        }
    }
}

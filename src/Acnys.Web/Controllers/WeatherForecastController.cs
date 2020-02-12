using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Hosting.Testing;
using Acnys.Core.Request.Application;
using Acnys.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
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
        private readonly TestHelper<TestEvent> _testHelper;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IClock clock, ISendCommand commandSender, TestHelper<TestEvent> testHelper)
        {
            _logger = logger;
            _clock = clock;
            _commandSender = commandSender;
            _testHelper = testHelper;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
        {
            await _commandSender.Send(new TestCommand("test", Guid.Empty, Guid.NewGuid()), cancellationToken);

            var result = await _testHelper.WaitFor(e => e.Data == "test");

            //var returnEvent = await _testHelper.SendCommandAndWaitForEvent<TestEvent, TestCommand>(
            //    new TestCommand("test", Guid.Empty, Guid.NewGuid()), 
            //    evnt => true, 
            //    TimeSpan.FromSeconds(5), 
            //    cancellationToken);

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = _clock.UtcNow.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

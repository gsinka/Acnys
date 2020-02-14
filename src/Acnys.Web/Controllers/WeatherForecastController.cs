using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Hosting.Events;
using Acnys.Core.Hosting.Request;
using Acnys.Core.Request.Application;
using Acnys.Core.Services;
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
        private readonly IRequestService _request;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IClock clock, IRequestService request)
        {
            _logger = logger;
            _clock = clock;
            _request = request;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
        {
            var command = new TestCommand("test", Guid.Empty, Guid.NewGuid());
            //var command = new TestCommand(_clock.UtcNow.Second % 2 == 0 ? "test" : "no test", Guid.Empty, Guid.NewGuid());

            IEvent evnt = null;

            try
            {
                evnt = await _request.ExeSendCommandAndWaitForEvent<TestEvent, TestCommand>(command,
                    @event => @event.CorrelationId == command.CorrelationId && @event.Data == "test",
                    TimeSpan.FromMilliseconds(50), cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Timeout");
            }
            
            var rng = new Random();

            return evnt == null ? new WeatherForecast[0] : Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = _clock.UtcNow.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })

                .ToArray();
        }
    }
}

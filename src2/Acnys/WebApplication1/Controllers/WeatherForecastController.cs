using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IPublishEvent _eventPublisher;
        private readonly ISendCommand _commandSender;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IPublishEvent eventPublisher, ISendCommand commandSender)
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
            _commandSender = commandSender;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            //_eventPublisher.Publish(new TestEvent("test"));
            _commandSender.Send(new TestCommand("data", correlationId: Guid.NewGuid()), arguments: new Dictionary<string, object>{ { "source", "weather forecast"} });

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

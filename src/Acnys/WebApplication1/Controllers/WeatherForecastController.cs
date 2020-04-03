using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Acnys.Core.Request.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ISendRequest _sender;
        private readonly UserContext _userContext;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(ISendRequest sender, UserContext userContext)
        {
            _sender = sender;
            _userContext = userContext;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            //_eventPublisher.Publish(new TestEvent("test"));
            _sender.Send(new TestCommand("data"), arguments: new Dictionary<string, object>{ { "source", "weather forecast"} }.UseCorrelationId(Guid.NewGuid()));

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acnys.Core.Correlation;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Acnys.Core.Request.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication1.Commands;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ISendRequest _sender;
        private readonly UserContext _userContext;
        private readonly IRecordEvent _eventRecorder;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(ISendRequest sender, UserContext userContext, IRecordEvent eventRecorder)
        {
            _sender = sender;
            _userContext = userContext;
            _eventRecorder = eventRecorder;
        }

        [HttpGet("Generate exception")]
        public async Task GenerateException()
        {
            var correlationId = Guid.NewGuid();

            //_eventPublisher.Publish(new TestEvent("test"));
            await _sender.Send(new AnotherTestCommand("data"), arguments: new Dictionary<string, object> { { "source", "weather forecast" } }.UseCorrelationId(correlationId));

        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var correlationId = Guid.NewGuid();

            //_eventPublisher.Publish(new TestEvent("test"));
            await _sender.Send(new TestCommand("data"), arguments: new Dictionary<string, object>{ { "source", "weather forecast"} }.UseCorrelationId(correlationId));
            
            await Task.Delay(50);

            var testEventAwaiter = _eventRecorder
                .WaitFor<TestEvent>(
                    (evnt, args) => evnt.Data == "data" && args.CorrelationId() == correlationId, 
                    TimeSpan.FromSeconds(5));

            var testEvent = await testEventAwaiter;

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = $"{testEvent?.Data} / {Summaries[rng.Next(Summaries.Length)]}"
            })
            .ToArray();
        }

        [HttpGet("query")]
        public async Task<IActionResult> Query()
        {
            var result = await _sender.Send(new TestQuery("test data"));
            return Ok(result);
        }
    }
}

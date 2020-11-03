using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Acnys.Request.Behaviors;
using Acnys.Request.Dispatchers;
using Acnys.Request.Rabbit;
using Acnys.Request.Senders;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace Acnys.Request.Tests
{
    public class Command_sender_pipeline_tests
    {
        private readonly IServiceCollection _preparedServiceCollection;

        public Command_sender_pipeline_tests(ITestOutputHelper testOutputHelper)
        {
            _preparedServiceCollection = new ServiceCollection();

            _preparedServiceCollection
                .AddLogging(builder =>
                {
                    builder.AddXunit(testOutputHelper, new LoggingConfig() { ScopePaddingSpaces = 2 });
                    builder.SetMinimumLevel(LogLevel.Trace);

                })
                .AddScoped<IDispatchCommand, CommandDispatcher>();
        }
        
        [Fact(DisplayName = "Command is handled by the appropriate command handler")]
        public async Task Command_handled_with_handler()
        {
            var requestId = Guid.Empty;

            _preparedServiceCollection.AddScoped<IHandleCommand<TestCommand>, TestCommandHandler>(provider => new TestCommandHandler(provider.GetRequiredService<ILogger<TestCommandHandler>>(), command =>
            {
                provider.GetService<ILogger<Command_dispatcher_tests>>().LogInformation("Setting request id in test");
                requestId = command.RequestId;
            }));
            
            _preparedServiceCollection.AddScoped<ISendCommand>(provider => new LoopbackCommandSender(provider.GetService<ILogger<LoopbackCommandSender>>(), provider.GetService<IDispatchCommand>()));

            var services = _preparedServiceCollection.BuildServiceProvider();

            var expectedRequestId = Guid.NewGuid();
            await services.GetService<ISendCommand>().SendAsync(new TestCommand(expectedRequestId), new Dictionary<string, object>() { {"isLocal", true}});
            Assert.Equal(expectedRequestId, requestId);

            await services.GetService<ISendCommand>().SendAsync(new TestCommand(expectedRequestId), new Dictionary<string, object>() { { "isLocal", true } });
        }
    }
}
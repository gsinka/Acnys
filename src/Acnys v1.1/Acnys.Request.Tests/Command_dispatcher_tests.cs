using System;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Acnys.Request.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace Acnys.Request.Tests
{
    public class Command_dispatcher_tests
    {
        private readonly IServiceCollection _preparedServiceCollection;

        public Command_dispatcher_tests(ITestOutputHelper testOutputHelper)
        {
            _preparedServiceCollection = new ServiceCollection();

            _preparedServiceCollection
                .AddLogging(builder =>
                {
                    builder.AddXunit(testOutputHelper);
                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .AddScoped<IDispatchCommand, CommandDispatcher>();
        }
        
        [Fact(DisplayName = "Exception is thrown in case of no handler registered for command")]
        public async Task Exception_thrown_in_case_of_no_handler_registered_for_command()
        {
            var services = _preparedServiceCollection.BuildServiceProvider();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await services.GetService<IDispatchCommand>().DispatchAsync(new TestCommand()));
        }

        [Fact(DisplayName = "Exception is thrown in case of multiple handlers registered for the same command")]
        public async Task Exception_thrown_in_case_of_multiple_handlers_registered_for_command()
        {
            _preparedServiceCollection.AddScoped<IHandleCommand<TestCommand>, TestCommandHandler>();
            _preparedServiceCollection.AddScoped<IHandleCommand<TestCommand>, TestCommandHandler>();
            var services = _preparedServiceCollection.BuildServiceProvider();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await services.GetService<IDispatchCommand>().DispatchAsync(new TestCommand()));
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

            var services = _preparedServiceCollection.BuildServiceProvider();

            var expectedRequestId = Guid.NewGuid();
            await services.GetService<IDispatchCommand>().DispatchAsync(new TestCommand(expectedRequestId));
            
            Assert.Equal(expectedRequestId, requestId);
        }
    }
}

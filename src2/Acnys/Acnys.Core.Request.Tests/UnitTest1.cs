using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Extensions;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Request.Tests
{
    public class UnitTest1
    {
        private readonly IContainer _container;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

            builder.RegisterLoopbackRequestSender();
            builder.RegisterCommandDispatcher();
            builder.RegisterQueryDispatcher();
            builder.RegisterType<TestCommandHandler>().AsImplementedInterfaces().InstancePerLifetimeScope();

            _container = builder.Build();
        }

        [Fact]
        public async Task Test1()
        {
            var sender = _container.Resolve<ISendRequest>();
            await sender.Send(new TestCommand(), CancellationToken.None);
        }
    }
}

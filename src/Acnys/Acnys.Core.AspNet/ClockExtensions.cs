using Acnys.Core.Services;
using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.AspNet
{
    public static class ClockExtensions
    {
        public static IHostBuilder AddComputerClock(this IHostBuilder hostBuilder)
        {
            Log.Verbose("Adding computer clock");
            return hostBuilder.ConfigureContainer<ContainerBuilder>(builder => builder.RegisterType<ComputerClock>().AsImplementedInterfaces().AsSelf().SingleInstance());
        }
    }
}

using System;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet
{
    public static class AppBuilder
    {
        public static IHost Build(string[] args, Action<IHostBuilder> build)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
            build(hostBuilder);
            return hostBuilder.Build();
        }
    }
}

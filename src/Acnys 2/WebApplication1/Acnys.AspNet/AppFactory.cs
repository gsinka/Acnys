using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.AspNet
{
    public class AppFactory
    {
        private readonly string[] _args;


        public AppFactory(string[] args)
        {
            _args = args;
        }

        public App Build(Action<AppBuilder> builder)
        {
            var appBuilder = new AppBuilder();
            builder(appBuilder);

            var hostBuilder = new HostBuilder();

            hostBuilder.UseServiceProviderFactory(hostContext => new AutofacServiceProviderFactory(containerBuilder =>
            {
                appBuilder._containerBuilderCallbacks.ForEach(action => action(hostContext, containerBuilder));
            }));

            hostBuilder.ConfigureWebHost(webHostBuilder =>
            {
                
            });
            
            return new App(_args);
        }

        public App Default => new App(_args);
    }

    public class AppBuilder
    {
        internal readonly List<Action<HostBuilderContext, ContainerBuilder>> _containerBuilderCallbacks = new List<Action<HostBuilderContext, ContainerBuilder>>();
        public void RegisterContainerBuilderCallback(Action<HostBuilderContext, ContainerBuilder> callback) => _containerBuilderCallbacks.Add(callback);
    }

    public class App
    {
        internal App(string[] args)
        {
        }

        public void Run()
        {

        }
    }
}

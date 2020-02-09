using Autofac;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Acnys.Core.Hosting.RabbitMQ.Pooled
{
    public static class RabbitServiceExtensions
    {
        public static ContainerBuilder AddRabbit(this ContainerBuilder builder)
        {
            //builder.Register(context => new RabbitService(context.Resolve<IConnection>())).As<IRabbitService>().SingleInstance();
            builder.RegisterType<RabbitService>().As<IRabbitService>().SingleInstance();
            builder.RegisterType<DefaultObjectPoolProvider>().As<ObjectPoolProvider>().SingleInstance();
            builder.RegisterType<RabbitModelPooledObjectPolicy>().As<IPooledObjectPolicy<IModel>>();
            builder.Register(context => context.Resolve<IRabbitService>().Channel).As<IModel>().InstancePerDependency();

            return builder;
        }

        
    }
}
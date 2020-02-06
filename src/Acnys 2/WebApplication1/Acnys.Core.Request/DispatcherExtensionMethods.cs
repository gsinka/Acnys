using System;
using System.Net.Http;
using System.Reflection;
using Acnys.Core.Request.Abstractions;
using Autofac;
using Serilog;

namespace Acnys.Core.Request
{
    public static class DispatcherExtensionMethods
    {
        /// <summary>
        /// Register command dispatcher
        /// </summary>
        /// <param name="builder">Container builder instance</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterLocalCommandDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<LocalCommandDispatcher>().AsImplementedInterfaces().SingleInstance();
            return builder;
        }

        /// <summary>
        /// Register command handlers from the assembly of the given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="builder">Container builder instance</param>
        /// <returns>Returns the container builder instance</returns>
        public static ContainerBuilder RegisterCommandHandlersFrom<T>(this ContainerBuilder builder)
        {
            return RegisterCommandHandlersFrom(builder, typeof(T).Assembly);
        }

        /// <summary>
        /// Register command handlers from assemblies
        /// </summary>
        /// <param name="builder">Container builder instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <returns>Returns the container builder instance</returns>
        /// <remarks>This method will scan the assemblies and registers all command handlers</remarks>
        public static ContainerBuilder RegisterCommandHandlersFrom(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IHandleCommand<>)).AsImplementedInterfaces();
            return builder;
        }
        

        /// <summary>
        /// Register query dispatcher
        /// </summary>
        /// <param name="builder">Container builder instance</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterLocalQueryDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<LocalQueryDispatcher>().AsImplementedInterfaces().SingleInstance();
            return builder;
        }

        /// <summary>
        /// Register query handlers from the assembly of the given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="builder">Container builder instance</param>
        /// <returns>Returns the container builder instance</returns>
        public static ContainerBuilder RegisterQueryHandlersFrom<T>(this ContainerBuilder builder)
        {
            return RegisterQueryHandlersFrom(builder, typeof(T).Assembly);
        }

        /// <summary>
        /// Register command handlers from assemblies
        /// </summary>
        /// <param name="builder">Container builder instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <returns>Returns the container builder instance</returns>
        /// <remarks>This method will scan the assemblies and registers all query handlers</remarks>
        public static ContainerBuilder RegisterQueryHandlersFrom(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IHandleQuery<,>)).AsImplementedInterfaces();
            return builder;
        }

        /// <summary>
        /// Register event dispatcher
        /// </summary>
        /// <param name="builder">Container builder instance</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterLocalEventDispatcher(this ContainerBuilder builder)
        {
            //builder.RegisterType<LocalEventDispatcher>().AsImplementedInterfaces().SingleInstance();
            return builder;
        }

        /// <summary>
        /// Register event handlers from the assembly of the given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="builder">Container builder instance</param>
        /// <returns>Returns the container builder instance</returns>
        public static ContainerBuilder RegisterEventHandlersFrom<T>(this ContainerBuilder builder)
        {
            return RegisterEventHandlersFrom(builder, typeof(T).Assembly);
        }

        /// <summary>
        /// Register query handlers from assemblies
        /// </summary>
        /// <param name="builder">Container builder instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <returns>Returns the container builder instance</returns>
        /// <remarks>This method will scan the assemblies and registers all event handlers</remarks>
        public static ContainerBuilder RegisterEventHandlersFrom(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IHandleEvent<>)).AsImplementedInterfaces();
            return builder;
        }

        public static ContainerBuilder RegisterHttpCommandDispatcher(this ContainerBuilder builder, string key, Uri uri)
        {
            builder.Register(context => new HttpCommandDispatcher(
                    context.Resolve<ILogger>().ForContext<HttpCommandDispatcher>(),
                    context.Resolve<HttpClientHandler>(), 
                    uri))
                .Keyed<IDispatchCommand>(key).InstancePerLifetimeScope();

            return builder;
        }

        public static ContainerBuilder RegisterHttpQueryDispatcher(this ContainerBuilder builder, string key, Uri uri)
        {
            builder.Register(context => new HttpQueryDispatcher(
                    context.Resolve<ILogger>().ForContext<HttpCommandDispatcher>(),
                    context.Resolve<HttpClientHandler>(), 
                    uri))
                .Keyed<IDispatchQuery>(key).InstancePerLifetimeScope();

            return builder;
        }
    }
}

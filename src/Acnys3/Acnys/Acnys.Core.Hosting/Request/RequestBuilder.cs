using Acnys.Core.Eventing;
using Acnys.Core.Request.Application;
using Acnys.Core.Request.ReadModel;
using Autofac;
using FluentValidation;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting.Request
{
    public class RequestBuilder
    {
        protected HostBuilderContext Context { get; }
        protected ContainerBuilder Builder { get; }

        public RequestBuilder(HostBuilderContext context, ContainerBuilder builder)
        {
            Context = context;
            Builder = builder;

            Builder.RegisterType<CommandDispatcher>().As<IDispatchCommand>().SingleInstance();
            Builder.RegisterType<QueryDispatcher>().As<IDispatchQuery>().SingleInstance();
        }

        public RequestBuilder RegisterHandlersFromAssemblyOf<T>()
        {
            Builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleCommand<>))
                .AsImplementedInterfaces();

            Builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleQuery<,>))
                .AsImplementedInterfaces();

            return this;
        }

        public RequestBuilder RegisterValidatorsFromAssemblyOf<T>()
        {
            Builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IValidator<>))
                .AsImplementedInterfaces();

            return this;

        }

        public RequestBuilder ValidateRequests()
        {
            Builder.RegisterGeneric(typeof(CommandValidator<>));
            Builder.RegisterGenericDecorator(typeof(CommandValidator<>), typeof(IHandleCommand<>));

            Builder.RegisterGeneric(typeof(QueryValidator<,>));
            Builder.RegisterGenericDecorator(typeof(QueryValidator<,>), typeof(IHandleQuery<,>));

            return this;
        }
    }
}
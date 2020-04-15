using System.Reflection;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Validation;
using Autofac;
using FluentValidation;

namespace Acnys.Core.Infrastructure
{
    public static class ValidationExtensions
    {
        public static ContainerBuilder AddCommandValidationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(CommandValidator<>));
            builder.RegisterGenericDecorator(typeof(CommandValidator<>), typeof(IHandleCommand<>));
            return builder;
        } 
        
        public static ContainerBuilder AddQueryValidationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(QueryValidator<,>));
            builder.RegisterGenericDecorator(typeof(QueryValidator<,>), typeof(IHandleQuery<,>));
            return builder;
        }

        public static ContainerBuilder RegisterValidatorsFromAssemblyOf<T>(this ContainerBuilder builder)
        {
            return RegisterValidatorsFromAssembly(builder, typeof(T).Assembly);
        }

        public static ContainerBuilder RegisterValidatorsFromAssembly(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            return builder;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Acnys.Core.Abstractions;

namespace Acnys.Core.CommandQuery.Infrastructure
{
    public class AppFactory
    {
    }
    
    public static class CommandQueryAppFactoryExtensions
    {
        public static AppFactory UseCommandQuery(this AppFactory factory, Action<CommandQueryBuilder> config)
        {
            return factory;
        }

        public static AppFactory UseRequests(this AppFactory factory, Action<RequestBuilder> builder)
        {
            return factory;
        }
    }

    public class RequestHandlerRegistrationBuilder : RequestHandlerBuilder
    {
        public RequestHandlerRegistrationBuilder Register<T>()
        {
            return this;
        }
    }

   

    public class CommandQueryBuilder
    {
        public CommandQueryHandlerBuilder Handlers => new CommandQueryHandlerBuilder();
        public CommandQueryDispatcherBuilder Dispatchers => new CommandQueryDispatcherBuilder();
        //public RequestRouteBuilder Sender => new RequestRouteBuilder();
    }

    public class CommandQueryHandlerBuilder : CommandQueryBuilder
    {
        public CommandQueryHandlerBuilder RegisterFrom<T>()
        {
            return this;
        }
        public CommandQueryHandlerBuilder RegisterFrom(params Assembly[] assemblies)
        {
            return this;
        }
    }

    public class RequestSenderBuilder : CommandQueryBuilder
    {
        protected RequestRouteBuilder RouteBuilder;

        public RequestRouteBuilder Route(Func<IRequest, bool> selector)
        {
            return RouteBuilder;
        }

        public RequestRouteBuilder Route<T>()
        {
            return RouteBuilder;
        }

        public RequestRouteBuilder Route(params Assembly[] assemblies)
        {
            return RouteBuilder;
        }
    }

    

    public class CommandQueryDispatcherBuilder : CommandQueryBuilder
    {
        public CommandQueryDispatcherBuilder BlaBla(Expression<Func<IRequest, bool>> selector)
        {
            return this;
        }
    }

}
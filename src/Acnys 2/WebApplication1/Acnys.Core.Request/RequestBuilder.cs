using System;
using Acnys.Core.Abstractions;
using Acnys.Core.Request.Abstractions;
using Autofac;

namespace Acnys.Core.Request
{
    public class RequestBuilder
    {
        protected ContainerBuilder Builder { get; }
        
        public RequestHandlerBuilder Handlers;
        public RequestSenderBuilder Sender;

        public RequestBuilder(ContainerBuilder builder)
        {
            Builder = builder;

            Handlers = new RequestHandlerBuilder(builder);
            Sender = new RequestSenderBuilder(builder);
        }
    }

    public class RequestSenderBuilder : RequestBuilder
    {
        public RequestSenderBuilder(ContainerBuilder builder) : base(builder)
        {
        }

        public RequestSenderBuilder Route(Func<IRequest, bool> filter)
        {
            return this;
        }
    }

    public class RequestHandlerBuilder : RequestBuilder
    {
        public RequestHandlerBuilder(ContainerBuilder builder) : base(builder)
        {
        }

        public RequestHandlerBuilder RegisterFrom<T>()
        {
            Builder.RegisterCommandHandlersFrom<T>();
            Builder.RegisterQueryHandlersFrom<T>();

            return this;
        }
    }
}

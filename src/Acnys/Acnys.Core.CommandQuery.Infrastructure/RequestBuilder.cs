using System;
using System.Security;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Abstractions;

namespace Acnys.Core.CommandQuery.Infrastructure
{
    public class RequestBuilder
    {
        public DispatchConfigBuilder Dispatcher;
        public SenderConfigBuilder Sender;
    }

    public class SenderConfigBuilder : RequestBuilder
    {
        protected ISendCommand CommandSender;
        protected ISendQuery QuerySender;

        public RequestRouteBuilder Route(Func<IRequest, bool> filter)
        {
            return new RequestRouteBuilder(filter);
        }

    }

    public class RequestRouteBuilder : SenderConfigBuilder
    {
        private readonly Func<IRequest, bool> _filter;

        public RequestRouteBuilder(Func<IRequest, bool> filter)
        {
            _filter = filter;
        }
    }

    public static class HttpRequestSenderExtnesions
    {
        public static RequestRouteBuilder ToHttp(this RequestRouteBuilder builder)
        {
            return builder;
        }
    }

    public class DispatchConfigBuilder : RequestBuilder
    {
        protected IDispatchCommand CommandDispatcher;
        protected IDispatchQuery QueryDispatcher;

    }

    public static class RequestFluentValidationExtensions
    {
        public static DispatchConfigBuilder UseFluentValidation(this DispatchConfigBuilder configBuilder)
        {
            return configBuilder;
        }
    }

    public class AutofacRequestBuilder
    {
        public AutofacRequestBuilder RegisterHandlersFromAssemblyOf<T>()
        {
            return this;
        }
    }

    public class RequestHandlerBuilder : RequestBuilder
    {
        public RequestHandlerRegistrationBuilder Handlers;
    }
}
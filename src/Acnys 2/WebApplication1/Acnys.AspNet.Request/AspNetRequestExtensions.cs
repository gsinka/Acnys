using System;
using Acnys.Core.Request;
using Microsoft.Extensions.Hosting;

namespace Acnys.AspNet.Request
{
    public static class AspNetRequestExtensions
    {
        public static AppBuilder UseRequest(this AppBuilder factory, Action<HostBuilderContext, RequestBuilder> builder)
        {
            factory.RegisterContainerBuilderCallback((context, containerBuilder) =>
            {
                var requestBuilder = new RequestBuilder(containerBuilder);
                builder(context, requestBuilder);

            });

            return factory;
        }
    }
}

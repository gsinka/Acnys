using System;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting.Request
{
    public static class HostBuilderRequestExtensions
    {
        public static IHostBuilder AddRequests(this IHostBuilder hostBuilder, Action<HostBuilderContext, RequestBuilder> requestBuilder)
        {
            
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                var requestConfig = new RequestBuilder();
                requestBuilder(context, requestConfig);
                
                requestConfig.Callbacks.ForEach(action => action(context, builder));
                
                //requestBuilder(context, new RequestBuilder(context, builder));
            });


        }

    }
}
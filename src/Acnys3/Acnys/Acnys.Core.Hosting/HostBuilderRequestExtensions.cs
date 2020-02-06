using System;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting
{
    public static class HostBuilderRequestExtensions
    {
        public static IHostBuilder UseRequest(this IHostBuilder hostBuilder, Action<HostBuilderContext, RequestBuilder> requestBuilder)
        {
            return hostBuilder;//.ConfigureWebHost(builder => builder.);
        }

    }

    public class RequestBuilder
    {
    }
}
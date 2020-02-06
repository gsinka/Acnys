using System;
using Autofac;

namespace Acnys.Core.CommandQuery.Infrastructure
{
    public static class RequestBuilderAutofacExtensions
    {
        public static DispatchConfigBuilder UseAutofac(this DispatchConfigBuilder configBuilder, Action<ContainerBuilder, AutofacRequestBuilder> builder)
        {
            return configBuilder;
        }
    }

    public static class RequestBuilderServiceCollectionExtensions
    {
        public static RequestBuilder UseServiceCollection(this RequestBuilder builder)
        {
            return builder;
        }
    }
}
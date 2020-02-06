using System;
using Acnys.Core.CommandQuery.Infrastructure;
using Xunit;

namespace Acnys.Core.CommandQuery.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Request_with_Autofac()
        {

            new AppFactory()
              
                .UseRequests(builder => builder
                    .Dispatcher
                        .UseAutofac(config => config.RegisterHandlersFromAssemblyOf<UnitTest1>())
                        .UseFluentValidation()
                    .Sender
                        .Route(request => true).ToHttp()
                        .Route(request => true).ToHttp()
                );


        }

        public void Request_with_ServiceCollection()
        {
            new AppFactory()
                .UseRequests(builder => builder.UseServiceCollection())
        }
    }
}

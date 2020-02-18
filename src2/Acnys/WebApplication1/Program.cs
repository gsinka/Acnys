using Acnys.Core.AspNet;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.Request;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            AppBuilder.Build(args, hostBuilder =>
            {
                hostBuilder

                    .PrebuildDefaultApp()
                    .RegisterRequestHandlersFromAssemblyOf<TestEventHandler>()
                    .RegisterEventHandlersFromAssemblyOf<TestEventHandler>()
                    .AddHttpRequestSender(context => "http://localhost:5000/api", "http")
                    .ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        builder.RegisterType<Setup>().As<IStartable>().SingleInstance();
                    });
            }).Run();
        }
    }


}

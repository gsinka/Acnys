using Acnys.AspNet;
using Acnys.AspNet.Request;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var appFactory = new AppFactory(args);
            
            appFactory.Build(builder => builder

                .UseRequest((context, config) =>
                {
                    config
                        .Handlers.RegisterFrom<Program>();
                    //.Sender.Route(request => true);
                })

                ).Run();
            

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

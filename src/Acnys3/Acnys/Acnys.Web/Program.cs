using System.Reflection;
using Acnys.Core.Hosting;
using Acnys.Core.Hosting.Events;
using Acnys.Core.Hosting.Request;
using Acnys.Core.Hosting.Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Api.Requests;
using Sample.Application.Handlers;
using Sample.ReadModel;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Acnys.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)

                .AddAutofac()
                
                .AddSerilog((context, config) => config
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{App}] {Message:lj} <-- [{SourceContext}]{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                )

                .AddComputerClock()
                
                .AddRequests((context, builder) => builder
                    .RegisterHandlersFromAssemblyOf<TestCommandHandler>()
                    .RegisterHandlersFromAssemblyOf<TestQueryHandler>()
                    .RegisterValidatorsFromAssemblyOf<TestCommand>()
                    .ValidateRequests()
                )

                .AddEvents((context, builder) => builder
                    .RegisterHandlersFromAssemblyOf<TestEventHandler>()
                    
                
                )

                .AddHttpRequestHandler()

                .ConfigureWebHostDefaults(builder => builder
                    .Configure((context, app) =>
                        {
                            if (context.HostingEnvironment.IsDevelopment())
                            {
                                app.UseDeveloperExceptionPage();
                            }

                            app.UseStatusCodePages();
                            app.UseRouting();
                            app.UseHttpRequestHandler();
                            app.UseAuthorization();
                            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

                        })

                    .ConfigureServices((context, services) =>
                    {
                        services.AddControllers().AddApplicationPart(Assembly.GetEntryAssembly()).AddControllersAsServices();
                        services.AddAuthorization();

                    })
                )

                .Build().Run();

        }

    }
}

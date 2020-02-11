using System;
using System.Collections.Generic;
using System.Reflection;
using Acnys.Core.Hosting;
using Acnys.Core.Hosting.Events;
using Acnys.Core.Hosting.HealthCheck;
using Acnys.Core.Hosting.Metrics;
using Acnys.Core.Hosting.OpenApiDocument;
using Acnys.Core.Hosting.RabbitMQ;
using Acnys.Core.Hosting.Request;
using Acnys.Core.Hosting.Request.Sender;
using Acnys.Core.Hosting.Serilog;
using Acnys.Core.Hosting.SingleSignOn;
using Acnys.Core.Request;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Sample.Api.Requests;
using Sample.Application.Handlers;
using Sample.ReadModel;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace Acnys.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var documentSettings = new OpenApiDocumentSettings();
            var securityScheme = new OpenApiSecurityScheme();

            var app = Host.CreateDefaultBuilder(args)
                
                .AddAutofac()

                .AddSerilog((context, config) => config
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{App}] {Message:lj} <-- [{SourceContext}]{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                    //.WriteTo.Console(new JsonFormatter())
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("App", "SPL")
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
                    
                    .AddLoopbackSender("loopback")
                    .AddHttpSender("local", builderContext => "http://localhost:5000/api")
                    .RouteRequest((request) => DateTime.Now.Second % 2 == 0 ? "local" : "loopback")
                )

                .AddEvents((context, builder) => builder
                    .RegisterHandlersFromAssemblyOf<TestEventHandler>()
                )

                .AddHttpRequestHandler()

                .AddRabbitEventBus("RabbitEventSettings")
                .AddRabbitEventBusHealthCheck()
                
                .ConfigureServices((context, services) =>
                {
                    services.AddHealthChecks()
                        .AddCheck("Self", () => HealthCheckResult.Healthy(), new List<string> { "Liveness" });

                    services.AddHttpMetrics();

                    services
                        .AddControllers()
                        .AddApplicationPart(Assembly.GetEntryAssembly()).AddControllersAsServices();
                    
                    services.AddAuthorization(options => options.AddPolicy("admin", builder => builder.RequireClaim("user-roles", new List<string>() { "admin" })));
                    services.AddSingleSignOn(configuration => context.Configuration.Bind(Defaults.CONFIGURATION_SECTION, configuration));

                    
                    // OpenAPI 
                    
                    context.Configuration.Bind("OpenApi", documentSettings);
                    context.Configuration.Bind("OpenApi:Security", securityScheme);
                    services.AddOpenApiDocumentation(documentSettings, securityScheme);
                })

                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                })

                .ConfigureWebHostDefaults(builder => builder
                    .Configure((context, app) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.AddLiveness();
                        app.AddReadiness();
                        app.UseHttpMetrics();
                        app.UseStatusCodePages();

                        app.AddOpenApiDocumentation(documentSettings, securityScheme);
                            
                        app.UseRouting();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHttpRequestHandler("/api");
                            endpoints.MapControllers();
                        });
                    })
                )

                .Build();
                
            Log.ForContext<Program>().Information("Running application");

            app.Run();
        }

    }
}

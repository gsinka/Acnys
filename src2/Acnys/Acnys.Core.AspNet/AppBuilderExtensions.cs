﻿using System.Collections.Generic;
using System.Reflection;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Acnys.Core.AspNet
{
    public static class AppBuilderExtensions
    {
        public static IHostBuilder PrebuildDefaultApp(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                
                .AddSerilog((context, config) => config
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{App}] {Message:lj} <-- [{SourceContext}]{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("App", "SMC")
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning))

                .AddAutofac()

                .AddHealthChecks((context, builder) => builder
                    .AddCheck("Self", () => HealthCheckResult.Healthy(), new List<string> {"Liveness", "Readiness"})
                )
                .AddHttpMetrics()
                .AddOpenApiDocumentation(
                    (context, app) => context.Configuration.Bind("Application", app),
                    (context, sso) => context.Configuration.Bind("SingleSignOn", sso))
                .AddRequests()
                .AddRequestValidation()
                
                .AddHttpRequestHandler()
                .AddEventing()
                .AddRabbitService((context, configuration) => context.Configuration.Bind("Rabbit", configuration))

                .ConfigureContainer<ContainerBuilder>((context, builder) => { })
                
                .ConfigureServices((context, services) =>
                {
                    services.AddControllers().AddApplicationPart(Assembly.GetEntryAssembly()).AddControllersAsServices();
                    //services.Configure<RabbitServiceConfiguration>(context.Configuration.GetSection("Rabbit"));
                })

                .ConfigureWebHostDefaults(builder => builder.Configure((context, app) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    app.UseRouting();
                    app.UseAuthorization();

                    var appSettings = new ApplicationOptions();
                    context.Configuration.Bind("Application", appSettings);

                    var ssoSettings = new SingleSignOnOptions();
                    context.Configuration.Bind("SingleSignOn", ssoSettings);

                    var openApiSettings = new OpenApiDocumentationOptions() { Path = "/swagger" };

                    app.AddOpenApiDocumentation(appSettings, ssoSettings, openApiSettings);

                    app.AddReadiness();
                    app.AddLiveness();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                        endpoints.MapHttpRequestHandler("api");
                    });
                }))
            ;
        }
    }
}
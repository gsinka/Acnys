using System.Collections.Generic;
using System.Reflection;
using Acnys.Core.Hosting;
using Acnys.Core.Hosting.Events;
using Acnys.Core.Hosting.HealthCheck;
using Acnys.Core.Hosting.Metrics;
using Acnys.Core.Hosting.OpenApiDocument;
using Acnys.Core.Hosting.RabbitMQ;
using Acnys.Core.Hosting.RabbitMQ.Lovisa;
using Acnys.Core.Hosting.Request;
using Acnys.Core.Hosting.Serilog;
using Acnys.Core.Hosting.SingleSignOn;
using Acnys.Core.Request.Application;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Prometheus;
using RabbitMQ.Client;
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
            var documentSettings = new OpenApiDocumentSettings();
            var securityScheme = new OpenApiSecurityScheme();

            Host.CreateDefaultBuilder(args)

                .AddAutofac()

                .AddSerilog((context, config) => config
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{App}] {Message:lj} <-- [{SourceContext}]{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                    .Enrich.FromLogContext()
                    .MinimumLevel.Information()
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

                    //services.AddHostedService<RabbitEventService>();
                    services.Configure<RabbitEventSettings>(context.Configuration.GetSection("RabbitEventSettings"));
                })

                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                    builder.RegisterType<RabbitEventService>().AsImplementedInterfaces().SingleInstance();
                    

                    //builder.AddRabbitConnection(factory => factory
                    //    .UseUri(context.Configuration["RabbitMQ:Uri"])
                    //    .EnableAutoRecovery()
                    //    );

                    //builder.RegisterType<BasicPropertiesMapper>().As<IMapEvent>().SingleInstance();

                    //builder.Register(componentContext => new RabbitConsumerFactory()
                    //    .UseConnection(componentContext.Resolve<IConnection>)
                    //    .CreateQueue()
                    //    .DispatchMessage(componentContext.Resolve<IMapEvent>(), componentContext.Resolve<IDispatchEvent>())
                    //    .CreateConsumer("test", false)
                    //    )
                    //    .AsSelf()
                    //    .SingleInstance()
                    //    .AutoActivate();
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
                            endpoints.MapHttpRequestHandler();
                            endpoints.MapControllers();
                        });
                    })
                )

                .Build().Run();

        }

    }
}

using Acnys.Core.AspNet;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Acnys.Core.AspNet.Tracing;
using Acnys.Core.Eventing.Infrastructure.Extensions;
using Acnys.Core.Services;
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
using System;
using System.Collections.Generic;
using System.Reflection;
using Acnys.Core.Correlation;

namespace WebApplication2
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var application = AppBuilder.Build(args, hostBuilder =>
                {
                    hostBuilder

                        .AddAutofac()
                        .AddComputerClock()
                        .AddSerilog((context, config) =>
                        {
                            config
                                .WriteTo.Console(
                                    outputTemplate:
                                    "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{App}] {Message:lj} <-- [{SourceContext}]{NewLine}{Exception}",
                                    theme: AnsiConsoleTheme.Code)
                                .WriteTo.Seq(context.Configuration["Seq:Url"])
                                .MinimumLevel.Verbose()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                .Enrich.FromLogContext()
                                .Enrich.WithProperty("Application", "TEST");
                        })

                        .AddHealthChecks((context, builder) => builder
                            .AddCheck("Self", () => HealthCheckResult.Healthy(), new List<string> { "Liveness" })
                        )
                        .AddHttpMetrics()
                        .AddOpenApiDocumentation(
                            (context, appConfig) => context.Configuration.Bind("Application", appConfig),
                            (context, sso) => context.Configuration.Bind("SingleSignOn", sso))
                        .AddRequests()
                        .AddRequestValidation()
                        .AddFullRequestMetrics()
                        .AddFullEventingMetrics()

                        .AddHttpRequestHandler()
                        .AddEventing()
                        .AddTracing((configuration) => configuration["Tracing:Host"], (configuration) => { var port = configuration["Tracing:Port"]; return string.IsNullOrWhiteSpace(port) ? 0 : Int32.Parse(port); })

                        .AddSingleSignOn((context, options) => context.Configuration.Bind("SingleSignOn", options))

                        .RegisterRequestHandlersFromAssemblyOf<TestEventHandler>()
                        .RegisterEventHandlersFromAssemblyOf<TestEventHandler>()

                        .AddRequestSender(request => "http")
                        .AddHttpRequestSender(context => "http://localhost:5000/api", "http")
                        .AddRabbit((context, factory) =>
                        {
                            factory.Uri = new Uri(context.Configuration["Rabbit:Uri"]);
                            factory.AutomaticRecoveryEnabled = true;
                            factory.NetworkRecoveryInterval = new TimeSpan(0, 0, 1);
                            factory.TopologyRecoveryEnabled = true;

                        }, "test", "test2", consumerCount: 5, consumerTag: "test-tag")

                        .ConfigureContainer<ContainerBuilder>((context, builder) =>
                        {
                            builder.RegisterType<ComputerClock>().AsImplementedInterfaces();
                            builder.RegisterType<Setup>().As<IStartable>().SingleInstance();
                            builder.RegisterType<UserContext>().AsSelf().InstancePerLifetimeScope();

                            builder.RegisterType<TestMiddleware>().AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope();

                            builder.RegisterEventRecorderService(10000);

                            builder.AddCorrelation();

                            //builder.RegisterType<EventRecorder>().AsImplementedInterfaces().SingleInstance();
                            //builder.Register((ctx => new EventRecorder(ctx.Resolve<ILogger>(), ctx.Resolve<IClock>(), 100))).AsImplementedInterfaces().SingleInstance();
                        })

                        .ConfigureServices((context, services) =>
                        {

                            services.AddControllers(options => { options.UseRequestBinder(); }).AddApplicationPart(Assembly.GetEntryAssembly()).AddControllersAsServices();

                            //services.AddTransient<TestMiddleware>();

                            services.AddAuthorization(options =>
                            {
                                options.AddPolicy("admin", builder => builder.RequireClaim("user-roles", new[] { "administrator" }));
                            });
                        })

                        .ConfigureWebHostDefaults(builder => builder.Configure((context, app) =>
                        {
                            if (context.HostingEnvironment.IsDevelopment())
                            {
                                app.UseDeveloperExceptionPage();
                            }

                            app.UseMiddleware<TestMiddleware>();

                            app.AddErrorHandling();
                            app.UseRouting();

                            app.UseAuthentication();
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
                        }));
                });

                Log.ForContext<Program>().Information("Running application");

                application.Run();

                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Application failed");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

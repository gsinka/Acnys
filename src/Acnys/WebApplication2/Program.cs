using Acnys.Core.AspNet;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.Extensions;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Acnys.Core.Eventing.Infrastructure.Extensions;
using Acnys.Core.Services;
using Autofac;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Reflection;

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
                        .AddTracing((context, services) =>
                        {
                            services.AddSingleton<ITracer>(serviceProvider =>
                            {
                                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                                var log = serviceProvider.GetRequiredService<Serilog.ILogger>();

                                Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                                    .RegisterSenderFactory<ThriftSenderFactory>();

                                var endpoint = context.Configuration.GetValue<string>("Tracing:Host") ?? "localhost";
                                var port = context.Configuration.GetValue<int?>("Tracing:Port") ?? 6831;
                                log.Information($"Tracing added with endpoint: {endpoint}");

                                var reporter = new RemoteReporter.Builder()
                                 .WithLoggerFactory(loggerFactory)
                                 .WithSender(new UdpSender(endpoint, port, 0))
                                 .Build();

                                var tracer = new Tracer.Builder(Assembly.GetEntryAssembly().GetName().Name)
                                 .WithLoggerFactory(loggerFactory)
                                 .WithSampler(new ConstSampler(true))
                                 .WithReporter(reporter)
                                 .Build();


                                if (!GlobalTracer.IsRegistered())
                                {
                                    GlobalTracer.Register(tracer);
                                }

                                return tracer;
                            });
                        })

                        .AddSingleSignOn((context, options) => context.Configuration.Bind("SingleSignOn", options))

                        .RegisterRequestHandlersFromAssemblyOf<TestEventHandler>()
                        .RegisterEventHandlersFromAssemblyOf<TestEventHandler>()

                        .AddRequestSender(request => "http")
                        .AddHttpRequestSender(context => "http://localhost:5000/api", "http")
                        .AddRabbit((context, factory) =>
                        {
                            factory.Uri = new Uri(context.Configuration["Rabbit:Uri"]);
                            factory.AutomaticRecoveryEnabled = true;

                        }, "test", "test2", consumerCount: 5, consumerTag: "test-tag")

                        .ConfigureContainer<ContainerBuilder>((context, builder) =>
                        {
                            builder.RegisterType<ComputerClock>().AsImplementedInterfaces();
                            builder.RegisterType<Setup>().As<IStartable>().SingleInstance();
                            builder.RegisterType<UserContext>().AsSelf().InstancePerLifetimeScope();

                            builder.RegisterType<TestMiddleware>().AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope();

                            builder.RegisterEventRecorderService(10000);

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

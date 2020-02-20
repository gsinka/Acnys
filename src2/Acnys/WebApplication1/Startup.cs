using System.Reflection;
using Acnys.Core.AspNet;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers().AddApplicationPart(Assembly.GetEntryAssembly()).AddControllersAsServices();

            services.Configure<RabbitServiceConfiguration>(Configuration.GetSection("Rabbit"));
            
            //services.AddHostedService<RabbitHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            
            var appSettings = new ApplicationOptions();
            Configuration.Bind("Application", appSettings);

            var ssoSettings = new SingleSignOnOptions();
            Configuration.Bind("SingleSignOn", ssoSettings);

            var openApiSettings = new OpenApiDocumentationOptions() { Path = "/swagger" };
            
            app.AddOpenApiDocumentation(appSettings, ssoSettings, openApiSettings);

            app.AddReadiness();
            app.AddLiveness();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHttpRequestHandler("api");
            });
        }
    }
}

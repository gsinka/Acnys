using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

namespace Acnys.Core.AspNet
{
    public static class SingleSignOnExtensions
    {
        public static IHostBuilder AddSingleSignOn(this IHostBuilder hostBuilder, Action<HostBuilderContext, SingleSignOnOptions> options)
        {
            var ssoOptions = new SingleSignOnOptions();

            return hostBuilder.ConfigureServices((context, services) =>
            {
                IdentityModelEventSource.ShowPII = true;
                options(context, ssoOptions);

                if (string.IsNullOrWhiteSpace(ssoOptions.Authority)) throw new ArgumentNullException(nameof(ssoOptions.Authority));
                
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(opt =>
                    {
                        opt.MetadataAddress = $"{ssoOptions.Authority}/.well-known/openid-configuration";
                        opt.Audience = ssoOptions.Audience;
                        opt.RequireHttpsMetadata = ssoOptions.RequireHttpsMetadata;
                        opt.SaveToken = ssoOptions.SaveToken;
                    });
            });
        }
    }
}

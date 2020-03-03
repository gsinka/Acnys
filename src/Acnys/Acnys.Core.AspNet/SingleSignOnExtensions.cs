using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Acnys.Core.AspNet
{
    public static class SingleSignOnExtensions
    {
        public static IHostBuilder AddSingleSignOn(this IHostBuilder hostBuilder,  Action<HostBuilderContext, SingleSignOnOptions> options)
        {
            return AddSingleSignOn(hostBuilder, options, (context, parameters) => {});
        }

        public static IHostBuilder AddSingleSignOn(this IHostBuilder hostBuilder, Action<HostBuilderContext, SingleSignOnOptions> options, Action<HostBuilderContext, TokenValidationParameters> tokenValidationParametersBuilder)
        {
            var ssoOptions = new SingleSignOnOptions();
            var tokenValidationParameters = new TokenValidationParameters();

            return hostBuilder.ConfigureServices((context, services) =>
            {
                options(context, ssoOptions);
                tokenValidationParametersBuilder(context, tokenValidationParameters);

                IdentityModelEventSource.ShowPII = ssoOptions.ShowPII;

                if (string.IsNullOrWhiteSpace(ssoOptions.Authority)) throw new ArgumentNullException(nameof(ssoOptions.Authority));
                
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(opt =>
                    {
                        opt.MetadataAddress = $"{ssoOptions.Authority}/.well-known/openid-configuration";
                        opt.Audience = ssoOptions.Audience;
                        opt.RequireHttpsMetadata = ssoOptions.RequireHttpsMetadata;
                        opt.SaveToken = ssoOptions.SaveToken;
                        opt.TokenValidationParameters = tokenValidationParameters;
                    });
            });
        }
    }
}

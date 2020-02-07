using System;
using Acnys.Core.SingleSignOn.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Acnys.Core.Hosting.SingleSignOn
{
    public static class SingleSignOnExtensions
    {
        public static void AddSingleSignOn(this IServiceCollection services, Action<JwtBearerConfiguration> jwtBearerConfigBuilder)
        {
            var jwtBearerConfiguration = new JwtBearerConfiguration();
            jwtBearerConfigBuilder(jwtBearerConfiguration);
            
            if (string.IsNullOrWhiteSpace(jwtBearerConfiguration.Authority)) throw new ArgumentNullException(nameof(jwtBearerConfiguration.Authority));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MetadataAddress = $"{jwtBearerConfiguration.Authority}/.well-known/openid-configuration";
                    options.Audience = jwtBearerConfiguration.Audience;
                    options.RequireHttpsMetadata = jwtBearerConfiguration.RequireHttpsMetadata;
                    options.SaveToken = jwtBearerConfiguration.SaveToken;
                });
        }
    }
}

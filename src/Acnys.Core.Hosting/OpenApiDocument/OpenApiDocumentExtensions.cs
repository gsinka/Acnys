using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace Acnys.Core.Hosting.OpenApiDocument
{
    public static class OpenApiDocumentExtensions
    {
        public static void AddOpenApiDocumentation(this IServiceCollection services, OpenApiDocumentSettings settings, OpenApiSecurityScheme securityScheme)
        {
            services.AddOpenApiDocument(options =>
            {
                options.DocumentName = settings.DocumentName;
                options.Title = settings.DocumentTitle;
                options.Version = settings.Version;

                options.DocumentProcessors.Add(new SecurityDefinitionAppender(securityScheme.Name, new NSwag.OpenApiSecurityScheme
                {
                    Name = securityScheme.Name,
                    Scheme = securityScheme.Scheme,
                    Type = securityScheme.Type,
                    Flow = securityScheme.Flow,
                    AuthorizationUrl = $"{securityScheme.Authority}/protocol/openid-connect/auth",
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "Open ID" },
                        { "profile", "Profile" }
                    }
                }));
            });
        }

        public static void AddOpenApiDocumentation(this IApplicationBuilder app, OpenApiDocumentSettings documentSettings, OpenApiSecurityScheme securityScheme)
        {
            app.UseOpenApi(settings =>
            {
                settings.DocumentName = settings.DocumentName;
            });

            app.UseSwaggerUi3(settings =>
            {
                settings.Path = documentSettings.Path;
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    AppName = securityScheme.AppName,
                    ClientId = securityScheme.ClientId
                };
                settings.OAuth2Client.AdditionalQueryStringParameters.Add("nonce", "123456");
            });
        }

        
    }
}

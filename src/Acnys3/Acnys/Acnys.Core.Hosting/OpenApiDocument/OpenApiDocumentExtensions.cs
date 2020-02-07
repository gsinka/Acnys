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
        public static void AddOpenApiDocumentation(this IServiceCollection services)
        {
            services.AddOpenApiDocument(options =>
            {
                options.DocumentName = "SampleApp";
                options.Title = "Sample Application";
                options.Version = "1.0.0";

                options.DocumentProcessors.Add(new SecurityDefinitionAppender("name", new NSwag.OpenApiSecurityScheme
                {
                    Name = "name",
                    Scheme = "http",
                    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
                    Flow = NSwag.OpenApiOAuth2Flow.Implicit,
                    AuthorizationUrl = $"{""}/protocol/openid-connect/auth",
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "Open ID" },
                        { "profile", "Profile" }
                    }
                }));
            });
        }

        public static void AddOpenApiDocumentation(this IApplicationBuilder app)
        {
            app.UseOpenApi(settings =>
            {
                settings.DocumentName = "Document name";
            });

            app.UseSwaggerUi3(settings =>
            {
                settings.Path = "/swagger";
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    AppName = "AppName",
                    ClientId = "ClientId"
                };
                settings.OAuth2Client.AdditionalQueryStringParameters.Add("nonce", "123456");
            });
        }

        
    }
}

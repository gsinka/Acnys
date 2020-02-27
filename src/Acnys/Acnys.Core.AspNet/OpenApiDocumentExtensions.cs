using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using Serilog;

namespace Acnys.Core.AspNet
{
    public static class OpenApiDocumentExtensions
    {
        public static IHostBuilder AddOpenApiDocumentation(this IHostBuilder hostBuilder, Action<HostBuilderContext, ApplicationOptions> appOptions, Action<HostBuilderContext, SingleSignOnOptions> ssoOptions)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                var app = new ApplicationOptions();
                appOptions(context, app);
                Log.Verbose("Adding OpenAPI service for application {appName}", app.Name);

                var sso = new SingleSignOnOptions();
                ssoOptions(context, sso);
                Log.Verbose("Adding OpenAPI security using authority {authority}", sso.Authority);

                services.AddOpenApiDocument(options =>
                {
                    options.DocumentName = app.Name;
                    options.Title = app.Title;
                    options.Version = app.Version;

                    options.OperationProcessors.Add(new OperationSecurityScopeProcessor(sso.SecuritySchemeName));

                    options.DocumentProcessors.Add(new SecurityDefinitionAppender(sso.SecuritySchemeName, new NSwag.OpenApiSecurityScheme
                    {
                        Name = sso.SecuritySchemeName,
                        Scheme = sso.SecurityScheme,
                        Type = sso.SecuritySchemeType,
                        Flow = sso.OAuthFlow,
                        AuthorizationUrl = $"{sso.Authority}/protocol/openid-connect/auth",
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "Open ID" },
                            { "profile", "Profile" }
                        }
                    }));
                });
            });
        }

        public static IApplicationBuilder AddOpenApiDocumentation(this IApplicationBuilder app, Action<ApplicationOptions> appOptions, Action<SingleSignOnOptions> ssoOptions, Action<OpenApiDocumentationOptions> openApiOptions)
        {
            var openApiSettings = new OpenApiDocumentationOptions();
            openApiOptions(openApiSettings);

            var appSettings = new ApplicationOptions();
            appOptions(appSettings);

            var ssoSettings = new SingleSignOnOptions();
            ssoOptions(ssoSettings);
            
            Log.Verbose("Configured OpenAPI on path {documentPath}", openApiSettings.Path);

            return app

                .UseOpenApi(settings => { settings.DocumentName = settings.DocumentName; })
                
                .UseSwaggerUi3(settings =>
                {
                    settings.Path = openApiSettings.Path;
                    settings.OAuth2Client = new OAuth2ClientSettings
                    {
                        AppName = appSettings.Name,
                        ClientId = ssoSettings.ClientId
                    };
                    settings.OAuth2Client.AdditionalQueryStringParameters.Add("nonce", "123456");
                });
        }
        
        [Obsolete]
        public static IApplicationBuilder AddOpenApiDocumentation(this IApplicationBuilder app, ApplicationOptions appSettings, SingleSignOnOptions ssoSettings, OpenApiDocumentationOptions openApiSettings)
        {
            Log.Verbose("Configured OpenAPI on path {documentPath}", openApiSettings.Path);

            return app

                .UseOpenApi(settings => { settings.DocumentName = settings.DocumentName; })
                
                .UseSwaggerUi3(settings =>
                {
                    settings.Path = openApiSettings.Path;
                    settings.OAuth2Client = new OAuth2ClientSettings
                    {
                        AppName = appSettings.Name,
                        ClientId = ssoSettings.ClientId
                    };
                    settings.OAuth2Client.AdditionalQueryStringParameters.Add("nonce", "123456");
                });
        }
    }
}

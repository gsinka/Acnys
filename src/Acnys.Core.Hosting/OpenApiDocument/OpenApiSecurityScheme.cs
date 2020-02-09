using NSwag;

namespace Acnys.Core.Hosting.OpenApiDocument
{
    public class OpenApiSecurityScheme
    {
        public string Name { get; set; } = "OAuth2";
        public string Scheme { get; set; } = "http";
        public OpenApiSecuritySchemeType Type { get; set; } = OpenApiSecuritySchemeType.OAuth2;
        public OpenApiOAuth2Flow Flow { get; set; } = OpenApiOAuth2Flow.Implicit;
        public string Authority { get; set; }
        public string AppName { get; set; } = "App";
        public string ClientId { get; set; } = "clientId";

    }
}
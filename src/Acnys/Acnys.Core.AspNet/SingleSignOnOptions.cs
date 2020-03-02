using NSwag;

namespace Acnys.Core.AspNet
{
    public class SingleSignOnOptions
    {
        public string SecuritySchemeName { get; set; } = "OAuth2";
        public string SecurityScheme { get; set; } = "http";
        public OpenApiSecuritySchemeType SecuritySchemeType { get; set; } = OpenApiSecuritySchemeType.OAuth2;
        public OpenApiOAuth2Flow OAuthFlow { get; set; } = OpenApiOAuth2Flow.Implicit;
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string Audience { get; set; } = "account";
        public bool RequireHttpsMetadata { get; set; } = true;
        public bool SaveToken { get; set; } = true;
        public bool ShowPII { get; set; } = false;

    }

    
}
using Acnys.Core.SingleSignOn.Constants;

namespace Acnys.Core.SingleSignOn.Configurations
{
    public class JwtBearerConfiguration
    {
        /// <summary>
        /// Gets or sets the Authority to use when making OpenIdConnect calls.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the audience for any received OpenIdConnect token. Default value: “account”.
        /// </summary>
        /// <remarks>
        ///  The “audience” defines who the intended recipient of the token.
        /// </remarks>
        public string Audience { get; set; } = Defaults.AUDIENCE;

        /// <summary>
        /// Gets or sets a String that represents a valid issuer that will be used to check against the token's issuer.
        /// If it is empty, it will be filled according to discovery endpoint.
        /// </summary>
        /// <remarks>
        /// The discovery endpoint is a special endpoint what defined by OpenIDConnect protocol. It contains endpoints of the Single Sign-On server.
        /// Url: “Server”/.well-known/openid-configuration
        /// </remarks>
        public string ValidIssuer { get; set; }

        /// <summary>
        /// Gets or sets if HTTPS is required for the metadata address or authority. The default is true. This should be disabled only in development environments.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// Defines whether the bearer token should be stored in the <see cref="AuthenticationProperties"/> after a successful authorization.
        /// </summary>
        public bool SaveToken { get; set; } = true;
    }
}

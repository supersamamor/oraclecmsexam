using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OracleCMS.Common.Web.Utility.Authorization
{
    /// <summary>
    /// The custom <see cref="AuthorizationPolicy"/> provider.
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private readonly bool _authenticationEnabled;

        /// <summary>
        /// Initializes an instance of <see cref="PermissionPolicyProvider"/>.
        /// </summary>
        /// <param name="options">The authorization options.</param>
        /// <param name="configuration">The application configuration.</param>
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options, IConfiguration configuration)
        {
            // There can only be one policy provider in ASP.NET Core.
            // We only handle permissions-related policies; for the rest, we use the default provider.
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);

            // Determine if authentication is enabled from the configuration.
            _authenticationEnabled = configuration.GetValue<bool>("Authentication:Enabled");
        }

        /// <summary>
        /// Gets the default authorization policy.
        /// </summary>
        /// <returns>The default <see cref="AuthorizationPolicy"/>.</returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            if (!_authenticationEnabled)
            {
                // Return a policy that allows anonymous access when authentication is disabled.
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
                return Task.FromResult(policy);
            }

            // Use the default policy when authentication is enabled.
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        /// <summary>
        /// Gets the authorization policy for the specified policy name.
        /// </summary>
        /// <param name="policyName">The name of the policy.</param>
        /// <returns>The <see cref="AuthorizationPolicy"/>.</returns>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!_authenticationEnabled)
            {
                // Return a policy that allows anonymous access when authentication is disabled.
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            if (policyName.StartsWith("Permission", StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(policyName));
                return Task.FromResult<AuthorizationPolicy?>(policy.Build());
            }

            // For other policies, fall back to the default provider.
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        /// <summary>
        /// Gets the fallback authorization policy.
        /// </summary>
        /// <returns>The fallback <see cref="AuthorizationPolicy"/>.</returns>
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            if (!_authenticationEnabled)
            {
                // Return a policy that allows anonymous access when authentication is disabled.
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // Use the default fallback policy when authentication is enabled.
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }
    }
}

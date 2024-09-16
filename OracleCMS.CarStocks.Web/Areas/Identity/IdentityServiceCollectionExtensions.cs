using OracleCMS.Common.Identity.Abstractions;
using OracleCMS.Common.Web.Utility.Authorization;
using OracleCMS.Common.Web.Utility.Identity;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.CarStocks.Core.Oidc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OracleCMS.CarStocks.Core.Identity;

namespace OracleCMS.CarStocks.Web.Areas.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection ConfigureIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {

        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseInMemoryDatabase("IdentityContext");
                options.UseOpenIddict<OidcApplication, OidcAuthorization, OidcScope, OidcToken, string>();
            });
        }
        else
        {
            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("IdentityContext"));
                options.UseOpenIddict<OidcApplication, OidcAuthorization, OidcScope, OidcToken, string>();
            });
        }

        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

        var microsoftClientId = configuration["Authentication:Microsoft:ClientId"];
        if (!string.IsNullOrEmpty(microsoftClientId))
        {
            services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = microsoftClientId;
                    options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                });
        }

        var googleClientId = configuration["Authentication:Google:ClientId"];
        if (!string.IsNullOrEmpty(googleClientId))
        {
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                });
        }

        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();
        services.AddTransient<IAuthenticatedUser, DefaultAuthenticatedUser>();

        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
        });

        if (configuration.GetValue<bool>("IsIdentityServerEnabled"))
        {
            services.AddOpenIddict()
               .AddCore(options =>
               {
                   options.UseEntityFrameworkCore()
                          .UseDbContext<IdentityContext>()
                          .ReplaceDefaultEntities<OidcApplication, OidcAuthorization, OidcScope, OidcToken, string>();
                   options.UseQuartz();
               })
               .AddServer(options =>
               {
                   // Enable the authorization, device, logout, token, userinfo and verification endpoints.
                   options.SetAuthorizationEndpointUris("/connect/authorize")
                 .SetDeviceEndpointUris("/connect/device")
                 .SetLogoutEndpointUris("/connect/logout")
                 .SetTokenEndpointUris("/connect/token")
                 .SetUserinfoEndpointUris("/connect/userinfo")
                 .SetVerificationEndpointUris("/connect/verify");

                   // Note: this sample uses the code, device code, password and refresh token flows, but you
                   // can enable the other flows if you need to support implicit or client credentials.
                   options.AllowAuthorizationCodeFlow()
                 .AllowDeviceCodeFlow()
                 .AllowClientCredentialsFlow()
                 .AllowPasswordFlow()
                 .AllowRefreshTokenFlow();

                   // Supported scopes
                   options.RegisterScopes(Scopes.Email,
                                 Scopes.Profile,
                                 Scopes.Phone,
                                 Scopes.Roles,
                                 Scopes.OfflineAccess,
                                 CustomClaimTypes.Entity,
                                 AuthorizationClaimTypes.Permission);

                   // Register the signing and encryption credentials.
                   //if (context.HostingEnvironment.EnvironmentName == "Development")
                   //{
                   //    options.AddDevelopmentEncryptionCertificate()
                   //           .AddDevelopmentSigningCertificate();
                   //}

                   options.AddEncryptionCertificate(configuration.GetValue<string>("SslThumbprint"))
                 .AddSigningCertificate(configuration.GetValue<string>("SslThumbprint"));

                   // Force client applications to use Proof Key for Code Exchange (PKCE).
                   options.RequireProofKeyForCodeExchange();

                   // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                   options.UseAspNetCore()
                 .EnableStatusCodePagesIntegration()
                 .EnableAuthorizationEndpointPassthrough()
                 .EnableLogoutEndpointPassthrough()
                 .EnableTokenEndpointPassthrough()
                 .EnableUserinfoEndpointPassthrough()
                 .EnableVerificationEndpointPassthrough();

                   // Note: if you don't want to specify a client_id when sending
                   // a token or revocation request, uncomment the following line:

                   options.AcceptAnonymousClients();

                   // Note: if you want to process authorization and token requests
                   // that specify non-registered scopes, uncomment the following line:

                   options.DisableScopeValidation();

                   // Note: if you don't want to use permissions, you can disable
                   // permission enforcement by uncommenting the following lines:
                   //
                   // options.IgnoreEndpointPermissions()
                   //        .IgnoreGrantTypePermissions()
                   //        .IgnoreResponseTypePermissions()
                   //        .IgnoreScopePermissions();

                   // Note: when issuing access tokens used by third-party APIs
                   // you don't own, you can disable access token encryption:

                   options.DisableAccessTokenEncryption();
               })
               .AddValidation(options =>
               {
                   // Import the configuration from the local OpenIddict server instance.
                   options.UseLocalServer();

                   // Register the ASP.NET Core host.
                   options.UseAspNetCore();
               });
        }
        return services;
    }
}

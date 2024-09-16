using OracleCMS.Common.API.Swagger;
using OracleCMS.Common.Identity.Abstractions;
using OracleCMS.Common.Web.Utility.Authorization;
using OracleCMS.Common.Web.Utility.Identity;
using OracleCMS.Common.Web.Utility.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;
using System.Configuration;
using Microsoft.AspNetCore.Authentication;

namespace OracleCMS.Common.API;

/// <summary>
/// A class containing extensions for enabling common API services.
/// </summary>
public static class CommonApiExtensions
{
    /// <summary>
    /// Enables Swagger API documentation with common options.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication EnableSwagger(this WebApplication app)
    {
        var scope = app.Services.CreateScope();

        app.UseSwagger();
        var provider = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseSwaggerUI(options =>
        {
            options.DisplayRequestDuration();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
				options.RoutePrefix = "";
       
            }
        });
        return app;
    }

    /// <summary>
    /// Registers default API services. Optionally, you can call the other methods in this class
    /// to register the services individually.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddDefaultApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.ConfigureAuthentication(configuration);
        services.ConfigureAuthorization(configuration);
        services.ConfigureVersioning();
        services.ConfigureSwagger();
        services.AddLogEnricherServices();
        services.AddApplicationInsightsTelemetry();
        services.AddTransient<IAuthenticatedUser, DefaultAuthenticatedUser>();
        return services;
    }

    /// <summary>
    /// Registers services related to API versioning with default options.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options => options.ReportApiVersions = true).AddApiExplorer();  
        return services;
    }

    /// <summary>
    /// Registers services related to Swagger documentation with default options defined in <see cref="ConfigureSwaggerOptions"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        return services;
    }

    /// <summary>
    /// Registers services related to authentication.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var enableAuthentication = configuration.GetValue<bool>("Authentication:Enabled");

        if (enableAuthentication)
        {
            // Existing authentication configuration
            var issuer = new Uri(configuration.GetValue<string>("Authentication:Issuer")!);
            var audience = configuration.GetValue<string>("Authentication:Audience")!;

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            services.AddOpenIddict()
                .AddValidation(options =>
                {
                    options.SetIssuer(issuer);
                    options.AddAudiences(audience);
                    options.UseSystemNetHttp();
                    options.UseAspNetCore();
                });
        }
        else
        {
            // Register dummy authentication handler
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "AllowAnonymous";
                options.DefaultChallengeScheme = "AllowAnonymous";
            })
            .AddScheme<AuthenticationSchemeOptions, AllowAnonymousAuthenticationHandler>("AllowAnonymous", null);
        }

        return services;
    }



    /// <summary>
    /// Registers services related to authorization. Uses <see cref="PermissionPolicyProvider"/>
    /// and <see cref="PermissionAuthorizationHandler"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var enableAuthentication = configuration.GetValue<bool>("Authentication:Enabled");

        services.AddAuthorization(options =>
        {
            if (!enableAuthentication)
            {
                // Set default policy to allow anonymous access
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            }
            // Else, use the default policy (which requires authenticated users)
        });

        // Always register the custom policy provider and handler
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }




}

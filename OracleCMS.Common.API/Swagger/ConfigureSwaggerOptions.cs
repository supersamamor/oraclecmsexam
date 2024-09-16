using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;
using System.Configuration;

namespace OracleCMS.Common.API.Swagger;

/// <summary>
/// A class for applying default configuration for generating Swagger documentation.
/// </summary>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration) : IConfigureOptions<SwaggerGenOptions>
{
    readonly IApiVersionDescriptionProvider _provider = provider;
    readonly string _appName = configuration.GetValue<string>("Application")!;

    /// <summary>
    /// Configures the Swagger documentation.
    /// </summary>
    /// <param name="options">Instance of <see cref="SwaggerGenOptions"/></param>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo()
                {
                    Title = $"{_appName} {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                });
        }

        // Check if authentication is enabled
        var enableAuthentication = configuration.GetValue<bool>("Authentication:Enabled");
        if (enableAuthentication)
        {

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Input your access token here to access this API",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "Bearer",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                }, new List<string>()
            }
        });
        }

    }
}

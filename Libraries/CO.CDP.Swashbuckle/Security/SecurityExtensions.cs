using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Swashbuckle.Security;

public static class SecurityExtensions
{
    public static void ConfigureOneLoginSecurity(this SwaggerGenOptions options)
    {
        var oneLoginSecurityScheme = new OpenApiSecurityScheme
        {
            Description = "GOV.UK One Login JWT Bearer token",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "OneLogin"
            }
        };
        options.AddSecurityDefinition("OneLogin", oneLoginSecurityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement { { oneLoginSecurityScheme, [] } });
    }

    public static void ConfigureApiKeySecurity(this SwaggerGenOptions options)
    {
        var apiKeySecurityScheme = new OpenApiSecurityScheme
        {
            Description = "Central Digital Platform API Key",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "CDP-Api-Key",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "ApiKey"
            }
        };
        options.AddSecurityDefinition("ApiKey", apiKeySecurityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement { { apiKeySecurityScheme, [] } });
    }
}
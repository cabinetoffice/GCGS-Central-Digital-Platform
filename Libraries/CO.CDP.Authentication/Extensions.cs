using CO.CDP.Authentication.Authorization;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace CO.CDP.Authentication;

public static class Extensions
{
    public const string JwtBearerOrApiKeyScheme = "JwtBearer_Or_ApiKey";

    public static IServiceCollection AddJwtBearerAndApiKeyAuthentication(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        services
            .AddAuthentication(JwtBearerOrApiKeyScheme)
            .AddJwtBearerAuthentication(configuration, webHostEnvironment)
            .AddApiKeyAuthentication()
            .AddPolicyScheme(JwtBearerOrApiKeyScheme, JwtBearerOrApiKeyScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.ContainsKey(ApiKeyAuthenticationHandler.ApiKeyHeaderName))
                    {
                        return ApiKeyAuthenticationHandler.AuthenticationScheme;
                    }
                    return JwtBearerDefaults.AuthenticationScheme;
                };
            });

        services.AddApiKeyAuthenticationServices();
        services.AddJwtClaimExtractor();

        return services;
    }

    public static AuthenticationBuilder AddJwtBearerAuthentication(this AuthenticationBuilder builder, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        var authority = configuration["Organisation:Authority"]
            ?? throw new Exception("Missing configuration key: Organisation:Authority.");

        return builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = authority;
            options.RequireHttpsMetadata = !webHostEnvironment.IsDevelopment();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuerSigningKey = true
            };
        });
    }

    public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder, Action<ApiKeyAuthenticationOptions>? configureOptions = null)
    {
        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.AuthenticationScheme, configureOptions);
    }

    public static IServiceCollection AddApiKeyAuthenticationServices(this IServiceCollection services)
    {
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
        services.AddScoped<IAuthenticationKeyRepository, DatabaseAuthenticationKeyRepository>();
        return services;
    }

    public static IServiceCollection AddOrganisationAuthorization(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantRepository, DatabaseTenantRepository>();
        services.AddSingleton<IAuthorizationPolicyProvider, OrganisationAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, ChannelAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, OrganisationScopeAuthorizationHandler>();
        services.AddAuthorization();

        return services;
    }

    public static AuthorizationBuilder AddEntityVerificationAuthorization(this IServiceCollection services)
    {
        return services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build());
    }

    public static IServiceCollection AddJwtClaimExtractor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<IClaimService, ClaimService>();
        services.TryAddScoped<ITenantRepository, DatabaseTenantRepository>();
        return services;
    }
}
using CO.CDP.RegisterOfCommercialTools.WebApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace CO.CDP.RegisterOfCommercialTools.WebApi;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAwsCognitoAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        services.Configure<AwsSettings>(configuration.GetSection("AWS"));
        services.Configure<CognitoAuthenticationSettings>(configuration.GetSection("AWS:CognitoAuthentication"));

        var awsSettings = configuration.GetSection("AWS").Get<AwsSettings>();
        var cognitoSettings = configuration.GetSection("AWS:CognitoAuthentication").Get<CognitoAuthenticationSettings>();

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = $"https://cognito-idp.{awsSettings?.Region}.amazonaws.com/{cognitoSettings?.UserPoolId}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://cognito-idp.{awsSettings?.Region}.amazonaws.com/{cognitoSettings?.UserPoolId}",
                    ValidateAudience = true,
                    ValidAudience = cognitoSettings?.UserPoolClientId,
                    ValidateLifetime = true
                };
            });

        return services;
    }
}
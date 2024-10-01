using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CO.CDP.GovUKNotify;

public static class Extensions
{
    public static IServiceCollection AddGovUKNotifyApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(configuration);

        services
            .AddSingleton<IAuthentication, Authentication>()
            .AddTransient<IGovUKNotifyApiClient, GovUKNotifyApiClient>()
            .AddHttpClient(GovUKNotifyApiClient.GovUKNotifyHttpClientName);

        return services;
    }
}
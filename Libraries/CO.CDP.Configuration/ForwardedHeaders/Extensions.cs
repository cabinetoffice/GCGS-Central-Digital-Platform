using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders;

namespace CO.CDP.Configuration.ForwardedHeaders;

public static class Extensions
{
    public static void ConfigureForwardedHeaders(this WebApplicationBuilder builder)
    {
        builder.ConfigureForwardedHeaders(_ => { });
    }

    private static void ConfigureForwardedHeaders(
        this WebApplicationBuilder builder,
        Action<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions> configureOptions)
    {
        ForwardedHeadersOptions config = builder.Configuration.ForwardedHeadersOptions();
        builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = XForwardedFor | XForwardedProto;
            if (!string.IsNullOrEmpty(config.KnownNetwork))
            {
                options.KnownNetworks.Add(IPNetwork.Parse(config.KnownNetwork));
            }
            configureOptions(options);
        });
    }

    private static ForwardedHeadersOptions ForwardedHeadersOptions(this ConfigurationManager configuration)
    {
        return configuration.GetSection(ForwardedHeaders.ForwardedHeadersOptions.Section)
            .Get<ForwardedHeadersOptions>() ?? new ForwardedHeadersOptions();
    }
}
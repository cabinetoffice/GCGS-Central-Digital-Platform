using System.Reflection;
using CO.CDP.Swashbuckle.Security;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi;

public static class SwaggerGenOptionsExtensions
{
    public static void DocumentCommercialToolsApi(this SwaggerGenOptions options, IConfiguration configuration)
    {
        var webApiAssembly = Assembly.GetExecutingAssembly();
        options.SwaggerDoc("v1",
            new OpenApiInfo
            {
                Title = "Register of Commercial Tools API",
                Version = webApiAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ?? "v1"
            });

        options.ConfigureBearerSecurity();
    }
}

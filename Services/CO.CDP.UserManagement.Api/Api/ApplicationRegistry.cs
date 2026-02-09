using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.UserManagement.Api.Api;

public static class ApiExtensions
{
    public static void DocumentApplicationRegistryApi(this SwaggerGenOptions options, IConfigurationManager configuration)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = configuration.GetValue("Version", "dev"),
            Title = "Application Registry API",
            Description = "API for managing application registry, organisations, and user assignments"
        });

        options.IncludeXmlComments(Assembly.GetExecutingAssembly());
        options.OperationFilter<ProblemDetailsOperationFilter>();
        options.ConfigureBearerSecurity();
    }
}

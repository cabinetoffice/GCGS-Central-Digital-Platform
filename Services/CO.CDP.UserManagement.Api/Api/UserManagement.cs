using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace CO.CDP.UserManagement.Api.Api;

public static class ApiExtensions
{
    public static void DocumentUserManagementApi(this SwaggerGenOptions options, IConfigurationManager configuration)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = configuration.GetValue("Version", "dev"),
            Title = "User Management API",
            Description = "API for managing user access, organisations, and application assignments"
        });

        options.IncludeXmlComments(Assembly.GetExecutingAssembly());
        options.OperationFilter<ProblemDetailsOperationFilter>();
        options.ConfigureBearerSecurity();
    }
}

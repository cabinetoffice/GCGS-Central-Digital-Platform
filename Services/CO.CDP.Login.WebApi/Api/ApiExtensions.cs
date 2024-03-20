using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CO.CDP.Login.WebApi.Api;

public static class ApiExtensions
{
    public static void DocumentLoginApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0.0",
            Title = "Login API",
            Description = "",
        });
    }
}
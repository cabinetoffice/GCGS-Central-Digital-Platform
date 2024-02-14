using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Person.Api
{
    public static class EndpointExtensions
    {
        public static void UsePersonEndpoints(this WebApplication app)
        {
        }
    }

    public static class ApiExtensions
    {
        public static void DocumentPersonApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0.0",
                Title = "Person API",
                Description = "",
            });
        }
    }
}
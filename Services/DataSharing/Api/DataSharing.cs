using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataSharing.Api
{
    public static class EndpointExtensions
    {
        public static void UseDataSharingEndpoints(this WebApplication app)
        {
        }
    }

    public static class ApiExtensions
    {
        public static void DocumentDataSharingApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0.0",
                Title = "Data Sharing API",
                Description = "",
            });
        }
    }
}
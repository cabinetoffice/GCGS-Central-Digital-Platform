
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Organisation.Api
{
    public static class EndpointExtensions
    {
        public static void UseOrganisationEndpoints(this WebApplication app)
        {
        }
    }
    
    public static class ApiExtensions
    {
        public static void DocumentOrganisationApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0.0",
                Title = "Organisation API",
                Description = "",
            });
        }
    }
}
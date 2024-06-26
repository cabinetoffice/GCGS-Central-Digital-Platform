using CO.CDP.Swashbuckle.Security;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Forms.WebApi.Api
{
    public static class EndpointExtensions
    {
        public static void UseFormsEndpoints(this WebApplication app)
        {
        }
    }

    public static class ApiExtensions
    {
        public static void DocumentFormsApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0",
                Title = "Forms API",
                Description = "",
            });
            options.ConfigureBearerSecurity();
        }
    }
}
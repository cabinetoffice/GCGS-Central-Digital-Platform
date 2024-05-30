using System.Reflection;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.Swashbuckle.SwaggerGen;

public static class SwaggerGenOptionsExtensions
{
    public static void IncludeXmlComments(this SwaggerGenOptions options, params Assembly?[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (assembly is not null)
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml"));
            }
        }
    }
}
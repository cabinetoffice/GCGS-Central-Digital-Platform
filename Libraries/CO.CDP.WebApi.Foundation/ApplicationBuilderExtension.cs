using Microsoft.AspNetCore.Builder;

namespace CO.CDP.WebApi.Foundation;

public static class ApplicationBuilderExtension
{
    public static void UseErrorHandler(this IApplicationBuilder app, Dictionary<Type, (int, string)> exceptionMap)
    {
        app.UseMiddleware<ResponseMiddleware>(exceptionMap);
    }
}
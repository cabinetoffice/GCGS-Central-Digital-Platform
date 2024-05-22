using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await this._next.Invoke(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case ApiException apiEx when apiEx.StatusCode == 400:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new { error = "Invalid data provided. Please check your inputs and try again." }));

            case ApiException apiEx when apiEx.StatusCode == 401:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new { error = "You are not authorised to perform this action. Please log in and try again." }));

            case ApiException apiEx when apiEx.StatusCode == 404:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new { error = "The requested resource was not found. Please try again." }));

            default:
                context.Response.Redirect("/error");
                return Task.CompletedTask;
        }
    }
}
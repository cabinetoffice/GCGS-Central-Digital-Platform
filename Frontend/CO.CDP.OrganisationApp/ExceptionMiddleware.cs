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
            context.Response.Redirect("/error");
        }
    }
}
using CO.CDP.OrganisationApp.Pages.Forms;
using System.Net;

namespace CO.CDP.OrganisationApp.Middleware;

public class FileUploadMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        string contentType = context.Request.ContentType ?? string.Empty;
        bool isMultipartFormData = contentType.Contains("multipart/form-data;");

        if (context.Response.StatusCode == (int)HttpStatusCode.BadRequest && isMultipartFormData)
        {
            await WriteAsync(context);
        }
    }

    protected virtual async Task WriteAsync(HttpContext context)
    {
        await context.Response.WriteAsync($"The uploaded file is too large. Maximum allowed size is {FormElementFileUploadModel.AllowedMaxFileSizeMB} MB.");
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CO.CDP.AspNetCore;
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred right");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = exception switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            JsonException => StatusCodes.Status422UnprocessableEntity,
            BadHttpRequestException => StatusCodes.Status422UnprocessableEntity,
            _ => (int)HttpStatusCode.InternalServerError,
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == (int)HttpStatusCode.InternalServerError ? "An unexpected error occurred" : exception.Message,
            Detail = _environment.IsDevelopment() ? exception.ToString() : (statusCode == (int)HttpStatusCode.InternalServerError ? "Internal Server Error" : exception.Message),
        };

        response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(problemDetails);
        return response.WriteAsync(json);
    }
}
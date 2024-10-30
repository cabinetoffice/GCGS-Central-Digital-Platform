using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CO.CDP.WebApi.Foundation.Tests;

public class ResponseMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ResponseMiddleware>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
    private readonly Dictionary<Type, (int, string)> _exceptionMap;
    private readonly ResponseMiddleware _middleware;
    private readonly MemoryStream _responseBodyStream;

    public ResponseMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ResponseMiddleware>>();
        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        _exceptionMap = new Dictionary<Type, (int, string)>
            {
                { typeof(ArgumentException), (StatusCodes.Status400BadRequest, "ARGUMENT_ERROR") }
            };

        _responseBodyStream = new MemoryStream();

        _middleware = new ResponseMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _webHostEnvironmentMock.Object,
            _exceptionMap
        );
    }

    [Fact]
    public async Task Invoke_ShouldNotModifyResponse_WhenRequestIsSuccessful()
    {
        var status = StatusCodes.Status200OK;
        var context = CreateContextWithStatus(status);
        _nextMock.Setup(next => next(context)).Returns(Task.CompletedTask);

        await _middleware.Invoke(context);

        _loggerMock.VerifyNoOtherCalls();
        context.Response.StatusCode.Should().Be(status);
        context.Response.ContentLength.Should().BeNull();
    }

    [Fact]
    public async Task Invoke_ShouldLogAndAddProblemDetails_WhenStatusIs4xx()
    {
        var status = StatusCodes.Status400BadRequest;
        var context = CreateContextWithStatus(status);
        _nextMock.Setup(next => next(context)).Returns(Task.CompletedTask);

        await _middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(status);

        var problemDetails = await GetResponseProblemDetailsAsync();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Invoke_ShouldHandleException_WhenUnhandledExceptionThrown(bool developmentEnvironment)
    {
        var status = StatusCodes.Status500InternalServerError;
        var context = CreateContextWithStatus(status);
        var exception = new InvalidOperationException("Unhandled exception");
        _nextMock.Setup(next => next(context)).Throws(exception);
        _webHostEnvironmentMock.Setup(env => env.EnvironmentName).Returns(developmentEnvironment ? "Development" : "Production");

        await _middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(status);
        VerifyLogger(LogLevel.Error, "Unhandled exception", exception);

        var problemDetails = await GetResponseProblemDetailsAsync();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(status);
        problemDetails.Detail.Should().Be(developmentEnvironment ? exception.ToString() : "An unexpected error has occurred");
        ((JsonElement)problemDetails.Extensions["code"]!).GetString().Should().Be("GENERIC_ERROR");
    }

    [Fact]
    public async Task Invoke_ShouldReturnMappedProblemDetails_WhenMappedExceptionOccurs()
    {
        var status = StatusCodes.Status400BadRequest;
        var context = CreateContextWithStatus(status);
        var exception = new ArgumentException("Invalid argument");
        _nextMock.Setup(next => next(context)).Throws(exception);

        await _middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(status);
        VerifyLogger(LogLevel.Information, "Response status: 400, for request: ", exception);

        var problemDetails = await GetResponseProblemDetailsAsync();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(status);
        problemDetails.Detail.Should().Be("Invalid argument");
        ((JsonElement)problemDetails.Extensions["code"]!).GetString().Should().Be("ARGUMENT_ERROR");
    }

    private HttpContext CreateContextWithStatus(int statusCode)
    {
        var context = new DefaultHttpContext();
        context.Response.StatusCode = statusCode;
        context.Response.Body = _responseBodyStream;

        var services = new ServiceCollection();
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();

        problemDetailsServiceMock.Setup(service => service.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Returns(async (ProblemDetailsContext ctx) =>
                {
                    var problemDetailsJson = JsonSerializer.Serialize(ctx.ProblemDetails);
                    var jsonBytes = System.Text.Encoding.UTF8.GetBytes(problemDetailsJson);
                    await context.Response.Body.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                });

        services.AddSingleton(problemDetailsServiceMock.Object);
        context.RequestServices = services.BuildServiceProvider();
        return context;
    }

    private async Task<ProblemDetails> GetResponseProblemDetailsAsync()
    {
        _responseBodyStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_responseBodyStream);
        var responseBody = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(responseBody)!;
    }

    private void VerifyLogger(LogLevel logLevel, string message, Exception? ex = null)
    {
        _loggerMock.Verify(logger => logger.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.Is<EventId>(e => e.Id == 0),
            It.Is<It.IsAnyType>((obj, @type) => obj.ToString() == message && @type.Name == "FormattedLogValues"),
            It.Is<Exception?>(Exception => Exception == ex),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }
}
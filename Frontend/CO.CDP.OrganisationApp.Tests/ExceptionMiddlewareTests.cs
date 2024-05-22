using CO.CDP.Organisation.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CO.CDP.OrganisationApp.Tests;
public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task ShouldReturnBadRequest_WhenApiExceptionWithStatusCode400Occurs()
    {
        var httpContextMock = new DefaultHttpContext();
        var responseMock = new Mock<HttpResponse>();
        var requestDelegateMock = new Mock<RequestDelegate>();

        httpContextMock.Response.Body = new MemoryStream();
        responseMock.SetupGet(r => r.Body).Returns(httpContextMock.Response.Body);
        httpContextMock.Response.ContentType = "application/json";

        var apiException = new ApiException("Bad Request", 400, null, null, null);
        requestDelegateMock.Setup(rd => rd(It.IsAny<HttpContext>())).ThrowsAsync(apiException);

        var middleware = new ExceptionMiddleware(requestDelegateMock.Object);
        await middleware.Invoke(httpContextMock);

        httpContextMock.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        httpContextMock.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(httpContextMock.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("Invalid data provided. Please check your inputs and try again.");
    }

    [Fact]
    public async Task ShouldReturnUnauthorised_WhenApiExceptionWithStatusCode401Occurs()
    {
        var httpContextMock = new DefaultHttpContext();
        var responseMock = new Mock<HttpResponse>();
        var requestDelegateMock = new Mock<RequestDelegate>();

        httpContextMock.Response.Body = new MemoryStream();
        responseMock.SetupGet(r => r.Body).Returns(httpContextMock.Response.Body);
        httpContextMock.Response.ContentType = "application/json";

        var apiException = new ApiException("Unauthorized", 401, null, null, null);
        requestDelegateMock.Setup(rd => rd(It.IsAny<HttpContext>())).ThrowsAsync(apiException);

        var middleware = new ExceptionMiddleware(requestDelegateMock.Object);
        await middleware.Invoke(httpContextMock);

        httpContextMock.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        httpContextMock.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(httpContextMock.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("You are not authorised to perform this action. Please log in and try again.");
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenApiExceptionWithStatusCode404Occurs()
    {
        var httpContextMock = new DefaultHttpContext();
        var responseMock = new Mock<HttpResponse>();
        var requestDelegateMock = new Mock<RequestDelegate>();

        httpContextMock.Response.Body = new MemoryStream();
        responseMock.SetupGet(r => r.Body).Returns(httpContextMock.Response.Body);
        httpContextMock.Response.ContentType = "application/json";

        var apiException = new ApiException("Not Found", 404, null, null, null);
        requestDelegateMock.Setup(rd => rd(It.IsAny<HttpContext>())).ThrowsAsync(apiException);

        var middleware = new ExceptionMiddleware(requestDelegateMock.Object);
        await middleware.Invoke(httpContextMock);

        httpContextMock.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        httpContextMock.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(httpContextMock.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("The requested resource was not found. Please try again.");
    }

}
using CO.CDP.OrganisationApp.Pages.Forms;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests;

public class FileUploadMiddlewareTests
{
    public class FileUploadMiddlewareTest : FileUploadMiddleware
    {
        public FileUploadMiddlewareTest(RequestDelegate next) : base(next)
        {
        }

        protected override async Task WriteAsync(HttpContext context)
        {
            var writer = new StreamWriter(context.Response.Body);

            await writer.WriteAsync($"The uploaded file is too large. Maximum allowed size is {FormElementFileUploadModel.AllowedMaxFileSizeMB} MB.");
            writer.Flush();
        }
    }

    [Fact]
    public async Task InvokeAsync_ShouldWriteResponse_WhenStatusCodeIs400AndContentTypeIsMultipartFormData()
    {
        var responseBody = new MemoryStream();
        var httpContextMock = new Mock<HttpContext>();
        var middleware = SetUp(HttpStatusCode.BadRequest, "multipart/form-data;", httpContextMock, responseBody);
        
        await middleware.InvokeAsync(httpContextMock.Object);
        
        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var responseBodyText = await reader.ReadToEndAsync();
        Assert.Contains("The uploaded file is too large. Maximum allowed size is", responseBodyText);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotWriteResponse_WhenStatusCodeIsNot400()
    {
        var responseBody = new MemoryStream();
        var httpContextMock = new Mock<HttpContext>();
        var middleware = SetUp(HttpStatusCode.OK, "multipart/form-data;", httpContextMock, responseBody);

        await middleware.InvokeAsync(httpContextMock.Object);

        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var responseBodyText = await reader.ReadToEndAsync();
        Assert.Contains(string.Empty, responseBodyText);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotWriteResponse_WhenContentTypeIsNotMultipartFormData()
    {
        var responseBody = new MemoryStream();
        var httpContextMock = new Mock<HttpContext>();
        var middleware = SetUp(HttpStatusCode.BadRequest, "application/json", httpContextMock, responseBody);

        await middleware.InvokeAsync(httpContextMock.Object);

        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var responseBodyText = await reader.ReadToEndAsync();
        Assert.Contains(string.Empty, responseBodyText);
    }

    private FileUploadMiddleware SetUp(HttpStatusCode statusCode,
        string contentType,
        Mock<HttpContext> httpContextMock,
        MemoryStream responseBody)
    {
        var requestMock = new Mock<HttpRequest>();
        var responseMock = new Mock<HttpResponse>();
        var requestDelegateMock = new Mock<RequestDelegate>();
        
        responseMock.Setup(r => r.StatusCode).Returns((int)statusCode);
        requestMock.Setup(h => h.ContentType).Returns(contentType);

        httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);
        httpContextMock.Setup(h => h.Response).Returns(responseMock.Object);
        responseMock.Setup(res => res.Body).Returns(responseBody);

        return new FileUploadMiddlewareTest(requestDelegateMock.Object);
    }
}
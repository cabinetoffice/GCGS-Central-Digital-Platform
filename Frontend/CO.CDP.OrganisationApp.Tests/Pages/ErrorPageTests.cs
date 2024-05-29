using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;
public class ErrorPageTests
{
    [Fact]
    public async Task ShouldRedirectToErrorPage_WhenExceptionOccurs()
    {
        var httpContextMock = new Mock<HttpContext>();
        var responseMock = new Mock<HttpResponse>();
        var requestDelegateMock = new Mock<RequestDelegate>();
        var logger = new Mock<ILogger<ExceptionMiddleware>>();

        responseMock.Setup(r => r.Redirect(It.IsAny<string>())).Verifiable();
        httpContextMock.SetupGet(h => h.Response).Returns(responseMock.Object);

        requestDelegateMock.Setup(rd => rd(It.IsAny<HttpContext>())).ThrowsAsync(new Exception());

        var middleware = new ExceptionMiddleware(requestDelegateMock.Object, logger.Object);
        await middleware.Invoke(httpContextMock.Object);

        responseMock.Verify(r => r.Redirect("/error"), Times.Once);
    }
}

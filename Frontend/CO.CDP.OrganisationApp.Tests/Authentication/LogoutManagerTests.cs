using CO.CDP.OrganisationApp.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests.Authentication;

public class LogoutManagerTests
{
    private readonly Mock<ICacheService> cacheMock = new();
    private readonly Mock<IHttpClientFactory> httpClienMockMock = new();
    private readonly Mock<ILogger<LogoutManager>> loggerMock = new();
    private readonly LogoutManager logoutManager;
    private const string UserUrn = "user123";
    private const string CacheKey = "LoggedOutUser_user123";

    public LogoutManagerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("SessionTimeoutInMinutes", "30"),
                new("OneLogin:ForwardLogoutNotificationUrls", "http://example1.com/logout,http://example2.com/logout"),
                ])
            .Build();

        logoutManager = new LogoutManager(configuration, cacheMock.Object, httpClienMockMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task MarkAsLoggedOut_ShouldSetCache_WithCorrectExpiration()
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        httpClienMockMock.Setup(h => h.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(mockHandler.Object));

        await logoutManager.MarkAsLoggedOut(UserUrn, "token123");

        cacheMock.Verify(c => c.Set(CacheKey, "1",
            It.Is<DistributedCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(30))),
            Times.Once);

        mockHandler.Invocations.Count.Should().Be(2);
    }

    [Fact]
    public async Task RemoveAsLoggedOut_ShouldRemoveCacheEntry()
    {
        await logoutManager.RemoveAsLoggedOut(UserUrn);

        cacheMock.Verify(c => c.Remove(CacheKey), Times.Once);
    }

    [Fact]
    public async Task HasLoggedOut_ShouldReturnTrue_WhenUserIsLoggedOut()
    {
        cacheMock.Setup(c => c.Get<string>(CacheKey)).ReturnsAsync("1");

        var result = await logoutManager.HasLoggedOut(UserUrn);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasLoggedOut_ShouldReturnFalse_WhenUserIsNotLoggedOut()
    {
        var UserUrn = "user123";
        cacheMock.Setup(c => c.Get<string?>(CacheKey)).ReturnsAsync((string?)null);

        var result = await logoutManager.HasLoggedOut(UserUrn);

        result.Should().BeFalse();
    }
}
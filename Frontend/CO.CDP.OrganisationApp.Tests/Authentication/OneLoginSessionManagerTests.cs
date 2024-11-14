using CO.CDP.OrganisationApp.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Authentication;

public class OneLoginSessionManagerTests
{
    private readonly Mock<ICacheService> cacheMock = new();
    private readonly OneLoginSessionManager sessionManager;
    private const string UserUrn = "user123";
    private const string CacheKey = "SignedOutUser_user123";

    public OneLoginSessionManagerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("SessionTimeoutInMinutes", "30")]).Build();

        sessionManager = new OneLoginSessionManager(configuration, cacheMock.Object);
    }

    [Fact]
    public async Task AddToSignedOutSessionsList_ShouldSetCache_WithCorrectExpiration()
    {
        await sessionManager.AddToSignedOutSessionsList(UserUrn);

        cacheMock.Verify(c => c.Set(CacheKey, "1",
            It.Is<DistributedCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(30))),
            Times.Once);
    }

    [Fact]
    public async Task RemoveFromSignedOutSessionsList_ShouldRemoveCacheEntry()
    {
        await sessionManager.RemoveFromSignedOutSessionsList(UserUrn);

        cacheMock.Verify(c => c.Remove(CacheKey), Times.Once);
    }

    [Fact]
    public async Task HasSignedOut_ShouldReturnTrue_WhenUserIsSignedOut()
    {
        cacheMock.Setup(c => c.Get<string>(CacheKey)).ReturnsAsync("1");

        var result = await sessionManager.HasSignedOut(UserUrn);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSignedOut_ShouldReturnFalse_WhenUserIsNotSignedOut()
    {
        var UserUrn = "user123";
        cacheMock.Setup(c => c.Get<string?>(CacheKey)).ReturnsAsync((string?)null);

        var result = await sessionManager.HasSignedOut(UserUrn);

        result.Should().BeFalse();
    }
}
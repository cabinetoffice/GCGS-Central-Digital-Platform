using CO.CDP.OrganisationApp.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;

namespace CO.CDP.OrganisationApp.Tests.Authentication;

public class OneLoginSessionManagerTests
{
    private readonly Mock<IDistributedCache> cacheMock = new();
    private readonly OneLoginSessionManager sessionManager;

    public OneLoginSessionManagerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("SessionTimeoutInMinutes", "30")]).Build();

        sessionManager = new OneLoginSessionManager(configuration, cacheMock.Object);
    }

    [Fact]
    public void AddUserLoggedOut_ShouldSetCache_WithCorrectExpiration()
    {
        var userUrn = "user123";

        sessionManager.AddUserLoggedOut(userUrn);

        cacheMock.Verify(c => c.Set(
            userUrn,
            Encoding.UTF8.GetBytes("1"),
            It.Is<DistributedCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(30))),
            Times.Once);
    }

    [Fact]
    public void RemoveUserLoggedOut_ShouldRemoveCacheEntry()
    {
        var userUrn = "user123";

        sessionManager.RemoveUserLoggedOut(userUrn);

        cacheMock.Verify(c => c.Remove(userUrn), Times.Once);
    }

    [Fact]
    public void IsLoggedOut_ShouldReturnTrue_WhenUserIsLoggedOut()
    {
        var userUrn = "user123";
        cacheMock.Setup(c => c.Get(userUrn)).Returns(Encoding.UTF8.GetBytes("1"));

        var result = sessionManager.IsLoggedOut(userUrn);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsLoggedOut_ShouldReturnFalse_WhenUserIsNotLoggedOut()
    {
        var userUrn = "user123";
        cacheMock.Setup(c => c.Get(userUrn)).Returns((byte[]?)null);

        var result = sessionManager.IsLoggedOut(userUrn);

        result.Should().BeFalse();
    }
}
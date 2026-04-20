using System.Text;
using CO.CDP.UserManagement.App.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Services;

public class ResendCooldownTests
{
    private readonly Mock<ISession> _session = new();

    private void SetupSession(string? storedValue)
    {
        if (storedValue is null)
        {
            var empty = Array.Empty<byte>();
            _session.Setup(s => s.TryGetValue(It.IsAny<string>(), out empty)).Returns(false);
        }
        else
        {
            var bytes = Encoding.UTF8.GetBytes(storedValue);
            _session.Setup(s => s.TryGetValue(It.IsAny<string>(), out bytes)).Returns(true);
        }
    }

    [Fact]
    public void IsAllowed_WhenNoEntry_ReturnsTrue()
    {
        SetupSession(null);

        ResendCooldown.IsAllowed(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_WhenEntryIsRecent_ReturnsFalse()
    {
        SetupSession(DateTimeOffset.UtcNow.AddSeconds(-10).ToString("O"));

        ResendCooldown.IsAllowed(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_WhenEntryIsExpired_ReturnsTrue()
    {
        SetupSession(DateTimeOffset.UtcNow.AddMinutes(-2).ToString("O"));

        ResendCooldown.IsAllowed(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_WhenEntryIsExactlyAtCooldownBoundary_ReturnsTrue()
    {
        // Slightly beyond the 1-minute cooldown: should be allowed
        SetupSession(DateTimeOffset.UtcNow.AddSeconds(-61).ToString("O"));

        ResendCooldown.IsAllowed(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_WhenEntryIsUnparseable_ReturnsTrue()
    {
        SetupSession("not-a-date");

        ResendCooldown.IsAllowed(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().BeTrue();
    }

    [Fact]
    public void Record_SetsStringOnSession()
    {
        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        var inviteGuid = Guid.NewGuid();
        ResendCooldown.Record(_session.Object, inviteGuid);

        _session.Verify(
            s => s.Set($"ResendCooldown:{inviteGuid}", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void GetRemainingSeconds_WhenNoEntry_ReturnsZero()
    {
        SetupSession(null);

        ResendCooldown.GetRemainingSeconds(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().Be(0);
    }

    [Fact]
    public void GetRemainingSeconds_WhenRecentEntry_ReturnsPositiveValue()
    {
        SetupSession(DateTimeOffset.UtcNow.AddSeconds(-10).ToString("O"));

        var remaining = ResendCooldown.GetRemainingSeconds(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1));

        remaining.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(60);
    }

    [Fact]
    public void GetRemainingSeconds_WhenExpiredEntry_ReturnsZero()
    {
        SetupSession(DateTimeOffset.UtcNow.AddMinutes(-2).ToString("O"));

        ResendCooldown.GetRemainingSeconds(_session.Object, Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().Be(0);
    }
}
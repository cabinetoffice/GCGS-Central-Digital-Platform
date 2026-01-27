using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using ISession = Microsoft.AspNetCore.Http.ISession;
using CO.CDP.UI.Foundation.Services;

namespace CO.CDP.UI.Foundation.Tests;

public class SessionTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly MockSession _mockSession;
    private readonly AppSessionService _session;

    public SessionTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextMock = new Mock<HttpContext>();
        _mockSession = new MockSession();

        _httpContextMock.Setup(c => c.Session).Returns(_mockSession);
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContextMock.Object);

        _session = new AppSessionService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void Get_ReturnsDeserializedValue_WhenKeyExists()
    {
        var userDetails = new UserDetails
        {
            UserUrn = "urn:test:user:123",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };
        _mockSession.SetString("TestKey", JsonSerializer.Serialize(userDetails));

        var result = _session.Get<UserDetails>("TestKey");

        result.Should().NotBeNull();
        result!.UserUrn.Should().Be("urn:test:user:123");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Get_ReturnsDefault_WhenKeyDoesNotExist()
    {
        var result = _session.Get<UserDetails>("NonExistentKey");

        result.Should().BeNull();
    }

    [Fact]
    public void Get_ReturnsDefault_ForValueType_WhenKeyDoesNotExist()
    {
        var result = _session.Get<int>("NonExistentKey");

        result.Should().Be(default);
    }

    [Fact]
    public void Set_SerializesAndStoresValue()
    {
        var userDetails = new UserDetails
        {
            UserUrn = "urn:test:user:456",
            FirstName = "Jane"
        };

        _session.Set("TestKey", userDetails);

        var storedValue = _mockSession.GetString("TestKey");
        storedValue.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<UserDetails>(storedValue!);
        deserialized.Should().NotBeNull();
        deserialized!.UserUrn.Should().Be("urn:test:user:456");
        deserialized.FirstName.Should().Be("Jane");
    }

    [Fact]
    public void Set_OverwritesExistingValue()
    {
        var original = new UserDetails { UserUrn = "original" };
        var updated = new UserDetails { UserUrn = "updated" };

        _session.Set("TestKey", original);
        _session.Set("TestKey", updated);

        var result = _session.Get<UserDetails>("TestKey");
        result.Should().NotBeNull();
        result!.UserUrn.Should().Be("updated");
    }

    [Fact]
    public void Remove_RemovesKey()
    {
        var userDetails = new UserDetails { UserUrn = "test" };
        _session.Set("TestKey", userDetails);

        _session.Remove("TestKey");

        var result = _session.Get<UserDetails>("TestKey");
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_DoesNotThrow_WhenKeyDoesNotExist()
    {
        var act = () => _session.Remove("NonExistentKey");

        act.Should().NotThrow();
    }

    [Fact]
    public void Clear_RemovesAllKeys()
    {
        _session.Set("Key1", new UserDetails { UserUrn = "user1" });
        _session.Set("Key2", new UserDetails { UserUrn = "user2" });

        _session.Clear();

        _session.Get<UserDetails>("Key1").Should().BeNull();
        _session.Get<UserDetails>("Key2").Should().BeNull();
    }

    [Fact]
    public void Get_ThrowsInvalidOperationException_WhenHttpContextIsNull()
    {
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var session = new AppSessionService(_httpContextAccessorMock.Object);

        var act = () => session.Get<UserDetails>("TestKey");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Session is not available*");
    }

    [Fact]
    public void Get_ThrowsInvalidOperationException_WhenSessionIsNull()
    {
        _httpContextMock.Setup(c => c.Session).Returns((ISession)null!);

        var act = () => _session.Get<UserDetails>("TestKey");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Session is not available*");
    }

    private class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id => "test-session-id";
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
        {
            if (_store.TryGetValue(key, out var result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        public string? GetString(string key)
        {
            return TryGetValue(key, out var value) ? Encoding.UTF8.GetString(value) : null;
        }

        public void SetString(string key, string value)
        {
            Set(key, Encoding.UTF8.GetBytes(value));
        }
    }
}

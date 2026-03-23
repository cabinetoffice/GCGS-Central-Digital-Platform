using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using CO.CDP.Authentication.Models;
using CO.CDP.Authentication.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OrgAuthTokens = CO.CDP.OrganisationApp.Models.AuthTokens;
using ISession = Microsoft.AspNetCore.Http.ISession;

namespace CO.CDP.Authentication.Tests;

public class SessionServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly DefaultHttpContext _httpContext = new();
    private readonly MockSession _session = new();
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _httpContext.Session = _session;
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);
        _sut = new SessionService(_httpContextAccessorMock.Object, Mock.Of<ILogger<SessionService>>());
    }

    [Fact]
    public async Task GetTokensAsync_ReadsOrganisationAppTokensFromSharedUserAuthTokensKey()
    {
        var sourceTokens = new OrgAuthTokens
        {
            AccessToken = "access-token",
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(5),
            RefreshToken = "refresh-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddHours(1)
        };
        _session.SetString(SessionService.AuthorityTokensKey, JsonSerializer.Serialize(sourceTokens));

        var result = await _sut.GetTokensAsync(_httpContext);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be(sourceTokens.AccessToken);
        result.RefreshToken.Should().Be(sourceTokens.RefreshToken);
        result.AccessTokenExpiry.Should().BeCloseTo(sourceTokens.AccessTokenExpiry, TimeSpan.FromSeconds(1));
        result.RefreshTokenExpiry.Should().BeCloseTo(sourceTokens.RefreshTokenExpiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SetTokensAsync_StoresTokensUnderSharedUserAuthTokensKey()
    {
        var tokens = AuthorityTokenSet.Create(
            "access-token",
            "refresh-token",
            3600,
            7200,
            DateTimeOffset.UtcNow);

        await _sut.SetTokensAsync(_httpContext, tokens);

        _session.GetString("UserAuthTokens").Should().NotBeNull();
        _session.GetString("AuthorityTokens").Should().BeNull();
    }

    [Fact]
    public async Task RemoveTokensAsync_RemovesTokensFromSharedUserAuthTokensKey()
    {
        _session.SetString(SessionService.AuthorityTokensKey, "{\"accessToken\":\"x\"}");

        await _sut.RemoveTokensAsync(_httpContext);

        _session.GetString(SessionService.AuthorityTokensKey).Should().BeNull();
    }

    private sealed class MockSession : ISession
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

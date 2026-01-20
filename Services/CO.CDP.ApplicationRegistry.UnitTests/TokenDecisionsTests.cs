using CO.CDP.ApplicationRegistry.Core.Tokens;
using FluentAssertions;

namespace CO.CDP.ApplicationRegistry.UnitTests;

public class TokenDecisionsTests
{
    private static readonly DateTimeOffset CurrentTime = new(2024, 1, 15, 12, 0, 0, TimeSpan.Zero);

    #region DetermineAction Tests

    [Fact]
    public void DetermineAction_NoTokens_OneLoginExpired_ReturnsOneLoginExpired()
    {
        var expiredAt = CurrentTime.AddMinutes(-10);

        var result = TokenDecisions.DetermineAction(
            currentTokens: null,
            oneLoginExpiresAt: expiredAt,
            oneLoginAccessToken: "valid-token",
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.OneLoginExpired>();
        var expired = (TokenAction.OneLoginExpired)result;
        expired.ExpiryTime.Should().Be(expiredAt);
    }

    [Fact]
    public void DetermineAction_NoTokens_NoOneLoginAccessToken_ReturnsUserLoggedOut()
    {
        var result = TokenDecisions.DetermineAction(
            currentTokens: null,
            oneLoginExpiresAt: CurrentTime.AddMinutes(10),
            oneLoginAccessToken: null,
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.UserLoggedOut>();
    }

    [Fact]
    public void DetermineAction_NoTokens_EmptyOneLoginAccessToken_ReturnsUserLoggedOut()
    {
        var result = TokenDecisions.DetermineAction(
            currentTokens: null,
            oneLoginExpiresAt: CurrentTime.AddMinutes(10),
            oneLoginAccessToken: "   ",
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.UserLoggedOut>();
    }

    [Fact]
    public void DetermineAction_NoTokens_ValidOneLoginToken_ReturnsFetchNew()
    {
        var oneLoginToken = "valid-one-login-token";

        var result = TokenDecisions.DetermineAction(
            currentTokens: null,
            oneLoginExpiresAt: CurrentTime.AddMinutes(10),
            oneLoginAccessToken: oneLoginToken,
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.FetchNew>();
        var fetchNew = (TokenAction.FetchNew)result;
        fetchNew.OneLoginAccessToken.Should().Be(oneLoginToken);
    }

    [Fact]
    public void DetermineAction_RefreshTokenExpired_NoOneLogin_ReturnsUserLoggedOut()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = CurrentTime.AddMinutes(-5),
            RefreshToken = "refresh",
            RefreshTokenExpiry = CurrentTime.AddMinutes(-1)
        };

        var result = TokenDecisions.DetermineAction(
            currentTokens: tokens,
            oneLoginExpiresAt: null,
            oneLoginAccessToken: null,
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.UserLoggedOut>();
    }

    [Fact]
    public void DetermineAction_RefreshTokenExpired_ValidOneLogin_ReturnsFetchNew()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = CurrentTime.AddMinutes(-5),
            RefreshToken = "refresh",
            RefreshTokenExpiry = CurrentTime.AddMinutes(-1)
        };
        var oneLoginToken = "valid-one-login-token";

        var result = TokenDecisions.DetermineAction(
            currentTokens: tokens,
            oneLoginExpiresAt: CurrentTime.AddMinutes(10),
            oneLoginAccessToken: oneLoginToken,
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.FetchNew>();
        var fetchNew = (TokenAction.FetchNew)result;
        fetchNew.OneLoginAccessToken.Should().Be(oneLoginToken);
    }

    [Fact]
    public void DetermineAction_AccessTokenExpired_RefreshValid_ReturnsRefreshTokens()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = CurrentTime.AddMinutes(-5),
            RefreshToken = "refresh-token",
            RefreshTokenExpiry = CurrentTime.AddMinutes(30)
        };

        var result = TokenDecisions.DetermineAction(
            currentTokens: tokens,
            oneLoginExpiresAt: CurrentTime.AddMinutes(10),
            oneLoginAccessToken: "one-login-token",
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.RefreshTokens>();
        var refresh = (TokenAction.RefreshTokens)result;
        refresh.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public void DetermineAction_AllTokensValid_ReturnsUseExisting()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access-token",
            AccessTokenExpiry = CurrentTime.AddMinutes(30),
            RefreshToken = "refresh-token",
            RefreshTokenExpiry = CurrentTime.AddHours(1)
        };

        var result = TokenDecisions.DetermineAction(
            currentTokens: tokens,
            oneLoginExpiresAt: CurrentTime.AddMinutes(10),
            oneLoginAccessToken: "one-login-token",
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.UseExisting>();
        var useExisting = (TokenAction.UseExisting)result;
        useExisting.Tokens.Should().Be(tokens);
    }

    [Fact]
    public void DetermineAction_NoTokens_NoOneLoginExpiry_ValidAccessToken_ReturnsFetchNew()
    {
        var oneLoginToken = "valid-one-login-token";

        var result = TokenDecisions.DetermineAction(
            currentTokens: null,
            oneLoginExpiresAt: null,
            oneLoginAccessToken: oneLoginToken,
            currentTime: CurrentTime);

        result.Should().BeOfType<TokenAction.FetchNew>();
        var fetchNew = (TokenAction.FetchNew)result;
        fetchNew.OneLoginAccessToken.Should().Be(oneLoginToken);
    }

    #endregion

    #region ParseOneLoginExpiry Tests

    [Fact]
    public void ParseOneLoginExpiry_NullString_ReturnsNull()
    {
        var result = TokenDecisions.ParseOneLoginExpiry(null);

        result.Should().BeNull();
    }

    [Fact]
    public void ParseOneLoginExpiry_EmptyString_ReturnsNull()
    {
        var result = TokenDecisions.ParseOneLoginExpiry(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void ParseOneLoginExpiry_InvalidFormat_ReturnsNull()
    {
        var result = TokenDecisions.ParseOneLoginExpiry("not-a-date");

        result.Should().BeNull();
    }

    [Fact]
    public void ParseOneLoginExpiry_ValidIsoDate_ReturnsParsedDateTimeOffset()
    {
        var result = TokenDecisions.ParseOneLoginExpiry("2024-01-15T12:00:00Z");

        result.Should().NotBeNull();
        result!.Value.UtcDateTime.Should().Be(new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ParseOneLoginExpiry_ValidIsoDateWithOffset_ReturnsParsedDateTimeOffset()
    {
        var result = TokenDecisions.ParseOneLoginExpiry("2024-01-15T12:00:00+01:00");

        result.Should().NotBeNull();
        result!.Value.UtcDateTime.Should().Be(new DateTime(2024, 1, 15, 11, 0, 0, DateTimeKind.Utc));
    }

    #endregion

    #region CreateAuthTokens Tests

    [Fact]
    public void CreateAuthTokens_WithDefaultBuffer_CreatesCorrectExpiry()
    {
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var expiresIn = 3600.0; // 1 hour
        var refreshExpiresIn = 86400.0; // 24 hours

        var result = TokenDecisions.CreateAuthTokens(
            accessToken,
            refreshToken,
            expiresIn,
            refreshExpiresIn,
            CurrentTime);

        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.AccessTokenExpiry.Should().Be(CurrentTime.AddSeconds(3600 - 30)); // Default 30s buffer
        result.RefreshTokenExpiry.Should().Be(CurrentTime.AddSeconds(86400 - 30));
    }

    [Fact]
    public void CreateAuthTokens_WithCustomBuffer_AppliesBufferCorrectly()
    {
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var expiresIn = 3600.0;
        var refreshExpiresIn = 86400.0;
        var customBuffer = 60;

        var result = TokenDecisions.CreateAuthTokens(
            accessToken,
            refreshToken,
            expiresIn,
            refreshExpiresIn,
            CurrentTime,
            customBuffer);

        result.AccessTokenExpiry.Should().Be(CurrentTime.AddSeconds(3600 - 60));
        result.RefreshTokenExpiry.Should().Be(CurrentTime.AddSeconds(86400 - 60));
    }

    [Fact]
    public void CreateAuthTokens_PreservesTokenValues()
    {
        var accessToken = "my-access-token";
        var refreshToken = "my-refresh-token";

        var result = TokenDecisions.CreateAuthTokens(
            accessToken,
            refreshToken,
            3600.0,
            86400.0,
            CurrentTime);

        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
    }

    #endregion

    #region ShouldRevokeRefreshToken Tests

    [Fact]
    public void ShouldRevokeRefreshToken_NullTokens_ReturnsFalse()
    {
        var result = TokenDecisions.ShouldRevokeRefreshToken(null, CurrentTime);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldRevokeRefreshToken_ExpiredRefresh_ReturnsFalse()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = CurrentTime.AddMinutes(-10),
            RefreshToken = "refresh",
            RefreshTokenExpiry = CurrentTime.AddMinutes(-5)
        };

        var result = TokenDecisions.ShouldRevokeRefreshToken(tokens, CurrentTime);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldRevokeRefreshToken_ValidRefresh_ReturnsTrue()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = CurrentTime.AddMinutes(-10),
            RefreshToken = "refresh",
            RefreshTokenExpiry = CurrentTime.AddMinutes(30)
        };

        var result = TokenDecisions.ShouldRevokeRefreshToken(tokens, CurrentTime);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldRevokeRefreshToken_RefreshExpiresAtCurrentTime_ReturnsFalse()
    {
        var tokens = new AuthTokens
        {
            AccessToken = "access",
            AccessTokenExpiry = CurrentTime.AddMinutes(-10),
            RefreshToken = "refresh",
            RefreshTokenExpiry = CurrentTime
        };

        var result = TokenDecisions.ShouldRevokeRefreshToken(tokens, CurrentTime);

        result.Should().BeFalse();
    }

    #endregion
}

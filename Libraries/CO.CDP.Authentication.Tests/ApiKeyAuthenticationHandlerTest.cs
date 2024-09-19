using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CO.CDP.Authentication.Tests;

public class ApiKeyAuthenticationHandlerTest
{
    private const string ValidApiKey = "valid-api-key";
    private const string InvalidApiKey = "invalid-api-key";

    private readonly Mock<IApiKeyValidator> _apiKeyValidatorMock;
    private readonly ApiKeyAuthenticationHandler _handler;
    private readonly HttpContext _httpContext;

    public ApiKeyAuthenticationHandlerTest()
    {
        _apiKeyValidatorMock = new Mock<IApiKeyValidator>();
        var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        _httpContext = new DefaultHttpContext();
        _handler = new ApiKeyAuthenticationHandler(options.Object, loggerFactory, UrlEncoder.Default, _apiKeyValidatorMock.Object);
    }

    private async Task SetHandlerAndApiKeyHeader(string? apiKey)
    {
        _httpContext.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = apiKey;
        await _handler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationHandler.AuthenticationScheme, null, typeof(ApiKeyAuthenticationHandler)), _httpContext);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task AuthenticateAsync_NoOrEmptyApiKey_ReturnsNoResult(string? apiKey)
    {
        await SetHandlerAndApiKeyHeader(apiKey);

        var result = await _handler.AuthenticateAsync();

        result.Succeeded.Should().BeFalse();
        result.None.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidApiKey_ReturnsFail()
    {
        await SetHandlerAndApiKeyHeader(InvalidApiKey);
        _apiKeyValidatorMock.Setup(v => v.Validate(InvalidApiKey))
            .ReturnsAsync((false, null, []));

        var result = await _handler.AuthenticateAsync();

        result.Succeeded.Should().BeFalse();

        result.Failure.Should().NotBeNull()
            .And.BeOfType<AuthenticationFailureException>()
            .Which.Message.Should().Be("Invalid API Key provided.");
    }

    [Fact]
    public async Task AuthenticateAsync_ValidApiKey_ReturnsSuccessWithOrganisationKeyClaimType()
    {
        var orgId = Guid.NewGuid();
        await SetHandlerAndApiKeyHeader(ValidApiKey);
        _apiKeyValidatorMock.Setup(v => v.Validate(ValidApiKey))
            .ReturnsAsync((true, orgId, ["ADMIN", "MANAGER"]));

        var result = await _handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();

        var claims = result.Principal?.Claims;
        claims.Should().NotBeNull();
        claims.Should().Contain(c => c.Type == "channel" && c.Value == "organisation-key");
        claims.Should()
            .Contain(c => c.Type == "org" && c.Value == orgId.ToString());
        claims.Should().Contain(c => c.Type == "scope" && c.Value == "ADMIN MANAGER");
    }

    [Fact]
    public async Task AuthenticateAsync_ValidApiKey_ReturnsSuccessWithServiceKeyClaimType()
    {
        await SetHandlerAndApiKeyHeader(ValidApiKey);
        _apiKeyValidatorMock.Setup(v => v.Validate(ValidApiKey))
            .ReturnsAsync((true, null, []));

        var result = await _handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();

        result.Principal.Should().NotBeNull()
            .And.BeOfType<ClaimsPrincipal>()
            .Which.Identity.Should().BeAssignableTo<ClaimsIdentity>()
            .And.BeOfType<ClaimsIdentity>().Which.Claims.Should().ContainSingle(c => c.Type == "channel" && c.Value == "service-key");
    }
}
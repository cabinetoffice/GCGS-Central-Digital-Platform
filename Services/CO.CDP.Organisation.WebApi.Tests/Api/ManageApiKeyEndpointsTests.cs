using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;

public class ManageApiKeyEndpointsTests
{
    private readonly Mock<IUseCase<Guid, IEnumerable<AuthenticationKey>>> _getAuthenticationKeyUseCase = new();
    private readonly Mock<IUseCase<(Guid, RegisterAuthenticationKey), bool>> _registerAuthenticationKeyUseCase = new();
    private readonly Mock<IUseCase<(Guid, string), bool>> _revokeAuthenticationKeyUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetAuthenticationKeys_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        _getAuthenticationKeyUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync([]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getAuthenticationKeyUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/api-keys");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(Created, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Created, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task CreateAuthenticationKey_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var registerAuthenticationKey = new RegisterAuthenticationKey { Name = "key-1", Key = "secret-key" };
        var command = (organisationId, registerAuthenticationKey);

        _registerAuthenticationKeyUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _registerAuthenticationKeyUseCase.Object));

        var response = await factory.CreateClient().PostAsJsonAsync($"/organisations/{organisationId}/api-keys", registerAuthenticationKey);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]    
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task RevokeAuthenticationKey_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var key = "key-1";
        var command = (organisationId, key);

        _revokeAuthenticationKeyUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _revokeAuthenticationKeyUseCase.Object));

        var response = await factory.CreateClient().PatchAsync($"/organisations/{organisationId}/api-keys/{key}/revoke", null);

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}

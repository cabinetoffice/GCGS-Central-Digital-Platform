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

public class OrganisationPartiesEndpointsTests
{
    private readonly Mock<IUseCase<Guid, OrganisationParties?>> _getOrganisationPartiesUseCase = new();
    private readonly Mock<IUseCase<(Guid, AddOrganisationParty), bool>> _addOrganisationPartyUseCase = new();
    private readonly Mock<IUseCase<(Guid, RemoveOrganisationParty), bool>> _removeOrganisationPartyUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(OK, Channel.OneLogin, null, PersonScope.SupportAdmin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationParties_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null, string? personScope = null)
    {
        var organisationId = Guid.NewGuid();

        _getOrganisationPartiesUseCase.Setup(uc => uc.Execute(organisationId))
            .ReturnsAsync(new OrganisationParties { Parties = [] });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getOrganisationPartiesUseCase.Object), personScope);

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/parties");

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
    public async Task AddOrganisationParty_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var organisationParty = new AddOrganisationParty
        {
            OrganisationPartyId = Guid.NewGuid(),
            OrganisationRelationship = OrganisationRelationship.Consortium,
            ShareCode = "Test"
        };
        var command = (organisationId, organisationParty);

        _addOrganisationPartyUseCase.Setup(uc => uc.Execute(command))
            .ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _addOrganisationPartyUseCase.Object));

        var response = await factory.CreateClient().PostAsJsonAsync($"/organisations/{organisationId}/add-party", organisationParty);

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
    public async Task RemoveOrganisationParty_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var organisationParty = new RemoveOrganisationParty
        {
            OrganisationPartyId = Guid.NewGuid()
        };
        var command = (organisationId, organisationParty);

        _removeOrganisationPartyUseCase.Setup(uc => uc.Execute(command))
            .ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _removeOrganisationPartyUseCase.Object));

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"/organisations/{organisationId}/remove-party", UriKind.RelativeOrAbsolute),
            Content = JsonContent.Create(organisationParty)
        };

        var response = await factory.CreateClient().SendAsync(request);

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;

public class PersonInvitesEndpointsTests
{
    private readonly Mock<IUseCase<Guid, IEnumerable<PersonInviteModel>>> _getPersonInvitesUseCase = new();
    private readonly Mock<IUseCase<(Guid, InvitePersonToOrganisation), PersonInvite>> _invitePersonToOrganisationUseCase = new();
    private readonly Mock<IUseCase<(Guid, Guid, UpdateInvitedPersonToOrganisation), bool>> _updateInvitedPersonToOrganisationUseCase = new();
    private readonly Mock<IUseCase<(Guid, Guid), bool>> _removePersonInviteFromOrganisationUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationPersonInvites_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        _getPersonInvitesUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync([]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _getPersonInvitesUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/invites");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task CreatePersonInvite_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var invitePersonToOrganisation = new InvitePersonToOrganisation
        {
            FirstName = "fn",
            LastName = "ln",
            Email = "fn.ln@test",
            Scopes = []
        };
        var command = (organisationId, invitePersonToOrganisation);

        _invitePersonToOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(Mock.Of<PersonInvite>());

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _invitePersonToOrganisationUseCase.Object));

        var response = await factory.CreateClient().PostAsJsonAsync($"/organisations/{organisationId}/invites", invitePersonToOrganisation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task UpdatePersonInvite_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var personInviteId = Guid.NewGuid();
        var updateInvitedPersonToOrganisation = new UpdateInvitedPersonToOrganisation { Scopes = [] };
        var command = (organisationId, personInviteId, updateInvitedPersonToOrganisation);

        _updateInvitedPersonToOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _updateInvitedPersonToOrganisationUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/organisations/{organisationId}/invites/{personInviteId}", updateInvitedPersonToOrganisation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task RemovePersonInviteFromOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var personInviteId = Guid.NewGuid();
        var command = (organisationId, personInviteId);

        _removePersonInviteFromOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _removePersonInviteFromOrganisationUseCase.Object));

        var response = await factory.CreateClient().DeleteAsync($"/organisations/{organisationId}/invites/{personInviteId}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
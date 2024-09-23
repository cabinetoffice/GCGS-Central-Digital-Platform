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

public class PersonsEndpointsTests
{
    private readonly Mock<IUseCase<Guid, IEnumerable<Person>>> _getPersonsUseCase = new();
    private readonly Mock<IUseCase<(Guid, Guid, UpdatePersonToOrganisation), bool>> _updatePersonToOrganisationUseCase = new();
    private readonly Mock<IUseCase<(Guid, RemovePersonFromOrganisation), bool>> _removePersonFromOrganisationUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationPersons_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        _getPersonsUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync([]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getPersonsUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/persons");

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
    public async Task UpdateOrganisationPerson_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var persoinId = Guid.NewGuid();
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = [] };
        var command = (organisationId, persoinId, updatePersonToOrganisation);

        _updatePersonToOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _updatePersonToOrganisationUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/organisations/{organisationId}/persons/{persoinId}", updatePersonToOrganisation);

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
    public async Task RemovePersonFromOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var removePersonFromOrganisation = new RemovePersonFromOrganisation { PersonId = Guid.NewGuid() };
        var command = (organisationId, removePersonFromOrganisation);

        _removePersonFromOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _removePersonFromOrganisationUseCase.Object));

        var response = await factory.CreateClient().SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/organisations/{organisationId}/persons")
            {
                Content = JsonContent.Create(removePersonFromOrganisation)
            });

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
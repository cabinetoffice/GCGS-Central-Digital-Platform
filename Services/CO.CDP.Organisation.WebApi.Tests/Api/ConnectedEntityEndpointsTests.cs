using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
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

public class ConnectedEntityEndpointsTests
{
    private readonly Mock<IUseCase<Guid, IEnumerable<ConnectedEntityLookup>>> _getConnectedEntitiesUseCase = new();
    private readonly Mock<IUseCase<(Guid, Guid), ConnectedEntity?>> _getConnectedEntityUseCase = new();
    private readonly Mock<IUseCase<(Guid organisationId, RegisterConnectedEntity updateConnectedEntity), bool>> _registerConnectedEntityUseCase = new();
    private readonly Mock<IUseCase<(Guid, Guid, UpdateConnectedEntity), bool>> _updateConnectedEntityUseCase = new();
    private readonly Mock<IUseCase<(Guid, Guid, DeleteConnectedEntity), bool>> _deleteConnectedEntityUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetConnectedEntities_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        _getConnectedEntitiesUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync([]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _getConnectedEntitiesUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/connected-entities");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetConnectedEntity_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var command = (organisationId, connectedEntityId);

        _getConnectedEntityUseCase.Setup(uc => uc.Execute(command))
            .ReturnsAsync(new ConnectedEntity { EntityType = ConnectedEntityType.Organisation });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _getConnectedEntityUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/connected-entities/{connectedEntityId}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task CreateConnectedEntity_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var updateConnectedEntity = new RegisterConnectedEntity { EntityType = ConnectedEntityType.Organisation };
        var command = (organisationId, updateConnectedEntity);

        _registerConnectedEntityUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _registerConnectedEntityUseCase.Object));

        var response = await factory.CreateClient().PostAsJsonAsync($"/organisations/{organisationId}/connected-entities", updateConnectedEntity);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task UpdateConnectedEntity_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var updateConnectedEntity = new UpdateConnectedEntity { EntityType = ConnectedEntityType.Organisation };
        var command = (organisationId, connectedEntityId, updateConnectedEntity);

        _updateConnectedEntityUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _updateConnectedEntityUseCase.Object));

        var response = await factory.CreateClient().PutAsJsonAsync($"/organisations/{organisationId}/connected-entities/{connectedEntityId}", updateConnectedEntity);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task DeleteConnectedEntity_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var deleteConnectedEntity = new DeleteConnectedEntity { EndDate = DateTimeOffset.Now };
        var command = (organisationId, connectedEntityId, deleteConnectedEntity);

        _deleteConnectedEntityUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            [new Claim(ClaimType.Channel, channel)],
            organisationId,
            scope,
            services => services.AddScoped(_ => _deleteConnectedEntityUseCase.Object));

        var response = await factory.CreateClient().SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/organisations/{organisationId}/connected-entities/{connectedEntityId}")
            {
                Content = JsonContent.Create(deleteConnectedEntity)
            });

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;

public class OrganisationPartiesEndpointsTests
{
    private readonly Mock<IUseCase<Guid, OrganisationParties?>> _getOrganisationPartiesUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationParties_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        _getOrganisationPartiesUseCase.Setup(uc => uc.Execute(organisationId))
            .ReturnsAsync(new OrganisationParties { Parties = [] });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getOrganisationPartiesUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/parties");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}

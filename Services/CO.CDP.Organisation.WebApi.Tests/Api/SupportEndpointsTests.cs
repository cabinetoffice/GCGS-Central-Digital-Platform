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

public class SupportEndpointsTests
{
    private readonly Mock<IUseCase<(Guid, SupportUpdateOrganisation), bool>> _supportUpdateOrganisationUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task SupportUpdateOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        var supportUpdateOrg = new SupportUpdateOrganisation
        {
            Type = SupportOrganisationUpdateType.Review,
            Organisation = null!
        };

        var tuple = (organisationId, supportUpdateOrg);

        _supportUpdateOrganisationUseCase.Setup(useCase => useCase.Execute(tuple)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _supportUpdateOrganisationUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/support/organisation/{organisationId}", supportUpdateOrg);

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
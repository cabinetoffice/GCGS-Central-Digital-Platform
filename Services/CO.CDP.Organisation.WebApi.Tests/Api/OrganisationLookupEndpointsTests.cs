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

public class OrganisationLookupEndpointsTests
{
    private readonly Mock<IUseCase<Model.Organisation?>> _getMyOrganisationUseCase = new();
    private readonly Mock<IUseCase<OrganisationQuery, Model.Organisation?>> _lookupOrganisationUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OrganisationKey)]
    [InlineData(Forbidden, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task MyOrganisation_Authorization_ReturnsExpectedStatusCode(HttpStatusCode expectedStatusCode, string channel)
    {
        _getMyOrganisationUseCase.Setup(useCase => useCase.Execute())
                                    .ReturnsAsync(OrganisationEndpointsTests.GivenOrganisation(Guid.NewGuid()));

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _getMyOrganisationUseCase.Object));
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/organisation/me");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin)]
    [InlineData(OK, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task LookupOrganisation_Authorization_ReturnsExpectedStatusCode(HttpStatusCode expectedStatusCode, string channel)
    {
        var name = "test";
        var command = new OrganisationQuery(name);
        _lookupOrganisationUseCase.Setup(useCase => useCase.Execute(command))
                                    .ReturnsAsync(OrganisationEndpointsTests.GivenOrganisation(Guid.NewGuid()));

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel,
            serviceCollection: s => s.AddScoped(_ => _lookupOrganisationUseCase.Object));
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync($"/organisation/lookup?name={name}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
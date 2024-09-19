using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;

public class OrganisationLookupEndpointsTests
{
    private readonly Mock<IUseCase<Model.Organisation?>> _getMyOrganisationUseCase = new();

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
            [new Claim(ClaimType.Channel, channel)],
            serviceCollection: services => services.AddScoped(_ => _getMyOrganisationUseCase.Object));
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/organisation/me");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
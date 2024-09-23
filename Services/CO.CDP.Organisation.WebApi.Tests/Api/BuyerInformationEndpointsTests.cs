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

public class BuyerInformationEndpointsTests
{
    private readonly Mock<IUseCase<(Guid, UpdateBuyerInformation), bool>> _updateBuyerInformationUseCase = new();

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task UpdateBuyerInformation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var updateBuyerInformation = new UpdateBuyerInformation { Type = BuyerInformationUpdateType.BuyerOrganisationType, BuyerInformation = new() };
        var command = (organisationId, updateBuyerInformation);

        _updateBuyerInformationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _updateBuyerInformationUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/organisations/{organisationId}/buyer-information", updateBuyerInformation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
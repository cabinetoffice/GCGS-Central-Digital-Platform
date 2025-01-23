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

public class OrganisationMouEndpointsTests
{
    private readonly Mock<IUseCase<Guid, MouSignatureLatest>> _getOrganisationMouSignatureLatestUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin, null, PersonScope.SupportAdmin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationLatestMouSignature_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? organisationPersonScope = null, string? personScope = null)
    {
        var organisationId = Guid.NewGuid();

        _getOrganisationMouSignatureLatestUseCase.Setup(uc => uc.Execute(organisationId))
            .ReturnsAsync(new MouSignatureLatest
            {
                Id = organisationId,
                IsLatest = true,
                JobTitle = "",
                Mou = null!,
                Name = "",
                SignatureOn = DateTimeOffset.Now,
                CreatedBy = null!
            });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel,
            organisationId,
            organisationPersonScope,
            services => services.AddScoped(_ => _getOrganisationMouSignatureLatestUseCase.Object),
            assignedPersonScopes: personScope);

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/mou/latest");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
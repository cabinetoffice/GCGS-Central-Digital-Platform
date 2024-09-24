using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.UseCase;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.EntityVerification.Tests.Api;

public class GetIdentifiersTests
{
    private readonly Mock<IUseCase<LookupIdentifierQuery, IEnumerable<Identifier>>> _lookupIdentifierUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetIdentifiers_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var identifier = "test_identifier";
        _lookupIdentifierUseCase.Setup(useCase => useCase.Execute(It.IsAny<LookupIdentifierQuery>()))
            .ReturnsAsync([new Identifier { Scheme = "SIC", Id = "01230", LegalName = "Acme Ltd" }]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _lookupIdentifierUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/identifiers/{identifier}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
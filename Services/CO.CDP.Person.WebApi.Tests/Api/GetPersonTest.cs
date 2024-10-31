using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Person.WebApi.Tests.Api;

public class GetPersonTest
{
    private readonly Mock<IUseCase<Guid, Model.Person?>> _getPersonUseCase = new();
    private readonly Mock<IUseCase<(Guid, ClaimPersonInvite), bool>> _claimPersonInviteUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetSharedData_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var personId = Guid.NewGuid();
        _getPersonUseCase.Setup(useCase => useCase.Execute(personId))
            .ReturnsAsync(new Model.Person { Id = personId, FirstName = "fn", LastName = "ln", Email = "fn.ln@test" });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _getPersonUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/persons/{personId}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task ClaimPersonInvite_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var personId = Guid.NewGuid();
        var claimPersonInvite = new ClaimPersonInvite { PersonInviteId = Guid.NewGuid() };
        var command = (personId, claimPersonInvite);

        _claimPersonInviteUseCase.Setup(useCase => useCase.Execute(command))
            .ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _claimPersonInviteUseCase.Object));

        var response = await factory.CreateClient().PostAsJsonAsync($"/persons/{personId}/claim-person-invite", claimPersonInvite);

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
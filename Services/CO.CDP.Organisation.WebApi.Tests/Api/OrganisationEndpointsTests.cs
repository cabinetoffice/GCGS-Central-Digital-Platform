using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;
public class OrganisationEndpointsTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterOrganisation, Model.Organisation>> _registerOrganisationUseCase = new();
    private readonly Mock<IUseCase<PaginatedOrganisationQuery, IEnumerable<OrganisationExtended>>> _getOrganisationsUseCase = new();
    private readonly Mock<IUseCase<Guid, Model.Organisation>> _getOrganisationUseCase = new();
    private readonly Mock<IUseCase<(Guid, UpdateOrganisation), bool>> _updatesOrganisationUseCase = new();
    private readonly Mock<IUseCase<(Guid, OrganisationJoinRequestStatus?), IEnumerable<OrganisationJoinRequest>>> _getOrganisationJoinRequestsUseCase = new();

    public OrganisationEndpointsTests()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _registerOrganisationUseCase.Object);
                services.AddScoped(_ => _getOrganisationsUseCase.Object);
                services.AddScoped(_ => _getOrganisationUseCase.Object);
                services.AddScoped(_ => _updatesOrganisationUseCase.Object);
            });
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItRegistersNewOrganisation()
    {
        var command = GivenRegisterOrganisationCommand();
        var organisation = SetupRegisterOrganisationUseCaseMock(command);

        var response = await _httpClient.PostAsJsonAsync("/organisations", command);

        response.Should().HaveStatusCode(Created, await response.Content.ReadAsStringAsync());

        var returnedOrganisation = await response.Content.ReadFromJsonAsync<Model.Organisation>();
        returnedOrganisation.Should().BeEquivalentTo(organisation, options => options.ComparingByMembers<Model.Organisation>());
    }

    [Fact]
    public async Task ItHandlesOrganisationCreationFailure()
    {
        var command = GivenRegisterOrganisationCommand();

        _registerOrganisationUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegisterOrganisation>()))
                                    .ReturnsAsync((Model.Organisation)null!);

        var response = await _httpClient.PostAsJsonAsync("/organisations", command);

        response.Should().HaveStatusCode(HttpStatusCode.InternalServerError, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(true, NoContent)]
    [InlineData(false, NotFound)]
    public async Task UpdateOrganisation_TestCases(bool useCaseResult, HttpStatusCode expectedStatusCode)
    {
        var organisationId = Guid.NewGuid();
        var updateOrganisation = new UpdateOrganisation { Type = OrganisationUpdateType.AdditionalIdentifiers, Organisation = new() };
        var command = (organisationId, updateOrganisation);

        if (useCaseResult)
            _updatesOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);
        else
            _updatesOrganisationUseCase.Setup(uc => uc.Execute(command)).ThrowsAsync(new UnknownOrganisationException(""));

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{organisationId}", updateOrganisation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task UpdateOrganisation_InvalidOrganisationId_ReturnsUnprocessableEntity()
    {
        var invalidOrganisationId = "invalid-guid";

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{invalidOrganisationId}",
            new UpdateOrganisation { Type = OrganisationUpdateType.AdditionalIdentifiers, Organisation = new() });

        response.StatusCode.Should().Be(UnprocessableEntity);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, PersonScope.SupportAdmin)]
    [InlineData(Forbidden, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetAllOrganisations_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? personScope = null)
    {
        _getOrganisationsUseCase.Setup(uc => uc.Execute(It.IsAny<PaginatedOrganisationQuery>()))
            .ReturnsAsync([]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel,
            serviceCollection: s => s.AddScoped(_ => _getOrganisationsUseCase.Object),
            assignedPersonScopes: personScope);

        var response = await factory.CreateClient().GetAsync("/organisations?type=any&limit=10&skip=1");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(Created, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task CreateOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var command = GivenRegisterOrganisationCommand();
        SetupRegisterOrganisationUseCaseMock(command);
        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _registerOrganisationUseCase.Object));
        var httpClient = factory.CreateClient();

        var response = await httpClient.PostAsJsonAsync("/organisations", command);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, null, PersonScope.SupportAdmin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(OK, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    public async Task GetOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? organisationPersonScope = null, string? personScope = null)
    {
        var organisationId = Guid.NewGuid();

        _getOrganisationUseCase.Setup(useCase => useCase.Execute(organisationId))
                                    .ReturnsAsync(GivenOrganisation(organisationId));

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, organisationPersonScope,
            services => services.AddScoped(_ => _getOrganisationUseCase.Object), personScope);

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task UpdateOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var updateOrganisation = new UpdateOrganisation { Type = OrganisationUpdateType.AdditionalIdentifiers, Organisation = new() };
        var command = (organisationId, updateOrganisation);

        _updatesOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _updatesOrganisationUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/organisations/{organisationId}", updateOrganisation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationJoinRequests_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        OrganisationJoinRequestStatus? status = OrganisationJoinRequestStatus.Pending;
        var command = (organisationId, status);

        _getOrganisationJoinRequestsUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync([]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getOrganisationJoinRequestsUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/join-requests/{status}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    public static Model.Organisation GivenOrganisation(Guid organisationId)
    {
        var command = GivenRegisterOrganisationCommand();
        return new Model.Organisation
        {
            Id = organisationId,
            Name = "TheOrganisation",
            Identifier = command.Identifier.AsView(),
            AdditionalIdentifiers = command.AdditionalIdentifiers.AsView(),
            Addresses = command.Addresses.AsView(),
            ContactPoint = command.ContactPoint.AsView(),
            Roles = command.Roles
        };
    }

    private static RegisterOrganisation GivenRegisterOrganisationCommand()
    {
        return new RegisterOrganisation
        {
            Name = "TheOrganisation",
            PersonId = Guid.NewGuid(),
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName"
                }
            },
            Addresses = [new OrganisationAddress
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Example St",
                Locality = "Example City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Exampleland",
                Country = "AB"
            }],
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "http://example.org/contact"
            },
            Roles = [PartyRole.Tenderer]
        };
    }

    private Model.Organisation SetupRegisterOrganisationUseCaseMock(RegisterOrganisation command)
    {
        var organisation = new Model.Organisation
        {
            Id = Guid.NewGuid(),
            Name = "TheOrganisation",
            Identifier = command.Identifier.AsView(),
            AdditionalIdentifiers = command.AdditionalIdentifiers.AsView(),
            Addresses = command.Addresses.AsView(),
            ContactPoint = command.ContactPoint.AsView(),
            Roles = command.Roles
        };

        _registerOrganisationUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegisterOrganisation>()))
                                    .ReturnsAsync(organisation);

        return organisation;
    }
}
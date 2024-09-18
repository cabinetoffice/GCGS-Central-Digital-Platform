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
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;
public class OrganisationEndpointsTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterOrganisation, Model.Organisation>> _registerOrganisationUseCase = new();
    private readonly Mock<IUseCase<string, IEnumerable<Model.Organisation>>> _getOrganisationsUseCase = new();
    private readonly Mock<IUseCase<(Guid, UpdateOrganisation), bool>> _updatesOrganisationUseCase = new();

    public OrganisationEndpointsTests()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _registerOrganisationUseCase.Object);
                services.AddScoped(_ => _getOrganisationsUseCase.Object);
                services.AddScoped(_ => _updatesOrganisationUseCase.Object);
            });
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItRegistersNewOrganisation()
    {
        var command = GivenRegisterOrganisationCommand();
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

    [Fact]
    public async Task ItListsOrganisationWhenFound()
    {
        var command = GivenRegisterOrganisationCommand();
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

        _getOrganisationsUseCase.Setup(useCase => useCase.Execute(It.IsAny<string>()))
                                    .ReturnsAsync([organisation]);

        var returnedOrganisations = await _httpClient.GetFromJsonAsync<IEnumerable<Model.Organisation>>(
            "/organisations?userUrn=urn:7wTqYGMFQxgukTSpSI2GodMwe9");

        returnedOrganisations.Should().ContainEquivalentOf(organisation);
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
}
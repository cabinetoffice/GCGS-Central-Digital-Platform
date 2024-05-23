using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using CO.CDP.TestKit.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;
public class RegisterOrganisationTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterOrganisation, Model.Organisation>> _registerOrganisationUseCase = new();
    private readonly Mock<IUseCase<string, IEnumerable<Model.Organisation>>> _getOrganisationsUseCase = new();

    public RegisterOrganisationTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IUseCase<RegisterOrganisation, Model.Organisation>>(_ =>
                    _registerOrganisationUseCase.Object);
                services.AddScoped(_ => _getOrganisationsUseCase.Object);
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
            Address = command.Address.AsView(),
            ContactPoint = command.ContactPoint,
            Types = command.Types
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
            Address = command.Address.AsView(),
            ContactPoint = command.ContactPoint,
            Types = command.Types
        };

        _getOrganisationsUseCase.Setup(useCase => useCase.Execute(It.IsAny<string>()))
                                    .ReturnsAsync([organisation]);

        var returnedOrganisations = await _httpClient.GetFromJsonAsync<IEnumerable<Model.Organisation>>(
            "/organisations?userUrn=urn:7wTqYGMFQxgukTSpSI2GodMwe9");

        returnedOrganisations.Should().ContainEquivalentOf(organisation);
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
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Example St",
                StreetAddress2 = "",
                Locality = "Example Region",
                PostalCode = "12345",
                CountryName = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "http://example.org/contact"
            },
            Types = new List<int> { 1 }
        };
    }
}

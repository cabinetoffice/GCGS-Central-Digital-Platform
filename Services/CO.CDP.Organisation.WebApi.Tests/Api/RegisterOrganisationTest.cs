using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.Api.WebApp;
using CO.CDP.Organisation.WebApi.UseCase;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace CO.CDP.Organisation.WebApi.Tests.Api;
public class RegisterOrganisationTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterOrganisation, Model.Organisation>> _registerOrganisationUseCase = new();

    public RegisterOrganisationTest()
    {
        TestWebApplicationFactory<Program> factory = new(services =>
        {
            services.AddScoped<IUseCase<RegisterOrganisation, Model.Organisation>>(_ => _registerOrganisationUseCase.Object);
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
            Identifier = command.Identifier,
            AdditionalIdentifiers = command.AdditionalIdentifiers,
            Address = command.Address,
            ContactPoint = command.ContactPoint,
            Roles = command.Roles
        };

        _registerOrganisationUseCase.Setup(useCase => useCase.Execute(command))
            .Returns(Task.FromResult(organisation));

        var response = await _httpClient.PostAsJsonAsync("/organisations", command);

        //response.Should().HaveStatusCode(Created, await response.Content.ReadAsStringAsync());
        //response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        //response.Headers.Location.Should().Match<string>("^/organisations/[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$");
        //var content = await response.Content.ReadFromJsonAsync<Model.Organisation>();
        //content.Should().BeEquivalentTo(organisation);

        //response.Should().HaveStatusCode(Created, await response.Content.ReadAsStringAsync());
        //response.Should().MatchLocation("^/organisations/[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$");
        //await response.Should().HaveContent(organisation);
    }

    private static RegisterOrganisation GivenRegisterOrganisationCommand()
    {
        return new RegisterOrganisation
        {
            Name = "TheOrganisation",
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName",
                Uri = "http://example.org"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName",
                    Uri = "http://example.com"
                }
            },
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Example St",
                Locality = "Example City",
                Region = "Example Region",
                PostalCode = "12345",
                CountryName = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                FaxNumber = "123-456-7891",
                Url = "http://example.org/contact"
            },
            Roles = new List<int> { 1 }
        };
    }
}
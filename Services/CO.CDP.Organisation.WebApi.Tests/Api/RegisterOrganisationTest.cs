using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.Api.WebApp;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;
public class RegisterOrganisationTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegistaerOrganisation, Model.Organisation>> _registerOrganisationUseCase = new();

    public RegisterOrganisationTest()
    {
        TestWebApplicationFactory<Program> factory = new(services =>
        {
            services.AddScoped<IUseCase<RegistaerOrganisation, Model.Organisation>>(_ => _registerOrganisationUseCase.Object);
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
            Types = command.Types
        };

        _registerOrganisationUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegistaerOrganisation>()))
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

        _registerOrganisationUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegistaerOrganisation>()))
                                    .ReturnsAsync((Model.Organisation)null!);

        var response = await _httpClient.PostAsJsonAsync("/organisations", command);

        response.Should().HaveStatusCode(HttpStatusCode.InternalServerError, await response.Content.ReadAsStringAsync());
    }

    private static RegistaerOrganisation GivenRegisterOrganisationCommand()
    {
        return new RegistaerOrganisation
        {
            Name = "TheOrganisation",
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName",
                Uri = "http://example.org",
                Number = "123456"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName",
                    Uri = "http://example.com",
                    Number = "123456"
                }
            },
            Address = new OrganisationAddress
            {
                AddressLine1 = "1234 Example St",
                City = "Example Region",
                PostCode = "12345",
                Country = "Exampleland"
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
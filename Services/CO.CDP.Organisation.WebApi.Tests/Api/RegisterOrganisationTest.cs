using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;
public class RegisterOrganisationTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterOrganisation, Model.Organisation>> _registerOrganisationUseCase = new();
    private readonly Mock<IUseCase<string, IEnumerable<Model.Organisation>>> _getOrganisationsUseCase = new();
    private readonly Mock<IUseCase<Guid, SupplierInformation?>> _getSupplierInformationUseCase = new();
    private readonly Mock<IUseCase<(Guid, UpdateSupplierInformation), bool>> _updatesSupplierInformationUseCase = new();
    private readonly Mock<IUseCase<(Guid, UpdateOrganisation), bool>> _updatesOrganisationUseCase = new();
    private readonly Mock<IUseCase<(Guid, DeleteSupplierInformation), bool>> _deleteSupplierInformationUseCase = new();

    public RegisterOrganisationTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _registerOrganisationUseCase.Object);
                services.AddScoped(_ => _getOrganisationsUseCase.Object);
                services.AddScoped(_ => _getSupplierInformationUseCase.Object);
                services.AddScoped(_ => _updatesSupplierInformationUseCase.Object);
                services.AddScoped(_ => _updatesOrganisationUseCase.Object);
                services.AddScoped(_ => _deleteSupplierInformationUseCase.Object);
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

    [Fact]
    public async Task GetSupplierInformation_ValidOrganisationId_ReturnsOk()
    {
        var organisationId = Guid.NewGuid();
        var supplierInformation = new SupplierInformation { OrganisationName = "FakeOrg" };
        _getSupplierInformationUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync(supplierInformation);

        var returnedSupplierInformation = await _httpClient.GetFromJsonAsync<SupplierInformation>(
            $"/organisations/{organisationId}/supplier-information");

        returnedSupplierInformation.Should().BeEquivalentTo(supplierInformation);
    }

    [Fact]
    public async Task GetSupplierInformation_OrganisationNotFound_ReturnsNotFound()
    {
        var organisationId = Guid.NewGuid();
        _getSupplierInformationUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync((SupplierInformation?)null);

        var response = await _httpClient.GetAsync($"/organisations/{organisationId}/supplier-information");

        response.StatusCode.Should().Be(NotFound);
    }

    [Fact]
    public async Task GetSupplierInformation_InvalidOrganisationId_ReturnsUnprocessableEntity()
    {
        var invalidOrganisationId = "invalid-guid";

        var response = await _httpClient.GetAsync($"/organisations/{invalidOrganisationId}/supplier-information");

        response.StatusCode.Should().Be(UnprocessableEntity);
    }

    [Theory]
    [InlineData(true, NoContent)]
    [InlineData(false, NotFound)]
    public async Task UpdateSupplierInformation_TestCases(bool useCaseResult, HttpStatusCode expectedStatusCode)
    {
        var organisationId = Guid.NewGuid();
        var updateSupplierInformation = new UpdateSupplierInformation { Type = SupplierInformationUpdateType.SupplierType, SupplierInformation = new() };
        var command = (organisationId, updateSupplierInformation);

        if (useCaseResult)
            _updatesSupplierInformationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);
        else
            _updatesSupplierInformationUseCase.Setup(uc => uc.Execute(command)).ThrowsAsync(new UnknownOrganisationException(""));

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{organisationId}/supplier-information", updateSupplierInformation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task UpdateSupplierInformation_InvalidOrganisationId_ReturnsUnprocessableEntity()
    {
        var invalidOrganisationId = "invalid-guid";

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{invalidOrganisationId}/supplier-information",
            new UpdateSupplierInformation { Type = SupplierInformationUpdateType.SupplierType, SupplierInformation = new() });

        response.StatusCode.Should().Be(UnprocessableEntity);
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
    [InlineData(true, NoContent)]
    [InlineData(false, NotFound)]
    public async Task DeleteSupplierInformation_TestCases(bool useCaseResult, HttpStatusCode expectedStatusCode)
    {
        var organisationId = Guid.NewGuid();
        var deleteSupplierInformation = new DeleteSupplierInformation { Type = SupplierInformationDeleteType.TradeAssurance, TradeAssuranceId = Guid.NewGuid() };
        var command = (organisationId, deleteSupplierInformation);

        if (useCaseResult)
            _deleteSupplierInformationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);
        else
            _deleteSupplierInformationUseCase.Setup(uc => uc.Execute(command)).ThrowsAsync(new UnknownOrganisationException(""));

        var response = await SendDeleteRequestAsync($"/organisations/{organisationId}/supplier-information", deleteSupplierInformation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task DeleteSupplierInformation_InvalidOrganisationId_ReturnsUnprocessableEntity()
    {
        var invalidOrganisationId = "invalid-guid";
        var response = await SendDeleteRequestAsync($"/organisations/{invalidOrganisationId}/supplier-information", "{}");

        response.StatusCode.Should().Be(UnprocessableEntity);
    }

    private async Task<HttpResponseMessage> SendDeleteRequestAsync(string relativeUri, object obj)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(_httpClient.BaseAddress!, relativeUri),
            Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json")
        };
        return await _httpClient.SendAsync(request);
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
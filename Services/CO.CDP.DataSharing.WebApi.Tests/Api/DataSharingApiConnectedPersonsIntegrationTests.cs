using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.Tests;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;
using Xunit.Abstractions;
using ContactPoint = CO.CDP.OrganisationInformation.Persistence.ContactPoint;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Identifier;
using ShareRequest = CO.CDP.DataSharing.WebApiClient.ShareRequest;
namespace CO.CDP.DataSharing.WebApi.Tests.Api;

public class DataSharingApiConnectedPersonsIntegrationTests: IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationInformationPostgreSqlFixture _postgreSql;
    private readonly OrganisationInformationContext _context;
    private readonly IDataSharingClient _client;
    private readonly Guid supplierInformationFormId = new("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");

    public DataSharingApiConnectedPersonsIntegrationTests(ITestOutputHelper testOutputHelper, OrganisationInformationPostgreSqlFixture postgreSql)
    {
        TestWebApplicationFactory<Program> _factory = new(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, services) =>
            {
                services.RemoveAll<OrganisationInformationContext>();
                services.AddScoped(_ => postgreSql.OrganisationInformationContext());
            });
        });

        _httpClient = _factory.CreateClient();
        _postgreSql = postgreSql;
        _context = _postgreSql.OrganisationInformationContext();
        _client = new DataSharingClient("https://localhost", _httpClient);
    }

    [Fact]
    public async Task DataSharingClient_Returns200_WhenShareCodeExists()
    {
        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.Id.Should().Be(organisation.Guid);
    }

    [Fact]
    public async Task DataSharingClient_Returns404_WhenShareCodeDoesNotExist()
    {
        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        Func<Task> act = async () => await _client.GetSharedDataAsync("ABC456");

        var exception = await act.Should().ThrowAsync<ApiException<ProblemDetails>>();
        exception.Which.StatusCode.Should().Be(404);
        exception.Which.Result.Detail.Should().Contain("Share code not found");
    }

    [Theory]
    [InlineData(-1, null)]    // connected individual created before share code
    [InlineData(-1, 1)] // connected individual created before and ended after share code
    public async Task DataSharingClient_ReturnsConnectedIndividual_IfDatesIntersect(int cpCreationOffset, int? cpEndDateOffset)
    {
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset).ToUniversalTime();
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value).ToUniversalTime() : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntity(organisation, connectedPersonCreationDate, connectedPersonEndDate, OrganisationInformation.ConnectedEntityType.Individual);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AssociatedPersons.Should().HaveCount(1);
        response.AssociatedPersons.First().FirstName.Should().Be("John");
        response.AssociatedPersons.First().LastName.Should().Be("Doe");
    }

    [Theory]
    [InlineData(1, null)]    // connected individual created after share code
    [InlineData(-2, -1)]   // connected individual created and ended before share code
    [InlineData(1, 2)]    // connected individual created and ended after share code
    public async Task DataSharingClient_StillReturnsConnectedIndividual_IfDatesDontIntersect(int cpCreationOffset, int? cpEndDateOffset)
    {
        // Note: The end_date filtering has been removed from the data sharing api and therefore this test has been inverted to ensure connected entities still return
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset).ToUniversalTime();
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value).ToUniversalTime() : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntity(organisation, connectedPersonCreationDate, connectedPersonEndDate, OrganisationInformation.ConnectedEntityType.Individual);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AssociatedPersons.Should().HaveCount(1);   // John Doe should still be there now that we have removed the end_date filtering from the data sharing api
        response.AssociatedPersons.First().FirstName.Should().Be("John");
        response.AssociatedPersons.First().LastName.Should().Be("Doe");
    }

    [Theory]
    [InlineData(-1, null)]    // connected individual created before share code
    [InlineData(-1, 1)] // connected individual created before and ended after share code
    public async Task DataSharingClient_ReturnsConnectedOrgs_IfDatesIntersect(int cpCreationOffset, int? cpEndDateOffset)
    {
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset).ToUniversalTime();
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value).ToUniversalTime() : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntity(organisation, connectedPersonCreationDate, connectedPersonEndDate, OrganisationInformation.ConnectedEntityType.Organisation);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AdditionalEntities.Should().HaveCount(1);
        response.AdditionalEntities.First().Name.Should().Be("Test org");
    }

    [Theory]
    [InlineData(1, null)]    // connected individual created after share code
    [InlineData(-2, -1)]   // connected individual created and ended before share code
    [InlineData(1, 2)]    // connected individual created and ended after share code
    public async Task DataSharingClient_StillReturnsConnectedOrgs_IfDatesDontIntersect(int cpCreationOffset, int? cpEndDateOffset)
    {
        // Note: The end_date filtering has been removed from the data sharing api and therefore this test has been inverted to ensure connected entities still return
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset).ToUniversalTime();
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value).ToUniversalTime() : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntity(organisation, connectedPersonCreationDate, connectedPersonEndDate, OrganisationInformation.ConnectedEntityType.Organisation);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AdditionalEntities.Should().HaveCount(1); // Test org should still be there now that we have removed the end_date filtering from the data sharing api
        response.AdditionalEntities.First().Name.Should().Be("Test org");
    }

    private Organisation CreateOrganisation()
    {
        var organisation = new Organisation
        {
            Guid = Guid.NewGuid(),
            Name = "Test org",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = "Test org",
            },
            Identifiers = new List<Identifier>
            {
                new Identifier
                {
                    IdentifierId = "1234567",
                    Scheme = "Whatever",
                    LegalName = "Org Legal Name",
                    Primary = true
                }
            },
            Addresses = new List<OrganisationAddress>
            {
                new OrganisationAddress
                {
                    Type = OrganisationInformation.AddressType.Registered,
                    Address = new OrganisationInformation.Persistence.Address
                    {
                        StreetAddress = "1234 New St",
                        Locality = "New City",
                        Region = "W.Yorkshire",
                        PostalCode = "123456",
                        CountryName = "Newland",
                        Country = "GB"
                    }
                }
            },
            ContactPoints = new List<ContactPoint>
            {
                new ContactPoint
                {
                    Name = "Main Contact",
                    Email = "foo@bar.com"
                }
            }
        };

        _context.Organisations.Add(organisation);
        _context.SaveChanges();

        return organisation;
    }

    private void CreateSharedConsent(Organisation organisation)
    {
        var form = _context.Forms.Where(f => f.Guid == supplierInformationFormId).First();

        _context.SharedConsents.Add(new OrganisationInformation.Persistence.Forms.SharedConsent
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = 1,
            Form = form,
            FormVersionId = "1",
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTimeOffset.Now.ToUniversalTime(),
        });

        _context.SaveChanges();
    }

    private void CreateConnectedEntity(Organisation organisation, DateTime connectedPersonCreationDate,
        DateTime? connectedPersonEndDate, OrganisationInformation.ConnectedEntityType type)
    {
        var entity = new ConnectedEntity
        {
            Guid = Guid.NewGuid(),
            EntityType = type,
            SupplierOrganisation = organisation,
            CreatedOn = connectedPersonCreationDate,
            EndDate = connectedPersonEndDate,
        };

        switch (type)
        {
            case OrganisationInformation.ConnectedEntityType.Individual:
                entity.IndividualOrTrust = new ConnectedEntity.ConnectedIndividualTrust
                {
                    Category = ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1980, 1, 1).ToUniversalTime(),
                };

                break;

            case OrganisationInformation.ConnectedEntityType.Organisation:
                entity.Organisation = new ConnectedEntity.ConnectedOrganisation
                {
                    Name = "Test org",
                    Category = OrganisationInformation.ConnectedOrganisationCategory.RegisteredCompany,
                };
                break;
        }

        _context.ConnectedEntities.Add(entity);

        _context.SaveChanges();

        entity.CreatedOn = connectedPersonCreationDate;
        _context.SaveChanges();
    }

    private void ClearDatabase()
    {
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""organisations"" CASCADE;");
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""tenants"" CASCADE;");
    }
}
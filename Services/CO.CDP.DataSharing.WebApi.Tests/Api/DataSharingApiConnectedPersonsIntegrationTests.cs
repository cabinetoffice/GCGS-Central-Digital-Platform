using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.Tests;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;
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
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset);
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value) : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Individual);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AssociatedPersons.Should().HaveCount(1);
        response.AssociatedPersons.First().Name.Should().Be("John Doe");
    }

    [Theory]
    [InlineData(1, null)]    // connected individual created after share code
    [InlineData(-2, -1)]   // connected individual created and ended before share code
    [InlineData(1, 2)]    // connected individual created and ended after share code
    public async Task DataSharingClient_DoesNotReturnConnectedIndividual_IfDatesDontIntersect(int cpCreationOffset, int? cpEndDateOffset)
    {
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset);
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value) : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Individual);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AssociatedPersons.Should().HaveCount(0);
    }

    [Theory]
    [InlineData(-1, null)]    // connected individual created before share code
    [InlineData(-1, 1)] // connected individual created before and ended after share code
    public async Task DataSharingClient_ReturnsConnectedOrgs_IfDatesIntersect(int cpCreationOffset, int? cpEndDateOffset)
    {
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset);
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value) : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Organisation);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AdditionalEntities.Should().HaveCount(1);
        response.AdditionalEntities.First().Name.Should().Be("Test org");
    }

    [Theory]
    [InlineData(1, null)]    // connected individual created after share code
    [InlineData(-2, -1)]   // connected individual created and ended before share code
    [InlineData(1, 2)]    // connected individual created and ended after share code
    public async Task DataSharingClient_DoesNotReturnConnectedOrgs_IfDatesDontIntersect(int cpCreationOffset, int? cpEndDateOffset)
    {
        DateTime connectedPersonCreationDate = DateTime.Now.AddHours(cpCreationOffset);
        DateTime? connectedPersonEndDate = cpEndDateOffset != null ? DateTime.Now.AddHours(cpEndDateOffset.Value) : null;

        ClearDatabase();
        Organisation organisation = CreateOrganisation();
        CreateSharedConsent(organisation);
        CreateConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Organisation);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        response.AdditionalEntities.Should().HaveCount(0);
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
            Identifiers = new List<Organisation.Identifier>
            {
                new Organisation.Identifier
                {
                    IdentifierId = "1234567",
                    Scheme = "Whatever",
                    LegalName = "Org Legal Name",
                    Primary = true
                }
            },
            Addresses = new List<Organisation.OrganisationAddress>
            {
                new Organisation.OrganisationAddress
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
            ContactPoints = new List<Organisation.ContactPoint>
            {
                new Organisation.ContactPoint
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
            Guid = new Guid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = 1,
            Form = form,
            FormVersionId = "1",
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTimeOffset.Now,
        });

        _context.SaveChanges();
    }

    private void CreateConnectedEntities(Organisation organisation, DateTime connectedPersonCreationDate, DateTime? connectedPersonEndDate, ConnectedEntity.ConnectedEntityType type)
    {
        var entity = new ConnectedEntity
        {
            Guid = new Guid(),
            EntityType = type,
            SupplierOrganisation = organisation,
            CreatedOn = connectedPersonCreationDate,
            EndDate = connectedPersonEndDate,
        };

        switch(type)
        {
            case ConnectedEntity.ConnectedEntityType.Individual:
                entity.IndividualOrTrust = new ConnectedEntity.ConnectedIndividualTrust
                {
                    Category = ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1980, 1, 1),
                };

            break;

            case ConnectedEntity.ConnectedEntityType.Organisation:
                entity.Organisation = new ConnectedEntity.ConnectedOrganisation
                {
                    Name = "Test org",
                    Category = ConnectedEntity.ConnectedOrganisationCategory.RegisteredCompany,
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
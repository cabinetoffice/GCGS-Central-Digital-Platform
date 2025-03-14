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
namespace CO.CDP.DataSharing.WebApi.Tests.Api;

public class DataSharingApiIntegrationTests: IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationInformationPostgreSqlFixture _postgreSql;
    private readonly OrganisationInformationContext _context;
    private readonly IDataSharingClient _client;
    private readonly Guid supplierInformationFormId = new("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");

    public DataSharingApiIntegrationTests(ITestOutputHelper testOutputHelper, OrganisationInformationPostgreSqlFixture postgreSql)
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
        Organisation organisation = SeedBaseData();
        SeedShareCode("ABC123", organisation);

        var response = await _client.GetSharedDataAsync("ABC123");
        response.Id.Should().Be(organisation.Guid);
    }

    [Fact]
    public async Task DataSharingClient_Returns404_WhenShareCodeDoesNotExist()
    {
        ClearDatabase();
        Organisation organisation = SeedBaseData();
        SeedShareCode("ABC123", organisation);

        Func<Task> act = async () => await _client.GetSharedDataAsync("ABC456");

        var exception = await act.Should().ThrowAsync<ApiException<ProblemDetails>>();
        exception.Which.StatusCode.Should().Be(404);
        exception.Which.Result.Detail.Should().Contain("Share code not found");
    }

    [Theory]
    [InlineData("2022-01-01T10:00:00", "2022-01-01T09:00:00", null)]    // connected individual created before share code
    [InlineData("2022-01-01T10:00:00", "2022-01-01T09:00:00", "2022-01-01T11:00:00")] // connected individual created before and ended after share code
    public async Task DataSharingClient_ReturnsConnectedIndividual_IfDatesIntersect(string scCreationDate, string cpCreationDate, string? cpEndDate)
    {
        DateTime shareCodeCreationDate = DateTime.Parse(scCreationDate);
        DateTime connectedPersonCreationDate = DateTime.Parse(cpCreationDate);
        DateTime? connectedPersonEndDate = cpEndDate != null ? DateTime.Parse(cpEndDate) : null;

        ClearDatabase();
        Organisation organisation = SeedBaseData();
        SeedConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Individual);
        SeedShareCode("ABC123", organisation, shareCodeCreationDate);
       
        var response = await _client.GetSharedDataAsync("ABC123");
        response.AssociatedPersons.Should().HaveCount(1);
        response.AssociatedPersons.First().Name.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("2022-01-01T10:00:00", "2022-01-01T11:00:00", null)]    // connected individual created after share code
    [InlineData("2022-01-01T10:00:00", "2022-01-01T09:00:00", "2022-01-01T09:30:00")]   // connected individual created and ended before share code
    [InlineData("2022-01-01T10:00:00", "2022-01-01T11:00:00", "2022-01-01T12:00:00")]    // connected individual created and ended after share code
    public async Task DataSharingClient_DoesNotReturnConnectedIndividual_IfDatesDontIntersect(string scCreationDate, string cpCreationDate, string? cpEndDate)
    {
        DateTime shareCodeCreationDate = DateTime.Parse(scCreationDate);
        DateTime connectedPersonCreationDate = DateTime.Parse(cpCreationDate);
        DateTime? connectedPersonEndDate = cpEndDate != null ? DateTime.Parse(cpEndDate) : null;

        ClearDatabase();
        Organisation organisation = SeedBaseData();
        SeedConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Individual);
        SeedShareCode("ABC123", organisation, shareCodeCreationDate);

        var response = await _client.GetSharedDataAsync("ABC123");
        response.AssociatedPersons.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("2022-01-01T10:00:00", "2022-01-01T09:00:00", null)]    // connected org created before share code
    [InlineData("2022-01-01T10:00:00", "2022-01-01T09:00:00", "2022-01-01T11:00:00")] // connected org created before and ended after share code
    public async Task DataSharingClient_ReturnsConnectedOrgs_IfDatesIntersect(string scCreationDate, string cpCreationDate, string? cpEndDate)
    {
        DateTime shareCodeCreationDate = DateTime.Parse(scCreationDate);
        DateTime connectedPersonCreationDate = DateTime.Parse(cpCreationDate);
        DateTime? connectedPersonEndDate = cpEndDate != null ? DateTime.Parse(cpEndDate) : null;

        ClearDatabase();
        Organisation organisation = SeedBaseData();
        SeedConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Organisation);
        SeedShareCode("ABC123", organisation, shareCodeCreationDate);

        var response = await _client.GetSharedDataAsync("ABC123");
        response.AdditionalEntities.Should().HaveCount(1);
        response.AdditionalEntities.First().Name.Should().Be("Test org");
    }

    [Theory]
    [InlineData("2022-01-01T10:00:00", "2022-01-01T11:00:00", null)]    // connected org created after share code
    [InlineData("2022-01-01T10:00:00", "2022-01-01T09:00:00", "2022-01-01T09:30:00")]   // connected org created and ended before share code
    [InlineData("2022-01-01T10:00:00", "2022-01-01T11:00:00", "2022-01-01T12:00:00")]    // connected org created and ended after share code
    public async Task DataSharingClient_DoesNotReturnConnectedOrgs_IfDatesDontIntersect(string scCreationDate, string cpCreationDate, string? cpEndDate)
    {
        DateTime shareCodeCreationDate = DateTime.Parse(scCreationDate);
        DateTime connectedPersonCreationDate = DateTime.Parse(cpCreationDate);
        DateTime? connectedPersonEndDate = cpEndDate != null ? DateTime.Parse(cpEndDate) : null;

        ClearDatabase();
        Organisation organisation = SeedBaseData();
        SeedConnectedEntities(organisation, connectedPersonCreationDate, connectedPersonEndDate, ConnectedEntity.ConnectedEntityType.Organisation);
        SeedShareCode("ABC123", organisation, shareCodeCreationDate);

        var response = await _client.GetSharedDataAsync("ABC123");
        response.AdditionalEntities.Should().HaveCount(0);
    }

    private Organisation SeedBaseData()
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
                    LegalName = "New Org Legal Name",
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

    private void SeedShareCode(string shareCode, Organisation organisation, DateTime? shareCodeCreationDate = null)
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
            SubmissionState = SubmissionState.Submitted,
            ShareCode = shareCode,
            SubmittedAt = shareCodeCreationDate ?? DateTimeOffset.Now,
        });

        _context.SaveChanges();
    }

    private void SeedConnectedEntities(Organisation organisation, DateTime connectedPersonCreationDate, DateTime? connectedPersonEndDate, ConnectedEntity.ConnectedEntityType type)
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
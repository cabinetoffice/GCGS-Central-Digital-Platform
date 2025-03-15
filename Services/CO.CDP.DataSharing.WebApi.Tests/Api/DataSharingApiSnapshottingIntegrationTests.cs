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

public class DataSharingApiSnapshottingIntegrationTests : IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationInformationPostgreSqlFixture _postgreSql;
    private readonly OrganisationInformationContext _context;
    private readonly IDataSharingClient _client;
    private readonly Guid supplierInformationFormId = new("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");

    public DataSharingApiSnapshottingIntegrationTests(ITestOutputHelper testOutputHelper, OrganisationInformationPostgreSqlFixture postgreSql)
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
    public async Task DataSharingClient_ReturnsCorrectOrgNameSnapshot_WhenChangingOrgNameBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");

        // Create first shared code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        organisation.Name = "Updated org name";
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.Name.Should().Be("Test org");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.Name.Should().Be("Updated org name");
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectSupplierInformation_WhenChangingDataBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        organisation.SupplierInfo = new Organisation.SupplierInformation
        {
            SupplierType = OrganisationInformation.SupplierType.Organisation,
            OperationTypes = new List<OrganisationInformation.OperationType> { OrganisationInformation.OperationType.NonGovernmental },
        };
        _context.SaveChanges();

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        organisation.SupplierInfo.OperationTypes = new List<OrganisationInformation.OperationType> { OrganisationInformation.OperationType.PublicService };
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.Details.Vcse.Should().BeTrue();
        shareData1.Details.Scale.Should().Be("large");
        shareData1.Details.ShelteredWorkshop.Should().BeFalse();
        shareData1.Details.PublicServiceMissionOrganization.Should().BeFalse();

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.Details.Vcse.Should().BeFalse();
        shareData2.Details.Scale.Should().Be("large");
        shareData2.Details.ShelteredWorkshop.Should().BeFalse();
        shareData2.Details.PublicServiceMissionOrganization.Should().BeTrue();
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectLegalForms_WhenChangingDataBetweenShareCodeCreation()
    {

    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectIdentifiers_WhenChangingDataBetweenShareCodeCreation()
    {

    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectOrganisationAddress_WhenChangingDataBetweenShareCodeCreation()
    {

    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectContactPoints_WhenChangingDataBetweenShareCodeCreation()
    {

    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectAddresses_WhenChangingDataBetweenShareCodeCreation()
    {

    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectConnectedEntityOrganisation_WhenChangingDataBetweenShareCodeCreation()
    {

    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectConnectedEntityIndividual_WhenChangingDataBetweenShareCodeCreation()
    {

    }

    private Organisation CreateOrganisation(string orgName)
    {
        var organisation = new Organisation
        {
            Guid = Guid.NewGuid(),
            Name = orgName,
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = orgName,
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

    private void ClearDatabase()
    {
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""organisations"" CASCADE;");
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""tenants"" CASCADE;");
    }
}
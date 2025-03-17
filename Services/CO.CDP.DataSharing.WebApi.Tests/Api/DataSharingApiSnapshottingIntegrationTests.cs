using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
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

        // Create first share code
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
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        organisation.SupplierInfo = new()
        {
            SupplierType = OrganisationInformation.SupplierType.Organisation,
            LegalForm = new()
            {
                LawRegistered = "Law",
                RegisteredLegalForm = "Registered legal form",
                RegisteredUnderAct2006 = true,
                RegistrationDate = DateTime.Parse("2025-03-14")
            }
        };
        _context.SaveChanges();

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        organisation.SupplierInfo.LegalForm.LawRegistered = "Updated law";
        organisation.SupplierInfo.LegalForm.RegisteredLegalForm = "Updated registered legal form";
        organisation.SupplierInfo.LegalForm.RegisteredUnderAct2006 = false;
        organisation.SupplierInfo.LegalForm.RegistrationDate = DateTime.Parse("2025-03-15");
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.Details.LegalForm.Should().NotBeNull();
        shareData1.Details.LegalForm!.LawRegistered.Should().Be("Law");
        shareData1.Details.LegalForm.RegisteredLegalForm.Should().Be("Registered legal form");
        shareData1.Details.LegalForm.RegisteredUnderAct2006.Should().BeTrue();
        shareData1.Details.LegalForm.RegistrationDate.Should().Be("2025-03-14");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.Details.LegalForm.Should().NotBeNull();
        shareData2.Details.LegalForm!.LawRegistered.Should().Be("Updated law");
        shareData2.Details.LegalForm.RegisteredLegalForm.Should().Be("Updated registered legal form");
        shareData2.Details.LegalForm.RegisteredUnderAct2006.Should().BeFalse();
        shareData2.Details.LegalForm.RegistrationDate.Should().Be("2025-03-15");
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectIdentifiers_WhenChangingDataBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        organisation.Identifiers.Add(new Organisation.Identifier
        {
            IdentifierId = "7654321",
            Scheme = "Something else",
            LegalName = "Different org legal name",
            Primary = false,
            Uri = "http://something.com/7654321"
        });
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.Identifier.LegalName.Should().Be("Org legal name");
        shareData1.Identifier.Id.Should().Be("1234567");
        shareData1.Identifier.Scheme.Should().Be("Whatever");
        shareData1.Identifier.Uri.Should().Be("http://whatever.com/1234567");

        shareData1.AdditionalIdentifiers.Should().HaveCount(0);

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.Identifier.LegalName.Should().Be("Org legal name");
        shareData2.Identifier.Id.Should().Be("1234567");
        shareData2.Identifier.Scheme.Should().Be("Whatever");

        shareData2.AdditionalIdentifiers.Should().HaveCount(1);
        shareData2.AdditionalIdentifiers.First().LegalName.Should().Be("Different org legal name");
        shareData2.AdditionalIdentifiers.First().Id.Should().Be("7654321");
        shareData2.AdditionalIdentifiers.First().Scheme.Should().Be("Something else");
        shareData2.AdditionalIdentifiers.First().Uri.Should().Be("http://something.com/7654321");
    }

    //[Fact]
    //public async Task DataSharingClientReturnsCorrectOrganisationAddresses_WhenChangingDataBetweenShareCodeCreation()
    //{
    //    // This test is primarily about the organisation_address table - i.e. adding a second (postal) address to the organisation
    //    // Because the address is not returned in the response, this test is not currently possible

    //    // Setup
    //    ClearDatabase();
    //    Organisation organisation = CreateOrganisation("Test org");

    //    // Create first share code
    //    CreateSharedConsent(organisation);
    //    var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

    //    // Update data
    //    organisation.Addresses.Add(new() {
    //        Type = OrganisationInformation.AddressType.Postal,
    //        Address = new() {
    //            StreetAddress = "456 Somewhere postal",
    //            Locality = "Postal locality",
    //            Region = "Postal region",
    //            PostalCode = "PL1 1LP",
    //            CountryName = "Postal country name",
    //            Country = "FR"
    //        }
    //    });

    //    _context.SaveChanges();

    //    // Create second share code
    //    CreateSharedConsent(organisation);
    //    var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

    //    // Verify original data in first share code
    //    var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);

    //    // TODO: Would need to verify here that the postal address did not appear.
    //    // This is not currently possible as the address is not returned in the response.

    //    shareData1.Address.StreetAddress.Should().Be("1234 New St");
    //    shareData1.Address.Locality.Should().Be("New City");
    //    shareData1.Address.Region.Should().Be("W.Yorkshire");
    //    shareData1.Address.PostalCode.Should().Be("123456");
    //    shareData1.Address.CountryName.Should().Be("Newland");
    //    shareData1.Address.Country.Should().Be("GB");

    //    // Verify updated data in second share code
    //    var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
    //    shareData2.Address.StreetAddress.Should().Be("456 Somewhere else ");
    //    shareData2.Address.Locality.Should().Be("Updated locality");
    //    shareData2.Address.Region.Should().Be("Updated region");
    //    shareData2.Address.PostalCode.Should().Be("PL1 1LP");
    //    shareData2.Address.CountryName.Should().Be("Updated country name");
    //    shareData2.Address.Country.Should().Be("FR");

    //    // TODO: Would need to verify here that the postal address appears.
    //    // This is not currently possible as the address is not returned in the response.
    //}

    [Fact]
    public async Task DataSharingClientReturnsCorrectContactPoints_WhenChangingDataBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        organisation.ContactPoints.First().Name = "Updated contact name";
        organisation.ContactPoints.First().Email = "something@else.com";
        organisation.ContactPoints.First().Url = "http://www.something.com";
        organisation.ContactPoints.First().Telephone = "9876543210";
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.ContactPoint.Name.Should().Be("Main Contact");
        shareData1.ContactPoint.Email.Should().Be("foo@bar.com");
        shareData1.ContactPoint.Url.Should().Be("http://www.bar.com");
        shareData1.ContactPoint.Telephone.Should().Be("0123456789");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.ContactPoint.Name.Should().Be("Updated contact name");
        shareData2.ContactPoint.Email.Should().Be("something@else.com");
        shareData2.ContactPoint.Url.Should().Be("http://www.something.com");
        shareData2.ContactPoint.Telephone.Should().Be("9876543210");
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectAddress_WhenChangingDataBetweenShareCodeCreation()
    {
        // This test is primarily about the addresses table - i.e. editing the main organisation address

        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        organisation.Addresses.First().Address.StreetAddress = "456 Somewhere else ";
        organisation.Addresses.First().Address.Locality = "Updated locality";
        organisation.Addresses.First().Address.Region = "Updated region";
        organisation.Addresses.First().Address.PostalCode = "PL1 1LP";
        organisation.Addresses.First().Address.CountryName = "Updated country name";
        organisation.Addresses.First().Address.Country = "FR";
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.Address.StreetAddress.Should().Be("1234 New St");
        shareData1.Address.Locality.Should().Be("New City");
        shareData1.Address.Region.Should().Be("W.Yorkshire");
        shareData1.Address.PostalCode.Should().Be("123456");
        shareData1.Address.CountryName.Should().Be("Newland");
        shareData1.Address.Country.Should().Be("GB");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.Address.StreetAddress.Should().Be("456 Somewhere else ");
        shareData2.Address.Locality.Should().Be("Updated locality");
        shareData2.Address.Region.Should().Be("Updated region");
        shareData2.Address.PostalCode.Should().Be("PL1 1LP");
        shareData2.Address.CountryName.Should().Be("Updated country name");
        shareData2.Address.Country.Should().Be("FR");
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectConnectedEntityOrganisation_WhenChangingDataBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        ConnectedEntity connectedEntity = CreateConnectedEntity(organisation, ConnectedEntity.ConnectedEntityType.Organisation, DateTime.Now.AddDays(-1), null);

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        connectedEntity.Organisation!.Name = "Updated org name";
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.AdditionalEntities.Should().HaveCount(1);
        shareData1.AdditionalEntities.First().Name.Should().Be("Test org");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.AdditionalEntities.Should().HaveCount(1);
        shareData2.AdditionalEntities.First().Name.Should().Be("Updated org name");
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectConnectedEntityIndividual_WhenChangingDataBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        ConnectedEntity connectedEntity = CreateConnectedEntity(organisation, ConnectedEntity.ConnectedEntityType.Individual, DateTime.Now.AddDays(-1), null);

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        connectedEntity.IndividualOrTrust!.FirstName = "Updated first name";
        connectedEntity.IndividualOrTrust!.LastName = "Updated last name";
        _context.SaveChanges();

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.AssociatedPersons.Should().HaveCount(1);
        shareData1.AssociatedPersons.First().Name.Should().Be("John Doe");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.AssociatedPersons.Should().HaveCount(1);
        shareData2.AssociatedPersons.First().Name.Should().Be("Updated first name Updated last name");
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectConnectedIndividuals_WhenAddingExtraBetweenShareCodeCreation()
    {
        // Some overlap here with the DataSharingApiConnectedPersonsIntegrationTests, but slightly different pattern in the tests so still has value

        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        ConnectedEntity connectedEntity = CreateConnectedEntity(organisation, ConnectedEntity.ConnectedEntityType.Individual, DateTime.Now.AddHours(-1), null);

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        ConnectedEntity connectedEntity2 = CreateConnectedEntity(organisation, ConnectedEntity.ConnectedEntityType.Individual, DateTime.Now, null, "Bob", "Bobbington");

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.AssociatedPersons.Should().HaveCount(1);
        shareData1.AssociatedPersons.First().Name.Should().Be("John Doe");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.AssociatedPersons.Should().HaveCount(2);
        shareData2.AssociatedPersons.First().Name.Should().Be("John Doe");
        shareData2.AssociatedPersons.Last().Name.Should().Be("Bob Bobbington");
    }

    [Fact]
    public async Task DataSharingClientReturnsCorrectConnectedIndividuals_WhenEndDatingOneBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        ConnectedEntity connectedEntity = CreateConnectedEntity(organisation, ConnectedEntity.ConnectedEntityType.Individual, DateTime.Now.AddDays(-2), null);

        // Create first share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        connectedEntity.EndDate = DateTime.Now.AddDays(-1);
        _context.SaveChanges();        
        ConnectedEntity connectedEntity2 = CreateConnectedEntity(organisation, ConnectedEntity.ConnectedEntityType.Individual, DateTime.Now, null, "Bob", "Bobbington");

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _client.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.AssociatedPersons.Should().HaveCount(1);
        shareData1.AssociatedPersons.First().Name.Should().Be("John Doe");

        // Verify updated data in second share code
        var shareData2 = await _client.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.AssociatedPersons.Should().HaveCount(1);
        shareData2.AssociatedPersons.First().Name.Should().Be("Bob Bobbington");
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
                    LegalName = "Org legal name",
                    Primary = true,
                    Uri = "http://whatever.com/1234567"
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
                    Email = "foo@bar.com",
                    Url = "http://www.bar.com",
                    Telephone = "0123456789"
                }
            }
        };

        _context.Organisations.Add(organisation);
        _context.SaveChanges();

        return organisation;
    }

    private ConnectedEntity CreateConnectedEntity(Organisation organisation, ConnectedEntity.ConnectedEntityType type, DateTime connectedPersonCreationDate, DateTime? connectedPersonEndDate, string? firstName = null, string? LastName = null)
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
            case ConnectedEntity.ConnectedEntityType.Individual:
                entity.IndividualOrTrust = new ConnectedEntity.ConnectedIndividualTrust
                {
                    Category = ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
                    FirstName = firstName ?? "John",
                    LastName = LastName ?? "Doe",
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

        return entity;
    }

    private void CreateSharedConsent(Organisation organisation)
    {
        var form = _context.Forms.Where(f => f.Guid == supplierInformationFormId).First();

        _context.SharedConsents.Add(new OrganisationInformation.Persistence.Forms.SharedConsent
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            FormVersionId = form.Version,
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTimeOffset.Now,
        });
       
        _context.SaveChanges();
    }

    private void ClearDatabase()
    {
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""organisations"" CASCADE;");
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""tenants"" CASCADE;");
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""identifiers"" CASCADE;");
    }
}
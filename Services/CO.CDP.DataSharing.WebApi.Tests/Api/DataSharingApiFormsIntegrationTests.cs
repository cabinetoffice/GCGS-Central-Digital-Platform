using CO.CDP.DataSharing.WebApi.Model;
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

public class DataSharingApiFormsIntegrationTests : IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationInformationPostgreSqlFixture _postgreSql;
    private readonly OrganisationInformationContext _context;
    private readonly IDataSharingClient _client;
    private readonly Guid supplierInformationFormId = new("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");

    public DataSharingApiFormsIntegrationTests(ITestOutputHelper testOutputHelper, OrganisationInformationPostgreSqlFixture postgreSql)
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
    public async Task DataSharingClient_ReturnsDocumentUriCorrectly_ForNormalOrganisation()
    {
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        var sharedConsent = CreateSharedConsent(organisation);
        CreateFinancialInformation(organisation, sharedConsent);
        var createShareCodeResponse = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponse.ShareCode);
        var financialInformationAnswerSet = response.SupplierInformationData.AnswerSets.First();

        var answer1 = financialInformationAnswerSet.Answers.First(q => q.QuestionName == "_FinancialInformation01");
        answer1.DateValue.Should().Be(DateTime.Now.Date);

        var answer2 = financialInformationAnswerSet.Answers.First(q => q.QuestionName == "_FinancialInformation02");
        answer2.DocumentUri?.ToString().Should().EndWith(string.Format("/share/data/{0}/document/a_dummy_file.pdf", createShareCodeResponse.ShareCode));

        var answer3 = financialInformationAnswerSet.Answers.First(q => q.QuestionName == "_FinancialInformation03");
        answer3.BoolValue.Should().BeTrue();
    }

    [Fact]
    public async Task DataSharingClient_ReturnsDocumentUriCorrectly_ForConsortium()
    {
        ClearDatabase();

        // Create org 1
        Organisation organisation1 = CreateOrganisation("Test org 1");
        var sharedConsent1 = CreateSharedConsent(organisation1);
        CreateFinancialInformation(organisation1, sharedConsent1);
        var createShareCodeResponseOrganisation1 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation1.Guid));

        // Create org 2
        Organisation organisation2 = CreateOrganisation("Test org 2");
        var sharedConsent2 = CreateSharedConsent(organisation2);
        CreateFinancialInformation(organisation2, sharedConsent1);
        var createShareCodeResponseOrganisation2 = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation2.Guid));

        // Stick them in a consortium
        Organisation consortium = CreateOrganisation("Consortium", OrganisationInformation.OrganisationType.InformalConsortium);
        var sharedConsentConsortium = CreateSharedConsent(consortium);
        AssociateSharedConsentsWithConsortium([sharedConsent1, sharedConsent2], sharedConsentConsortium);
        var createShareCodeResponseConsortium = await _client.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, consortium.Guid));

        var response = await _client.GetSharedDataAsync(createShareCodeResponseConsortium.ShareCode);
        var financialInformationAnswerSet = response.SupplierInformationData.AnswerSets.First();

        var answer1 = financialInformationAnswerSet.Answers.First(q => q.QuestionName == "_FinancialInformation01");
        answer1.DateValue.Should().Be(DateTime.Now.Date);

        var answer2 = financialInformationAnswerSet.Answers.First(q => q.QuestionName == "_FinancialInformation02");
        answer2.DocumentUri?.ToString().Should().EndWith(string.Format("/share/data/{0}/document/a_dummy_file.pdf", createShareCodeResponseOrganisation1.ShareCode));

        var answer3 = financialInformationAnswerSet.Answers.First(q => q.QuestionName == "_FinancialInformation03");
        answer3.BoolValue.Should().BeTrue();
    }

    private void AssociateSharedConsentsWithConsortium(List<OrganisationInformation.Persistence.Forms.SharedConsent> sharedConsents, OrganisationInformation.Persistence.Forms.SharedConsent sharedConsentConsortium)
    {
        foreach (var sharedConsent in sharedConsents)
        {
            _context.SharedConsentConsortiums.Add(new()
            {
                ChildSharedConsentId = sharedConsent.Id,
                ParentSharedConsentId = sharedConsentConsortium.Id
            });
        }        

        _context.SaveChanges();
    }

    private Organisation CreateOrganisation(string name, OrganisationInformation.OrganisationType type = OrganisationInformation.OrganisationType.Organisation)
    {
        var organisation = new Organisation
        {
            Guid = Guid.NewGuid(),
            Name = name,
            Type = type,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = name,
            },
            Identifiers = new List<Identifier>
            {
                new Identifier
                {
                    IdentifierId = "1234567" + name,
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

    private OrganisationInformation.Persistence.Forms.SharedConsent CreateSharedConsent(Organisation organisation)
    {
        var form = _context.Forms.Where(f => f.Guid == supplierInformationFormId).First();

        var sharedConsent = _context.SharedConsents.Add(new OrganisationInformation.Persistence.Forms.SharedConsent
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

        return sharedConsent.Entity;
    }

    private void CreateFinancialInformation(Organisation organisation, OrganisationInformation.Persistence.Forms.SharedConsent sharedConsent)
    {
        var financialInformationSection = _context.Set<FormSection>()
            .Where(s => s.Form.Guid == supplierInformationFormId && s.Title == "FinancialInformation_SectionTitle")
            .Include(s => s.Form)
            .Include(s => s.Questions)
            .First();

        var formAnswerSet = new OrganisationInformation.Persistence.Forms.FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SectionId = financialInformationSection.Id,
            Section = financialInformationSection,
            CreatedOn = DateTimeOffset.Now.ToUniversalTime(),
            UpdatedOn = DateTimeOffset.Now.ToUniversalTime(),
            SharedConsent = sharedConsent,
            SharedConsentId = sharedConsent.Id,
            FurtherQuestionsExempted = false,
        };

        // Answer question 1 - end date
        var question1 = financialInformationSection.Questions.First(q => q.Title == "FinancialInformation_01_Title");
        var answer1 = new OrganisationInformation.Persistence.Forms.FormAnswer
        {
            Guid = Guid.NewGuid(),
            FormAnswerSetId = formAnswerSet.Id,
            QuestionId = question1.Id,
            Question = question1,
            DateValue = DateTime.UtcNow,
            CreatedOn = DateTimeOffset.Now.ToUniversalTime(),
            UpdatedOn = DateTimeOffset.Now.ToUniversalTime(),
        };
        formAnswerSet.Answers.Add(answer1);

        // Answer question 2 - file upload
        var question2 = financialInformationSection.Questions.First(q => q.Title == "FinancialInformation_02_Title");
        var answer2 = new OrganisationInformation.Persistence.Forms.FormAnswer
        {
            Guid = Guid.NewGuid(),
            FormAnswerSetId = formAnswerSet.Id,
            QuestionId = question2.Id,
            Question = question2,
            TextValue = "a_dummy_file.pdf",
            CreatedOn = DateTimeOffset.Now.ToUniversalTime(),
            UpdatedOn = DateTimeOffset.Now.ToUniversalTime(),
        };
        formAnswerSet.Answers.Add(answer2);

        // Answer question 3 - accounts audited
        var question3 = financialInformationSection.Questions.First(q => q.Title == "FinancialInformation_03_Title");
        var answer3 = new OrganisationInformation.Persistence.Forms.FormAnswer
        {
            Guid = Guid.NewGuid(),
            FormAnswerSetId = formAnswerSet.Id,
            QuestionId = question3.Id,
            Question = question3,
            BoolValue = true,
            CreatedOn = DateTimeOffset.Now.ToUniversalTime(),
            UpdatedOn = DateTimeOffset.Now.ToUniversalTime(),
        };
        formAnswerSet.Answers.Add(answer3);

        _context.FormAnswerSets.Add(formAnswerSet);

        _context.SaveChanges();
    }

    private void ClearDatabase()
    {
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""organisations"" CASCADE;");
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""tenants"" CASCADE;");
    }
}
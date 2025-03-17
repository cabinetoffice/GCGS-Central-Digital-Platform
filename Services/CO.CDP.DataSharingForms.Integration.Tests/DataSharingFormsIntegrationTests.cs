using FormsProgram = CO.CDP.Forms.WebApi.Program;
using DataSharingProgram = CO.CDP.DataSharing.WebApi.Program;
using CO.CDP.DataSharing.WebApi;
using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;
using ShareRequest = CO.CDP.DataSharing.WebApiClient.ShareRequest;
namespace CO.CDP.DataSharingForms.Integration.Tests;

public class DataSharingFormsIntegrationTests : IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly OrganisationInformationPostgreSqlFixture _postgreSql;
    private readonly OrganisationInformationContext _context;
    private readonly IDataSharingClient _dataSharingClient;
    private readonly IFormsClient _formsClient;
    private readonly Guid supplierInformationFormId = new("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");
    private readonly Guid qualificationsFormSectionId = new("798cf1c1-40be-4e49-9adb-252219d5599d");

    public DataSharingFormsIntegrationTests(ITestOutputHelper testOutputHelper, OrganisationInformationPostgreSqlFixture postgreSql)
    {
        TestWebApplicationFactory<CO.CDP.DataSharing.WebApi.Program> dataSharingfactory = new(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, services) =>
            {
                services.RemoveAll<OrganisationInformationContext>();
                services.AddScoped(_ => postgreSql.OrganisationInformationContext());
            });
        });

        TestWebApplicationFactory<CO.CDP.Forms.WebApi.Program> formsfactory = new(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, services) =>
            {
                services.RemoveAll<OrganisationInformationContext>();
                services.AddScoped(_ => postgreSql.OrganisationInformationContext());
            });
        });

        var dataSharingHttpClient = dataSharingfactory.CreateClient();
        var formsHttpClient = formsfactory.CreateClient();
        _postgreSql = postgreSql;
        _context = _postgreSql.OrganisationInformationContext();
        _dataSharingClient = new DataSharingClient("https://localhost", dataSharingHttpClient);
        _formsClient = new FormsClient("https://localhost", formsHttpClient);
    }   

    [Fact]
    public async Task DataSharingClient_ReturnsCorrectFormAnswer_WhenAddingAnswersDataBetweenShareCodeCreation()
    {
        // Setup
        ClearDatabase();
        Organisation organisation = CreateOrganisation("Test org");
        var form = GetForm();
        await AnswerFormQuestions(organisation, form, qualificationsFormSectionId, ["Qualification name", "Plymouth College", DateTime.Now]);

        // Create first share code
        var createShareCodeResponse1 = await _dataSharingClient.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Update data
        await AnswerFormQuestions(organisation, form, qualificationsFormSectionId, ["Another qualification", "Exeter College", DateTime.Now]);

        // Create second share code
        CreateSharedConsent(organisation);
        var createShareCodeResponse2 = await _dataSharingClient.CreateSharedDataAsync(new ShareRequest(supplierInformationFormId, organisation.Guid));

        // Verify original data in first share code
        var shareData1 = await _dataSharingClient.GetSharedDataAsync(createShareCodeResponse1.ShareCode);
        shareData1.SupplierInformationData.AnswerSets.Should().HaveCount(1);

        // Verify updated data in second share code
        var shareData2 = await _dataSharingClient.GetSharedDataAsync(createShareCodeResponse2.ShareCode);
        shareData2.SupplierInformationData.AnswerSets.Should().HaveCount(2);
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

    private OrganisationInformation.Persistence.Forms.SharedConsent CreateSharedConsent(Organisation organisation)
    {
        var form = GetForm();

        var sharedConsent = new OrganisationInformation.Persistence.Forms.SharedConsent
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            FormVersionId = form.Version,
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTimeOffset.Now,
        };

        _context.SharedConsents.Add(sharedConsent);

        _context.SaveChanges();

        return sharedConsent;
    }

    private OrganisationInformation.Persistence.Forms.Form GetForm()
    {
        return _context.Forms.Where(f => f.Guid == supplierInformationFormId).First();
    }

    private async Task AnswerFormQuestions(Organisation organisation, OrganisationInformation.Persistence.Forms.Form form, Guid sectionId, List<object> answersList)
    {
        var formSection = _context.FormSections.First(s => s.Guid == sectionId);

        var nullAddressValue = new CO.CDP.Forms.WebApiClient.FormAddress(null, null, null, null, null, null);

        List<Forms.WebApiClient.FormAnswer> answers = new();

        //var sharedConsent = CreateSharedConsent(organisation);
        
        //var formAnswerSet = CreateAnswerset(organisation, sharedConsent, sectionId);

        int index = 0;

        foreach (var question in formSection.Questions)
        {
            DateTime? dateValue = null;
            string? textValue = null;

            switch (question.Type)
            {
                case OrganisationInformation.Persistence.Forms.FormQuestionType.Date:
                    dateValue = (DateTime)answersList[index];
                    break;

                case OrganisationInformation.Persistence.Forms.FormQuestionType.Text:
                    textValue = (string)answersList[index];
                    break;
            }

            answers.Add(new Forms.WebApiClient.FormAnswer(nullAddressValue, null, dateValue, null, Guid.NewGuid(), null, null, null, question.Guid, null, textValue));

            index++;
        }
      
        await _formsClient.PutFormSectionAnswersAsync(form.Guid, sectionId, organisation.Guid, Guid.NewGuid(), new UpdateFormSectionAnswers(answers, true));
    }

    private void ClearDatabase()
    {
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""organisations"" CASCADE;");
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""tenants"" CASCADE;");
        _context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE ""identifiers"" CASCADE;");
    }
}
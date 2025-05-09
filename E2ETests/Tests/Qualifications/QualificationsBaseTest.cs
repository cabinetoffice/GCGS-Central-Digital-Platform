using E2ETests.ApiTests;
using E2ETests.Pages;
using E2ETests.Pages.Qualifications;

namespace E2ETests.Tests.Qualifications;

public class QualificationsBaseTest : BaseTest
{
    protected static string _organisationId;

    // Page Objects
    protected OrganisationSupplierInformationPage _organisationSupplierInformationPage;
    protected QualificationSummaryPage _qualificationsSummaryPage;
    protected QualificationYesNoPage _qualificationsYesNoPage;
    protected QualificationNamePage _qualificationNamePage;
    protected QualificationWhoAwardedPage _qualificationWhoAwardedPage;
    protected QualificationWhenAwardedPage _qualificationWhenAwardedPage;
    protected QualificationCheckYourAnswersPage _qualificationCheckYourAnswersPage;
    

    [SetUp]
    public async Task SetupQualificationData()
    {
        await Setup();

        string accessToken = GetAccessToken();
        string storageKey = "Qualification_Org";

        await OrganisationApi.CreateOrganisation(accessToken, "QualificationOrg", storageKey);
        _organisationId = OrganisationApi.GetOrganisationId(storageKey);
        Console.WriteLine($"📌 Stored Organisation ID for Qualifications Tests: {_organisationId}");

        _qualificationsSummaryPage = new QualificationSummaryPage(_page);
        _qualificationsYesNoPage = new QualificationYesNoPage(_page);
        _qualificationNamePage = new QualificationNamePage(_page);
        _qualificationWhoAwardedPage = new QualificationWhoAwardedPage(_page);
        _qualificationWhenAwardedPage = new QualificationWhenAwardedPage(_page);
        _qualificationCheckYourAnswersPage = new QualificationCheckYourAnswersPage(_page);
        _organisationSupplierInformationPage = new OrganisationSupplierInformationPage(_page);        
    }
}
using E2ETests.ApiTests;
using E2ETests.Pages.Exclusions;

namespace E2ETests.Tests.Exclusions;

public class ExclusionsBaseTest : BaseTest
{
    protected static string _organisationId;

    // Page Objects
    protected ExclusionsDeclarationPage _exclusionsDeclarationPage;
    protected AddExclusionsYesNoPage _addExclusionsYesNoPage;
    protected ExclusionsInUKYesNoPage _exclusionsInUKYesNoPage;
    protected SelectExclusionPage _selectExclusionPage;
    protected SelectExclusionAppliesToPage _selectExclusionAppliesToPage;
    protected ExclusionsEnterEmailPage _exclusionsEnterEmailPage;
    protected ExclusionsDescribeInDetailsPage _exclusionsDescribeInDetailsPage;
    protected ExclusionsHowBeingManagedPage _exclusionsHowBeingManagedPage;
    protected ExclusionsSupportingDocumentsYesNoPage _exclusionsSupportingDocumentsYesNoPage;
    protected ExclusionsDecisionRecordedYesNoPage _exclusionsDecisionRecordedYesNoPage;
    protected ExclusionsEndedYesNoPage _exclusionsEndedYesNoPage;
    protected ExclusionsCheckAnswerPage _exclusionsCheckAnswerPage;
    protected ExclusionSummaryPage _exclusionSummaryPage;

    [SetUp]
    public async Task SetupQualificationData()
    {
        await Setup();

        string accessToken = GetAccessToken();
        string storageKey = "Exclusions_Org";

        await OrganisationApi.CreateOrganisation(accessToken, "ExclusionsOrg", storageKey);

        _organisationId = OrganisationApi.GetOrganisationId(storageKey);

        Console.WriteLine($"📌 Stored Organisation ID for Exclusions Tests: {_organisationId}");

        _exclusionsDeclarationPage = new ExclusionsDeclarationPage(_page);
        _addExclusionsYesNoPage = new AddExclusionsYesNoPage(_page);
        _exclusionsInUKYesNoPage = new ExclusionsInUKYesNoPage(_page);
        _selectExclusionPage = new SelectExclusionPage(_page);
        _selectExclusionAppliesToPage = new SelectExclusionAppliesToPage(_page);
        _exclusionsEnterEmailPage = new ExclusionsEnterEmailPage(_page);
        _exclusionsDescribeInDetailsPage = new ExclusionsDescribeInDetailsPage(_page);
        _exclusionsHowBeingManagedPage = new ExclusionsHowBeingManagedPage(_page);
        _exclusionsSupportingDocumentsYesNoPage = new ExclusionsSupportingDocumentsYesNoPage(_page);
        _exclusionsDecisionRecordedYesNoPage = new ExclusionsDecisionRecordedYesNoPage(_page);
        _exclusionsEndedYesNoPage = new ExclusionsEndedYesNoPage(_page);
        _exclusionsCheckAnswerPage = new ExclusionsCheckAnswerPage(_page);
        _exclusionSummaryPage = new ExclusionSummaryPage(_page);
    }
}
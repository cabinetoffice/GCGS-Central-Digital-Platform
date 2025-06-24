namespace E2ETests.OrganisationDetails;

public class OrganisationDetailsNavigationTests : OrganisationDetailsBaseTest
{
    private const string OrgKey = "OrganisationDetails_Org";

    [Test]
    public async Task NavigateToYourOrganisationDetailsPage()
    {
        await _yourOrganisationDetailsPage.NavigateTo(OrgKey);
        TestContext.Out.WriteLine("✅ Navigated to Your Organisation Details Page");
    }
}
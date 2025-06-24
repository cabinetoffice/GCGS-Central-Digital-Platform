namespace E2ETests.Tests.SuperAdmin;

internal class SuperAdminFunctionalTests : SuperAdminBaseTest
{
    private const string OrganisationKey = "OrganisationDetails_Org";

    [Category("SuperAdmin")]
    [Test]
    public async Task NavigateToChangeOrganisationNamePage()
    {
        TestContext.Out.WriteLine("🔹 Scenario: SuperAdmin able to change Organisation Name");

        string uniqueOrgName = $"{OrganisationKey} {Guid.NewGuid()}";

        await _organisationDetailsPage.NavigateTo(OrganisationKey);
        await _organisationDetailsPage.ClickChangeOrganisationName();
        await _organisationName.CompletePage(uniqueOrgName);
        await _organisationDetailsPage.AssertOrganisationName(uniqueOrgName);

        TestContext.Out.WriteLine("✅ Completed Change Organisation Name Journey");
    }
}

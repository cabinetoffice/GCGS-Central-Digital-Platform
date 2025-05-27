namespace E2ETests.Tests.Exclusions;

public class ExclusionsNavigationTest : ExclusionsBaseTest
{
    private const string OrgKey = "Exclusions_Org";
    [Test]
    public async Task NavigateToYesNoPage()
    {
        await _addExclusionsYesNoPage.NavigateTo(OrgKey);
        TestContext.Out.WriteLine("✅ Navigated to Add Exclusions Yes or No Page");
    }
}
using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.Pages;

namespace E2ETests.Tests.Qualifications;

public class QualificationsNavigationTest : QualificationsBaseTest
{
    private const string OrgKey = "Qualification_Org";

    [Test]
    public async Task NavigateToYesNoPage()
    {
        await _qualificationsYesNoPage.NavigateTo(OrgKey);
        TestContext.WriteLine("✅ Navigated to Yes or No Page");
    }
}
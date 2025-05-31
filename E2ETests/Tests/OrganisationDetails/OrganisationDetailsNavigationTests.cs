using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.Pages;

namespace E2ETests.OrganisationDetails
{
    public class OrganisationDetailsNavigationTests : OrganisationDetailsBaseTest
    {
        private const string OrgKey = "OrganisationDetails_Org";

        [Test]
        public async Task NavigateToYourOrganisationDetailsPage()
        {
            await _yourOrganisationDetailsPage.NavigateTo(OrgKey);
            TestContext.WriteLine("✅ Navigated to Your Organisation Details Page");
        }
    }
}
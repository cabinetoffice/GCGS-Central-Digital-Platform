using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.Pages;

namespace E2ETests.ConnectedPersons
{
    public class ConnectedPersonsFunctionalTests : ConnectedPersonsBaseTest
    {
        [Test]
        public async Task AddConnectedPersonsJourneyHappyPath()
        {
            TestContext.WriteLine("🔹 Scenario: Completing the full journey of adding a connected person.");

            await _declarationPage.NavigateTo("ConnectedPersons_Org");
            await _declarationPage.CompletePage();

            await _supplierCompanyQuestionPage.CompletePage(isRegistered: false);
            await _personTypePage.CompletePage(selection: "Organisation");
            await _organisationCategoryPage.CompletePage(category: "Registered company");
            await _organisationNamePage.CompletePage(organisationName: "Test Connected Org");

            await _registeredAddressPage.CompletePage(
                addressLine1: "1 Test Gardens",
                city: "Test City",
                postcode: "B22 2GG"
            );

            await _postalAddressSamePage.CompletePage(isSame: true);
            await _lawRegisterPage.CompletePage(legalForm: "Limited Company", lawEnforced: "Companies Act 2006");
            await _companyQuestionPage.CompletePage(isRegistered: false);
            await _checkYourAnswerPage.CompletePage();

            // Navigate back to ensure navigation works
            await _summaryPage.ClickBack();
            await _checkYourAnswerPage.IsLoaded();

            TestContext.WriteLine("✅ Successfully completed the Connected Persons journey.");
        }
    }
}

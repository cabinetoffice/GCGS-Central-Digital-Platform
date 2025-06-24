namespace E2ETests.ConnectedPersons;

public class ConnectedPersonsFunctionalTests : ConnectedPersonsBaseTest
{
    [Test]
    public async Task AddConnectedPersonsJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing the full journey of adding a connected person.");

        string uniqueOrgId = Guid.NewGuid().ToString();
        string uniqueOrgName = $"ConnectedPersons_ {uniqueOrgId}";


        await _declarationPage.NavigateTo("ConnectedPersons_Org");
        await _declarationPage.CompletePage();
        await _supplierHasControlPage.CompletePage(true);
        await _supplierCompanyQuestionPage.CompletePage(true);
        await _personTypePage.CompletePage("Organisation");
        await _organisationCategoryPage.CompletePage("Registered Company");
        await _organisationNamePage.CompletePage(uniqueOrgName);
        await _registeredAddressPage.CompletePage("AddressLine1", "Town", "Postcode");
        await _postalAddressSamePage.CompletePage(false);
        await _lawRegisterPage.CompletePage("Law", "LawRegistered");
        await _companyQuestionPage.CompletePage(false);
        await _natureOfControlPage.CompletePage(new List<string> { "owns shares", "has voting rights" });
        await _dateRegisteredPage.CompletePage("27", "03", "2007");
        await _companyRegisterNamePage.CompletePage("Companies House");
        await _checkYourAnswersPage.CompletePage();
        await _summaryPage.CompletePage(false);
        await _organisationSupplierInformationPage.AssertConnectedPersonsCount(1);

    }


    [Test]
    public async Task AddMultipleConnectedPersonsJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Add multiple Connected Persons.");

        string uniqueOrgId = Guid.NewGuid().ToString();
        string uniqueOrgName = $"ConnectedPersons_ {uniqueOrgId}";


        await _declarationPage.NavigateTo("ConnectedPersons_Org");
        await _declarationPage.CompletePage();
        await _supplierHasControlPage.CompletePage(true);
        await _supplierCompanyQuestionPage.CompletePage(true);
        await _personTypePage.CompletePage("Organisation");
        await _organisationCategoryPage.CompletePage("Registered Company");
        await _organisationNamePage.CompletePage(uniqueOrgName);
        await _registeredAddressPage.CompletePage("AddressLine1", "Town", "Postcode");
        await _postalAddressSamePage.CompletePage(false);
        await _lawRegisterPage.CompletePage("Law", "LawRegistered");
        await _companyQuestionPage.CompletePage(false);
        await _natureOfControlPage.CompletePage(new List<string> { "owns shares", "has voting rights" });
        await _dateRegisteredPage.CompletePage("27", "03", "2007");
        await _companyRegisterNamePage.CompletePage("Companies House");
        await _checkYourAnswersPage.CompletePage();
        await _summaryPage.CompletePage(true);
        await _declarationPage.NavigateTo("ConnectedPersons_Org");
        await _declarationPage.CompletePage();
        await _supplierHasControlPage.CompletePage(true);
        await _supplierCompanyQuestionPage.CompletePage(true);
        await _personTypePage.CompletePage("Organisation");
        await _organisationCategoryPage.CompletePage("Registered Company");
        await _organisationNamePage.CompletePage(uniqueOrgName);
        await _registeredAddressPage.CompletePage("AddressLine1", "Town", "Postcode");
        await _postalAddressSamePage.CompletePage(false);
        await _lawRegisterPage.CompletePage("Law", "LawRegistered");
        await _companyQuestionPage.CompletePage(false);
        await _natureOfControlPage.CompletePage(new List<string> { "owns shares", "has voting rights" });
        await _dateRegisteredPage.CompletePage("27", "03", "2007");
        await _companyRegisterNamePage.CompletePage("Companies House");
        await _checkYourAnswersPage.CompletePage();
        await _summaryPage.CompletePage(false);

        await _organisationSupplierInformationPage.AssertConnectedPersonsCount(2);

    }

}

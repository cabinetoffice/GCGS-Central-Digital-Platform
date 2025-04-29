using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.ApiTests;
using E2ETests.Utilities;
using E2ETests.Pages;

namespace E2ETests.ConnectedPersons
{
    public class ConnectedPersonsBaseTest : BaseTest
    {
        protected static string _organisationId;

        // Page Objects
        protected ConnectedPersonsOrganisationCategoryPage _organisationCategoryPage;
        protected ConnectedPersonsCheckYourAnswersPage _checkYourAnswerPage;
        protected ConnectedPersonDeclarationPage _declarationPage;
        protected ConnectedPersonsSupplierCompanyQuestionPage _supplierCompanyQuestionPage;
        protected ConnectedPersonPersonTypePage _personTypePage;
        protected ConnectedPersonsOrganisationNamePage _organisationNamePage;
        protected ConnectedPersonsOrganisationRegisteredAddressPage _registeredAddressPage;
        protected ConnectedPersonsPostalAddressSamePage _postalAddressSamePage;
        protected ConnectedPersonsLawRegisterPage _lawRegisterPage;
        protected ConnectedPersonsCompanyQuestionPage _companyQuestionPage;
        protected ConnectedPersonsCheckYourAnswersPage _checkYourAnswersPage;
        protected ConnectedPersonsSummaryPage _summaryPage;
        protected ConnectedPersonsSupplierHasControlPage _supplierHasControlPage;
        protected ConnectedPersonsNatureOfControl _natureOfControlPage;
        protected ConnectedPersonsDateRegisteredPage _dateRegisteredPage;
        protected ConnectedPersonsCompanyRegisterNamePage _companyRegisterNamePage;
        protected OrganisationSupplierInformationPage _organisationSupplierInformationPage;
        

       [SetUp]
        public async Task SetupConnectedPersonsData()
        {
            await base.Setup();

            string accessToken = GetAccessToken();
            string storageKey = "ConnectedPersons_Org";

            await OrganisationApi.CreateOrganisation(accessToken, "ConnectedPersonsOrg", storageKey);
            _organisationId = OrganisationApi.GetOrganisationId(storageKey);
            Console.WriteLine($"📌 Stored Organisation ID for ConnectedPersons Tests: {_organisationId}");

            _organisationCategoryPage = new ConnectedPersonsOrganisationCategoryPage(_page);
            _declarationPage = new ConnectedPersonDeclarationPage(_page);
            _supplierCompanyQuestionPage = new ConnectedPersonsSupplierCompanyQuestionPage(_page);
            _supplierHasControlPage = new ConnectedPersonsSupplierHasControlPage(_page);
            _personTypePage = new ConnectedPersonPersonTypePage(_page);
            _organisationNamePage = new ConnectedPersonsOrganisationNamePage(_page);
            _registeredAddressPage = new ConnectedPersonsOrganisationRegisteredAddressPage(_page);
            _postalAddressSamePage = new ConnectedPersonsPostalAddressSamePage(_page);
            _lawRegisterPage = new ConnectedPersonsLawRegisterPage(_page);
            _companyQuestionPage = new ConnectedPersonsCompanyQuestionPage(_page);
            _checkYourAnswersPage = new ConnectedPersonsCheckYourAnswersPage(_page);
            _summaryPage = new ConnectedPersonsSummaryPage(_page);
            _natureOfControlPage = new ConnectedPersonsNatureOfControl(_page);
            _dateRegisteredPage = new ConnectedPersonsDateRegisteredPage(_page);
            _companyRegisterNamePage = new ConnectedPersonsCompanyRegisterNamePage(_page);
            _organisationSupplierInformationPage = new OrganisationSupplierInformationPage(_page);
            
        }
    }
}

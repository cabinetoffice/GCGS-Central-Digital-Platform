using System.Threading.Tasks;
using NUnit.Framework;
using E2ETests.ApiTests;
using E2ETests.Utilities;
using E2ETests.Pages;

namespace E2ETests.OrganisationDetails
{
    public class OrganisationDetailsBaseTest : BaseTest
    {
        protected static string _organisationId;

        // Page Objects
        protected YourOrganisationDetailsPage _yourOrganisationDetailsPage;
        protected SupplierTypePage _supplierTypePage;
        protected IndividualOrOrganisationPage _individualOrOrganisationPage;
        protected RegisteredAddressPage _registeredAddressPage;
        protected RegisteredAddressNonUkPage _registeredAddressNonUkPage;
        protected PostalAddressSameAsRegisteredPage _postalAddressSameAsRegisteredPage;
        protected PostalAddressUkPage _postalAddressUkPage;
        protected PostalAddressNonUkPage _postalAddressNonUkPage;
        protected VatQuestionPage _vatQuestionPage;
        protected WebsiteQuestionPage _websiteQuestionPage;
        protected OrganisationEmailAddressPage _organisationEmailAddressPage;
        protected OrganisationTypePage _organisationTypePage;
        protected IsCompaniesHouseRegisteredPage _isCompaniesHouseRegisteredPage;
        protected HowOrganisationRegisteredPage _howOrganisationRegisteredPage;
        protected DateOrganisationRegisteredPage _dateOrganisationRegisteredPage;

        [SetUp]
        public async Task SetupOrganisationDetailsData()
        {
            await base.Setup();

            string accessToken = GetAccessToken();
            string storageKey = "OrganisationDetails_Org";

            await OrganisationApi.CreateOrganisation(accessToken, "OrganisationDetailsOrg", storageKey);
            _organisationId = OrganisationApi.GetOrganisationId(storageKey);
            Console.WriteLine($"📌 Stored Organisation ID for OrganisationDetails Tests: {_organisationId}");

            _yourOrganisationDetailsPage = new YourOrganisationDetailsPage(_page);
            _supplierTypePage = new SupplierTypePage(_page);
            _individualOrOrganisationPage = new IndividualOrOrganisationPage(_page);
            _registeredAddressPage = new RegisteredAddressPage(_page);
            _registeredAddressNonUkPage = new RegisteredAddressNonUkPage(_page);
            _postalAddressSameAsRegisteredPage = new PostalAddressSameAsRegisteredPage(_page);
            _postalAddressUkPage = new PostalAddressUkPage(_page);
            _postalAddressNonUkPage = new PostalAddressNonUkPage(_page);
            _vatQuestionPage = new VatQuestionPage(_page);
            _websiteQuestionPage = new WebsiteQuestionPage(_page);
            _organisationEmailAddressPage = new OrganisationEmailAddressPage(_page);
            _organisationTypePage = new OrganisationTypePage(_page);
            _isCompaniesHouseRegisteredPage = new IsCompaniesHouseRegisteredPage(_page);
            _howOrganisationRegisteredPage = new HowOrganisationRegisteredPage(_page);
            _dateOrganisationRegisteredPage = new DateOrganisationRegisteredPage(_page);
        }
    }
}

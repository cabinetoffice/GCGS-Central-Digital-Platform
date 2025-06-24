namespace E2ETests.OrganisationDetails;

public class OrganisationDetailsFunctionalTests : OrganisationDetailsBaseTest
{
    private const string OrganisationKey = "OrganisationDetails_Org";

    [Category("OrganisationDetails")]
    [Test]
    public async Task CompleteOrganisationDetailsJourney_WithUkRegisteredAddresses()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing organisation details journey using UK addresses");

        string uniqueEmail = $"john+{Guid.NewGuid():N}@johnson.com";

        await _yourOrganisationDetailsPage.NavigateTo(OrganisationKey);
        await _yourOrganisationDetailsPage.ClickChangeSupplierType();
        await _individualOrOrganisationPage.CompletePage(true);

        await _yourOrganisationDetailsPage.ClickChangeRegisteredAddress();
        await _registeredAddressPage.CompletePage("123 High Street", "Birmingham", "B1 1AA");

        await _yourOrganisationDetailsPage.AssertRegisteredAddressContains("123 High Street");
        await _yourOrganisationDetailsPage.AssertRegisteredAddressContains("Birmingham");
        await _yourOrganisationDetailsPage.AssertRegisteredAddressContains("B1 1AA");

        await _yourOrganisationDetailsPage.ClickAddPostalAddress();
        await _postalAddressSameAsRegisteredPage.CompletePage(true);
        await _postalAddressUkPage.CompletePage("456 Secondary Road", "London", "E1 2BB");

        await _yourOrganisationDetailsPage.ClickAddVatNumber();
        await _vatQuestionPage.CompletePage(true, "GB123456789");

        await _yourOrganisationDetailsPage.ClickChangeWebsiteAddress();
        await _websiteQuestionPage.CompletePage(true, "https://website.co.uk");

        await _yourOrganisationDetailsPage.ClickChangeEmailAddress();
        await _organisationEmailAddressPage.CompletePage(uniqueEmail);

        await _yourOrganisationDetailsPage.ClickAddOrganisationType();
        await _organisationTypePage.CompletePage(new[] { "SupportedEmployment" });

        await _yourOrganisationDetailsPage.ClickAddLegalForm();
        await _isCompaniesHouseRegisteredPage.CompletePage(true);
        await _howOrganisationRegisteredPage.CompletePage("LimitedCompany");
        await _dateOrganisationRegisteredPage.CompletePage("27", "03", "2007");

        TestContext.Out.WriteLine("✅ Completed Organisation Details Journey with UK address");
    }

    [Test]
    [Category("OrganisationDetails")]
    public async Task CompleteOrganisationDetailsJourney_WithNonUkRegisteredAddress()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing journey with non-UK registered address");

        string uniqueEmail = $"nonuk+{Guid.NewGuid():N}@example.com";

        await _yourOrganisationDetailsPage.NavigateTo(OrganisationKey);
        await _yourOrganisationDetailsPage.ClickChangeSupplierType();
        await _individualOrOrganisationPage.CompletePage(true);

        await _yourOrganisationDetailsPage.ClickChangeRegisteredAddress();
        await _registeredAddressPage.ClickEnterNonUkAddress();
        await _registeredAddressNonUkPage.CompletePage("1 Rue de Lyon", "Paris", "75001", "FR");

        await _yourOrganisationDetailsPage.AssertRegisteredAddressContains("1 Rue de Lyon");
        await _yourOrganisationDetailsPage.AssertRegisteredAddressContains("Paris");
        await _yourOrganisationDetailsPage.AssertRegisteredAddressContains("75001");
        await _yourOrganisationDetailsPage.AssertRegisteredAddressContains("FRANCE");

        TestContext.Out.WriteLine("✅ Completed Organisation Details Journey with non-UK registered address");
    }


    [Test]
    [Category("OrganisationDetails")]
    public async Task CompleteOrganisationDetailsJourney_WithPostalAddressUk()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing journey with UK postal address");

        await _yourOrganisationDetailsPage.NavigateTo(OrganisationKey);
        await _yourOrganisationDetailsPage.ClickChangeSupplierType();
        await _individualOrOrganisationPage.CompletePage(true);
        await _yourOrganisationDetailsPage.ClickChangeRegisteredAddress();
        await _registeredAddressPage.CompletePage("123 High Street", "Birmingham", "B1 1AA");
        await _yourOrganisationDetailsPage.ClickAddPostalAddress();
        await _postalAddressSameAsRegisteredPage.CompletePage(true);
        await _postalAddressUkPage.CompletePage("321 Low Street", "Birmingham", "Y2 1ZZ");
        await _yourOrganisationDetailsPage.AssertPostalAddressContains("321 Low Street");
        await _yourOrganisationDetailsPage.AssertPostalAddressContains("Birmingham");
        await _yourOrganisationDetailsPage.AssertPostalAddressContains("Y2 1ZZ");

        TestContext.Out.WriteLine("✅ Entered UK postal address");
    }

    [Category("OrganisationDetails")]
    [Test]
    public async Task CompleteOrganisationDetailsJourney_WithNonUkPostalAddress()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing journey with non-UK postal address");

        await _yourOrganisationDetailsPage.NavigateTo(OrganisationKey);
        await _yourOrganisationDetailsPage.ClickChangeSupplierType();
        await _individualOrOrganisationPage.CompletePage(true);
        await _yourOrganisationDetailsPage.ClickAddPostalAddress();
        await _postalAddressSameAsRegisteredPage.CompletePage(true);
        await _postalAddressUkPage.ClickEnterNonUkAddress();
        await _postalAddressNonUkPage.CompletePage("1 Rue de Lyon", "Paris", "75001", "FR");

        await _yourOrganisationDetailsPage.AssertPostalAddressContains("1 Rue de Lyon");
        await _yourOrganisationDetailsPage.AssertPostalAddressContains("Paris");
        await _yourOrganisationDetailsPage.AssertPostalAddressContains("75001");
        await _yourOrganisationDetailsPage.AssertPostalAddressContains("FRANCE");

        TestContext.Out.WriteLine("✅ Entered Non-UK postal address");
    }

    [Category("OrganisationDetails")]
    [Test]
    public async Task CompleteOrganisationDetailsJourney_WithPostalAddressSameAsOrg()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing journey with non-UK postal address");

        await _yourOrganisationDetailsPage.NavigateTo(OrganisationKey);
        await _yourOrganisationDetailsPage.ClickChangeSupplierType();
        await _individualOrOrganisationPage.CompletePage(true);
        await _yourOrganisationDetailsPage.ClickChangeRegisteredAddress();
        await _registeredAddressPage.CompletePage("123 High Street", "Birmingham", "B1 1AA");
        await _yourOrganisationDetailsPage.ClickAddPostalAddress();
        await _postalAddressSameAsRegisteredPage.CompletePage(false);
        await _yourOrganisationDetailsPage.AssertOrganisationAndPostalAddressSame();

        TestContext.Out.WriteLine("✅ Entered Non-UK postal address");
    }

    [Category("OrganisationDetails")]
    [Test]
    public async Task CompleteOrganisationDetailsJourney_AsIndividualSupplier()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Completing journey as an Individual supplier");

        string uniqueEmail = $"individual+{Guid.NewGuid():N}@example.com";

        await _yourOrganisationDetailsPage.NavigateTo(OrganisationKey);

        await _yourOrganisationDetailsPage.ClickChangeSupplierType();
        await _individualOrOrganisationPage.CompletePage(false); // false = Individual

        await _yourOrganisationDetailsPage.ClickAddPostalAddress();
        await _postalAddressUkPage.CompletePage("Flat 9", "Manchester", "M1 2AB");

        await _yourOrganisationDetailsPage.ClickAddVatNumber();
        await _vatQuestionPage.CompletePage(true, "GB987654321");

        await _yourOrganisationDetailsPage.ClickChangeWebsiteAddress();
        await _websiteQuestionPage.CompletePage(true, "https://independenttrader.biz");

        await _yourOrganisationDetailsPage.ClickChangeEmailAddress();
        await _organisationEmailAddressPage.CompletePage(uniqueEmail);

        await _yourOrganisationDetailsPage.AssertRegisteredAddressDoesNotExist();
        await _yourOrganisationDetailsPage.AssertSupplierType("Individual");
        await _yourOrganisationDetailsPage.AssertPostalAddressContains("Flat 9");

        TestContext.Out.WriteLine("✅ Completed Organisation Details Journey as Individual");
    }
}

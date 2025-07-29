using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules;

[TestFixture]
public class DataProtectionTestsWelsh : OrganisationDetailsBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheDataProtectionLongJourneyWelsh()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Data Protection");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe(
            "Cyflwyno eich gwybodaeth am ddiogelu data - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Start")
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "A oes gennych yr adnoddau i sicrhau cydymffurfiaeth â GDPR y DU? - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Do/Oes/Ydy")
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Pryd bydd gennych yr adnoddau i sicrhau cydymffurfiaeth â GDPR y DU? - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Do/Oes/Ydy")
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Sut mae sicrhau cydymffurfiaeth â chyfraith diogelu data y DU? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for data protection !@£$123");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Naddo/Nac oes/Nac ydy")
        await InteractionUtilities.ClickRadioButtonByText("Rhif");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Gwiriwch eich atebion - Canfod Tendr - GOV.UK");




    }


        [Test]
    public async Task ShouldCompleteTheDataProtectionErrorMessageJourneyWelsh()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Data Protection");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe(
            "Cyflwyno eich gwybodaeth am ddiogelu data - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Start")
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "A oes gennych yr adnoddau i sicrhau cydymffurfiaeth â GDPR y DU? - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Do/Oes/Ydy")
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Pryd bydd gennych yr adnoddau i sicrhau cydymffurfiaeth â GDPR y DU? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        //Step below to be updated once change/update is pushed (Should be "Do/Oes/Ydy")
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Sut mae sicrhau cydymffurfiaeth â chyfraith diogelu data y DU? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for data protection !@£$123");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Naddo/Nac oes/Nac ydy")
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewis ffeil");
        await InteractionUtilities.ClickRadioButtonByText("Rhif");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Gwiriwch eich atebion - Canfod Tendr - GOV.UK");

    }

}
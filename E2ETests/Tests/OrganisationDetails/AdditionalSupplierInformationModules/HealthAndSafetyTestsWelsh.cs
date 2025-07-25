using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules;

[TestFixture]
public class HealthAndSafetyTestsWelsh : OrganisationDetailsBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheHealthAndSafetyJourneyWelsh()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Health and Safety");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe(
            "Cyflwyno eich gwybodaeth am iechyd a diogelwch - Canfod Tendr - GOV.UK");
        // Step below to be updated once change/update is pushed (Should be "Dechrau")
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Sut ydych yn rheoli iechyd a diogelwch? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for health and Safety !@£$123");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        // Step below to be updated once change/update is pushed (Should be "Do/Oes/Ydy")
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        await InteractionUtilities.PageShouldContainText("Example test for health and Safety !@£$123");
        await InteractionUtilities.PageShouldContainText("cat");
        await InteractionUtilities.PageShouldContainText(".jpeg");
        // Step below to be updated once change/update is pushed (Should be "Do/Oes/Ydy")
        await InteractionUtilities.PageShouldContainText("Yes");
        await InteractionUtilities.ClickSecondLinkByText("Newid");
        await InteractionUtilities.PageTitleShouldBe(
            "A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        // Step below to be updated once change/update is pushed (Should be "Naddo/Nac oes/Nac ydy")
        await InteractionUtilities.ClickRadioButtonByText("Rhif");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        // Step below to be updated once change/update is pushed (Should be "Naddo/Nac oes/Nac ydy")
        await InteractionUtilities.PageShouldContainText("No");
        await InteractionUtilities.PageShouldContainText("Example test for health and Safety !@£$123");
        await InteractionUtilities.ClickButtonByText("Cadw");
    }

    [Test]
    public async Task ShouldCompleteTheHealthAndSafetyErrorsJourneyWelsh()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Health and Safety");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe(
            "Cyflwyno eich gwybodaeth am iechyd a diogelwch - Canfod Tendr - GOV.UK");
        //Step below to be updated once change/update is pushed (Should be "Start")
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Sut ydych yn rheoli iechyd a diogelwch? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for health and Safety !@£$123");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        // Step below to be updated once change/update is pushed (Should be "Do/Oes/Ydy")
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewis ffeil");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        await InteractionUtilities.PageShouldContainText("Example test for health and Safety !@£$123");
        await InteractionUtilities.ClickButtonByText("Cadw");
    }
}
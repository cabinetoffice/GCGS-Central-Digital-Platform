using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules;

[TestFixture]
public class SteelTestsWelsh : OrganisationDetailsBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheSteelModuleJourneyWelsh()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Steel module");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe("Cyflwyno eich gwybodaeth am ddur - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("Rheoli'r gadwyn gyflenwi ym maes caffael dur - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("I love steel");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        await InteractionUtilities.PageShouldContainText(".jpeg");
        await InteractionUtilities.ClickSecondLinkByText("Newid");
        await InteractionUtilities.PageTitleShouldBe("A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Rhif");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        await InteractionUtilities.PageShouldContainText("No");
        await InteractionUtilities.ClickButtonByText("Cadw");
    }

    [Test]
    public async Task ShouldCompleteTheSteelModuleErrorsJourneyWelsh()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Steel module");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe("Cyflwyno eich gwybodaeth am ddur - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("Rheoli'r gadwyn gyflenwi ym maes caffael dur - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("I love steel");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("A oes gennych ddogfennau ategol i'w lanlwytho? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewis ffeil");
        await InteractionUtilities.ClickRadioButtonByText("Rhif");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Cadw");
    }

}
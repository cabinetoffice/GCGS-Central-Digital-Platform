using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules;

[TestFixture]
public class SteelTests : OrganisationDetailsBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheSteelModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Steel module");
        await InteractionUtilities.PageTitleShouldBe("Submitting your steel information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe("Supply chain management in steel procurement - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("I love steel");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.PageShouldContainText(".jpeg");
        await InteractionUtilities.ClickSecondLinkByText("Change");
        await InteractionUtilities.PageTitleShouldBe("Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.PageShouldContainText("No");

        await InteractionUtilities.ClickButtonByText("Save");
    }

    [Test]
    public async Task ShouldCompleteTheSteelModuleErrorsJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Steel module");
        await InteractionUtilities.PageTitleShouldBe("Submitting your steel information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe("Supply chain management in steel procurement - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("I love steel");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Save");
    }

}
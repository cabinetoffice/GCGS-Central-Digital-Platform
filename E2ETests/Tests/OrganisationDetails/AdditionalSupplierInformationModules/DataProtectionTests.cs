using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules;

[TestFixture]
public class DataProtectionTests : OrganisationDetailsBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheDataProtectionLongJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Data Protection");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your data protection information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I have the resources or will have the resources by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently have the resources");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "How do you ensure compliance with UK data protection law? - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for data protection !@£$123");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickNthLinkByText("Change", 3);
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
                "Check your answers - Find a Tender - GOV.UK");
        // await InteractionUtilities.PageShouldContainText("Yes, I have the resources or will have the resources by contract award");
        // await InteractionUtilities.PageShouldContainText("Yes, I currently have the resources");
        await InteractionUtilities.PageShouldContainText("Example test for data protection !@£$123");        await InteractionUtilities.PageShouldContainText("cat");
        await InteractionUtilities.PageShouldContainText(".jpeg");



    }

    [Test]
    public async Task ShouldCompleteTheDataProtectionMediumJourney()
    {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Data Protection");
            await InteractionUtilities.PageTitleShouldBe(
                "Submitting your data protection information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Start");
            await InteractionUtilities.PageTitleShouldBe(
                "Do you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("No, I do not have the resources");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "How do you ensure compliance with UK data protection law? - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("Example test for data protection !@£$123");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Do you have supporting documents to upload? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Check your answers - Find a Tender - GOV.UK");
            // await InteractionUtilities.PageShouldContainText("No, I don't have the resources");
            await InteractionUtilities.PageShouldContainText("Example test for data protection !@£$123");




    }

    [Test]
    public async Task ShouldCompleteTheDataProtectionEditJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Data Protection");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your data protection information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No, I do not have the resources");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "How do you ensure compliance with UK data protection law? - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for data protection !@£$123");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        // await InteractionUtilities.PageShouldContainText("No, I don't have the resources");
        await InteractionUtilities.PageShouldContainText("Example test for data protection !@£$123");
        await InteractionUtilities.ClickNthLinkByText("Change", 0);
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I have the resources or will have the resources by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently have the resources");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "How do you ensure compliance with UK data protection law? - Find a Tender - GOV.UK");
        // await InteractionUtilities.PageShouldContainText("Example test for data protection !@£$123");
        await InteractionUtilities.EnterTextIntoTextArea("Updated test data protection");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        // await InteractionUtilities.PageShouldContainText("Example test for data protection !@£$123 Updated test data protection");


    }

        [Test]
    public async Task ShouldCompleteTheDataProtectionErrorMessageJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Data Protection");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your data protection information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I have the resources or will have the resources by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you have the resources to ensure compliance with UK GDPR? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("I currently have the resources");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "How do you ensure compliance with UK data protection law? - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for data protection !@£$123");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have supporting documents to upload? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");

    }

}
using E2ETests.Utilities;

namespace E2ETests.Tests.AdditionalSupplierInformationModules;

[TestFixture]
public class CyberEssentialsTests : CyberEssentialsBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheCyberEssentialsLongModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Cyber Essentials certification scheme information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDate(01, 06, 2035);
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDate(02, 07, 2040);
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
    }

    [Test]
    public async Task ShouldCompleteTheCyberEssentialsShortModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Cyber Essentials certification scheme information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No, I do not meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
    }

    [Test]
    public async Task ShouldCompleteTheCyberEssentialsMediumModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Cyber Essentials certification scheme information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I do not currently meet the requirements, but will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No, I do not meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
    }

    [Test]
    public async Task ShouldCompleteTheCyberEssentialsShortMediumModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Cyber Essentials certification scheme information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I do not currently meet the requirements, but will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I do not currently meet the requirements, but will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
    }

    [Test]
    public async Task ShouldCompleteTheCyberEssentialsErrorMessageJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Cyber Essentials certification scheme information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select a file");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Date must include a day");
        await InteractionUtilities.PageShouldContainText("Date must include a month");
        await InteractionUtilities.PageShouldContainText("Date must include a year");
        await InteractionUtilities.EnterDate(0!, 0!, 0!);
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Day must be a valid number");
        await InteractionUtilities.PageShouldContainText("Month must be a valid number");
        await InteractionUtilities.PageShouldContainText("Year must be a valid number");
        await InteractionUtilities.PageShouldContainText("Date must be a real date");
        await InteractionUtilities.EnterDateMinusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Date must be in the future");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select a file");
        await InteractionUtilities.UploadFileName("cat.jpeg");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Date must include a day");
        await InteractionUtilities.PageShouldContainText("Date must include a month");
        await InteractionUtilities.PageShouldContainText("Date must include a year");
        await InteractionUtilities.EnterDate(0!, 0!, 0!);
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Day must be a valid number");
        await InteractionUtilities.PageShouldContainText("Month must be a valid number");
        await InteractionUtilities.PageShouldContainText("Year must be a valid number");
        await InteractionUtilities.PageShouldContainText("Date must be a real date");
        await InteractionUtilities.EnterDateMinusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Date must be in the future");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
    }


    [Test]
    public async Task ShouldCompleteTheCyberEssentialsBackwardsNavigationJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Cyber Essentials certification scheme information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No, I do not meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I do not currently meet the requirements, but will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No, I do not meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No, I do not meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes, I meet the requirements or will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I do not currently meet the requirements, but will meet them by contract award");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickLinkByText2("Back");
        await InteractionUtilities.PageTitleShouldBe(
            "When will you meet the Cyber Essentials Plus certification requirements? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("I currently meet the requirements");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Upload evidence of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter the expiry date of your Cyber Essentials Plus certification (or equivalent) - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Check your answers - Find a Tender - GOV.UK");
    }
}
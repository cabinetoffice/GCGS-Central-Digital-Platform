using E2ETests.Utilities;

namespace E2ETests.Tests.AdditionalSupplierInformationModules;

[TestFixture]
public class CarbonNetZeroTests : CarbonNetZeroBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheCarbonNetZeroModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Carbon Net Zero");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Carbon Net Zero information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Have you published a Carbon Reduction Plan which meets the required reporting standard? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have a link to your most recently published Carbon Reduction Plan? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.EnterTextIntoInputField("testwebsite");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter your Carbon Reduction Plan expiry date - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDate(01, 06, 2040);
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your Net Zero target year - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputField("2035");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your baseline emissions data - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Baseline year", "2020");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 1", "10");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 2", "11");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 3", "12");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your current emissions data - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Reporting year", "2022");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 1", "13");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 2", "14");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 3", "15");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.PageShouldContainText("2035");
        await InteractionUtilities.PageShouldContainText("01 June 2040");
        await InteractionUtilities.PageShouldContainText("2020");
        await InteractionUtilities.PageShouldContainText("10");
        await InteractionUtilities.PageShouldContainText("11");
        await InteractionUtilities.PageShouldContainText("12");
        await InteractionUtilities.PageShouldContainText("13");
        await InteractionUtilities.PageShouldContainText("14");
        await InteractionUtilities.PageShouldContainText("15");
    }

    [Test]
    public async Task ShouldCompleteTheCarbonNetZeroModuleErrorsJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Carbon Net Zero");
        await InteractionUtilities.PageTitleShouldBe(
            "Submitting your Carbon Net Zero information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe(
            "Have you published a Carbon Reduction Plan which meets the required reporting standard? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Do you have a link to your most recently published Carbon Reduction Plan? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Enter a website address");
        await InteractionUtilities.EnterTextIntoInputField("!");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText(
            "Enter a website address in the correct format, like www.companyname.com");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Enter your Carbon Reduction Plan expiry date - Find a Tender - GOV.UK");
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
        await InteractionUtilities.EnterDate(01, 06, 2040);
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your Net Zero target year - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Enter a value");
        await InteractionUtilities.EnterTextIntoInputField("2035");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your baseline emissions data - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Enter a value");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Baseline year", "2020");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 1", "10");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 2", "11");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 3", "12");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your current emissions data - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Enter a value");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Reporting year", "2022");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 1", "13");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 2", "14");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Scope 3", "15");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
    }
}
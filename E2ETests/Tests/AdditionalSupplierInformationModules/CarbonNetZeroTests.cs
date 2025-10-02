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
    [Test, Category("Failing")]
    public async Task ShouldCompleteTheCarbonNetZeroModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Carbon Net Zero");
        await InteractionUtilities.PageTitleShouldBe("Submitting your Carbon Net Zero information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe("Have you published a Carbon Reduction Plan which meets the required reporting standard? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Do you have a link to your most recently published Carbon Reduction Plan? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.EnterTextIntoInputField("testwebsite");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your Carbon Reduction Plan expiry date - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDate(01, 06, 2040);
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your Net Zero target year - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputField("2035");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your baseline emissions data - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputField("Q_4625cdf8-6b68-497f-979a-144120c9c95e_TextInput", "2020");
        await InteractionUtilities.EnterTextIntoInputField("Q_5573b8f8-b7af-48db-8fae-6c1e7d098b44_TextInput", "10");
        await InteractionUtilities.EnterTextIntoInputField("Q_cdd6e1d2-730c-46d1-85d1-e5f5ef5ee5cd_TextInput", "11");
        await InteractionUtilities.EnterTextIntoInputField("Q_6270ccda-ca12-46b5-9033-cd53d3d3603c_TextInput", "12");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your current emissions data - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputField("Q_00377d25-a09f-4566-86d6-6681750a5386_TextInput", "2022");
        await InteractionUtilities.EnterTextIntoInputField("Q_7f9b84b6-1492-45cd-b04f-e825637fde52_TextInput", "13");
        await InteractionUtilities.EnterTextIntoInputField("Q_7667d2f9-8a44-4ae5-8f13-176385593dd6_TextInput", "14");
        await InteractionUtilities.EnterTextIntoInputField("Q_d9ca80c3-34f4-4376-bfae-741064085f0e_TextInput", "15");
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

    [Test, Category("Failing")]
    public async Task ShouldCompleteTheCarbonNetZeroModuleErrorsJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Carbon Net Zero");
        await InteractionUtilities.PageTitleShouldBe("Submitting your Carbon Net Zero information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Start");
        await InteractionUtilities.PageTitleShouldBe("Have you published a Carbon Reduction Plan which meets the required reporting standard? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Do you have a link to your most recently published Carbon Reduction Plan? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Select an option");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Enter a website address");
        await InteractionUtilities.EnterTextIntoInputField("!");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Enter a website address in the correct format, like www.companyname.com");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your Carbon Reduction Plan expiry date - Find a Tender - GOV.UK");
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
        await InteractionUtilities.EnterTextIntoInputField("Q_4625cdf8-6b68-497f-979a-144120c9c95e_TextInput", "2020");
        await InteractionUtilities.EnterTextIntoInputField("Q_5573b8f8-b7af-48db-8fae-6c1e7d098b44_TextInput", "10");
        await InteractionUtilities.EnterTextIntoInputField("Q_cdd6e1d2-730c-46d1-85d1-e5f5ef5ee5cd_TextInput", "11");
        await InteractionUtilities.EnterTextIntoInputField("Q_6270ccda-ca12-46b5-9033-cd53d3d3603c_TextInput", "12");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Enter your current emissions data - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageShouldContainText("Enter a value");
        await InteractionUtilities.EnterTextIntoInputField("Q_00377d25-a09f-4566-86d6-6681750a5386_TextInput", "2022");
        await InteractionUtilities.EnterTextIntoInputField("Q_7f9b84b6-1492-45cd-b04f-e825637fde52_TextInput", "13");
        await InteractionUtilities.EnterTextIntoInputField("Q_7667d2f9-8a44-4ae5-8f13-176385593dd6_TextInput", "14");
        await InteractionUtilities.EnterTextIntoInputField("Q_d9ca80c3-34f4-4376-bfae-741064085f0e_TextInput", "15");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
    }

}
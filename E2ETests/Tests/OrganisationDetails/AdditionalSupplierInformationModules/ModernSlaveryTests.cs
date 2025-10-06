using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules
{
    [TestFixture]
    public class ModernSlaveryTests : OrganisationDetailsBaseTest
    {
        private InteractionUtilities InteractionUtilities;

        [SetUp]
        public Task TestSetup()
        {
            InteractionUtilities = new InteractionUtilities(_page);
            return Task.CompletedTask;
        }

        [Test]
        public async Task ShouldCompleteTheModernSlaveryJourneyOne()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.PageTitleShouldBe(
                "Submitting your modern slavery information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Start");
            await InteractionUtilities.PageTitleShouldBe(
                "Are you a commercial organisation subject to Section 54 of the Modern Slavery Act 2015? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Yes");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Is your latest Modern Slavery Statement available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Yes");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter the link to your Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your organisation's Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
            await InteractionUtilities.PageShouldContainText("http://testwebsite");
            await InteractionUtilities.PageShouldContainText("test instructions on Modern Slavery Statement");
            await InteractionUtilities.PageShouldContainText("test text on missing information modern slavery");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your organisation's Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter the link to your Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Is your latest Modern Slavery Statement available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Upload a copy of your latest Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.UploadFile();
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your organisation's Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        }

[Test]
        public async Task ShouldCompleteTheModernSlaveryJourneyTwo()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.PageTitleShouldBe(
                "Submitting your modern slavery information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Start");
            await InteractionUtilities.PageTitleShouldBe(
                "Are you a commercial organisation subject to Section 54 of the Modern Slavery Act 2015? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Are your documents or statements related to modern slavery available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Yes");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter the link to your relevant documentation or published statements on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your relevant published statements or documents on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
            await InteractionUtilities.PageShouldContainText("http://testwebsite");
            await InteractionUtilities.PageShouldContainText("test instructions on Modern Slavery Statement");
            await InteractionUtilities.PageShouldContainText("test text on missing information modern slavery");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your relevant published statements or documents on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter the link to your relevant documentation or published statements on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Back");
            await InteractionUtilities.PageTitleShouldBe(
                "Are your documents or statements related to modern slavery available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Upload your relevant documentation or published statements on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.UploadFile();
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        }


[Test]
        public async Task ModernSlaveryJourneyOneErrors()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.PageTitleShouldBe(
                "Submitting your modern slavery information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Start");
            await InteractionUtilities.PageTitleShouldBe(
                "Are you a commercial organisation subject to Section 54 of the Modern Slavery Act 2015? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("Yes");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Is your latest Modern Slavery Statement available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("Yes");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter the link to your Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Enter a website address");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your organisation's Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("All information is required on this page");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("All information is required on this page");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        }

        [Test]
        public async Task ModernSlaveryJourneyTwoErrors()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.PageTitleShouldBe(
                "Submitting your modern slavery information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Start");
            await InteractionUtilities.PageTitleShouldBe(
                "Are you a commercial organisation subject to Section 54 of the Modern Slavery Act 2015? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("Yes");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Is your latest Modern Slavery Statement available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Upload a copy of your latest Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select a file");
            await InteractionUtilities.UploadFile();
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your organisation's Modern Slavery Statement - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("All information is required on this page");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("All information is required on this page");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        }

                [Test]
        public async Task ModernSlaveryJourneyThreeErrors()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.PageTitleShouldBe(
                "Submitting your modern slavery information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Start");
            await InteractionUtilities.PageTitleShouldBe(
                "Are you a commercial organisation subject to Section 54 of the Modern Slavery Act 2015? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Are your documents or statements related to modern slavery available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("Yes");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter the link to your relevant documentation or published statements on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Enter a website address");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Enter instructions on how to find your relevant published statements or documents on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("All information is required on this page");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("All information is required on this page");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
            await InteractionUtilities.PageShouldContainText("http://testwebsite");
            await InteractionUtilities.PageShouldContainText("test instructions on Modern Slavery Statement");
            await InteractionUtilities.PageShouldContainText("test text on missing information modern slavery");
        }

        [Test]
        public async Task ModernSlaveryJourneyFourErrors()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.PageTitleShouldBe(
                "Submitting your modern slavery information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Start");
            await InteractionUtilities.PageTitleShouldBe(
                "Are you a commercial organisation subject to Section 54 of the Modern Slavery Act 2015? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Are your documents or statements related to modern slavery available digitally? - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select an option");
            await InteractionUtilities.ClickRadioButtonByText("No");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Upload your relevant documentation or published statements on modern slavery - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("Select a file");
            await InteractionUtilities.UploadFile();
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe(
                "Missing information - Find a Tender - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageShouldContainText("All information is required on this page");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            await InteractionUtilities.ClickButtonByText("Continue");
            await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        }

    }
}
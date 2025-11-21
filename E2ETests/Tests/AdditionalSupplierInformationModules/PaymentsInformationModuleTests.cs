using E2ETests.Utilities;

namespace E2ETests.Tests.AdditionalSupplierInformationModules;

[TestFixture]
public class PaymentsInformationModuleTests : PaymentsInformationModuleBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteThePaymentsInformationModuleJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Payments Information");
        await InteractionUtilities.PageTitleShouldBe("Submitting your payments information - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Do you have processes to ensure you pay your supply chain on time? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Do you have processes to resolve disputed payments and invoices in your supply chain on time? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Confirmation of payment terms for public sector contracts - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Do you report under the Payment Practices Reporting Regulations? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.EnterTextIntoInputField("testwebsite");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Payment and invoice reporting within your immediate supply chain - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Day ", "19");
        await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Month ", "08");
        await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Year ", "2020");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Average number of days to pay an invoice", "55");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Within 30 days", "23");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("31 to 60 days", "45");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("61 days or more", "87");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("overdue within this reporting period", "10");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Payment and invoice reporting within your immediate supply chain - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Day ", "22");
        await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Month ", "5");
        await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Year ", "2021");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Average number of days to pay an invoice", "76");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("Within 30 days", "27");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("31 to 60 days", "44");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("61 days or more", "67");
        await InteractionUtilities.EnterTextIntoInputFieldByLabel("overdue within this reporting period", "32");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Explain why you have not paid all invoices within agreed contractual terms (where relevant) - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoTextArea("Example test for health and Safety !@£$123");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Have you shown that 95% or more of your payable invoices have been paid? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Yes");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Can you confirm you have paid all your invoices within an average of 55 days? - Find a Tender - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("No");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Submit your action plan (where relevant) - Find a Tender - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
        await InteractionUtilities.PageShouldContainText("http://testwebsite");
        await InteractionUtilities.PageShouldContainText("19 August 2020");
        await InteractionUtilities.PageShouldContainText("55");
        await InteractionUtilities.PageShouldContainText("23");
        await InteractionUtilities.PageShouldContainText("45");
        await InteractionUtilities.PageShouldContainText("87");
        await InteractionUtilities.PageShouldContainText("10");
        await InteractionUtilities.PageShouldContainText("22 May 2021");
        await InteractionUtilities.PageShouldContainText("76");
        await InteractionUtilities.PageShouldContainText("27");
        await InteractionUtilities.PageShouldContainText("44");
        await InteractionUtilities.PageShouldContainText("67");
        await InteractionUtilities.PageShouldContainText("32");
        await InteractionUtilities.ClickButtonByText("Save");
    }

     [Test]
     public async Task ShouldCompleteThePaymentInformationErrorsJourney()
     {
         var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
         await InteractionUtilities.NavigateToUrl(organisationPageUrl);
         await InteractionUtilities.ClickLinkByText("Complete supplier information");
         await InteractionUtilities.ClickLinkByText("Payments Information");
         await InteractionUtilities.PageTitleShouldBe("Submitting your payments information - Find a Tender - GOV.UK");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Do you have processes to ensure you pay your supply chain on time? - Find a Tender - GOV.UK");
         await InteractionUtilities.ClickRadioButtonByText("Yes");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Do you have processes to resolve disputed payments and invoices in your supply chain on time? - Find a Tender - GOV.UK");
         await InteractionUtilities.ClickRadioButtonByText("Yes");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Confirmation of payment terms for public sector contracts - Find a Tender - GOV.UK");
         await InteractionUtilities.ClickRadioButtonByText("Yes");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Do you report under the Payment Practices Reporting Regulations? - Find a Tender - GOV.UK");
         await InteractionUtilities.ClickRadioButtonByText("Yes");
         await InteractionUtilities.EnterTextIntoInputField("testwebsite");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe(
             "Payment and invoice reporting within your immediate supply chain - Find a Tender - GOV.UK");
         await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Day ", "19");
         await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Month ", "08");
         await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Year ", "2020");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("Average number of days to pay an invoice", "55");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("Within 30 days", "23");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("31 to 60 days", "45");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("61 days or more", "87");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("overdue within this reporting period", "10");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe(
             "Payment and invoice reporting within your immediate supply chain - Find a Tender - GOV.UK");
         await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Day ", "22");
         await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Month ", "5");
         await InteractionUtilities.EnterDateIntoInputFieldByLabel(" Year ", "2021");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("Average number of days to pay an invoice", "76");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("Within 30 days", "27");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("31 to 60 days", "44");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("61 days or more", "67");
         await InteractionUtilities.EnterTextIntoInputFieldByLabel("overdue within this reporting period", "32");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Explain why you have not paid all invoices within agreed contractual terms (where relevant) - Find a Tender - GOV.UK");
         await InteractionUtilities.EnterTextIntoTextArea("Example test for health and Safety !@£$123");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Have you shown that 95% or more of your payable invoices have been paid? - Find a Tender - GOV.UK");
         await InteractionUtilities.ClickRadioButtonByText("Yes");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Can you confirm you have paid all your invoices within an average of 55 days? - Find a Tender - GOV.UK");
         await InteractionUtilities.ClickRadioButtonByText("No");
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Submit your action plan (where relevant) - Find a Tender - GOV.UK");
         await InteractionUtilities.UploadFile();
         await InteractionUtilities.ClickButtonByText("Continue");
         await InteractionUtilities.PageTitleShouldBe("Check your answers - Find a Tender - GOV.UK");
         await InteractionUtilities.PageShouldContainText("http://testwebsite");
         await InteractionUtilities.PageShouldContainText("19 August 2020");
         await InteractionUtilities.PageShouldContainText("55");
         await InteractionUtilities.PageShouldContainText("23");
         await InteractionUtilities.PageShouldContainText("45");
         await InteractionUtilities.PageShouldContainText("87");
         await InteractionUtilities.PageShouldContainText("10");
         await InteractionUtilities.PageShouldContainText("22 May 2021");
         await InteractionUtilities.PageShouldContainText("76");
         await InteractionUtilities.PageShouldContainText("27");
         await InteractionUtilities.PageShouldContainText("44");
         await InteractionUtilities.PageShouldContainText("67");
         await InteractionUtilities.PageShouldContainText("32");
         await InteractionUtilities.ClickButtonByText("Save");
     }
}
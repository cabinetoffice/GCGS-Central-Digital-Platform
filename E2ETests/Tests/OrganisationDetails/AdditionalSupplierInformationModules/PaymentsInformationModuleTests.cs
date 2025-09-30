using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules;

[TestFixture]
public class PaymentsInformationModuleTests : OrganisationDetailsBaseTest
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
        await InteractionUtilities.EnterTextIntoInputField("Q_d694c45d-67cb-49b3-a8d1-93ee5a83ff6f_Day", "19");
        await InteractionUtilities.EnterTextIntoInputField("Q_d694c45d-67cb-49b3-a8d1-93ee5a83ff6f_Month", "08");
        await InteractionUtilities.EnterTextIntoInputField("Q_d694c45d-67cb-49b3-a8d1-93ee5a83ff6f_Year", "2020");
        await InteractionUtilities.EnterTextIntoInputField("Q_288d0d52-4f1c-4eef-b8b4-add93a0c969c_TextInput", "55");
        await InteractionUtilities.EnterTextIntoInputField("Q_2e041c6e-d6ca-43de-97b6-6740a31c751f_TextInput", "23");
        await InteractionUtilities.EnterTextIntoInputField("Q_98c3b526-530a-4631-8746-6ea4c6d13643_TextInput", "45");
        await InteractionUtilities.EnterTextIntoInputField("Q_5787c080-bca1-4f32-997a-99b3f22a6cc8_TextInput", "87");
        await InteractionUtilities.EnterTextIntoInputField("Q_1f53bd7d-e6db-4aaa-bc20-e66e1870de14_TextInput", "10");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Payment and invoice reporting within your immediate supply chain - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputField("Q_c10c04d5-4398-4a5d-a03d-8d89e0911b65_Day", "22");
        await InteractionUtilities.EnterTextIntoInputField("Q_c10c04d5-4398-4a5d-a03d-8d89e0911b65_Month", "5");
        await InteractionUtilities.EnterTextIntoInputField("Q_c10c04d5-4398-4a5d-a03d-8d89e0911b65_Year", "2021");
        await InteractionUtilities.EnterTextIntoInputField("Q_2f386bda-0f99-4a6d-bc8e-4b4a0720759e_TextInput", "76");
        await InteractionUtilities.EnterTextIntoInputField("Q_3f5f58cd-bbbe-43eb-bb5f-518b8ef8e027_TextInput", "27");
        await InteractionUtilities.EnterTextIntoInputField("Q_e41b66b3-7450-4c12-864e-4b4655e66662_TextInput", "44");
        await InteractionUtilities.EnterTextIntoInputField("Q_2e21ae61-6c31-4914-9236-23541cdd4225_TextInput", "67");
        await InteractionUtilities.EnterTextIntoInputField("Q_d470a443-716a-4fc4-a9c2-44ec8c1e095e_TextInput", "32");
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
    public async Task ShouldCompleteTheSteelModuleErrorsJourney()
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
        await InteractionUtilities.EnterTextIntoInputField("Q_d694c45d-67cb-49b3-a8d1-93ee5a83ff6f_Day", "19");
        await InteractionUtilities.EnterTextIntoInputField("Q_d694c45d-67cb-49b3-a8d1-93ee5a83ff6f_Month", "08");
        await InteractionUtilities.EnterTextIntoInputField("Q_d694c45d-67cb-49b3-a8d1-93ee5a83ff6f_Year", "2020");
        await InteractionUtilities.EnterTextIntoInputField("Q_288d0d52-4f1c-4eef-b8b4-add93a0c969c_TextInput", "55");
        await InteractionUtilities.EnterTextIntoInputField("Q_2e041c6e-d6ca-43de-97b6-6740a31c751f_TextInput", "23");
        await InteractionUtilities.EnterTextIntoInputField("Q_98c3b526-530a-4631-8746-6ea4c6d13643_TextInput", "45");
        await InteractionUtilities.EnterTextIntoInputField("Q_5787c080-bca1-4f32-997a-99b3f22a6cc8_TextInput", "87");
        await InteractionUtilities.EnterTextIntoInputField("Q_1f53bd7d-e6db-4aaa-bc20-e66e1870de14_TextInput", "10");
        await InteractionUtilities.ClickButtonByText("Continue");
        await InteractionUtilities.PageTitleShouldBe(
            "Payment and invoice reporting within your immediate supply chain - Find a Tender - GOV.UK");
        await InteractionUtilities.EnterTextIntoInputField("Q_c10c04d5-4398-4a5d-a03d-8d89e0911b65_Day", "22");
        await InteractionUtilities.EnterTextIntoInputField("Q_c10c04d5-4398-4a5d-a03d-8d89e0911b65_Month", "5");
        await InteractionUtilities.EnterTextIntoInputField("Q_c10c04d5-4398-4a5d-a03d-8d89e0911b65_Year", "2021");
        await InteractionUtilities.EnterTextIntoInputField("Q_2f386bda-0f99-4a6d-bc8e-4b4a0720759e_TextInput", "76");
        await InteractionUtilities.EnterTextIntoInputField("Q_3f5f58cd-bbbe-43eb-bb5f-518b8ef8e027_TextInput", "27");
        await InteractionUtilities.EnterTextIntoInputField("Q_e41b66b3-7450-4c12-864e-4b4655e66662_TextInput", "44");
        await InteractionUtilities.EnterTextIntoInputField("Q_2e21ae61-6c31-4914-9236-23541cdd4225_TextInput", "67");
        await InteractionUtilities.EnterTextIntoInputField("Q_d470a443-716a-4fc4-a9c2-44ec8c1e095e_TextInput", "32");
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
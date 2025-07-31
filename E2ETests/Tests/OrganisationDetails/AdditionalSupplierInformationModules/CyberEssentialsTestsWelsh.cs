using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules;

[TestFixture]
public class CyberEssentialsTestsWelsh : OrganisationDetailsBaseTest
{
    private InteractionUtilities InteractionUtilities;

    [SetUp]
    public Task TestSetup()
    {
        InteractionUtilities = new InteractionUtilities(_page);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldCompleteTheCyberEssentialsLongModuleJourneyWelsh()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe(
            "Cyflwyno eich gwybodaeth ar gyfer cynllun ardystio Cyber Essentials - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Ydych chi'n bodloni gofynion ardystio Cyber Essentials? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Pryd byddwch yn bodloni gofynion ardystio Cyber Essentials? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Lanlwythwch dystiolaeth o'ch ardystiad Cyber Essentials (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Cofnodwch ddyddiad dirwyn i ben eich ardystiad Cyber Essentials (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterDate(01, 06, 2017);
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Ydych chi'n bodloni gofynion ardystio Cyber Essentials Plus? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Pryd byddwch yn bodloni gofynion ardystio Cyber Essentials Plus? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Lanlwythwch dystiolaeth o'ch ardystiad Cyber Essentials Plus (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Cofnodwch ddyddiad dirwyn i ben eich ardystiad Cyber Essentials Plus (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.EnterDate(02, 07, 2018);
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
    }



    [Test]
    public async Task ShouldCompleteTheCyberEssentialsErrorMessageJourney()
    {
        var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await InteractionUtilities.NavigateToUrl(organisationPageUrl);
        await InteractionUtilities.ClickLinkByText("Complete supplier information");
        await InteractionUtilities.ClickLinkByText("Cyber Essentials certification scheme");
        await InteractionUtilities.ClickLinkByText("Cymraeg");
        await InteractionUtilities.PageTitleShouldBe(
            "Cyflwyno eich gwybodaeth ar gyfer cynllun ardystio Cyber Essentials - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Ydych chi'n bodloni gofynion ardystio Cyber Essentials? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Pryd byddwch yn bodloni gofynion ardystio Cyber Essentials? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Lanlwythwch dystiolaeth o'ch ardystiad Cyber Essentials (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewis ffeil");
        await InteractionUtilities.UploadFile();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Cofnodwch ddyddiad dirwyn i ben eich ardystiad Cyber Essentials (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad gynnwys diwrnod");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad gynnwys mis");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad gynnwys blwyddyn");
        await InteractionUtilities.EnterDate(0!, 0!, 0!);
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r diwrnod fod yn rhif dilys");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r mis fod yn rhif dilys");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r flwyddyn fod yn rhif dilys");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad fod yn ddyddiad go iawn");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i’r dyddiad fod heddiw neu yn y gorffennol");
        await InteractionUtilities.EnterDate(23, 8, 2012);
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Ydych chi'n bodloni gofynion ardystio Cyber Essentials Plus? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Pryd byddwch yn bodloni gofynion ardystio Cyber Essentials Plus? - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
        await InteractionUtilities.ClickRadioButtonByText("Do/Oes");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Lanlwythwch dystiolaeth o'ch ardystiad Cyber Essentials Plus (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Dewis ffeil");
        await InteractionUtilities.UploadFileName("cat.jpeg");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Cofnodwch ddyddiad dirwyn i ben eich ardystiad Cyber Essentials Plus (neu ardystiad cyfatebol) - Canfod Tendr - GOV.UK");
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad gynnwys diwrnod");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad gynnwys mis");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad gynnwys blwyddyn");
        await InteractionUtilities.EnterDate(0!, 0!, 0!);
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r diwrnod fod yn rhif dilys");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r mis fod yn rhif dilys");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r flwyddyn fod yn rhif dilys");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i'r dyddiad fod yn ddyddiad go iawn");
        await InteractionUtilities.EnterDatePlusOne();
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageShouldContainText("Mae'n rhaid i’r dyddiad fod heddiw neu yn y gorffennol");
        await InteractionUtilities.EnterDate(21, 3, 2009);
        await InteractionUtilities.ClickButtonByText("Parhau");
        await InteractionUtilities.PageTitleShouldBe(
            "Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
    }

}
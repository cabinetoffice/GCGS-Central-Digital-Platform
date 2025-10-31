using E2ETests.Utilities;
using E2ETests.OrganisationDetails;

namespace E2ETests.Tests.OrganisationDetails.AdditionalSupplierInformationModules
{
    [TestFixture]
    public class ModernSlaveryTestsWelsh : OrganisationDetailsBaseTest
    {
        private InteractionUtilities InteractionUtilities;

        [SetUp]
        public Task TestSetup()
        {
            InteractionUtilities = new InteractionUtilities(_page);
            return Task.CompletedTask;
        }

        [Test]
        public async Task ShouldCompleteTheModernSlaveryJourneyOneWelsh()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.ClickLinkByText("Cymraeg");
            await InteractionUtilities.PageTitleShouldBe(
                "Cyflwyno eich gwybodaeth am gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            //Step below to be updated once change/update is pushed (Should be "Start")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "A ydych yn sefydliad masnachol sy'n ddarostyngedig i Adran 54 o Ddeddf Caethwasiaeth Fodern 2015? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Do/oes");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch Datganiad Caethwasiaeth Fodern diweddaraf ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Do/oes");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofiwch gynnwys y ddolen i'ch Datganiad Caethwasiaeth Fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i Ddatganiad Caethwasiaeth Fodern eich sefydliad - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
            await InteractionUtilities.PageShouldContainText("http://testwebsite");
            await InteractionUtilities.PageShouldContainText("test instructions on Modern Slavery Statement");
            await InteractionUtilities.PageShouldContainText("test text on missing information modern slavery");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i Ddatganiad Caethwasiaeth Fodern eich sefydliad - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofiwch gynnwys y ddolen i'ch Datganiad Caethwasiaeth Fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch Datganiad Caethwasiaeth Fodern diweddaraf ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Rhif");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Lanlwythwch gopi o'ch Datganiad Caethwasiaeth Fodern diweddaraf - Canfod Tendr - GOV.UK");
            await InteractionUtilities.UploadFile();
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i Ddatganiad Caethwasiaeth Fodern eich sefydliad - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        }

[Test]
        public async Task ShouldCompleteTheModernSlaveryJourneyTwo()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.ClickLinkByText("Cymraeg");
            await InteractionUtilities.PageTitleShouldBe(
                "Cyflwyno eich gwybodaeth am gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            //Step below to be updated once change/update is pushed (Should be "Start")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "A ydych yn sefydliad masnachol sy'n ddarostyngedig i Adran 54 o Ddeddf Caethwasiaeth Fodern 2015? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Rhif");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch dogfennau neu ddatganiadau sy'n gysylltiedig â chaethwasiaeth fodern ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Do/oes");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch y ddolen i'ch dogfennaeth berthnasol neu ddatganiadau cyhoeddedig ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i'ch datganiadau cyhoeddedig neu ddogfennau perthnasol ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
            await InteractionUtilities.PageShouldContainText("http://testwebsite");
            await InteractionUtilities.PageShouldContainText("test instructions on Modern Slavery Statement");
            await InteractionUtilities.PageShouldContainText("test text on missing information modern slavery");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i'ch datganiadau cyhoeddedig neu ddogfennau perthnasol ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch y ddolen i'ch dogfennaeth berthnasol neu ddatganiadau cyhoeddedig ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickLinkByText2("Yn ôl");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch dogfennau neu ddatganiadau sy'n gysylltiedig â chaethwasiaeth fodern ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickRadioButtonByText("Rhif");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Lanlwythwch eich dogfennaeth berthnasol neu ddatganiadau cyhoeddedig ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.UploadFile();
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        }


[Test]
        public async Task ModernSlaveryJourneyOneErrors()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.ClickLinkByText("Cymraeg");
            await InteractionUtilities.PageTitleShouldBe(
                "Cyflwyno eich gwybodaeth am gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            //Step below to be updated once change/update is pushed (Should be "Start")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "A ydych yn sefydliad masnachol sy'n ddarostyngedig i Adran 54 o Ddeddf Caethwasiaeth Fodern 2015? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Do/oes");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch Datganiad Caethwasiaeth Fodern diweddaraf ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Do/oes");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofiwch gynnwys y ddolen i'ch Datganiad Caethwasiaeth Fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Rhowch gyfeiriad gwefan");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i Ddatganiad Caethwasiaeth Fodern eich sefydliad - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Mae angen yr holl wybodaeth ar y dudalen hon");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Mae angen yr holl wybodaeth ar y dudalen hon");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        }

        [Test]
        public async Task ModernSlaveryJourneyTwoErrorsWelsh()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.ClickLinkByText("Cymraeg");
            await InteractionUtilities.PageTitleShouldBe(
                "Cyflwyno eich gwybodaeth am gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            //Step below to be updated once change/update is pushed (Should be "Start")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "A ydych yn sefydliad masnachol sy'n ddarostyngedig i Adran 54 o Ddeddf Caethwasiaeth Fodern 2015? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Do/oes");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch Datganiad Caethwasiaeth Fodern diweddaraf ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Rhif");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Lanlwythwch gopi o'ch Datganiad Caethwasiaeth Fodern diweddaraf - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewis ffeil");
            await InteractionUtilities.UploadFile();
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i Ddatganiad Caethwasiaeth Fodern eich sefydliad - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Mae angen yr holl wybodaeth ar y dudalen hon");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Mae angen yr holl wybodaeth ar y dudalen hon");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        }

                [Test]
        public async Task ModernSlaveryJourneyThreeErrorsWelsh()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.ClickLinkByText("Cymraeg");
            await InteractionUtilities.PageTitleShouldBe(
                "Cyflwyno eich gwybodaeth am gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            //Step below to be updated once change/update is pushed (Should be "Start")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "A ydych yn sefydliad masnachol sy'n ddarostyngedig i Adran 54 o Ddeddf Caethwasiaeth Fodern 2015? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Rhif");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch dogfennau neu ddatganiadau sy'n gysylltiedig â chaethwasiaeth fodern ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Do/oes");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch y ddolen i'ch dogfennaeth berthnasol neu ddatganiadau cyhoeddedig ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Rhowch gyfeiriad gwefan");
            await InteractionUtilities.EnterTextIntoInputField("testwebsite");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Cofnodwch gyfarwyddiadau ar sut i ddod o hyd i'ch datganiadau cyhoeddedig neu ddogfennau perthnasol ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Mae angen yr holl wybodaeth ar y dudalen hon");
            await InteractionUtilities.EnterTextIntoTextArea("test instructions on Modern Slavery Statement");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Mae angen yr holl wybodaeth ar y dudalen hon");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
            await InteractionUtilities.PageShouldContainText("http://testwebsite");
            await InteractionUtilities.PageShouldContainText("test instructions on Modern Slavery Statement");
            await InteractionUtilities.PageShouldContainText("test text on missing information modern slavery");
        }

        [Test]
        public async Task ModernSlaveryJourneyFourErrorsWelsh()
        {
            var organisationPageUrl = $"{_baseUrl}/organisation/{_organisationId}";
            await InteractionUtilities.NavigateToUrl(organisationPageUrl);
            await InteractionUtilities.ClickLinkByText("Complete supplier information");
            await InteractionUtilities.ClickLinkByText("Modern Slavery");
            await InteractionUtilities.ClickLinkByText("Cymraeg");
            await InteractionUtilities.PageTitleShouldBe(
                "Cyflwyno eich gwybodaeth am gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            //Step below to be updated once change/update is pushed (Should be "Start")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "A ydych yn sefydliad masnachol sy'n ddarostyngedig i Adran 54 o Ddeddf Caethwasiaeth Fodern 2015? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Rhif");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Ydy'ch dogfennau neu ddatganiadau sy'n gysylltiedig â chaethwasiaeth fodern ar gael yn ddigidol? - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewiswch opsiwn");
            await InteractionUtilities.ClickRadioButtonByText("Rhif");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Lanlwythwch eich dogfennaeth berthnasol neu ddatganiadau cyhoeddedig ar gaethwasiaeth fodern - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Dewis ffeil");
            await InteractionUtilities.UploadFile();
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe(
                "Gwybodaeth goll - Canfod Tendr - GOV.UK");
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageShouldContainText("Mae angen yr holl wybodaeth ar y dudalen hon");
            await InteractionUtilities.EnterTextIntoTextArea("test text on missing information modern slavery");
            //Step below to be updated once change/update is pushed (Should be "Save")
            await InteractionUtilities.ClickButtonByText("Parhau");
            await InteractionUtilities.PageTitleShouldBe("Gwiriwch eich atebion - Canfod Tendr - GOV.UK");
        }

    }
}
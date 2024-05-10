using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationIdentificationModelTests
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationIdentificationModelTests()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void OnGet_WheEmptyModel_ShouldThrowException()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object);
        Action action = () => model.OnGet();
        action.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public void OnGet_WhenValidSession_ShouldSaveRegistrationDetails()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object);

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();

        model.OrganisationScheme.Should().Be(registrationDetails.OrganisationScheme);

        switch (registrationDetails.OrganisationScheme)
        {
            case "CHN":
                model.CompaniesHouseNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;            
            case "CCEW":
                model.CharityCommissionEnglandWalesNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "SCR":
                model.ScottishCharityRegulatorNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "CCNI":
                model.CharityCommissionNorthernIrelandNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;

            case "MPR":
                model.MutualsPublicRegisterNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "GRN":
                model.GuernseyRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "JFSC":
                model.JerseyFinancialServicesCommissionRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "IMCR":
                model.IsleofManCompaniesRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;

            case "NHOR":
                model.NationalHealthServiceOrganisationsRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;

            case "UKPRN":
                model.UKLearningProviderReferenceNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "VAT":
                model.VATNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
        }

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void OnPost_WhenOrganisationTypeIsNullOrEmpty_ShouldReturnPageWithModelStateError(string? organisationType)
    {

        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType
        };
        model.ModelState.AddModelError("OrganisationType", "Please select your organisation type");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("CHN", null)]
    [InlineData("CHN", "")]
    public void OnPost_WhenOrganisationTypeIsCHNAndCompaniesHouseNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? companiesHouseNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            CompaniesHouseNumber = companiesHouseNumber
        };
        model.ModelState.AddModelError("CompaniesHouseNumber", "The Companies House Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("CCEW", null)]
    [InlineData("CCEW", "")]
    public void OnPost_WhenOrganisationTypeIsCCEWAndCharityCommissionEnglandWalesNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? charityCommissionEnglandWalesNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            CharityCommissionEnglandWalesNumber = charityCommissionEnglandWalesNumber
        };
        model.ModelState.AddModelError("CharityCommissionEnglandWalesNumber", "The Charity Commission for England & Wales Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("SCR", null)]
    [InlineData("SCR", "")]
    public void OnPost_WhenOrganisationTypeIsOSCRAndScottishCharityRegulatorNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? scottishCharityRegulatorNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            ScottishCharityRegulatorNumber = scottishCharityRegulatorNumber
        };
        model.ModelState.AddModelError("ScottishCharityRegulatorNumber", "The Office of the Scottish Charity Regulator (OSCR) Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("CCNI", null)]
    [InlineData("CCNI", "")]
    public void OnPost_WhenOrganisationTypeIsCCNIAndCharityCommissionNorthernIrelandNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? charityCommissionNorthernIrelandNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            CharityCommissionNorthernIrelandNumber = charityCommissionNorthernIrelandNumber
        };
        model.ModelState.AddModelError("CharityCommissionNorthernIrelandNumber", "The Charity Commission for Northren Ireland Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("MPR", null)]
    [InlineData("MPR", "")]
    public void OnPost_WhenOrganisationTypeIsMPRAndMutualsPublicRegisterNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? mutualsPublicRegisterNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            MutualsPublicRegisterNumber = mutualsPublicRegisterNumber
        };
        model.ModelState.AddModelError("MutualsPublicRegisterNumber", "Mutuals Public Register Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GRN", null)]
    [InlineData("GRN", "")]
    public void OnPost_WhenOrganisationTypeIsGRNAndGuernseyRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? guernseyRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            GuernseyRegistryNumber = guernseyRegistryNumber
        };
        model.ModelState.AddModelError("GuernseyRegistryNumber", "Guernsey Registry Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("JFSC", null)]
    [InlineData("JFSC", "")]
    public void OnPost_WhenOrganisationTypeIsJFSCAndJerseyFinancialServicesCommissionRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? jerseyFinancialServicesCommissionRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            JerseyFinancialServicesCommissionRegistryNumber = jerseyFinancialServicesCommissionRegistryNumber
        };
        model.ModelState.AddModelError("JerseyFinancialServicesCommissionRegistryNumber", "Jersey Financial Services Commission Registry Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("IMCR", null)]
    [InlineData("IMCR", "")]
    public void OnPost_WhenOrganisationTypeIsIMCRAndIsleofManCompaniesRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? isleofManCompaniesRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            IsleofManCompaniesRegistryNumber = isleofManCompaniesRegistryNumber
        };
        model.ModelState.AddModelError("IsleofManCompaniesRegistryNumber", "Isle of Man Companies Registry Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("NHOR", null)]
    [InlineData("NHOR", "")]
    public void OnPost_WhenOrganisationTypeIsNHORAndNationalHealthServiceOrganisationsRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? nationalHealthServiceOrganisationsRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            NationalHealthServiceOrganisationsRegistryNumber = nationalHealthServiceOrganisationsRegistryNumber
        };
        model.ModelState.AddModelError("NationalHealthServiceOrganisationsRegistryNumber", "The National health Service Organisations Registry Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("UKPRN", null)]
    [InlineData("UKPRN", "")]
    public void OnPost_WhenOrganisationTypeIsUKPRNAndUKLearningProviderReferenceNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? ukLearningProviderReferenceNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            UKLearningProviderReferenceNumber = ukLearningProviderReferenceNumber
        };
        model.ModelState.AddModelError("UKLearningProviderReferenceNumber", "UK Register of Learning Provider Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("VAT", null)]
    [InlineData("VAT", "")]
    public void OnPost_WhenOrganisationTypeIsVATAndVATNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? vatNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType,
            VATNumber = vatNumber
        };
        model.ModelState.AddModelError("VATNumber", "VAT Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenModelStateIsValid_ShouldRedirectToOrganisationRegisteredAddress()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = "Other"
        };

        GivenRegistrationIsInProgress();

        var result = model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("./OrganisationRegisteredAddress");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = "Other",
            RedirectToSummary = true
        };
        GivenRegistrationIsInProgress();

        var result = model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Theory]
    [InlineData("CHN", "123456")]
    [InlineData("CCEW", "ABCDEF")]
    [InlineData("SCR", "GHIJKL")]
    [InlineData("CCNI", "MNOPQR")]
    [InlineData("MPR", "MPR123")]
    [InlineData("GRN", "GRN123")]
    [InlineData("JFSC", "JFSC123")]
    [InlineData("IMCR", "IMCR123")]
    [InlineData("NHOR", "STUVWX")]
    [InlineData("UKPRN", "PRN1234")]
    [InlineData("VAT", "GB1234")]
    public void OnPost_WhenModelStateIsValid_ShouldStoreOrganisationTypeAndIdentificationNumberInSession(string organisationType, string identificationNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationScheme = organisationType
        };

        SetIdentificationNumber(model, organisationType, identificationNumber);
        GivenRegistrationIsInProgress();

        var result = model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        sessionMock.Verify(s => s.Set(It.IsAny<string>(), It.Is<RegistrationDetails>(rd =>
            rd.OrganisationScheme == organisationType &&
            rd.OrganisationIdentificationNumber == identificationNumber)), Times.Once);
    }

    private static void SetIdentificationNumber(OrganisationIdentificationModel model, string organisationType, string identificationNumber)
    {
        switch (organisationType)
        {
            case "CHN":
                model.CompaniesHouseNumber = identificationNumber;
                break;
            case "CCEW":
                model.CharityCommissionEnglandWalesNumber = identificationNumber;
                break;
            case "SCR":
                model.ScottishCharityRegulatorNumber = identificationNumber;
                break;
            case "CCNI":
                model.CharityCommissionNorthernIrelandNumber = identificationNumber;
                break;            
            case "MPR":
                model.MutualsPublicRegisterNumber = identificationNumber;
                break;
            case "GRN":
                model.GuernseyRegistryNumber = identificationNumber;
                break;
            case "JFSC":
                model.JerseyFinancialServicesCommissionRegistryNumber = identificationNumber;
                break;
            case "IMCR":
                model.IsleofManCompaniesRegistryNumber = identificationNumber;
                break;
            case "NHOR":
                model.NationalHealthServiceOrganisationsRegistryNumber = identificationNumber;
                break;
            case "UKPRN":
                model.UKLearningProviderReferenceNumber = identificationNumber;
                break;
            case "VAT":
                model.VATNumber = identificationNumber;
                break;
        }
    }

    private void GivenRegistrationIsInProgress()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);
    }

    private static RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            UserUrn = "urn:fdc:gov.uk:2022:37d8856672e84a57ae9c86b27b226225",
            OrganisationScheme = "CHN",
            OrganisationIdentificationNumber = "12345678",
        };

        return registrationDetails;
    }
}

using CO.CDP.EntityVerificationClient;
using CO.CDP.Organisation.WebApiClient;
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
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private static readonly Guid _organisationId = Guid.NewGuid();
    private readonly Mock<IPponClient> _pponClientMock;

    public OrganisationIdentificationModelTests()
    {
        sessionMock = new Mock<ISession>();
        _pponClientMock = new Mock<IPponClient>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Fact]
    public void OnGet_WhenValidSession_ShouldSaveRegistrationDetails()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object);
        model.OnGet();

        model.OrganisationScheme.Should().Be(registrationDetails.OrganisationScheme);

        switch (registrationDetails.OrganisationScheme)
        {
            case "GB-CHC":
                model.CharityCommissionEnglandWalesNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "GB-SC":
                model.ScottishCharityRegulatorNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "GB-NIC":
                model.CharityCommissionNorthernIrelandNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;

            case "GB-MPR":
                model.MutualsPublicRegisterNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "GG-RCE":
                model.GuernseyRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "JE-FSC":
                model.JerseyFinancialServicesCommissionRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "IM-CR":
                model.IsleofManCompaniesRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;

            case "GB-NHS":
                model.NationalHealthServiceOrganisationsRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;

            case "GB-UKPRN":
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

        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType
        };
        model.ModelState.AddModelError("OrganisationType", "Please select your organisation type");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GB-CHC", null)]
    [InlineData("GB-CHC", "")]
    public void OnPost_WhenOrganisationTypeIsCCEWAndCharityCommissionEnglandWalesNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? charityCommissionEnglandWalesNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("GB-SC", null)]
    [InlineData("GB-SC", "")]
    public void OnPost_WhenOrganisationTypeIsOSCRAndScottishCharityRegulatorNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? scottishCharityRegulatorNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("GB-NIC", null)]
    [InlineData("GB-NIC", "")]
    public void OnPost_WhenOrganisationTypeIsGbNicAndCharityCommissionNorthernIrelandNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? charityCommissionNorthernIrelandNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("GB-MPR", null)]
    [InlineData("GB-MPR", "")]
    public void OnPost_WhenOrganisationTypeIsMPRAndMutualsPublicRegisterNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? mutualsPublicRegisterNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("GG-RCE", null)]
    [InlineData("GG-RCE", "")]
    public void OnPost_WhenOrganisationTypeIsGRNAndGuernseyRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? guernseyRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("JE-FSC", null)]
    [InlineData("JE-FSC", "")]
    public void OnPost_WhenOrganisationTypeIsJFSCAndJerseyFinancialServicesCommissionRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? jerseyFinancialServicesCommissionRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("IM-CR", null)]
    [InlineData("IM-CR", "")]
    public void OnPost_WhenOrganisationTypeIsIMCRAndIsleofManCompaniesRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? isleofManCompaniesRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("GB-NHS", null)]
    [InlineData("GB-NHS", "")]
    public void OnPost_WhenOrganisationTypeIsNHORAndNationalHealthServiceOrganisationsRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? nationalHealthServiceOrganisationsRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    [InlineData("GB-UKPRN", null)]
    [InlineData("GB-UKPRN", "")]
    public void OnPost_WhenOrganisationTypeIsUKPRNAndUKLearningProviderReferenceNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? ukLearningProviderReferenceNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
    public async Task OnPost_WhenModelStateIsValid_ShouldRedirectToOrganisationName()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = "Other"
        };

        GivenRegistrationIsInProgress();

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationName");
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = "Other",
            RedirectToSummary = true
        };
        GivenRegistrationIsInProgress();

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Theory]
    [InlineData("GB-CHC", "ABCDEF")]
    [InlineData("GB-SC", "GHIJKL")]
    [InlineData("GB-NIC", "MNOPQR")]
    [InlineData("GB-MPR", "MPR123")]
    [InlineData("GG-RCE", "GRN123")]
    [InlineData("JE-FSC", "JFSC123")]
    [InlineData("IM-CR", "IMCR123")]
    [InlineData("GB-NHS", "STUVWX")]
    [InlineData("GB-UKPRN", "PRN1234")]
    [InlineData("VAT", "GB1234")]
    public void OnPost_WhenModelStateIsValid_ShouldStoreOrganisationTypeAndIdentificationNumberInSession(string organisationType, string identificationNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
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
            case "GB-CHC":
                model.CharityCommissionEnglandWalesNumber = identificationNumber;
                break;
            case "GB-SC":
                model.ScottishCharityRegulatorNumber = identificationNumber;
                break;
            case "GB-NIC":
                model.CharityCommissionNorthernIrelandNumber = identificationNumber;
                break;
            case "GB-MPR":
                model.MutualsPublicRegisterNumber = identificationNumber;
                break;
            case "GG-RCE":
                model.GuernseyRegistryNumber = identificationNumber;
                break;
            case "JE-FSC":
                model.JerseyFinancialServicesCommissionRegistryNumber = identificationNumber;
                break;
            case "IM-CR":
                model.IsleofManCompaniesRegistryNumber = identificationNumber;
                break;
            case "GB-NHS":
                model.NationalHealthServiceOrganisationsRegistryNumber = identificationNumber;
                break;
            case "GB-UKPRN":
                model.UKLearningProviderReferenceNumber = identificationNumber;
                break;
            case "VAT":
                model.VATNumber = identificationNumber;
                break;
        }
    }

    private static Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, _organisationId, null, "Test Org", []);
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
            OrganisationScheme = "GB-COH",
            OrganisationIdentificationNumber = "12345678",
        };

        return registrationDetails;
    }
}
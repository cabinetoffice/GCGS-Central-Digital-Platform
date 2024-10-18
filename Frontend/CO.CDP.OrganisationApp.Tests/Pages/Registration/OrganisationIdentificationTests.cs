using CO.CDP.EntityVerificationClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

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
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
        _pponClientMock = new Mock<IPponClient>();
        organisationClientMock = new Mock<IOrganisationClient>();

        organisationClientMock.Setup(api => api.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new OrganisationWebApiClient.ApiException("Organisation does not exist", 404, "", null, null));
        _pponClientMock.Setup(api => api.GetIdentifiersAsync(It.IsAny<string>()))
            .ThrowsAsync(new EntityVerificationClient.ApiException("Organisation does not exist", 404, "", null, null));
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
        }

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task OnPost_WhenOrganisationTypeIsNullOrEmpty_ShouldReturnPageWithModelStateError(string? organisationType)
    {

        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType
        };
        model.ModelState.AddModelError("OrganisationType", "Please select your organisation type");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GB-CHC", null)]
    [InlineData("GB-CHC", "")]
    public async Task OnPost_WhenOrganisationTypeIsCCEWAndCharityCommissionEnglandWalesNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? charityCommissionEnglandWalesNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            CharityCommissionEnglandWalesNumber = charityCommissionEnglandWalesNumber
        };
        model.ModelState.AddModelError("CharityCommissionEnglandWalesNumber", "The Charity Commission for England & Wales Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GB-SC", null)]
    [InlineData("GB-SC", "")]
    public async Task OnPost_WhenOrganisationTypeIsOSCRAndScottishCharityRegulatorNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? scottishCharityRegulatorNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            ScottishCharityRegulatorNumber = scottishCharityRegulatorNumber
        };
        model.ModelState.AddModelError("ScottishCharityRegulatorNumber", "The Office of the Scottish Charity Regulator (OSCR) Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GB-NIC", null)]
    [InlineData("GB-NIC", "")]
    public async Task OnPost_WhenOrganisationTypeIsGbNicAndCharityCommissionNorthernIrelandNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? charityCommissionNorthernIrelandNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            CharityCommissionNorthernIrelandNumber = charityCommissionNorthernIrelandNumber
        };
        model.ModelState.AddModelError("CharityCommissionNorthernIrelandNumber", "The Charity Commission for Northern Ireland Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GB-MPR", null)]
    [InlineData("GB-MPR", "")]
    public async Task OnPost_WhenOrganisationTypeIsMPRAndMutualsPublicRegisterNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? mutualsPublicRegisterNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            MutualsPublicRegisterNumber = mutualsPublicRegisterNumber
        };
        model.ModelState.AddModelError("MutualsPublicRegisterNumber", "Mutuals Public Register Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GG-RCE", null)]
    [InlineData("GG-RCE", "")]
    public async Task OnPost_WhenOrganisationTypeIsGRNAndGuernseyRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? guernseyRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            GuernseyRegistryNumber = guernseyRegistryNumber
        };
        model.ModelState.AddModelError("GuernseyRegistryNumber", "Guernsey Registry Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("JE-FSC", null)]
    [InlineData("JE-FSC", "")]
    public async Task OnPost_WhenOrganisationTypeIsJFSCAndJerseyFinancialServicesCommissionRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? jerseyFinancialServicesCommissionRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            JerseyFinancialServicesCommissionRegistryNumber = jerseyFinancialServicesCommissionRegistryNumber
        };
        model.ModelState.AddModelError("JerseyFinancialServicesCommissionRegistryNumber", "Jersey Financial Services Commission Registry Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("IM-CR", null)]
    [InlineData("IM-CR", "")]
    public async Task OnPost_WhenOrganisationTypeIsIMCRAndIsleofManCompaniesRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? isleofManCompaniesRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            IsleofManCompaniesRegistryNumber = isleofManCompaniesRegistryNumber
        };
        model.ModelState.AddModelError("IsleofManCompaniesRegistryNumber", "Isle of Man Companies Registry Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GB-NHS", null)]
    [InlineData("GB-NHS", "")]
    public async Task OnPost_WhenOrganisationTypeIsNHORAndNationalHealthServiceOrganisationsRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? nationalHealthServiceOrganisationsRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            NationalHealthServiceOrganisationsRegistryNumber = nationalHealthServiceOrganisationsRegistryNumber
        };
        model.ModelState.AddModelError("NationalHealthServiceOrganisationsRegistryNumber", "The National health Service Organisations Registry Number field is required.");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GB-UKPRN", null)]
    [InlineData("GB-UKPRN", "")]
    public async Task OnPost_WhenOrganisationTypeIsUKPRNAndUKLearningProviderReferenceNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string? ukLearningProviderReferenceNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType,
            UKLearningProviderReferenceNumber = ukLearningProviderReferenceNumber
        };
        model.ModelState.AddModelError("UKLearningProviderReferenceNumber", "UK Register of Learning Provider Number field is required.");

        var result = await model.OnPost();

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

    [Fact]
    public async Task OnPost_WhenSchemeNotOtherAndOrganisationExistsInOganisationService_ShouldRedirectToOrganisationAlreadyRegisteredPage()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = "JE-FSC",
            RedirectToSummary = true
        };
        GivenRegistrationIsInProgress();

        organisationClientMock.Setup
            (api => api.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationAlreadyRegistered");
    }

    [Fact]
    public async Task OnPost_WhenSchemeNotOtherAndIdentifierExistsInEntityVerificationService_ShouldRedirectToOrganisationAlreadyRegisteredPage()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = "JE-FSC",
            RedirectToSummary = true
        };
        GivenRegistrationIsInProgress();

        _pponClientMock.Setup
            (api => api.GetIdentifiersAsync(It.IsAny<string>()))
            .ReturnsAsync(GivenEntityVerificationIdentifiers());

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationAlreadyRegistered");
    }

    [Fact]
    public async Task OnPost_WhenSchemeNotOtherAndEntityVerificationServiceOffLine_ShouldRedirectToOrganisationServiceUnavailablePage()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = "JE-FSC",
            RedirectToSummary = true
        };

        GivenRegistrationIsInProgress();

        _pponClientMock.Setup(api => api.GetIdentifiersAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Entity Verification service offline."));

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationRegistrationUnavailable");
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
    public async Task OnPost_WhenModelStateIsValid_ShouldStoreOrganisationTypeAndIdentificationNumberInSession(string organisationType, string identificationNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object, organisationClientMock.Object, _pponClientMock.Object)
        {
            OrganisationScheme = organisationType
        };

        SetIdentificationNumber(model, organisationType, identificationNumber);
        GivenRegistrationIsInProgress();

        var result = await model.OnPost();

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
        }
    }

    private static OrganisationWebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new OrganisationWebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: _organisationId, identifier: null, name: "Test Org", roles: [], details: new Details(approval: null, pendingRoles: []));
    }

    private static ICollection<EntityVerificationClient.Identifier> GivenEntityVerificationIdentifiers()
    {
        return new List<EntityVerificationClient.Identifier>() {
            new EntityVerificationClient.Identifier("12345", "Acme Ltd", "VAT", new Uri("http://acme.org")) };
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
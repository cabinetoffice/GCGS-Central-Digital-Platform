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

        model.OrganisationType.Should().Be(registrationDetails.OrganisationType);

        switch (registrationDetails.OrganisationType)
        {
            case "CHN":
                model.CompaniesHouseNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "DUN":
                model.DunBradstreetNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "CCEW":
                model.CharityCommissionEnglandWalesNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "OSCR":
                model.OfficeOfScottishCharityRegulatorNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "CCNI":
                model.CharityCommissionNorthernIrelandNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "NHOR":
                model.NationalHealthServiceOrganisationsRegistryNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "DFE":
                model.DepartmentForEducationNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
            case "Other":
                model.OtherNumber.Should().Be(registrationDetails.OrganisationIdentificationNumber);
                break;
        }

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void OnPost_WhenOrganisationTypeIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType)
    {

        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType
        };
        model.ModelState.AddModelError("OrganisationType", "Please select your organisation type");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("CHN", null)]
    [InlineData("CHN", "")]
    public void OnPost_WhenOrganisationTypeIsCHNAndCompaniesHouseNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string companiesHouseNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType,
            CompaniesHouseNumber = companiesHouseNumber
        };
        model.ModelState.AddModelError("CompaniesHouseNumber", "The Companies House Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("DUN", null)]
    [InlineData("DUN", "")]
    public void OnPost_WhenOrganisationTypeIsDUNAndDunBradstreetNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string dunBradstreetNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType,
            DunBradstreetNumber = dunBradstreetNumber
        };
        model.ModelState.AddModelError("DunBradstreetNumber", "The Dun & Bradstreet Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("CCEW", null)]
    [InlineData("CCEW", "")]
    public void OnPost_WhenOrganisationTypeIsCCEWAndCharityCommissionEnglandWalesNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string charityCommissionEnglandWalesNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType,
            CharityCommissionEnglandWalesNumber = charityCommissionEnglandWalesNumber
        };
        model.ModelState.AddModelError("CharityCommissionEnglandWalesNumber", "The Charity Commission for England & Wales Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("OSCR", null)]
    [InlineData("OSCR", "")]
    public void OnPost_WhenOrganisationTypeIsOSCRAndOfficeOfScottishCharityRegulatorNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string officeOfScottishCharityRegulatorNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType,
            OfficeOfScottishCharityRegulatorNumber = officeOfScottishCharityRegulatorNumber
        };
        model.ModelState.AddModelError("OfficeOfScottishCharityRegulatorNumber", "The Office of the Scottish Charity Regulator (OSCR) Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("CCNI", null)]
    [InlineData("CCNI", "")]
    public void OnPost_WhenOrganisationTypeIsCCNIAndCharityCommissionNorthernIrelandNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string charityCommissionNorthernIrelandNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType,
            CharityCommissionNorthernIrelandNumber = charityCommissionNorthernIrelandNumber
        };
        model.ModelState.AddModelError("CharityCommissionNorthernIrelandNumber", "The Charity Commission for Northren Ireland Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("NHOR", null)]
    [InlineData("NHOR", "")]
    public void OnPost_WhenOrganisationTypeIsNHORAndNationalHealthServiceOrganisationsRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string nationalHealthServiceOrganisationsRegistryNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType,
            NationalHealthServiceOrganisationsRegistryNumber = nationalHealthServiceOrganisationsRegistryNumber
        };
        model.ModelState.AddModelError("NationalHealthServiceOrganisationsRegistryNumber", "The National health Service Organisations Registry Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("DFE", null)]
    [InlineData("DFE", "")]
    public void OnPost_WhenOrganisationTypeIsDFEAndDepartmentForEducationNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string departmentForEducationNumber)
    {

        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType,
            DepartmentForEducationNumber = departmentForEducationNumber
        };
        model.ModelState.AddModelError("DepartmentForEducationNumber", "The Department For Education Number field is required.");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenModelStateIsValid_ShouldRedirectToOrganisationRegisteredAddress()
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = "Other"
        };
        var result = model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("./OrganisationRegisteredAddress");
    }

    [Theory]
    [InlineData("CHN", "123456")]
    [InlineData("DUN", "987654")]
    [InlineData("CCEW", "ABCDEF")]
    [InlineData("OSCR", "GHIJKL")]
    [InlineData("CCNI", "MNOPQR")]
    [InlineData("NHOR", "STUVWX")]
    [InlineData("DFE", "YZ1234")]
    [InlineData("Other", "567890")]
    public void OnPost_WhenModelStateIsValid_ShouldStoreOrganisationTypeAndIdentificationNumberInSession(string organisationType, string identificationNumber)
    {
        var model = new OrganisationIdentificationModel(sessionMock.Object)
        {
            OrganisationType = organisationType
        };

        SetIdentificationNumber(model, organisationType, identificationNumber);
        var result = model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        sessionMock.Verify(s => s.Set(It.IsAny<string>(), It.Is<RegistrationDetails>(rd =>
            rd.OrganisationType == organisationType &&
            rd.OrganisationIdentificationNumber == identificationNumber)), Times.Once);
    }

    private void SetIdentificationNumber(OrganisationIdentificationModel model, string organisationType, string identificationNumber)
    {
        switch (organisationType)
        {
            case "CHN":
                model.CompaniesHouseNumber = identificationNumber;
                break;
            case "DUN":
                model.DunBradstreetNumber = identificationNumber;
                break;
            case "CCEW":
                model.CharityCommissionEnglandWalesNumber = identificationNumber;
                break;
            case "OSCR":
                model.OfficeOfScottishCharityRegulatorNumber = identificationNumber;
                break;
            case "CCNI":
                model.CharityCommissionNorthernIrelandNumber = identificationNumber;
                break;
            case "NHOR":
                model.NationalHealthServiceOrganisationsRegistryNumber = identificationNumber;
                break;
            case "DFE":
                model.DepartmentForEducationNumber = identificationNumber;
                break;
            case "Other":
                model.OtherNumber = identificationNumber;
                break;
        }
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationType = "CHN",
            OrganisationIdentificationNumber = "12345678",
        };

        return registrationDetails;
    }
}

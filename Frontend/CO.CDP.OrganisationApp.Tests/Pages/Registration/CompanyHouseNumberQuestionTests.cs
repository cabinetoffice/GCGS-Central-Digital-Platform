using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.ThirdPartyApiClients;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;
public class CompanyHouseNumberQuestionTests
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IFlashMessageService> flashMessageServiceMock;
    private readonly Mock<ICompaniesHouseApi> companiesHouseApiMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;

    public CompanyHouseNumberQuestionTests()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
        flashMessageServiceMock = new Mock<IFlashMessageService>();
        companiesHouseApiMock = new Mock<ICompaniesHouseApi>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task OnPost_WhenOptionIsNullOrEmpty_ShouldReturnPageWithModelStateError(bool? hasNumber)
    {
        var model = GivenCompaniesHouseQuestionModel();

        model.HasCompaniesHouseNumber = hasNumber;
        model.ModelState.AddModelError("Question", "Please select an option");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsValid_ShouldRedirectToOrganisationIdentification()
    {
        var model = GivenCompaniesHouseQuestionModel();

        model.HasCompaniesHouseNumber = false;
        model.CompaniesHouseNumber = "12345678";

        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationIdentification");
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsValid_And_CompaniesNumber_Provided_ShouldRedirectToOrganisationName()
    {
        var model = GivenCompaniesHouseQuestionModel();

        model.HasCompaniesHouseNumber = true;
        model.CompaniesHouseNumber = "12345678";

        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        organisationClientMock.Setup(o => o.LookupOrganisationAsync(string.Empty, It.IsAny<string>()))
            .Throws(new ApiException(string.Empty, 404, string.Empty, null, null));

        var profile = GivenProfileOnCompaniesHouse(organisationName: "Acme Ltd");
        companiesHouseApiMock.Setup(ch => ch.GetProfile(model.CompaniesHouseNumber))
            .ReturnsAsync(profile);

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationName");
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = GivenCompaniesHouseQuestionModel();

        model.HasCompaniesHouseNumber = true;
        model.RedirectToSummary = true;
        model.CompaniesHouseNumber = "123456";

        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        organisationClientMock.Setup(o => o.LookupOrganisationAsync(string.Empty, It.IsAny<string>()))
            .Throws(new ApiException(string.Empty, 404, string.Empty, null, null));

        var profile = GivenProfileOnCompaniesHouse(organisationName: "Acme Ltd");
        companiesHouseApiMock.Setup(ch => ch.GetProfile(model.CompaniesHouseNumber))
            .ReturnsAsync(profile);

        var result = await model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsValidAndOrganisationAlreadyExists_ShouldShowNotificationBanner()
    {
        var model = GivenCompaniesHouseQuestionModel();

        model.HasCompaniesHouseNumber = true;
        model.RedirectToSummary = true;
        model.CompaniesHouseNumber = "123456";
        model.OrganisationIdentifier = "GB-COH:123456789";
        model.OrganisationName = "Test company";

        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        organisationClientMock.Setup(o => o.LookupOrganisationAsync(string.Empty, It.IsAny<string>()))
            .ReturnsAsync(GivenOrganisation());

        var result = await model.OnPost();

        Dictionary<string, string> urlParameters = new() { ["organisationIdentifier"] = model.OrganisationIdentifier };
        Dictionary<string, string> htmlParameters = new() { ["organisationName"] = model.OrganisationName };

        flashMessageServiceMock.Verify(api => api.SetFlashMessage(
            FlashMessageType.Important,
            StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyAlreadyRegistered_NotificationBanner,
            null,
            null,
            It.Is<Dictionary<string, string>>(d => d["organisationIdentifier"] == model.OrganisationIdentifier),
            It.Is<Dictionary<string, string>>(d => d["organisationName"] == model.OrganisationName)
        ),
        Times.Once);


        result.Should().BeOfType<PageResult>();
    }

    private static CDP.Organisation.WebApiClient.Organisation GivenOrganisation()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(null, null, null, null, new Guid(), null, "Test Org", [], CDP.Organisation.WebApiClient.OrganisationType.Organisation);
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsValidAndCompanyNotFoundAtComapniesHouse_ShouldShowNotificationBanner()
    {
        var model = GivenCompaniesHouseQuestionModel();

        model.HasCompaniesHouseNumber = true;
        model.RedirectToSummary = true;
        model.CompaniesHouseNumber = "123456";

        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        organisationClientMock.Setup(o => o.LookupOrganisationAsync(string.Empty, It.IsAny<string>()))
            .Throws(new ApiException(string.Empty, 404, string.Empty, null, null));

        var result = await model.OnPost();

        flashMessageServiceMock.Verify(api => api.SetFlashMessage(
            FlashMessageType.Important,
            StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyNotFound_NotificationBanner,
            null,
            null,
            null,
            null
        ),
        Times.Once);

        result.Should().BeOfType<PageResult>();
    }

    private void GivenRegistrationIsInProgress(bool? hasNumber, string companyHouseNumber)
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails(hasNumber, companyHouseNumber);
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);
    }

    private CompanyProfile GivenProfileOnCompaniesHouse(string organisationName = "")
    {
        return new CompanyProfile() { CompanyName = organisationName };
    }

    private CompanyHouseNumberQuestionModel GivenCompaniesHouseQuestionModel()
    {
        return new CompanyHouseNumberQuestionModel(sessionMock.Object,
            companiesHouseApiMock.Object,
            organisationClientMock.Object,
            flashMessageServiceMock.Object);
    }
    private static RegistrationDetails DummyRegistrationDetails(bool? hasNumber, string companyHouseNumber)
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationScheme = "CCEW",
            OrganisationIdentificationNumber = companyHouseNumber,
            OrganisationHasCompaniesHouseNumber = hasNumber
        };

        return registrationDetails;
    }
}
using CO.CDP.OrganisationApp.CharityCommission;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationEmailModelTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<ICharityCommissionApi> charityCommissionMock;

    public OrganisationEmailModelTest()
    {
        sessionMock = new Mock<ISession>();
        charityCommissionMock = new Mock<ICharityCommissionApi>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationEmailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }

    [Fact]
    public void WhenEmailIsEmpty_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationEmailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(nameof(StaticTextResource.OrganisationRegistration_EnterOrganisationEmail_Heading));
    }

    [Fact]
    public void WhenEmailAddressIsInvalid_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationEmailModel();
        model.EmailAddress = "dummy";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();
        
        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(nameof(StaticTextResource.Global_EmailAddress_Error));
    }

    [Fact]
    public void WhenEmailAddressIsValid_ShouldNotRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationEmailModel();
        model.EmailAddress = "dummay@test.com";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenOrganisationEmailModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
    {
        var model = GivenOrganisationEmailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnPost();

        sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationEmailModel();
        model.OnGet();

        model.EmailAddress.Should().Be(registrationDetails.OrganisationEmailAddress);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationAddressPage()
    {
        var model = GivenOrganisationEmailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationRegisteredAddress");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = GivenOrganisationEmailModel();
        model.RedirectToSummary = true;

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public async Task OnGet_WhenCharityCommissionNumberProvided_ShouldPrepopulateEmail()
    {
        var registrationDetails = DummyRegistrationDetails(organisationName: "",
            scheme: OrganisationSchemeType.CharityCommissionEnglandWales,
            organisationEmailAddress: "");
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var chartiyDetails = GivenEmailOnCharitiesCommission();
        var model = GivenOrganisationEmailModel();

        charityCommissionMock.Setup(s => s.GetCharityDetails(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(chartiyDetails);

        await model.OnGet();

        model.EmailAddress.Should().Be(chartiyDetails.Email);
    }

    [Fact]
    public async Task OnGet_WhenCharityCommissionNumberProvidedRegDetailsProvided_ShouldNotPrepopulateEmail()
    {
        var registrationDetails = DummyRegistrationDetails(scheme: OrganisationSchemeType.CharityCommissionEnglandWales);
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var chartiyDetails = GivenEmailOnCharitiesCommission();
        var model = GivenOrganisationEmailModel();

        charityCommissionMock.Setup(s => s.GetCharityDetails(registrationDetails.OrganisationIdentificationNumber!))
            .ReturnsAsync(chartiyDetails);

        await model.OnGet();

        model.EmailAddress.Should().Be(registrationDetails.OrganisationEmailAddress);
    }

    private CharityDetails GivenEmailOnCharitiesCommission()
    {
        return new CharityDetails()
        {
            Email = "contactus@britishredcross.org"
        };
    }

    private RegistrationDetails DummyRegistrationDetails(string organisationName = "TestOrg",
        string scheme = "TestType",
        string organisationEmailAddress = "test@example.com")
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = organisationName,
            OrganisationScheme = scheme,
            OrganisationEmailAddress = organisationEmailAddress,
            OrganisationIdentificationNumber = "987654321"
        };

        return registrationDetails;
    }

    private OrganisationEmailModel GivenOrganisationEmailModel()
    {
        return new OrganisationEmailModel(sessionMock.Object, charityCommissionMock.Object);
    }
}
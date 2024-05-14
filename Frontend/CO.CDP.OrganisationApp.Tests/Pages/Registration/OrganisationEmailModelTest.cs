using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationEmailModelTest
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationEmailModelTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }


    [Fact]
    public void WhenEmailIsEmpty_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be("Enter the organisation email address");
    }

    [Fact]
    public void WhenEmailAddressIsInvalid_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.EmailAddress = "dummy";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be("Enter an email address in the correct format, like name@example.com");
    }

    [Fact]
    public void WhenEmailAddressIsValid_ShouldNotRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationDetailModel();
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

        var model = GivenOrganisationDetailModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
    {
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnPost();

        sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationAddressPage()
    {
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationRegisteredAddress");
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();

        model.EmailAddress.Should().Be(registrationDetails.OrganisationEmailAddress);
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            UserUrn = "urn:fdc:gov.uk:2022:91e9da3e77824185b7f8639d4a63dfc7",
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com"
        };

        return registrationDetails;
    }

    [Fact]
    public void OnGet_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(value: null);

        var model = GivenOrganisationDetailModel();

        Action action = () => model.OnGet();
        action.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    private OrganisationEmailModel GivenOrganisationDetailModel()
    {
        return new OrganisationEmailModel(sessionMock.Object);
    }
}
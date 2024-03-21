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

public class OrganisationDetailModelTest
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationDetailModelTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(4);
    }

    [Fact]
    public void WhenOrganisationNameIsEmpty_ShouldRaiseOrganisationNameValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("OrganisationName")).First()
            .ErrorMessage.Should().Be("Enter your organisation name");
    }

    [Fact]
    public void WhenOrganisationNameIsNotEmpty_ShouldNotRaiseOrganisationNameValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.OrganisationName = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeFalse();
    }

    [Fact]
    public void WhenOrganisationTypeIsEmpty_ShouldRaiseOrganisationTypeValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("OrganisationType")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("OrganisationType")).First()
            .ErrorMessage.Should().Be("Enter your organisation type");
    }

    [Fact]
    public void WhenOrganisationTypeIsNotEmpty_ShouldNotRaiseOrganisationTypeValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.OrganisationType = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("OrganisationType")).Should().BeFalse();
    }

    [Fact]
    public void WhenEmailIsEmpty_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be("Enter your email address");
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
    public void WhenTelephoneNumberIsEmpty_ShouldRaiseTelephoneNumberValidationError()
    {
        var model = GivenOrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("TelephoneNumber")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("TelephoneNumber")).First()
            .ErrorMessage.Should().Be("Enter your telephone number");
    }

    [Fact]
    public void WhenTelephoneNumberIsNotEmpty_ShouldNotRaiseTelephoneNumberValidationError()
    {
        var model = GivenOrganisationDetailModel();
        model.TelephoneNumber = "0123456789";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("TelephoneNumber")).Should().BeFalse();
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
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationDetailsPage()
    {
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationIdentification");
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {        
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();

        Assert.Equal(registrationDetails.OrganisationName, model.OrganisationName);
        Assert.Equal(registrationDetails.OrganisationType, model.OrganisationType);
        Assert.Equal(registrationDetails.OrganisationEmailAddress, model.EmailAddress);
        Assert.Equal(registrationDetails.OrganisationTelephoneNumber, model.TelephoneNumber);
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationType = "TestType",
            OrganisationEmailAddress = "test@example.com",
            OrganisationTelephoneNumber = "1234567890"
        };

        return registrationDetails;
    }

    [Fact]
    public void OnGet_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns<RegistrationDetails>(null);

        var model = GivenOrganisationDetailModel();

        var ex = Assert.Throws<Exception>(() => model.OnGet());
        Assert.Equal("Shoudn't be here", ex.Message);
    }

    private OrganisationDetailModel GivenOrganisationDetailModel()
    {
        return new OrganisationDetailModel(sessionMock.Object);
    }
}

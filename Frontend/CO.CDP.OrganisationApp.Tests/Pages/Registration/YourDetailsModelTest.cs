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

public class YourDetailsModelTest
{
    private readonly Mock<ISession> sessionMock;

    public YourDetailsModelTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void Model_WhenNotPolpulated_ShouldRaiseValidationErrors()
    {
        var model = GivenYourDetailsModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(2);
    }

    [Fact]
    public void Model_WhenFirstNameIsEmpty_ShouldRaiseFirstNameValidationError()
    {
        var model = GivenYourDetailsModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("FirstName")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("FirstName")).First()
            .ErrorMessage.Should().Be("Enter your first name");
    }

    [Fact]
    public void Model_WhenFirstNameIsNotEmpty_ShouldNotRaiseFirstNameValidationError()
    {
        var model = GivenYourDetailsModel();
        model.FirstName = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("FirstName")).Should().BeFalse();
    }

    [Fact]
    public void Model_WhenLastNameIsEmpty_ShouldRaiseLastNameValidationError()
    {
        var model = GivenYourDetailsModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("LastName")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("LastName")).First()
            .ErrorMessage.Should().Be("Enter your last name");
    }

    [Fact]
    public void Model_WhenLastNameIsNotEmpty_ShouldNotRaiseLastNameValidationError()
    {
        var model = GivenYourDetailsModel();
        model.LastName = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("LastName")).Should().BeFalse();
    }

    [Fact]
    public void OnGet_WhenRegistrationDetailsNotInSession_ShouldThrowException()
    {
        var model = GivenYourDetailsModel();

        Action act = () => model.OnGet();

        act.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public void OnGet_WhenRegistrationDetailsInSession_ShouldPopulatePageModel()
    {
        var model = GivenYourDetailsModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                UserPrincipal = "urn:fdc:gov.uk:2022:b1829a14353c429ea6e23798e020d775",
                TenantId = Guid.NewGuid(),
                FirstName = "firstdummy",
                LastName = "lastdummy"
            });

        model.OnGet();

        model.FirstName.Should().Be("firstdummy");
        model.LastName.Should().Be("lastdummy");
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenYourDetailsModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenRegistrationDetailsNotInSession_ShouldThrowException()
    {
        var model = GivenYourDetailsModel();

        Action act = () => model.OnPost();

        act.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
    {
        var model = GivenYourDetailsModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                UserPrincipal = "urn:fdc:gov.uk:2022:bad51498dcfe4572959c4540aef2397e",
                TenantId = Guid.NewGuid()
            });

        model.OnPost();

        sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationDetailsPage()
    {
        var model = GivenYourDetailsModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                UserPrincipal = "urn:fdc:gov.uk:2022:5973548c6f464dd98109b11cdda30947",
                TenantId = Guid.NewGuid()
            });

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetails");
    }

    [Fact]
    public void OnPost_WhenValidModelAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = GivenYourDetailsModel();
        model.RedirectToSummary = true;
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                UserPrincipal = "urn:fdc:gov.uk:2022:928db154d82d496fb81a113acf923872",
                TenantId = Guid.NewGuid()
            });

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var model = GivenYourDetailsModel();

        RegistrationDetails registrationDetails = new RegistrationDetails
        {
            UserPrincipal = "urn:fdc:gov.uk:2022:3771d941a5774a8e8058972661fd4f7c",
            FirstName = "first name",
            LastName = "last name",
            Email = "test@co.com"
        };

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();

        model.FirstName.Should().Be(registrationDetails.FirstName);
        model.LastName.Should().Be(registrationDetails.LastName);
    }

    private YourDetailsModel GivenYourDetailsModel()
    {
        return new YourDetailsModel(sessionMock.Object);
    }
}
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.Person.WebApiClient;
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
    private readonly Mock<IPersonClient> personClientMock;

    public YourDetailsModelTest()
    {
        sessionMock = new Mock<ISession>();
        personClientMock = new Mock<IPersonClient>();
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

        act.Should().Throw<Exception>().WithMessage("Session not found");
    }

    [Fact]
    public void OnGet_WhenRegistrationDetailsInSession_ShouldPopulatePageModel()
    {
        var model = GivenYourDetailsModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                UserUrn = "urn:fdc:gov.uk:2022:b1829a14353c429ea6e23798e020d775",
                FirstName = "firstdummy",
                LastName = "lastdummy"
            });

        model.OnGet();

        model.FirstName.Should().Be("firstdummy");
        model.LastName.Should().Be("lastdummy");
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var model = GivenYourDetailsModel();

        var registrationDetails = new RegistrationDetails
        {
            UserUrn = "urn:fdc:gov.uk:2022:3771d941a5774a8e8058972661fd4f7c",
            FirstName = "first name",
            LastName = "last name",
            Email = "test@co.com"
        };

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();

        model.FirstName.Should().Be(registrationDetails.FirstName);
        model.LastName.Should().Be(registrationDetails.LastName);
    }

    [Fact]
    public async Task OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenYourDetailsModel();
        model.PageContext = pageContext;

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WhenRegistrationDetailsNotInSession_ShouldThrowException()
    {
        var model = GivenYourDetailsModel();

        Func<Task> act = model.OnPost;

        await act.Should().ThrowAsync<Exception>().WithMessage("Session not found");
    }

    [Fact]
    public async Task OnPost_WhenValidModel_ShouldRegisterPerson()
    {
        var model = GivenYourDetailsModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                UserUrn = "urn:fdc:gov.uk:2022:bad51498dcfe4572959c4540aef2397e",
                Email = "dummy@test.com"
            });

        personClientMock.Setup(s => s.CreatePersonAsync(It.IsAny<NewPerson>()))
            .ReturnsAsync(dummyPerson);

        var actionResult = await model.OnPost();

        personClientMock.Verify(s => s.CreatePersonAsync(It.IsAny<NewPerson>()), Times.Once);
        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationSelection");
    }

    [Fact]
    public async Task OnPost_WhenErrorInRegisteringPerson_ShouldReturnPageWithError()
    {
        var model = GivenYourDetailsModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                UserUrn = "urn:fdc:gov.uk:2022:bad51498dcfe4572959c4540aef2397e",
                Email = "dummy@test.com"
            });

        personClientMock.Setup(s => s.CreatePersonAsync(It.IsAny<NewPerson>()))
            .ThrowsAsync(new ApiException("Unexpected error", 500, "", default, null));

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    private readonly Person.WebApiClient.Person dummyPerson
        = new("dummy@test.com", "firstdummy", Guid.NewGuid(), "lastdummy");

    private YourDetailsModel GivenYourDetailsModel()
    {
        return new YourDetailsModel(sessionMock.Object, personClientMock.Object);
    }
}
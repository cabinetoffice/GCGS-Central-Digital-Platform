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

		results.Count.Should().Be(3);
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
	public void Model_WhenEmailIsEmpty_ShouldRaiseEmailValidationError()
	{
		var model = GivenYourDetailsModel();

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("Email")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("Email")).First()
			.ErrorMessage.Should().Be("Enter your email address");
	}

	[Fact]
	public void Model_WhenEmailIsInvalid_ShouldRaiseEmailValidationError()
	{
		var model = GivenYourDetailsModel();
		model.Email = "dummy";

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("Email")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("Email")).First()
			.ErrorMessage.Should().Be("Enter an email address in the correct format, like name@example.com");
	}

	[Fact]
	public void Model_WhenEmailIsValid_ShouldNotRaiseEmailValidationError()
	{
		var model = GivenYourDetailsModel();
		model.Email = "dummay@test.com";

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("Email")).Should().BeFalse();
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
	public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
	{
		var model = GivenYourDetailsModel();

		model.OnPost();

		sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
		sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
	}

	[Fact]
	public void OnPost_WhenValidModel_ShouldRedirectToOrganisationDetailsPage()
	{
		var model = GivenYourDetailsModel();

		var actionResult = model.OnPost();

		actionResult.Should().BeOfType<RedirectToPageResult>()
			.Which.PageName.Should().Be("OrganisationDetails");
	}

	private YourDetailsModel GivenYourDetailsModel()
	{
		return new YourDetailsModel(sessionMock.Object);
	}
}

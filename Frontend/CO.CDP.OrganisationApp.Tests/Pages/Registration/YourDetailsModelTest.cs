using FluentAssertions;
using CO.CDP.OrganisationApp.Pages.Registration;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class YourDetailsModelTest
{
	[Fact]
	public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
	{
		var model = new YourDetailsModel();

		var results = ModelValidationHelper.Validate(model);

		results.Count.Should().Be(3);
	}

	[Fact]
	public void WhenFirstNameIsEmpty_ShouldRaiseFirstNameValidationError()
	{
		var model = new YourDetailsModel();

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("FirstName")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("FirstName")).First()
			.ErrorMessage.Should().Be("Enter your first name");
	}

	[Fact]
	public void WhenFirstNameIsNotEmpty_ShouldNotRaiseFirstNameValidationError()
	{
		var model = new YourDetailsModel { FirstName = "dummay" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("FirstName")).Should().BeFalse();
	}

	[Fact]
	public void WhenLastNameIsEmpty_ShouldRaiseLastNameValidationError()
	{
		var model = new YourDetailsModel();

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("LastName")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("LastName")).First()
			.ErrorMessage.Should().Be("Enter your last name");
	}

	[Fact]
	public void WhenLastNameIsNotEmpty_ShouldNotRaiseLastNameValidationError()
	{
		var model = new YourDetailsModel { LastName = "dummay" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("LastName")).Should().BeFalse();
	}

	[Fact]
	public void WhenEmailIsEmpty_ShouldRaiseEmailValidationError()
	{
		var model = new YourDetailsModel();

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("Email")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("Email")).First()
			.ErrorMessage.Should().Be("Enter your email address");
	}

	[Fact]
	public void WhenEmailIsInvalid_ShouldRaiseEmailValidationError()
	{
		var model = new YourDetailsModel { Email = "dummy" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("Email")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("Email")).First()
			.ErrorMessage.Should().Be("Enter an email address in the correct format, like name@example.com");
	}

	[Fact]
	public void WhenEmailIsValid_ShouldNotRaiseEmailValidationError()
	{
		var model = new YourDetailsModel { Email = "dummay@test.com" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("Email")).Should().BeFalse();
	}
}

using FluentAssertions;
using CO.CDP.OrganisationApp.Pages.Registration;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationDetailModelTest
{
	[Fact]
	public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
	{
		var model = new OrganisationDetailModel();

		var results = ModelValidationHelper.Validate(model);

		results.Count.Should().Be(4);
	}

	[Fact]
	public void WhenOrganisationNameIsEmpty_ShouldRaiseOrganisationNameValidationError()
	{
		var model = new OrganisationDetailModel();

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("OrganisationName")).First()
			.ErrorMessage.Should().Be("Enter your organisation name");
	}

	[Fact]
	public void WhenOrganisationNameIsNotEmpty_ShouldNotRaiseOrganisationNameValidationError()
	{
		var model = new OrganisationDetailModel { OrganisationName = "dummay" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeFalse();
	}

	[Fact]
	public void WhenOrganisationTypeIsEmpty_ShouldRaiseOrganisationTypeValidationError()
	{
		var model = new OrganisationDetailModel();

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("OrganisationType")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("OrganisationType")).First()
			.ErrorMessage.Should().Be("Enter your organisation type");
	}

	[Fact]
	public void WhenOrganisationTypeIsNotEmpty_ShouldNotRaiseOrganisationTypeValidationError()
	{
		var model = new OrganisationDetailModel { OrganisationType = "dummay" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("OrganisationType")).Should().BeFalse();
	}

	[Fact]
	public void WhenEmailIsEmpty_ShouldRaiseEmailValidationError()
	{
		var model = new OrganisationDetailModel();

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
			.ErrorMessage.Should().Be("Enter your email address");
	}

	[Fact]
	public void WhenEmailAddressIsInvalid_ShouldRaiseEmailAddressValidationError()
	{
		var model = new OrganisationDetailModel { EmailAddress = "dummy" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

		results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
			.ErrorMessage.Should().Be("Enter an email address in the correct format, like name@example.com");
	}

	[Fact]
	public void WhenEmailAddressIsValid_ShouldNotRaiseEmailAddressValidationError()
	{
		var model = new OrganisationDetailModel { EmailAddress = "dummay@test.com" };

		var results = ModelValidationHelper.Validate(model);

		results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeFalse();
	}

    [Fact]
    public void WhenTelephoneNumberIsEmpty_ShouldRaiseTelephoneNumberValidationError()
    {
        var model = new OrganisationDetailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("TelephoneNumber")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("TelephoneNumber")).First()
            .ErrorMessage.Should().Be("Enter your telephone number");
    }

    [Fact]
    public void WhenTelephoneNumberIsNotEmpty_ShouldNotRaiseTelephoneNumberValidationError()
    {
        var model = new OrganisationDetailModel { TelephoneNumber = "0123456789" };

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("TelephoneNumber")).Should().BeFalse();
    }
}

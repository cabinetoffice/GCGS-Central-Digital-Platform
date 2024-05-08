using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class PrivacyPolicyTest
{
    [Fact]
    public void Model_WhenAgreeToPrivacyPolicyNotSet_ShouldRaiseAgreeToPrivacyPolicyValidationError()
    {
        var model = GivenPrivacyPolicyModel();

        var results = ModelValidationHelper.Validate(model);

        results.Where(c => c.MemberNames.Contains("AgreeToPrivacy"))
            .First().ErrorMessage.Should()
            .Be("Select if you have read and agree to the Central Digital Platform service privacy policy");
    }

    [Fact]
    public void Model_WhenAgreeToPrivacyPolicySetTrue_ShouldRedirectToTourDetailsPage()
    {
        var model = GivenPrivacyPolicyModel();
        model.AgreeToPrivacy = true;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Registration/YourDetails");
    }

    private PrivacyPolicyModel GivenPrivacyPolicyModel()
    {
        return new PrivacyPolicyModel();
    }
}
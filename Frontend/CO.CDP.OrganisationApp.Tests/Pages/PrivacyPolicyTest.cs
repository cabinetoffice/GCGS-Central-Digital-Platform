using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class PrivacyPolicyTest
{
    private PrivacyPolicyModel GivenPrivacyPolicyModel()
    {
        return new PrivacyPolicyModel(); 
    }

    [Fact]
    public void Model_WhenAgreeToPrivacyPolicyIsEmpty_ShouldRaiseAgreeToPrivacyPolicyValidationError()
    {
        var model = GivenPrivacyPolicyModel();
        model.AgreeToPrivacy = false;
        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("AgreeToPrivacy")).Should().BeFalse();

        results.Where(c => c.MemberNames.Contains("AgreeToPrivacy")).First()
            .ErrorMessage.Should().Be("Select if you have read and agree to the Central Digital Platform service privacy policy");
    }

}

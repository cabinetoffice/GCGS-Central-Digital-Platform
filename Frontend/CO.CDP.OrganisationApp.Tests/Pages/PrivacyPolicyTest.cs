using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class PrivacyPolicyTest
{
    private readonly Mock<ISession> sessionMock;

    public PrivacyPolicyTest()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

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
            .Which.PageName.Should().Be("YourDetails");
    }

    [Fact]
    public void Model_WhenAgreeToPrivacyPolicySetTrueAndRelativeRedirectUrlProvided_ShouldRedirectToTourDetailsPageWithRedirectUrlQueryString()
    {
        var model = GivenPrivacyPolicyModel();
        model.AgreeToPrivacy = true;

        var results = model.OnPost("/org/1");

        var redirectToPageResult = results.Should().BeOfType<RedirectToPageResult>();
        redirectToPageResult.Which.PageName.Should().Be("YourDetails");
        redirectToPageResult.Which.RouteValues.Should().BeEquivalentTo(new Dictionary<string, string> { { "RedirectUri", "/org/1" } });
    }

    [Fact]
    public void Model_WhenAgreeToPrivacyPolicySetTrueAndAbsoluteRedirectUrlProvided_ShouldRedirectToTourDetailsPageWithoutRedirectUrlQueryString()
    {
        var model = GivenPrivacyPolicyModel();
        model.AgreeToPrivacy = true;

        var results = model.OnPost("http://test-domain/org/1");

        var redirectToPageResult = results.Should().BeOfType<RedirectToPageResult>();
        redirectToPageResult.Which.PageName.Should().Be("YourDetails");
        redirectToPageResult.Which.RouteValues.Should().BeEquivalentTo(new Dictionary<string, string?> { { "RedirectUri", default } });
    }

    private PrivacyPolicyModel GivenPrivacyPolicyModel()
    {
        return new PrivacyPolicyModel(sessionMock.Object);
    }
}
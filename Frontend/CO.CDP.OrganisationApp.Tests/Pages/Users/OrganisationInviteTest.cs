using CO.CDP.OrganisationApp.Pages.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class OrganisationInviteModelTests
{
    [Fact]
    public void OnGet_RedirectsToOneLoginWithRedirectUriQuerystring()
    {
        var personInviteId = Guid.NewGuid();

        var result = new OrganisationInviteModel().OnGet(personInviteId);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/one-login/sign-in?redirecturi=%2Fclaim-organisation-invite%2F{personInviteId}");
    }
}
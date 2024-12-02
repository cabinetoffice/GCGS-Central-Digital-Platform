using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumStartTest
{
    [Fact]
    public void OnPost_ShouldRedirectToConsortiumNamePageWith()
    {
        var model = new ConsortiumStartModel();

        var result = model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("ConsortiumName");

    }
}
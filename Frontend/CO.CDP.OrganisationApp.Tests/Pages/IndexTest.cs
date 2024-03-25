using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class IndexTest
{
    [Fact]
    public void OnPost_ShouldRedirectToOneLoginCallbackPage()
    {
        var model = GivenIndexModel();

        var results = model.OnPost();

        results.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("YourDetails");
    }

    private static IndexModel GivenIndexModel()
    {
        return new IndexModel();
    }
}

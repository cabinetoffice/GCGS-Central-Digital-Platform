using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class ManageApiKeyStartTest
{
    [Fact]
    public void OnPost_ShouldRedirectToCreateApiKeyPage()
    {
        var model = new ManageApiKeyStartModel { Id = Guid.NewGuid() };

        var result = model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("CreateApiKey");

    }
}
using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class NewApiKeyDetailsTest
{
    [Fact]
    public void OnGet_ShouldReturnPageResult()
    {
        var pageModel = new NewApiKeyDetailsModel
        {
            Id = Guid.NewGuid(),
            ApiKey = "test-api-key"
        };

        var result = pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        pageModel.Id.Should().NotBeEmpty();
        pageModel.ApiKey.Should().Be("test-api-key");
    }
}
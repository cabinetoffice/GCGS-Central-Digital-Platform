using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class CreateApiKeyTest
{
    private readonly CreateApiKeyModel _model = new();
    [Fact]
    public void OnPost_ShouldRedirectToId()
    {
        var model = new CreateApiKeyModel { Id = Guid.NewGuid() };

        var result = model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("CreateApiKey");

    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenCreateApiKeyModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("ApiKeyName", "Enter the api key name");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }
}
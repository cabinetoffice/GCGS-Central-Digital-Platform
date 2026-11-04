using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class NewApiKeyDetailsTest
{
    private static NewApiKeyDetailsModel CreateModel(Guid id) => new()
    {
        Id = id,
        TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
    };

    [Fact]
    public void OnGet_WhenKeyInTempData_ShouldReturnPageWithKeyAndRemoveItFromTempData()
    {
        var pageModel = CreateModel(Guid.NewGuid());
        pageModel.TempData[NewApiKeyDetailsModel.TempDataKey] = "test-api-key";

        var result = pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        pageModel.ApiKey.Should().Be("test-api-key");
        pageModel.TempData.ContainsKey(NewApiKeyDetailsModel.TempDataKey).Should().BeFalse();
    }

    [Fact]
    public void OnGet_WhenKeyNotInTempData_ShouldRedirectToManageApiKeys()
    {
        var id = Guid.NewGuid();
        var pageModel = CreateModel(id);

        var result = pageModel.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/organisation/{id}/manage-api-keys");
        pageModel.ApiKey.Should().BeNull();
    }

    [Fact]
    public void OnGet_WhenCalledTwice_ShouldOnlyShowKeyOnce()
    {
        var pageModel = CreateModel(Guid.NewGuid());
        pageModel.TempData[NewApiKeyDetailsModel.TempDataKey] = "test-api-key";

        pageModel.OnGet().Should().BeOfType<PageResult>();
        pageModel.OnGet().Should().BeOfType<RedirectResult>();
    }
}
